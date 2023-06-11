using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexMassCalculator : MonoBehaviour
{
    public MeshFilter meshFilter;
    public float baseMass = 1f;

    void Start()
    {
        CalculateVertexMass();
    }

    void CalculateVertexMass()
    {
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        float[] vertexMasses = new float[vertices.Length];

        // 计算每个顶点周围的相邻顶点数量
        int[] adjacentVertexCounts = new int[vertices.Length];
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int vertexIndexA = triangles[i];
            int vertexIndexB = triangles[i + 1];
            int vertexIndexC = triangles[i + 2];

            adjacentVertexCounts[vertexIndexA]++;
            adjacentVertexCounts[vertexIndexB]++;
            adjacentVertexCounts[vertexIndexC]++;
        }

        // 根据相邻顶点数量计算顶点质量
        for (int i = 0; i < vertices.Length; i++)
        {
            int adjacentCount = adjacentVertexCounts[i];
            float mass = baseMass * (adjacentCount + 1); // 这里简单地将相邻顶点数量作为质量的权重
            vertexMasses[i] = mass;
        }

        // 在这里可以将顶点质量用于布料模拟等其他需要的地方
        // ...

        // 打印顶点质量
        for (int i = 0; i < vertexMasses.Length; i++)
        {
            Debug.Log("Mass of vertex " + i + ": " + vertexMasses[i]);
        }
    }
}

