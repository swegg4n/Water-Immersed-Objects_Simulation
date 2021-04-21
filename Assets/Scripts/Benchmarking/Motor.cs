using UnityEngine;

public class Motor : MonoBehaviour
{
    [SerializeField] private float speed = 1.0f;
    private Rigidbody rb;


    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.AddForceAtPosition(speed * transform.forward, transform.position, ForceMode.Acceleration);
    }
}
