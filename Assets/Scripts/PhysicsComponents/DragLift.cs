using System;
using UnityEngine;

public class DragLift
{
    private Rigidbody rb;
    private MeshSampler ms;
    private Transform transform;

    private float dragCoefficient;
    private float liftCoefficient;

    private float totalSurfaceArea;

    private Vector3[] debugDragForces;
    private Vector3[] debugLiftForces;



    public DragLift(Rigidbody rb, MeshSampler ms, float dragCoefficient, float liftCoefficient, Mesh[] meshes, Transform[] transforms, Transform modelTransform, float totalSurfaceArea)
    {
        this.rb = rb;
        this.ms = ms;
        this.transform = modelTransform;
        this.dragCoefficient = dragCoefficient;
        this.liftCoefficient = liftCoefficient;
        this.totalSurfaceArea = totalSurfaceArea;

        ms.MeshApproximation.UpdateSamplesPosition();

        debugDragForces = new Vector3[ms.MeshApproximation.OnHullIndices.Length];
        debugLiftForces = new Vector3[ms.MeshApproximation.OnHullIndices.Length];
    }


    ///// <summary>
    ///// Maps all samples to a normal vector, based on the normal of the sample's closest vertex
    ///// </summary>
    //private void CalculateSampleNormals(Mesh[] meshes, Transform[] transforms)
    //{
    //    sampleNormals = new Vector3[ms.MeshApproximation.SampleCount];

    //    //for (int i = 0; i < ms.MeshApproximation.SampleCount; i++)
    //    //{
    //    //    Vector3 boundsClosestPos =mesh
    //    //}

    //    //Vector3[] vertexPositions = BenchmarkHelper.MeshArrayToVerticesArray(meshes, transforms);
    //    //debugVertices = vertexPositions;     //DEBUG
    //    //Vector3[] vertexNormals = MeshArrayToNormalsArray(meshes, transforms);

    //    //for (int i = 0; i < ms.MeshApproximation.SampleCount; i++)
    //    //{
    //    //    sampleNormals[i] = Vector3.zero;

    //    //    for (int j = 0; j < vertexPositions.Length; j++)
    //    //    {
    //    //        float inverseDistance = 1.0f / Mathf.Pow(Vector3.SqrMagnitude(ms.MeshApproximation.Samples[i].GlobalPosition - vertexPositions[j]), 2.0f);

    //    //        sampleNormals[i] += (vertexNormals[j] * inverseDistance) / vertexNormals.Length;
    //    //    }

    //    //    sampleNormals[i].Normalize();
    //    //}
    //}


    ///// <summary>
    ///// Retrieves all vertex-normals from a collection of meshes
    ///// </summary>
    //private Vector3[] MeshArrayToNormalsArray(Mesh[] meshes, Transform[] transforms)
    //{
    //    Vector3[][] meshNormals = new Vector3[meshes.Length][];

    //    int normalsCount = 0;
    //    for (int i = 0; i < meshes.Length; i++)
    //    {
    //        meshNormals[i] = meshes[i].normals;
    //        normalsCount += meshes[i].vertexCount;
    //    }
    //    Vector3[] normals = new Vector3[normalsCount];

    //    int c = 0;
    //    for (int i = 0; i < meshNormals.Length; i++)
    //    {
    //        Matrix4x4 rot = Matrix4x4.Rotate(transforms[i].rotation);
    //        for (int j = 0; j < meshNormals[i].Length; j++, c++)
    //        {
    //            normals[c] = rot.MultiplyPoint3x4(meshNormals[i][j]);
    //        }
    //    }

    //    return normals;
    //}


    ///// <summary>
    ///// Updates the normals based on the object's change in rotation
    ///// </summary>
    //private void UpdateSampleNormals()
    //{
    //    Matrix4x4 m = Matrix4x4.Rotate(transform.rotation * Quaternion.Inverse(lastRotation));

