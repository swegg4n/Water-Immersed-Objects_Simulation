using UnityEngine;

public class RotationLock : MonoBehaviour
{
    [SerializeField] private bool lock_pitch, lock_yaw, lock_roll;

    Vector3 rot_orig;


    private void Awake()
    {
        rot_orig = transform.rotation.eulerAngles;
    }

    private void LateUpdate()
    {
        Vector3 newRot = transform.rotation.eulerAngles;

        if (lock_pitch) newRot.x = rot_orig.x;
        if (lock_yaw) newRot.y = rot_orig.y;
        if (lock_roll) newRot.z = rot_orig.z;

        transform.rotation = Quaternion.Euler(newRot);
    }
}
