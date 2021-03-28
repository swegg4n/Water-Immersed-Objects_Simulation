using UnityEngine;

public class WaterDrag
{
    private Rigidbody rb;
    private MeshSampler ms;
    private float viscosity;

    public WaterDrag(Rigidbody rb, MeshSampler ms, float viscosity)
    {
        this.rb = rb;
        this.ms = ms;
        this.viscosity = viscosity;
    }


    public void Update()
    {
        for (int i = 0; i < ms.MeshApproximation.SampleCount; i++)
        {
            SamplePoint sp = ms.MeshApproximation.Samples[i];

            if (ms.MeshApproximation.IsUnderWater[i] == 1)
            {
                if (sp.LastPosition != null)
                {
                    Vector3 deltaVelocity = sp.GlobalPosition - (Vector3)sp.LastPosition;
                    rb.AddForceAtPosition(-deltaVelocity * viscosity / ms.MeshApproximation.SampleCount, sp.GlobalPosition, ForceMode.VelocityChange);
                }

                sp.LastPosition = sp.GlobalPosition;
            }
            else
            {
                sp.LastPosition = null;
            }
        }
    }

}
