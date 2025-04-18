using System;
using System.Collections.Generic;
using UnityEngine;

struct VertexKey : IEquatable<VertexKey>
{
    public Vector3 vertex;
    public Vector3 normal;
    public Vector2[] uvs;

    public VertexKey(Vector3 vertex, Vector3 normal, List<Vector2> uvs)
    {
        this.vertex = vertex;
        this.normal = normal;
        this.uvs = uvs.ToArray();
    }

    public bool Equals(VertexKey other)
    {
        if (!vertex.Equals(other.vertex) || !normal.Equals(other.normal)) return false;
        if (uvs.Length != other.uvs.Length) return false;
        for (int i = 0; i < uvs.Length; i++)
        {
            if (!uvs[i].Equals(other.uvs[i])) return false;
        }
        return true;
    }

    public override bool Equals(object obj) => obj is VertexKey other && Equals(other);

    public override int GetHashCode()
    {
        int hash = vertex.GetHashCode();
        hash = (hash * 397) ^ normal.GetHashCode();
        foreach (var uv in uvs)
        {
            hash = (hash * 397) ^ uv.GetHashCode();
        }
        return hash;
    }
}
