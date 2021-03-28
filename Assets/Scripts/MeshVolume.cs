using UnityEngine;


public static class MeshVolume
{
    public static float VolumeOfMesh(Mesh[] _meshes, Transform[] _transforms)
    {
        float totalVolume = 0;

        for (int i = 0; i < _meshes.Length; i++)
        {
            totalVolume += VolumeOfMesh(_meshes[i], _transforms[i]);
        }

        return totalVolume;
    }

    public static float VolumeOfMesh(Mesh _mesh, Transform _transform)
    {
        float volume = 0;
        Vector3[] vertices = _mesh.vertices;
        int[] triangles = _mesh.triangles;
        for (int i = 0; i < _mesh.triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        Vector3 localScale = _transform.localScale;
        volume *= (localScale.x * localScale.y * localScale.z);

        return Mathf.Abs(volume);
    }

    private static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }
}
