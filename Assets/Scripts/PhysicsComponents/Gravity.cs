using UnityEngine;


public class Gravity
{
    private Rigidbody rb;
    private MeshSampler ms;


    public Gravity(Rigidbody rb, MeshSampler ms)
    {
        this.rb = rb;
        this.ms = ms;
    }


    public void Update()
    {
        rb.AddForceAtPosition(Physics.gravity, ms.MeshApproximation.AverageSamplePosition(), ForceMode.Acceleration);
    }

}
