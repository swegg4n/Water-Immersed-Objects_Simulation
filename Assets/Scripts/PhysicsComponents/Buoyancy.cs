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
        float underWaterSamples = (float)ms.MeshApproximation.IsUnderWater.Sum();

        if (underWaterSamples > 0)
        {
            float underWaterRatio = underWaterSamples / ms.MeshApproximation.SampleCount;
            float approxUnderwaterVolume = meshVolume * underWaterRatio;

            Vector3 buoyantForce = -997 * Physics.gravity * approxUnderwaterVolume; //997 kg/m^3 is the density of water
            rb.AddForceAtPosition(buoyantForce, ms.MeshApproximation.AverageUnderWaterSamplePosition());
        }
    }


    public void DebugDraw()
    {
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
