using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MeshSampler
{
    private MeshRenderer[] meshRenderers;
    public MeshApproximation MeshApproximation { get; private set; }



    public MeshSampler(MeshRenderer[] meshRenderers, Transform[] linkedTransforms, int[] sampleCount_distribution)
    {
        this.meshRenderers = meshRenderers;

        MeshApproximation = new MeshApproximation(sampleCount_distribution);

        SampleMesh(sampleCount_distribution, linkedTransforms);
    }

    private void SampleMesh(int[] sampleCount_distribution, Transform[] linkedTransforms)
    {
        int counter = 0;
        for (int i = 0; i < sampleCount_distribution.Length; i++)
        {
            MeshCollider collider = linkedTransforms[i].GetComponent<MeshCollider>();
            if (collider) collider.convex = false;

            int stratifiedDivisions = Mathf.Max((int)Mathf.Pow(sampleCount_distribution[i], 1.0f / 3) - 1, 0);

            BoundingBox bounds = new BoundingBox(meshRenderers[i].bounds.center, meshRenderers[i].bounds.size);
            BoundingBox[] bounds_stratified = GenerateStratifiedBounds(bounds, stratifiedDivisions);

            bounds_stratified = bounds_stratified.OrderBy(x => Random.value).ToArray();

            int loopCap = 1000 * sampleCount_distribution[i];
            int j = 0;
            while (j < sampleCount_distribution[i] && --loopCap > 0)
            {
                Vector3 sample_pos = bounds_stratified[j % bounds_stratified.Length].RandomPoint();

                if (ValidateSample(sample_pos))
                {
                    SamplePoint sample = new SamplePoint(sample_pos - linkedTransforms[i].position, linkedTransforms[i].rotation, linkedTransforms[i]);
                    MeshApproximation.Samples[counter++] = sample;
                    ++j;
                }
            }
            if (j < sampleCount_distribution[i])
            {
                throw new System.Exception("Failed to place all sample points");
            }

            if (collider) collider.convex = true;
        }
    }

    private bool ValidateSample(Vector3 sample_pos)
    {
        return Physics.CheckSphere(sample_pos, 0.01f);
    }


    private BoundingBox[] GenerateStratifiedBounds(BoundingBox boundingBox_original, int divisions)
    {
        List<BoundingBox> boundingBoxes = new List<BoundingBox>((int)Mathf.Pow((divisions + 1), 3.0f)); //(divisions + 1)^3  gives the max number of new bounding boxes

        Vector3 newBoundsSize = boundingBox_original.Size / (divisions + 1);
        for (int x = 0; x <= divisions; x++)
        {
            for (int y = 0; y <= divisions; y++)
            {
                for (int z = 0; z <= divisions; z++)
                {
                    Vector3 newBoundsCenter = new Vector3(x * newBoundsSize.x, y * newBoundsSize.y, z * newBoundsSize.z)
                        + (newBoundsSize / 2)
                        + boundingBox_original.MinCorner;
                    BoundingBox b = new BoundingBox(newBoundsCenter, newBoundsSize);

                    if (b.Valid()) boundingBoxes.Add(b);
                }
            }
        }

        return boundingBoxes.ToArray();
    }


    public void Update()
    {
        MeshApproximation.Update();
    }

    //public void DebugDraw()
    //{
    //    Gizmos.color = Color.white;
    //    for (int i = 0; i < MeshApproximation.SampleCount; i++)
    //    {
    //        if (MeshApproximation.IsUnderWater[i] == 0)
    //        {
    //            Gizmos.DrawSphere(MeshApproximation.Samples[i].GlobalPosition, Gizmos.probeSize);
    //        }
    //    }

    //    //Gizmos.color = Color.green;
    //    //foreach (BoundingBox[] b_arr in bounds_stratified)
    //    //{
    //    //    foreach (BoundingBox b in b_arr)
    //    //    {
    //    //        Gizmos.DrawWireCube(b.Center, b.Size);
    //    //    }
    //    //}
    //}

}
