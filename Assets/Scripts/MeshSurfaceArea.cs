using UnityEngine;


public static class MeshSurfaceArea
{
    public static float SurfaceAreaOfMesh(Mesh[] _meshes, Transform[] _transforms)
    {
        float totalSurfaceArea = 0.0f;

        for (int i = 0; i < _meshes.Length; i++)
        {
            totalSurfaceArea += SurfaceAreaOfMesh(_meshes[i], _transforms[i]);
        }

        return totalSurfaceArea;
    }

    /// <summary>
    /// See thesis paper for formula reference
    /// </summary>
    public static float SurfaceAreaOfMesh(Mesh _mesh, Transform _transform)
    {
        int[] triangles = _mesh.triangles;
        Vector3[] vertices = _mesh.vertices;

        float area = 0.0f;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 corner = vertices[triangles[i]];
            Vector3 a = vertices[triangles[i + 1]] - corner;
            Vector3 b = vertices[triangles[i + 2]] - corner;

            area += Vector3.Cross(a, b).magnitude;
        }
        Vector3 localScale = _transform.localScale;
        area *= (localScale.x * localScale.y * localScale.z);

        return area / 2.0f;
    }

}