using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;


public partial class CaptureRebuild : MonoBehaviour
{
    private void CutMesh(Mesh mesh, Plane plane, Transform objectTransform, bool planeOutSide = false)
    {
        // 初始化网格数据
        List<Vector3> meshVertices = new();
        mesh.GetVertices(meshVertices);
        NativeArray<float3> jobMeshVertices = CommonTools.ToNativeFloat3Array(meshVertices, Allocator.TempJob);

        List<Vector3> meshNormals = new();
        mesh.GetNormals(meshNormals);

        int validUVCount = 0;
        List<List<Vector2>> meshUVs = new();
        for (VertexAttribute attr = VertexAttribute.TexCoord0; attr <= VertexAttribute.TexCoord7; attr++)
        {
            if (mesh.HasVertexAttribute(attr)) validUVCount++;
        }
        for (int i = 0; i < validUVCount; i++)
        {
            meshUVs.Add(new List<Vector2>());
            mesh.GetUVs(i, meshUVs[i]);
        }

        int subMeshCount = mesh.subMeshCount;
        List<List<int>> meshTriangles = new();
        for (int i = 0; i < subMeshCount; i++)
        {
            meshTriangles.Add(new List<int>());
            mesh.GetTriangles(meshTriangles[i], i);
        }

        // 预留新网格数据结构
        List<Vector3> newVertices = new List<Vector3>(meshVertices.Count);
        List<Vector3> newNormals = new List<Vector3>(meshNormals.Count);
        List<List<Vector2>> newUVs = new List<List<Vector2>>(meshUVs.Count);
        List<List<int>> newTriangles = new List<List<int>>(meshTriangles.Count);
        List<(Vector3, Vector3)> newEdgePoints = new List<(Vector3, Vector3)>();
        foreach (List<Vector2> uvs in meshUVs)
        {
            newUVs.Add(new List<Vector2>(uvs.Count * 2));
        }
        foreach (List<int> triangles in meshTriangles)
        {
            newTriangles.Add(new List<int>(triangles.Count * 2));
        }

        // 本地空间转到世界空间
        Matrix4x4 localToWorldMatrix = objectTransform.localToWorldMatrix;
        VertexTransformJob vertexLocalToWorldJob = new VertexTransformJob
        {
            transformMatrix = localToWorldMatrix,
            meshVertices = jobMeshVertices
        };
        JobHandle vertexLocalToWorldJobHandle = vertexLocalToWorldJob.Schedule(meshVertices.Count, 64);
        vertexLocalToWorldJobHandle.Complete();
        meshVertices = CommonTools.ToVector3List(jobMeshVertices);

        // 判断顶点在平面内外侧情况
        BurstPlane burstPlane = new BurstPlane(plane.normal, plane.distance);
        NativeArray<bool> jobSideResults = new NativeArray<bool>(meshVertices.Count, Allocator.TempJob);
        CheckPlaneSideJob checkPlaneSideJob = new CheckPlaneSideJob
        {
            burstPlane = burstPlane,
            meshVertices = jobMeshVertices,
            sideResults = jobSideResults
        };
        JobHandle sideHandle = checkPlaneSideJob.Schedule(meshVertices.Count, 64);
        sideHandle.Complete();

        List<bool> sideResults = CommonTools.ToBoolList(jobSideResults);
        jobMeshVertices.Dispose();
        jobSideResults.Dispose();
        if (!sideResults.Contains(true) && planeOutSide) mesh.Clear();
        if (!sideResults.Contains(true) || !sideResults.Contains(false)) return;

        // 网格切割
        Dictionary<VertexKey, int> newMeshVertexDict = new();
        int[] verticesInside;
        int[] verticesOutside;
        List<int> triangleVertices;

        for (int i = 0; i < subMeshCount; i++)
        {
            for (int j = 0; j < meshTriangles[i].Count; j += 3)
            {
                triangleVertices = new List<int> 
                { 
                    meshTriangles[i][j], 
                    meshTriangles[i][j + 1], 
                    meshTriangles[i][j + 2] 
                };

                verticesInside = triangleVertices.Where(x => sideResults[x]).ToArray();
                verticesOutside = triangleVertices.Where(x => !sideResults[x]).ToArray();

                switch (verticesInside.Count())
                {
                    case 1:
                        if (triangleVertices.IndexOf(verticesInside[0]) == 1)
                            Check1In2Out(verticesInside[0], verticesOutside[1], verticesOutside[0], i);
                        else
                            Check1In2Out(verticesInside[0], verticesOutside[0], verticesOutside[1], i);
                        break;
                    case 2:
                        if (triangleVertices.IndexOf(verticesOutside[0]) == 1)
                            Check2In1Out(verticesInside[0], verticesInside[1], verticesOutside[0], i);
                        else
                            Check2In1Out(verticesInside[1], verticesInside[0], verticesOutside[0], i);
                        break;
                    case 3:
                        CheckAllIn(triangleVertices[0], triangleVertices[1], triangleVertices[2], i);
                        break;
                }
            }
        }

        // 查找循环边生成多边形
        Vector3 polygonNormal = plane.flipped.normal;
        List<List<Vector3>> newPolygons = new();
        List<Vector2> polygonUVs = Enumerable.Repeat(Vector2.zero, validUVCount).ToList();

        int edgeIndex = -1;
        while (newEdgePoints.Count != 0)
        {
            if (edgeIndex == -1)
            {
                newPolygons.Add(new List<Vector3>());
                edgeIndex = 0;
            }
            var (p1, p2) = newEdgePoints[edgeIndex];
            newEdgePoints.RemoveAt(edgeIndex);
            newPolygons.Last().Add(p1);
            edgeIndex = newEdgePoints.FindIndex(x => x.Item1 == p2);
        }

        foreach (List<Vector3> polygon in newPolygons)
        {
            for (int i = polygon.Count - 2; i >= 1 ; i--)
            {
                if (CommonTools.CheckCollinear(polygon[i - 1], polygon[i], polygon[i + 1]))
                {
                    polygon.RemoveAt(i);
                }
            }

            int[] indices = polygon.Select(vertex => AddNewVertex(vertex, polygonNormal, polygonUVs)).ToArray();
            int pivotIndex = indices[0];
            for (int i = 1; i < polygon.Count - 1; i++)
            {
                newTriangles.Last().AddRange(new int[] { pivotIndex, indices[i + 1], indices[i] });
            }
        }

        // 世界空间转回本地空间
        Matrix4x4 worldToLocalMatrix = objectTransform.worldToLocalMatrix;
        NativeArray<float3> jobNewVertices = CommonTools.ToNativeFloat3Array(newVertices, Allocator.TempJob);
        VertexTransformJob vertexWorldToLocalJob = new VertexTransformJob
        {
            transformMatrix = worldToLocalMatrix,
            meshVertices = jobNewVertices
        };
        JobHandle vertexWorldToLocalHandle = vertexWorldToLocalJob.Schedule(newVertices.Count, 64);
        vertexWorldToLocalHandle.Complete();
        newVertices = CommonTools.ToVector3List(jobNewVertices);
        jobNewVertices.Dispose();

        // 回填网格数据
        mesh.Clear();
        mesh.SetVertices(newVertices);
        mesh.SetNormals(newNormals);
        for (int i = 0; i < validUVCount; i++)
        {
            mesh.SetUVs(i, newUVs[i]);
        }
        mesh.subMeshCount = subMeshCount;
        for (int i = 0; i < subMeshCount; i++)
        {
            mesh.SetTriangles(newTriangles[i], i);
        }

        // 局部方法
        int AddOldVertex(int index)
        {
            return AddNewVertex(meshVertices[index], meshNormals[index], meshUVs.Select(x => x[index]).ToList());
        }

        int AddNewVertex(Vector3 vertex, Vector3 normal, List<Vector2> uvs)
        {
            VertexKey vertexKey = new VertexKey(vertex, normal, uvs);
            if (!newMeshVertexDict.TryGetValue(vertexKey, out int index))
            {
                index = newMeshVertexDict.Count;
                newMeshVertexDict.Add(vertexKey, index);
                newVertices.Add(vertex);
                newNormals.Add(normal);
                for(int i = 0; i < validUVCount; i++)
                {
                    newUVs[i].Add(uvs[i]);
                }
            }
            return index;
        }

        void Check1In2Out(int index1In, int index2Out, int index3Out, int submesh)
        {
            int newVertex1Index = EdgeCut(index1In, index2Out);
            int newVertex2Index = EdgeCut(index1In, index3Out);

            newEdgePoints.Add((newVertices[newVertex1Index], newVertices[newVertex2Index]));

            int oldVert1Index = AddOldVertex(index1In);

            int[] trianglesToAdd = new int[] { oldVert1Index, newVertex1Index, newVertex2Index };

            newTriangles[submesh].AddRange(trianglesToAdd);
        }

        void Check2In1Out(int index1In, int index2In, int index3Out, int submesh)
        {
            int newVertex1Index = EdgeCut(index1In, index3Out);
            int newVertex2Index = EdgeCut(index2In, index3Out);

            newEdgePoints.Add((newVertices[newVertex1Index], newVertices[newVertex2Index]));

            int oldVertex1Index = AddOldVertex(index1In);
            int oldVertex2Index = AddOldVertex(index2In);

            int[] trianglesToAdd = new int[] 
            {
                oldVertex1Index, newVertex1Index, oldVertex2Index,
                newVertex1Index, newVertex2Index, oldVertex2Index
            };

            newTriangles[submesh].AddRange(trianglesToAdd);
        }

        void CheckAllIn(int index1In, int index2In, int index3In, int submesh)
        {
            int oldVertex1Index = AddOldVertex(index1In);
            int oldVertex2Index = AddOldVertex(index2In);
            int oldVertex3Index = AddOldVertex(index3In);

            int[] trianglesToAdd = new int[] { oldVertex1Index, oldVertex2Index, oldVertex3Index };

            newTriangles[submesh].AddRange(trianglesToAdd);
        }

        int EdgeCut(int vertexInIndex, int vertexOutIndex)
        {
            Vector3 vertexIn = meshVertices[vertexInIndex];
            Vector3 vertexOut = meshVertices[vertexOutIndex];
            Vector3 direction = vertexOut - vertexIn;

            Ray ray = new Ray(vertexIn, direction);
            plane.Raycast(ray, out float distance);
            float ratioIntersection = distance / direction.magnitude;

            List<Vector2> newUVs = new();
            for (int i = 0; i < validUVCount; i++)
            {
                Vector2 uvIn = meshUVs[i][vertexInIndex];
                Vector2 uvOut = meshUVs[i][vertexOutIndex];
                newUVs.Add(Vector2.Lerp(uvIn, uvOut, ratioIntersection));
            }

            Vector3 normalIn = meshNormals[vertexInIndex];
            Vector3 normalOut = meshNormals[vertexOutIndex];
            Vector3 newNormal = Vector3.Lerp(normalIn, normalOut, ratioIntersection).normalized;

            Vector3 newVertex = ray.GetPoint(distance);
            int newVertexIndex = AddNewVertex(newVertex, newNormal, newUVs);

            return newVertexIndex;
        }
    }
}
