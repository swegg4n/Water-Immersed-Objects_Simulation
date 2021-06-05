using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MeshSampler
{
    public MeshApproximation MeshApproximation { get; private set; }

    List<BoundingBox[]> debugBounds = new List<BoundingBox[]>(); //For Debugging


    public MeshSampler(BoundingBox[] boudingBoxes, Transform[] linkedTransforms, int[] sampleCount_distribution, float straightness, Mesh[] meshes)
    {
        this.MeshApproximation = new MeshApproximation(sampleCount_distribution);

        SampleMesh(boudingBoxes, sampleCount_distribution, linkedTransforms, straightness);

        MeshApproximation.UpdateSamplesPosition();

        CalculateOnSurfaceParticles(meshes, linkedTransforms);
    }


    /// <summary>
    /// Stratified sampling of a chosen number of particles inside subdivided bounds
    /// </summary>
    private void SampleMesh(BoundingBox[] boudingBoxes, int[] sampleCount_distribution, Transform[] linkedTransforms, float straightness)
    {
        int c = 0;
        for (int i = 0; i < sampleCount_distribution.Length; i++)
        {
            MeshCollider collider = linkedTransforms[i].GetComponent<MeshCollider>();
            if (collider) collider.convex = true;

            int stratifiedDivisions = Mathf.Max((int)Mathf.Pow(sampleCount_distribution[i], 1.0f / 3) - 1, 0);

            BoundingBox bounds = boudingBoxes[i];
            BoundingBox[] bounds_stratified = GenerateStratifiedBounds(bounds, stratifiedDivisions);
            bounds_stratified = bounds_stratified.OrderBy(x => Random.value).ToArray();

            debugBounds.Add(bounds_stratified);

            int sampledPoints = 0;
            int loops = -1;

            while (sampledPoints < sampleCount_distribution[i])
            {
                loops++;

                Vector3 sample_pos = bounds_stratified[loops % bounds_stratified.Length].RandomPoint(straightness);

                Vector3 correctedPos = collider.ClosestPoint(sample_pos);
                if (Vector3.SqrMagnitude(sample_pos - correctedPos) > 0.001f)   //if the sample is not within the mesh -> try again
                    continue;

                sampledPoints++;

                SamplePoint sample = new SamplePoint(sample_pos - linkedTransforms[i].position, linkedTransforms[i].rotation, linkedTransforms[i]);
                MeshApproximation.Samples[c++] = sample;
            }
        }
    }


    private void CalculateOnSurfaceParticles(Mesh[] meshes, Transform[] linkedTransforms)
    {
        Vector3[] vertices = BenchmarkHelper.MeshArrayToVerticesArray(meshes, linkedTransforms);

        (float, int)[] distanceToClosestVertex = new (float, int)[MeshApproximation.SampleCount];

        Vector3 centerOfMass = MeshApproximation.AverageSamplePosition();
        for (int i = 0; i < MeshApproximation.SampleCount; i++)
        {
            vertices = vertices.OrderBy(x => Vector3.Distance(x, MeshApproximation.Samples[i].GlobalPosition)).ToArray();

            Vector3 rayDir = centerOfMass - MeshApproximation.Samples[i].GlobalPosition;
            Vector3 rayOrigin = MeshApproximation.Samples[i].GlobalPosition - rayDir;
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDir, out hit))
                MeshApproximation.Samples[i].Normal = hit.normal;

            distanceToClosestVertex[i] = (Vector3.Distance(vertices[0], MeshApproximation.Samples[i].GlobalPosition), i);
        }

        MeshApproximation.OnSurfaceIndices = new int[distanceToClosestVertex.Length / 2];

        (float, int)[] ordered = (from s in distanceToClosestVertex
                                  orderby s.Item1
                                  select s).ToArray();

        for (int i = 0; i < MeshApproximation.OnSurfaceIndices.Length; i++)
        {
            MeshApproximation.OnSurfaceIndices[i] = ordered[i].Item2;
        }
    }


    /// <summary>
    /// Divides a bounding box into several sub-bounding boxes based on the number of divisions
    /// </summary>
    private BoundingBox[] GenerateStratifiedBounds(BoundingBox boundingBox_original, int divisions)
    {
        BoundingBox[] boundingBoxes = new BoundingBox[(int)Mathf.Pow((divisions + 1), 3.0f)];

        Vector3 newBoundsSize = boundingBox_original.Size / (divisions + 1);
        int c = 0;
        for (int x = 0; x <= divisions; x++)
        {
            for (int y = 0; y <= divisions; y++)
            {
                for (int z = 0; z <= divisions; z++, c++)
                {
                    Vector3 newBoundsCenter = new Vector3(x * newBoundsSize.x, y * newBoundsSize.y, z * newBoundsSize.z)
                        + (newBoundsSize / 2)
                        + boundingBox_original.MinCorner;

                    boundingBoxes[c] = new BoundingBox(newBoundsCenter, newBoundsSize);
                }
            }
        }

        return boundingBoxes.ToArray();
    }


    public void Update()
    {
        MeshApproximation.Update();
    }

    public void DebugDraw()
    {
        /*Debug non-submerged samples*/
        if (DebugManager.Instance && DebugManager.Instance.DebugAirSamples)
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < MeshApproximation.SampleCount; i++)
            {
                if (MeshApproximation.IsUnderWater[i] == 0)
                {
                    Gizmos.DrawSphere(MeshApproximation.Samples[i].GlobalPosition, Gizmos.probeSize);
                }
            }
        }

        /*Debug sub-bounding boxes (Note that these bounding boxes are NOT updated with the object's translation, for performace)*/
        if (DebugManager.Instance && DebugManager.Instance.DebugBounds)
        {
            Gizmos.color = Color.green;
            foreach (BoundingBox[] b_arr in debugBounds)
            {
                foreach (BoundingBox b in b_arr)
                {
                    Gizmos.DrawWireCube(b.Center, b.Size);
                }
            }
        }


        /*Debug if the particle is on the hull or not*/
        if (DebugManager.Instance && DebugManager.Instance.DebugOnHull)
        {
            for (int i = 0; i < MeshApproximation.SampleCount; i++)
            {
                Gizmos.color = Color.gray;

                if (MeshApproximation.OnSurfaceIndices.Contains(i))
                {
                    Gizmos.color = Color.green;
                }

                Gizmos.DrawSphere(MeshApproximation.Samples[i].GlobalPosition, Gizmos.probeSize);
            }
        }


        /*Debug sample normals*/
        if (DebugManager.Instance && DebugManager.Instance.DebugSampleNormals)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < MeshApproximation.OnSurfaceIndices.Length; i++)
            {
                Vector3 samplePos = MeshApproximation.Samples[MeshApproximation.OnSurfaceIndices[i]].GlobalPosition;
                Gizmos.DrawLine(samplePos, samplePos + MeshApproximation.Samples[MeshApproximation.OnSurfaceIndices[i]].Normal * Gizmos.probeSize * 10);
            }
        }

    }

}
