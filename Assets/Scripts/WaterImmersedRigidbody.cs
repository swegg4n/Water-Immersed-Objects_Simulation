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
    [SerializeField] private float straightness = 0.0f;

    MeshSampler meshSampler;
    Gravity gravity;
    Buoyancy buoyancy;
    Drag waterDrag;


    [HideInInspector] public List<Mesh> meshList;


    private void Awake()
    {
        Initialize();
    }


    public void Initialize()
    {
        meshList = new List<Mesh>();
        List<MeshRenderer> meshRendererList = new List<MeshRenderer>();
        List<Transform> transformList = new List<Transform>();

        if (GetComponent<Collider>() != null)
        {
            meshList.Add(GetComponent<MeshFilter>().sharedMesh);
            meshRendererList.Add(GetComponent<MeshRenderer>());
            transformList.Add(transform);
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<Collider>() != null)
            {
                Transform child = transform.GetChild(i);

                meshList.Add(child.GetComponent<MeshFilter>().sharedMesh);
                meshRendererList.Add(child.GetComponent<MeshRenderer>());
                transformList.Add(child);
            }
        }

        MeshSurfaceArea.SurfaceAreaOfMesh(meshList[0], transformList[0]);

        Mesh[] meshes = meshList.ToArray();
        MeshRenderer[] meshRenderers = meshRendererList.ToArray();
        Transform[] transforms = transformList.ToArray();

        float[] boundsVolumes = new float[meshes.Length];
        for (int i = 0; i < boundsVolumes.Length; i++)
            boundsVolumes[i] = meshRenderers[i].bounds.size.x * meshRenderers[i].bounds.size.y * meshRenderers[i].bounds.size.z;
        float totalBoundsVolume = boundsVolumes.Sum();

        float[] meshVolumes = new float[meshes.Length];
        for (int i = 0; i < meshVolumes.Length; i++)
            meshVolumes[i] = MeshVolume.VolumeOfMesh(meshes[i], transforms[i]);
        float totalMeshVolume = meshVolumes.Sum();

        float totalSurfaceArea = MeshSurfaceArea.SurfaceAreaOfMesh(meshes, transforms);


        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.mass = density * totalMeshVolume;
        rb.drag = 0.0f;
        rb.angularDrag = 0.0f;

        meshSampler = new MeshSampler(meshRenderers, transforms, DistributeSamples(boundsVolumes, totalBoundsVolume), straightness);
        gravity = new Gravity(rb, meshSampler);
        buoyancy = new Buoyancy(rb, meshSampler, totalMeshVolume);
        waterDrag = new Drag(rb, meshSampler, dragCoefficient, meshes, transform, totalSurfaceArea);

        rb.isKinematic = false;
    }


    public void Set(int sampleCount, float density, float viscosity)
    {
        this.sampleCount = sampleCount;
        this.density = density;
        this.dragCoefficient = viscosity;

        Initialize();
    }


    private int[] DistributeSamples(float[] boundsVolumes, float totalBoundsVolume)
    {
        int[] distribution = new int[boundsVolumes.Length];
        int totalDistributions = 0;

        for (int i = 0; i < distribution.Length; i++)
        {
            int d = (int)(boundsVolumes[i] / totalBoundsVolume * sampleCount);
            distribution[i] = d;
            totalDistributions += d;
        }
        for (int i = 0; i < sampleCount - totalDistributions; i++)
        {
            ++distribution[i % distribution.Length];
        }

        return distribution;
    }


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
