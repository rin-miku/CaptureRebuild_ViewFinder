using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct VertexTransformJob : IJobParallelFor
{
    [ReadOnly] public float4x4 transformMatrix;
    public NativeArray<float3> meshVertices;

    public void Execute(int index)
    {
        float4 v = new float4(meshVertices[index], 1f);
        meshVertices[index] = math.mul(transformMatrix, v).xyz;
    }
}
