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

        //ms.MeshApproximation.UpdateSamplesPosition();

        debugDragForces = new Vector3[ms.MeshApproximation.OnSurfaceIndices.Length];
        debugLiftForces = new Vector3[ms.MeshApproximation.OnSurfaceIndices.Length];
    }



    public void Update()
    {
        for (int i = 0; i < ms.MeshApproximation.OnSurfaceIndices.Length; i++)
        {
            SamplePoint sp = ms.MeshApproximation.Samples[ms.MeshApproximation.OnSurfaceIndices[i]];

            if (sp.LastPosition != null)
            {
                Vector3 deltaDistance = (sp.GlobalPosition - (Vector3)sp.LastPosition);
                Vector3 deltaVelocity = deltaDistance / Time.deltaTime;

                float velocitySquared = Vector3.SqrMagnitude(deltaVelocity);

                //if (i == 0)
                //{
                //    Debug.Log($"Velocity:  {deltaVelocity.z}");
                //}

                float density = (ms.MeshApproximation.IsUnderWater[ms.MeshApproximation.OnSurfaceIndices[i]] == 1) ? WaterImmersedRigidbody.FluidDensity : 1.225f;      // Water drag  vs  air drag 

                Vector3 dragDirection = -deltaDistance.normalized;
                Vector3 T = (Vector3.Cross(deltaVelocity, -sp.Normal));
                Vector3 liftDirection = Vector3.Cross(T, deltaVelocity).normalized;


                float areaFraction = totalSurfaceArea / ms.MeshApproximation.OnSurfaceIndices.Length;

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
                Vector3 samplePos = ms.MeshApproximation.Samples[ms.MeshApproximation.OnSurfaceIndices[i]].GlobalPosition;
                Gizmos.DrawLine(samplePos, samplePos + debugDragForces[i]);
            }
        }

        /*Debug lift forces*/
        if (DebugManager.Instance && DebugManager.Instance.DebugLift)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < debugLiftForces.Length; i++)
            {
                Vector3 samplePos = ms.MeshApproximation.Samples[ms.MeshApproximation.OnSurfaceIndices[i]].GlobalPosition;
                Gizmos.DrawLine(samplePos, samplePos + debugLiftForces[i]);
            }
        }

    }

}
