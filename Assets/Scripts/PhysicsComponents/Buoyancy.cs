using System.Linq;
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
        int underWaterSamples = ms.MeshApproximation.IsUnderWater.Sum();   //The number of samples under water

        if (underWaterSamples > 0)
        {
            float underWaterRatio = (float)underWaterSamples / ms.MeshApproximation.SampleCount;
            float approxUnderwaterVolume = meshVolume * underWaterRatio;        //The approximated under water volume, based on the approximated sample under water ratio

            Vector3 buoyantForce = -997.0f * Physics.gravity * approxUnderwaterVolume;      //997 kg/m^3 is the density of water
            rb.AddForceAtPosition(buoyantForce, ms.MeshApproximation.AverageUnderWaterSamplePosition(), ForceMode.Force);
        }
    }


    public void DebugDraw()
    {
        /*Debug submerged samples*/
        Gizmos.color = Color.blue;
        for (int i = 0; i < ms.MeshApproximation.SampleCount; i++)
        {
            if (ms.MeshApproximation.IsUnderWater[i] == 1)
            {
                Gizmos.DrawSphere(ms.MeshApproximation.Samples[i].GlobalPosition, Gizmos.probeSize);
            }
        }
    }

}
