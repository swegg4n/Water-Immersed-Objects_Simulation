using UnityEngine;

public class MouseDrag : MonoBehaviour
{
    private Camera cam;

    Vector3 startDragPos;
    WaterImmersedRigidbody draggedBoat;
    [SerializeField] private LayerMask mask;


    private void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                if (hit.transform.GetComponentInParent<WaterImmersedRigidbody>())
                {
                    startDragPos = Input.mousePosition;
                    draggedBoat = hit.transform.GetComponentInParent<WaterImmersedRigidbody>();
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (draggedBoat != null)
            {
                Vector3 dragVector = Input.mousePosition - startDragPos;
                draggedBoat.GetComponent<Rigidbody>().AddForce(dragVector / 500.0f, ForceMode.VelocityChange);

                startDragPos = Vector3.zero;
                draggedBoat = null;
            }
        }
    }

}