    //    for (int i = 0; i < sampleNormals.Length; i++)
    //    {
    //        sampleNormals[i] = m.MultiplyPoint3x4(sampleNormals[i]);
    //    }

    //    lastRotation = transform.rotation;
    //}


    public void Update()
    {
        for (int i = 0; i < ms.MeshApproximation.OnHullIndices.Length; i++)
        {
            SamplePoint sp = ms.MeshApproximation.Samples[ms.MeshApproximation.OnHullIndices[i]];

            if (sp.LastPosition != null)
            {
                Vector3 deltaDistance = (sp.GlobalPosition - (Vector3)sp.LastPosition);
                Vector3 deltaVelocity = deltaDistance / Time.deltaTime;

                float velocitySquared = Vector3.SqrMagnitude(deltaVelocity);

                float density = (ms.MeshApproximation.IsUnderWater[ms.MeshApproximation.OnHullIndices[i]] == 1) ? WaterImmersedRigidbody.FluidDensity : 1.225f;      // Water drag  vs  air drag 

                Vector3 dragDirection = -deltaDistance.normalized;
                Vector3 T = (Vector3.Cross(deltaVelocity, -sp.Normal));
                Vector3 liftDirection = Vector3.Cross(T, deltaVelocity).normalized;


                float areaFraction = totalSurfaceArea / ms.MeshApproximation.OnHullIndices.Length;

                float dragSurfaceArea = areaFraction * Mathf.Max(Vector3.Dot(deltaDistance.normalized, sp.Normal), 0);
                float liftSurfaceArea = Mathf.Sqrt(Mathf.Pow(areaFraction, 2.0f) - Mathf.Pow(dragSurfaceArea, 2.0f)) * Mathf.Max(Vector3.Dot(deltaDistance.normalized, sp.Normal), 0.0f);        

                float dragMagnitude = dragCoefficient * density * velocitySquared * 0.5f * dragSurfaceArea;    //See formula reference in paper
                float liftMagnitude = liftCoefficient * density * velocitySquared * 0.5f * liftSurfaceArea;

                float maxDragForce = rb.mass * deltaVelocity.magnitude / Time.deltaTime; //F_{max} = m*a_{particle}
                dragMagnitude = Mathf.Min(dragMagnitude, maxDragForce);


                Vector3 dragForce = dragMagnitude * dragDirection;
                Vector3 liftForce = liftMagnitude * liftDirection;

                //  m^2 / s^2 * kg / m^3 * m^2  <=>  (m^2 * kg * m^2) / (s^2 * m^3) <=> (kg m/s^2) <=> mass * acceleration = F
                rb.AddForceAtPosition(dragForce, sp.GlobalPosition, ForceMode.Force);
                rb.AddForceAtPosition(liftForce, sp.GlobalPosition, ForceMode.Force);

                debugDragForces[i] = dragForce / 100.0f;     //For debugging
                debugLiftForces[i] = liftForce / 100.0f;
            }
            else
            {
                debugDragForces[i] = Vector3.zero;
                debugLiftForces[i] = Vector3.zero;
            }

            sp.LastPosition = sp.GlobalPosition;
        }

    }


    public void DebugDraw()
    {
        /*Debug drag forces*/
        if (DebugManager.Instance && DebugManager.Instance.DebugDrag)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < debugDragForces.Length; i++)
            {
                Vector3 samplePos = ms.MeshApproximation.Samples[ms.MeshApproximation.OnHullIndices[i]].GlobalPosition;
                Gizmos.DrawLine(samplePos, samplePos + debugDragForces[i]);
            }
        }

        /*Debug lift forces*/
        if (DebugManager.Instance && DebugManager.Instance.DebugLift)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < debugLiftForces.Length; i++)
            {
                Vector3 samplePos = ms.MeshApproximation.Samples[ms.MeshApproximation.OnHullIndices[i]].GlobalPosition;
                Gizmos.DrawLine(samplePos, samplePos + debugLiftForces[i]);
            }
        }

    }

}
