using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
public class WaterImmersedRigidbody : MonoBehaviour
{
    [SerializeField] private int sampleCount = 100;
    [SerializeField] private float density = 997.0f;
    [SerializeField] private float viscosity = 1.0f;

    MeshSampler meshSampler;
    Gravity gravity;
    Buoyancy buoyancy;
    WaterDrag waterDrag;


    private void Awake()
    {
        Initialize();
    }


    public void Initialize()
    {
        List<Mesh> meshList = new List<Mesh>();
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

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.mass = density * totalMeshVolume;
        rb.drag = 0.0f;
        rb.angularDrag = 0.0f;

        meshSampler = new MeshSampler(meshRenderers, transforms, DistributeSamples(boundsVolumes, totalBoundsVolume));
        gravity = new Gravity(rb, meshSampler);
        buoyancy = new Buoyancy(rb, meshSampler, totalMeshVolume);
        waterDrag = new WaterDrag(rb, meshSampler, viscosity);

        rb.isKinematic = false;
    }


    public void Set(int sampleCount, float density, float viscosity)
    {
        this.sampleCount = sampleCount;
        this.density = density;
        this.viscosity = viscosity;
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


    //private void OnDrawGizmos()
    //{
    //    if (debugParticles)
    //    {
    //        try
    //        {
    //            //meshSampler.DebugDraw();
    //            gravity.DebugDraw();
    //            buoyancy.DebugDraw();
    //            waterDrag.DebugDraw();
    //        }
    //        catch (Exception) { }
    //    }
    //    if (debugBounds)
    //    {
    //        try
    //        {
    //            meshSampler.DebugDraw();
    //        }
    //        catch (Exception) { }
    //    }
    //}

}
