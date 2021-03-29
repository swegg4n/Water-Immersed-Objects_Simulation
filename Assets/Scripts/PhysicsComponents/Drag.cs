using UnityEngine;

public class Drag
{
    private Rigidbody rb;
    private MeshSampler ms;
    private float dragCoefficient;

    private float totalSurfaceArea;
    private Vector3[] sampleNormals;

    private Vector3[] debugDragForces;


    public Drag(Rigidbody rb, MeshSampler ms, float dragCoefficient, Mesh[] meshes, Transform transform, float totalSurfaceArea)
    {
        this.rb = rb;
        this.ms = ms;
        this.dragCoefficient = dragCoefficient;
        this.totalSurfaceArea = totalSurfaceArea;

        ms.MeshApproximation.UpdateSamplesPosition();
        CalculateSampleNormals(meshes, transform);

        debugDragForces = new Vector3[ms.MeshApproximation.SampleCount];
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
        int leftSamples = 0;
        int rightSamples = 0;
        for (int i = 0; i < ms.MeshApproximation.SampleCount; i++)
        {
            if (ms.MeshApproximation.Samples[i].GlobalPosition.x < 0) leftSamples++;
            else rightSamples++;
        }

        for (int i = 0; i < ms.MeshApproximation.SampleCount; i++)
        {
            SamplePoint sp = ms.MeshApproximation.Samples[i];

            if (sp.LastPosition != null)
            {
                Vector3 deltaDistance = (sp.GlobalPosition - (Vector3)sp.LastPosition);
                Vector3 deltaVelocity = deltaDistance / Time.deltaTime;
                float velocitySquared = Vector3.SqrMagnitude(deltaVelocity);

                float area = totalSurfaceArea / ms.MeshApproximation.SampleCount * Mathf.Max(Vector3.Dot(deltaDistance.normalized, -sampleNormals[i].normalized), 0); //Mathf.Max(Vector3.Dot(deltaVelocity.normalized, sampleNormals[i].normalized), 0) * totalSurfaceArea;

                float density = (ms.MeshApproximation.IsUnderWater[i] == 1) ? 997.0f : 1.225f;

                Vector3 dragDirection = -deltaDistance.normalized;  //-sampleNormals[i].normalized;

                float dragMagnitude = dragCoefficient * density * velocitySquared * 0.5f * area;
                dragMagnitude = Mathf.Clamp(dragMagnitude, 0.0f, Vector3.Magnitude(deltaVelocity) * rb.mass);
                Vector3 dragForce = dragMagnitude * dragDirection;

                //  m^2 / s^2 * kg / m^3 * m^2    // (m^2 * kg * m^2) / (s^2 * m^3) <=> (kg m/s^2) <=> mass*acceleration = F
                rb.AddForceAtPosition(dragForce, sp.GlobalPosition, ForceMode.Force);

                debugDragForces[i] = dragForce / rb.mass * 100;
            }
            else
            {
                debugDragForces[i] = Vector3.zero;
            }

            sp.LastPosition = sp.GlobalPosition;
        }
    }


    public void DebugDraw()
    {
        Gizmos.color = Color.magenta;
        //for (int i = 0; i < sampleNormals.Length; i++)
        //{
        //    Vector3 samplePos = ms.MeshApproximation.Samples[i].GlobalPosition;
        //    Gizmos.DrawLine(samplePos, samplePos + sampleNormals[i] * Gizmos.probeSize * 10);
        //}

        for (int i = 0; i < debugDragForces.Length; i++)
        {
            Vector3 samplePos = ms.MeshApproximation.Samples[i].GlobalPosition;
            Gizmos.DrawLine(samplePos, samplePos + debugDragForces[i]);
        }
    }

}
