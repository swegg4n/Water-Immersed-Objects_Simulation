using UnityEngine;

public class Drag
{
    private Rigidbody rb;
    private MeshSampler ms;
    private float dragCoefficient;

    Vector3[] sampleNormals;


    public Drag(Rigidbody rb, MeshSampler ms, float viscosity, Mesh[] meshes, Transform transform)
    {
        this.rb = rb;
        this.ms = ms;
        this.dragCoefficient = viscosity;

        ms.MeshApproximation.UpdateSamplesPosition();
        CalculateSampleNormals(meshes, transform);
    }

    private void CalculateSampleNormals(Mesh[] meshes, Transform transform)
    {
        sampleNormals = new Vector3[ms.MeshApproximation.SampleCount];

        Vector3[] vertexPositions = BenchmarkHelper.MeshArrayToVerticesArray(meshes, transform);
        Vector3[] vertexNormals = MeshArrayToNormalsArray(meshes, transform);

        for (int i = 0; i < ms.MeshApproximation.SampleCount; i++)
        {
            float closestVertexDistance = float.MaxValue;
            int closestVertIndex = -1;

            for (int j = 0; j < vertexPositions.Length; j++)
            {
                float sqrDistance = Vector3.SqrMagnitude(ms.MeshApproximation.Samples[i].GlobalPosition - vertexPositions[j]);
                if (sqrDistance < closestVertexDistance)
                {
                    closestVertexDistance = sqrDistance;
                    closestVertIndex = j;
                }
            }
            sampleNormals[i] = vertexNormals[closestVertIndex];
        }
    }


    private Vector3[] MeshArrayToNormalsArray(Mesh[] meshes, Transform transform)
    {
        Vector3[][] meshNormals = new Vector3[meshes.Length][];

        int normalsCount = 0;
        for (int i = 0; i < meshes.Length; i++)
        {
            meshNormals[i] = meshes[i].normals;
            normalsCount += meshes[i].vertexCount;
        }
        Vector3[] normals = new Vector3[normalsCount];

        int c = 0;
        for (int i = 0; i < meshNormals.Length; i++)
        {
            for (int j = 0; j < meshNormals[i].Length; j++, c++)
            {
                normals[c] = meshNormals[i][j];
            }
        }

        return normals;
    }


    public void Update()
    {
        for (int i = 0; i < ms.MeshApproximation.SampleCount; i++)
        {
            SamplePoint sp = ms.MeshApproximation.Samples[i];

            if (sp.LastPosition != null)
            {
                Vector3 deltaVelocity = sp.GlobalPosition - (Vector3)sp.LastPosition;

                float velocitySquared = Vector3.SqrMagnitude(deltaVelocity);

                float dotFraction = Mathf.Max(Vector3.Dot(deltaVelocity.normalized, sampleNormals[i]), 0.0f);
                float density = (ms.MeshApproximation.IsUnderWater[i] == 1) ? 997.0f : 1.225f;
  
                Vector3 dragDirection = -deltaVelocity.normalized;
                Vector3 dragForce = dragCoefficient * density * velocitySquared * 0.5f * dotFraction * dragDirection;

                rb.AddForceAtPosition(dragForce / ms.MeshApproximation.SampleCount, sp.GlobalPosition, ForceMode.VelocityChange);
            }

            sp.LastPosition = sp.GlobalPosition;
        }
    }

    public void DebugDraw()
    {
        Gizmos.color = Color.magenta;
        for (int i = 0; i < sampleNormals.Length; i++)
        {
            Vector3 samplePos = ms.MeshApproximation.Samples[i].GlobalPosition;
            Gizmos.DrawLine(samplePos, samplePos + sampleNormals[i] * Gizmos.probeSize * 10);
        }
    }

}
