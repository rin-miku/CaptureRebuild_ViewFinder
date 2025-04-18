using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct BurstPlane
{
    public float3 normal;
    public float distance;

    public BurstPlane(float3 normal, float distance)
    {
        this.normal = normal;
        this.distance = distance;
    }

    public bool GetSide(float3 point)
    {
        return math.dot(normal, point) + distance > 0;
    }
}
