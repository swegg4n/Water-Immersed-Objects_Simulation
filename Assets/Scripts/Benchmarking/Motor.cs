using UnityEngine;

public class Motor : MonoBehaviour
{
    [SerializeField] private float force = 1.0f;
    [SerializeField] private bool impulse = false;
    private Rigidbody rb;


    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    bool first = true;
    private void FixedUpdate()
    {
        if (first || impulse == false)
        {
            rb.AddForceAtPosition(force * transform.forward, transform.position, ForceMode.Acceleration);
            first = false;
        }
    }
}
