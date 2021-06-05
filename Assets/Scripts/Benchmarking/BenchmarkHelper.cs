using UnityEngine;

public static class BenchmarkHelper
{

    /// <summary>
    /// Calculate the average value of an array
    /// </summary>
    public static T AverageValue<T>(params T[] t)
    {
        dynamic avg = 0;

        if (t == null || t.Length < 0)
            return (dynamic)0.0f;

        for (int i = 0; i < t.Length; i++)
            avg += (dynamic)t[i] / t.Length;

        return avg;
    }


    /// <summary>
    /// Get the time this frame took to compute (in milliseconds)
    /// </summary>
    public static float GetComputeTimeMS()
    {
        return Time.deltaTime * 1000;
    }

    /// <summary>
    /// Get the current frame rate
    /// </summary>
    public static float GetFPS()
    {
        return 1.0f / Time.deltaTime;
    }

    /// <summary>
    /// Get (an approximation of) the current size of all allocated memory
    /// </summary>
    public static long GetMemoryUsage()
    {
        return System.GC.GetTotalMemory(false); //Returns the memory usage in bytes
    }


    /// <summary>
    /// Get all vertices' global position from a collection of meshes
    /// </summary>
    public static Vector3[] MeshArrayToVerticesArray(Mesh[] originalMeshes, Transform[] transforms)
    {
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
            Matrix4x4 rot = Matrix4x4.Rotate(transforms[i].rotation);
            for (int j = 0; j < meshVertices[i].Length; j++, c++)
            {
                originalVertices[c] = rot.MultiplyPoint3x4(meshVertices[i][j]) + transforms[i].position;
            }
        }

        return originalVertices;
    }


    /// <summary>
    /// Get the square error between two collections of vertex positions
    /// </summary>
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
