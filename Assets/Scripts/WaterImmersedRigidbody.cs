using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterImmersedRigidbody : MonoBehaviour
{
    [SerializeField] private int sampleCount = 100;
    [SerializeField] private float density = 700.0f;
    [SerializeField] private float dragCoefficient = 1.0f;
    [SerializeField] private float liftCoefficient = 1.0f;
    [SerializeField] private float straightness = 0.0f;

    MeshSampler meshSampler;
    Gravity gravity;
    Buoyancy buoyancy;
    DragLift waterDrag;


    [HideInInspector] public Mesh[] meshes;
    [HideInInspector] public Transform[] transforms;


    private void Awake()
    {
        Initialize();   //OBS! THIS SHOULD NOT BE CALLED WHEN BENCHMARKING
    }


    /// <summary>
    /// Performs all initializaton and pre-computation of the object's components
    /// </summary>
    public void Initialize()
    {
        List<Mesh> meshList = new List<Mesh>();
        List<Transform> transformList = new List<Transform>();

        if (GetComponent<Collider>() != null)
        {
            meshList.Add(GetComponent<MeshFilter>().sharedMesh);
            transformList.Add(transform);
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<Collider>() != null)
            {
                Transform child = transform.GetChild(i);

                meshList.Add(child.GetComponent<MeshFilter>().sharedMesh);
                transformList.Add(child);
            }
        }

        meshes = meshList.ToArray();
        transforms = transformList.ToArray();

        float[] meshVolumes = new float[meshes.Length];
        float[] surfaceAreas = new float[meshes.Length];
        BoundingBox[] boudingBoxes = new BoundingBox[meshes.Length];

        for (int i = 0; i < meshes.Length; i++)
        {
            meshVolumes[i] = MeshVolume.VolumeOfMesh(meshes[i], transforms[i]);
            surfaceAreas[i] = MeshSurfaceArea.SurfaceAreaOfMesh(meshes[i], transforms[i]);
            boudingBoxes[i] = new BoundingBox(meshes[i].bounds.center, 
                new Vector3(meshes[i].bounds.size.x * transform.localScale.x,
                            meshes[i].bounds.size.y * transform.localScale.y, 
                            meshes[i].bounds.size.z * transform.localScale.z) 
                * 1.5f, transforms[i]);
        }

        float totalMeshVolume = meshVolumes.Sum();  //MeshVolume.VolumeOfMesh(meshes, transforms);
        float totalSurfaceArea = surfaceAreas.Sum();    //MeshSurfaceArea.SurfaceAreaOfMesh(meshes, transforms);

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.mass = density * totalMeshVolume;
        rb.drag = 0.0f;
        rb.angularDrag = 0.0f;

        meshSampler = new MeshSampler(boudingBoxes, transforms, DistributeSamples(meshVolumes, totalMeshVolume, surfaceAreas, totalSurfaceArea), straightness);
        gravity = new Gravity(rb, meshSampler);
        buoyancy = new Buoyancy(rb, meshSampler, totalMeshVolume);
        waterDrag = new DragLift(rb, meshSampler, dragCoefficient, liftCoefficient, meshes, transforms, transform, totalSurfaceArea);

        rb.isKinematic = false;
    }


    /// <summary>
    /// Sets the properties of this object and initializes it. (Used in benchmarking)
    /// </summary>
    public void Set(int sampleCount, float density, float viscosity)
    {
        this.sampleCount = sampleCount;
        this.density = density;
        this.dragCoefficient = viscosity;

        Initialize();
    }

    /// <summary>
    /// Distributes the particles among the meshes, based on the meshes' bounds volume
    /// </summary>
    private int[] DistributeSamples(float[] meshVolumes, float totalMeshVolume, float[] surfaceAreas, float totalSurfaceArea)
    {
        int[] distribution = new int[meshVolumes.Length];
        int totalDistributions = 0;

        float total = totalMeshVolume * totalSurfaceArea;

        for (int i = 0; i < distribution.Length; i++)
        {
            int d = (int)((meshVolumes[i] * surfaceAreas[i]) / total * sampleCount);
            distribution[i] = d;
            totalDistributions += d;
        }
        for (int i = 0; i < sampleCount - totalDistributions; i++)
        {
            ++distribution[i % distribution.Length];
        }

        return distribution;
    }


    /// <summary>
    /// Updates all components
    /// </summary>
    private void FixedUpdate()
    {
        meshSampler.Update();
        gravity.Update();
        buoyancy.Update();
        waterDrag.Update();
    }


    private void OnDrawGizmos()
    {
        try
        {
            buoyancy.DebugDraw();
            meshSampler.DebugDraw();
            waterDrag.DebugDraw();
        }
        catch (Exception) { }

    }

}
