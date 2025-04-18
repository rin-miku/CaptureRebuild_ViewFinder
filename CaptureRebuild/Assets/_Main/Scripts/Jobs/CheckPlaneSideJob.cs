using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct CheckPlaneSideJob : IJobParallelFor
{
    [ReadOnly] public BurstPlane burstPlane;
    [ReadOnly] public NativeArray<float3> meshVertices;
    [WriteOnly] public NativeArray<bool> sideResults;

    public void Execute(int index)
    {
        sideResults[index] = burstPlane.GetSide(meshVertices[index]);
    }
}