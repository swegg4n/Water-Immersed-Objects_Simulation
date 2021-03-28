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


    public static float VerticesSqrError(Mesh[] originalMeshes, Vector3[] referenceVertices)
    {
        int meshesVertexCount = 0;
        for (int i = 0; i < originalMeshes.Length; i++)
        {
            meshesVertexCount += originalMeshes[i].vertexCount;
        }
        Vector3[] originalVertices = new Vector3[meshesVertexCount];

        int vertexCounter = 0;
        for (int i = 0; i < originalMeshes.Length; i++)
        {
            for (int j = 0; j < originalMeshes[i].vertexCount; j++, vertexCounter++)
            {
                originalVertices[vertexCounter] = originalMeshes[i].vertices[j];
            }
        }

        return VerticesSqrError(originalVertices, referenceVertices);
    }

    public static float VerticesSqrError(Vector3[] originalVertices, Vector3[] referenceVertices)
    {
        float[] verticesSqrError = new float[originalVertices.Length];

        for (int i = 0; i < verticesSqrError.Length; i++)
        {
            verticesSqrError[i] = Vector3.SqrMagnitude(referenceVertices[i] - originalVertices[i]);
        }

        float avgVerticesSqrError = AverageValue(verticesSqrError);
        Debug.Log("Avg sqr error:  " + avgVerticesSqrError);

        return avgVerticesSqrError;
    }

}
