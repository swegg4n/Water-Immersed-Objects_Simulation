using UnityEngine;

public static class BenchmarkHelper
{

    public static T AverageValue<T>(T[] t)
    {
        dynamic avg = 0;

        if (t == null || t.Length < 0)
            return (dynamic)0.0f;

        for (int i = 0; i < t.Length; i++)
            avg += (dynamic)t[i] / t.Length;

        return avg;
    }


    public static float GetComputeTimeMS()
    {
        return Time.deltaTime * 1000;
    }
    public static float GetFPS()
    {
        return 1.0f / Time.deltaTime;
    }

    public static long GetMemoryUsage()
    {
        return System.GC.GetTotalMemory(false); //Returns the memory usage in bytes
    }


    public static Vector3[] MeshArrayToVerticesArray(Mesh[] originalMeshes, Transform transform)
    {
        Matrix4x4 localToWorld = transform.localToWorldMatrix;

        Vector3[][] meshVertices = new Vector3[originalMeshes.Length][];

        int vertCount = 0;
        for (int i = 0; i < originalMeshes.Length; i++)
        {
            meshVertices[i] = originalMeshes[i].vertices;
            vertCount += originalMeshes[i].vertexCount;
        }
        Vector3[] originalVertices = new Vector3[vertCount];

        int c = 0;
        for (int i = 0; i < meshVertices.Length; i++)
        {
            for (int j = 0; j < meshVertices[i].Length; j++, c++)
            {
                originalVertices[c] = localToWorld.MultiplyPoint3x4(meshVertices[i][j]);
            }
        }

        return originalVertices;
    }

    public static float VerticesSqrError(Vector3[] originalVertices, Vector3[] referenceVertices)
    {
        float[] verticesSqrError = new float[originalVertices.Length];

        for (int i = 0; i < verticesSqrError.Length; i++)
        {
            verticesSqrError[i] = Vector3.SqrMagnitude(referenceVertices[i] - originalVertices[i]);
        }

        float avgVerticesSqrError = AverageValue(verticesSqrError);
        //Debug.Log("Avg sqr error:  " + avgVerticesSqrError);
        return avgVerticesSqrError;
    }

}
