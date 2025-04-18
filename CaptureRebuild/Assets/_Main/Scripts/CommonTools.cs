using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class CommonTools
{
    public static Texture2D GetCameraTexture(Camera camera)
    {
        RenderTexture targetTexture = camera.targetTexture;
        RenderTexture activeRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;
        int width = camera.targetTexture.width;
        int height = camera.targetTexture.height;

        Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = activeRT;

        return texture2D;
    }

    public static NativeArray<float3> ToNativeFloat3Array(List<Vector3> vector3List, Allocator allocator)
    {
        NativeArray<float3> result = new NativeArray<float3>(vector3List.Count, allocator, NativeArrayOptions.UninitializedMemory);

        for (int i = 0; i < vector3List.Count; i++)
        {
            result[i] = vector3List[i];
        }    

        return result;
    }

    public static List<Vector3> ToVector3List(NativeArray<float3> float3Array)
    {
        List<Vector3> result = new List<Vector3>(float3Array.Length);

        for (int i = 0; i < float3Array.Length; i++)
        {
            float3 f = float3Array[i];
            result.Add(new Vector3(f.x, f.y, f.z));
        }

        return result;
    }

    public static List<bool> ToBoolList(NativeArray<bool> boolArray)
    {
        List<bool> results = new List<bool>(boolArray.Length);

        for (int i = 0; i < boolArray.Length; i++)
        {
            results.Add(boolArray[i]);
        }

        return results;
    }

    public static bool CheckCollinear(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        return Vector3.Cross(ab, ac).magnitude == 0;
    }
}
