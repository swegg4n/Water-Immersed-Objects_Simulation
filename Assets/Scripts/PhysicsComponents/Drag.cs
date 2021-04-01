using UnityEngine;

public class Drag
{
    private Rigidbody rb;
    private MeshSampler ms;
    private Transform transform;

    private float dragCoefficient;

    private float totalSurfaceArea;
    private Vector3[] sampleNormals;

    private Quaternion lastRotation;

    private Vector3[] debugDragForces;
    private Vector3[] debugDeltaDirForces;


    public Drag(Rigidbody rb, MeshSampler ms, float dragCoefficient, Mesh[] meshes, Transform transform, float totalSurfaceArea)
    {
        this.rb = rb;
        this.ms = ms;
        this.transform = transform;
        this.dragCoefficient = dragCoefficient;
        this.totalSurfaceArea = totalSurfaceArea;

        ms.MeshApproximation.UpdateSamplesPosition();
        CalculateSampleNormals(meshes, transform);

        lastRotation = transform.rotation;

        debugDragForces = new Vector3[ms.MeshApproximation.SampleCount];
        debugDeltaDirForces = new Vector3[ms.MeshApproximation.SampleCount];
    }


    /// <summary>
    /// Maps all samples to a normal vector, based on the normal of the sample's closest vertex
    /// </summary>
    private void CalculateSampleNormals(Mesh[] meshes, Transform transform)
    {
        sampleNormals = new Vector3[ms.MeshApproximation.SampleCount];

        Vector3[] vertexPositions = BenchmarkHelper.MeshArrayToVerticesArray(meshes, transform);
        Vector3[] vertexNormals = MeshArrayToNormalsArray(meshes);

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

    /// <summary>
    /// Retrieves all vertex-normals from a collection of meshes
    /// </summary>
    private Vector3[] MeshArrayToNormalsArray(Mesh[] meshes)
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


    /// <summary>
    /// Updates the normals based on the object's change in rotation
    /// </summary>
    private void UpdateSampleNormals()
    {
        Matrix4x4 m = Matrix4x4.Rotate(transform.rotation * Quaternion.Inverse(lastRotation));

        for (int i = 0; i < sampleNormals.Length; i++)
        {
            sampleNormals[i] = m.MultiplyPoint3x4(sampleNormals[i]);
        }

        lastRotation = transform.rotation;
    }


    public void Update()
    {
        UpdateSampleNormals();

        for (int i = 0; i < ms.MeshApproximation.SampleCount; i++)
        {
            SamplePoint sp = ms.MeshApproximation.Samples[i];

            if (sp.LastPosition != null)
            {
                Vector3 deltaDistance = (sp.GlobalPosition - (Vector3)sp.LastPosition);

                debugDeltaDirForces[i] = deltaDistance;

                Vector3 deltaVelocity = deltaDistance / Time.deltaTime;
                float velocitySquared = Vector3.SqrMagnitude(deltaVelocity);

                float area = totalSurfaceArea / ms.MeshApproximation.SampleCount * Vector3.Dot(deltaDistance.normalized, sampleNormals[i]);

                float density = (ms.MeshApproximation.IsUnderWater[i] == 1) ? 997.0f : 1.225f;      // Water drag  vs  air drag 

                Vector3 dragDirection = -sampleNormals[i];  //Force is directed against the normal, which contributes to both drag & lift

                float dragMagnitude = dragCoefficient * density * velocitySquared * 0.5f * area;    //See formula reference in paper
                dragMagnitude = Mathf.Clamp(dragMagnitude, 0.0f, Vector3.Magnitude(deltaVelocity));
                Vector3 dragForce = dragMagnitude * dragDirection;

                //  m^2 / s^2 * kg / m^3 * m^2  <=>  (m^2 * kg * m^2) / (s^2 * m^3) <=> (kg m/s^2) <=> mass * acceleration = F
                rb.AddForceAtPosition(dragForce, sp.GlobalPosition, ForceMode.Force);

                debugDragForces[i] = dragForce;     //For debugging
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
        /*Debug normals*/
        //Gizmos.color = Color.cyan;
        //for (int i = 0; i < sampleNormals.Length; i++)
        //{
        //    Vector3 samplePos = ms.MeshApproximation.Samples[i].GlobalPosition;
        //    Gizmos.DrawLine(samplePos, samplePos + sampleNormals[i] * Gizmos.probeSize * 10);
        //}

        /*Debug drag forces*/
        Gizmos.color = Color.magenta;
        for (int i = 0; i < debugDragForces.Length; i++)
        {
            Vector3 samplePos = ms.MeshApproximation.Samples[i].GlobalPosition;
            Gizmos.DrawLine(samplePos, samplePos + debugDragForces[i]);
        }
    }

}
