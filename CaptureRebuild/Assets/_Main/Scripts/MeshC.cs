using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshC : MonoBehaviour
{
    public MeshFilter meshFilter;
    private Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        mesh = meshFilter.mesh;

        int subMeshCount = mesh.subMeshCount;
        int UVCount = 0;

        // 计算有效UV通道数量
        for (VertexAttribute attr = VertexAttribute.TexCoord0; attr <= VertexAttribute.TexCoord7; attr++)
            if (mesh.HasVertexAttribute(attr))
                UVCount++;

        var meshVerts = new List<Vector3>();
        var meshNormals = new List<Vector3>();
        var meshUVChannels = new List<List<Vector2>>();
        var meshTrigs = new List<List<int>>();

        // 获得顶点数据列表
        mesh.GetVertices(meshVerts);
        Debug.Log($"mesh顶点数量为 {meshVerts.Count}");
        /*
        Debug.Log(meshVerts.Count);
        for(int i = 0; i < meshVerts.Count; i++)
        {
            Debug.Log(meshVerts[i]);
        }
        */
        // 获得顶点法线数据列表
        mesh.GetNormals(meshNormals);
        // 获取纹理坐标数据
        for (int i = 0; i < UVCount; i++)
        {
            meshUVChannels.Add(new List<Vector2>());
            mesh.GetUVs(i, meshUVChannels[i]);
            Debug.Log($"uv{i} 的坐标数据为{meshUVChannels[i].Count}");
        }
        /*
        Debug.Log(meshUVChannels[0].Count);
        for (int i = 0; i < meshUVChannels[0].Count; i++)
        {
            Debug.Log(meshUVChannels[0][i]);
        }
        */
        // 获取指定子网格的三角形索引，表示网格顶点组成三角形的方式
        for (int i = 0; i < subMeshCount; i++)
        {
            meshTrigs.Add(new List<int>());
            mesh.GetTriangles(meshTrigs[i], i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
