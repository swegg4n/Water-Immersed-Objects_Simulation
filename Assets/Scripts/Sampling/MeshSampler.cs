using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MeshSampler
{
    public MeshApproximation MeshApproximation { get; private set; }


    List<BoundingBox[]> debugBounds = new List<BoundingBox[]>(); //For Debugging


    public MeshSampler(BoundingBox[] boudingBoxes, Transform[] linkedTransforms, int[] sampleCount_distribution, float straightness)
    {
        this.MeshApproximation = new MeshApproximation(sampleCount_distribution);

        SampleMesh(boudingBoxes, sampleCount_distribution, linkedTransforms, straightness);
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


            for (int j = 0; j < sampleCount_distribution[i]; j++, c++)
            {
                Vector3 sample_pos = bounds_stratified[j % bounds_stratified.Length].RandomPoint(straightness);
                SampleCorrection(ref sample_pos, collider);

                SamplePoint sample = new SamplePoint(sample_pos - linkedTransforms[i].position, linkedTransforms[i].rotation, linkedTransforms[i]);
                MeshApproximation.Samples[c] = sample;
            }
        }
    }


    /// <summary>
    /// Corrects the sample to be placed on the surface of the mesh, if the sample exists outside of the mesh
    /// </summary>
    private void SampleCorrection(ref Vector3 sample_pos, Collider collider)
    {
        if (collider)
        {
            sample_pos = collider.ClosestPoint(sample_pos);
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
    }

}
