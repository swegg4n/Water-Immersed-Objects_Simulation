using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterImmersedRigidbody : MonoBehaviour
{
    public static float FluidDensity = 997.0f; //1.225f; //1450.0f; //997.0f;

    [SerializeField] private int sampleCount = 100;
    [SerializeField] private float density = 700.0f;
    [SerializeField] private float dragCoefficient = 1.0f;
    [SerializeField] private float liftCoefficient = 1.0f;
    [Range(0f, 1.0f)] [SerializeField] private float straightness = 0f;

    MeshSampler meshSampler;
    Gravity gravity;
    Buoyancy buoyancy;
    DragLift waterDrag;


    [HideInInspector] public Mesh[] meshes;
    [HideInInspector] public Transform[] transforms;


    private void Awake()
    {
        Initialize();
    }


    /// <summary>
    /// Performs all initializaton and pre-computation of the object's components
    /// </summary>
    public void Initialize()
    {
        Quaternion rotation = transform.rotation;
        //Vector3 scale = transform.localScale;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        //transform.localScale = Vector3.one;

        List<Mesh> meshList = new List<Mesh>();
        List<Transform> transformList = new List<Transform>();

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.GetComponent<Collider>() != null)
            {
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
                new Vector3(meshes[i].bounds.size.x * transform.localScale.x * transforms[i].localScale.x,
                            meshes[i].bounds.size.y * transform.localScale.y * transforms[i].localScale.y,
                            meshes[i].bounds.size.z * transform.localScale.z * transforms[i].localScale.z)
                , transforms[i]);
        }

        float totalMeshVolume = meshVolumes.Sum();
        float totalSurfaceArea = surfaceAreas.Sum();

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.mass = density * totalMeshVolume;
        rb.drag = 0.0f;
        rb.angularDrag = 0.0f;

        meshSampler = new MeshSampler(boudingBoxes, transforms, DistributeSamples(meshVolumes, totalMeshVolume, surfaceAreas, totalSurfaceArea), straightness, meshes);
        gravity = new Gravity(rb, meshSampler);
        buoyancy = new Buoyancy(rb, meshSampler, totalMeshVolume);
        waterDrag = new DragLift(rb, meshSampler, dragCoefficient, liftCoefficient, meshes, transforms, transform, totalSurfaceArea);


        transform.rotation = rotation;
        //transform.localScale = scale;

        rb.isKinematic = false;
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
