using UnityEngine;

public class Buoyancy
{
    private Rigidbody rb;
    private MeshSampler ms;
    private float meshVolume;



    public Buoyancy(Rigidbody rb, MeshSampler ms, float meshVolume)
    {
        this.rb = rb;
        this.ms = ms;
        this.meshVolume = meshVolume;
    }


    public void Update()
    {
        int underWaterSampleCount = 0;  //The number of samples under water
        for (int i = 0; i < ms.MeshApproximation.IsUnderWater.Length; i++)
            underWaterSampleCount += ms.MeshApproximation.IsUnderWater[i];

        if (underWaterSampleCount != 0)
        {
            float underWaterRatio = (float)underWaterSampleCount / ms.MeshApproximation.SampleCount;
            float approxUnderwaterVolume = meshVolume * underWaterRatio;        //The approximated under water volume, based on the approximated sample under water ratio

            Vector3 buoyantForce = -WaterImmersedRigidbody.FluidDensity * Physics.gravity * approxUnderwaterVolume;      //997 kg/m^3 is the density of water
            rb.AddForceAtPosition(buoyantForce, ms.MeshApproximation.AverageUnderWaterSamplePosition(), ForceMode.Force);
        }
    }


    public void DebugDraw()
    {
        /*Debug submerged samples*/
        if (DebugManager.Instance && DebugManager.Instance.DebugUnderWaterSamples)
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < ms.MeshApproximation.SampleCount; i++)
            {
                if (ms.MeshApproximation.IsUnderWater[i] == 1)
                {
                    Gizmos.DrawSphere(ms.MeshApproximation.Samples[i].GlobalPosition, Gizmos.probeSize);
                }
            }
        }
    }

}
