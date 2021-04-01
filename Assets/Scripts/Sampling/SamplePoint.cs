using UnityEngine;

public class SamplePoint
{
    public SamplePoint(Vector3 localPosition, Quaternion localRotation, Transform linkedTransform)
    {
        this.localPosition = localPosition;
        this.localRotation = localRotation;
        this.linkedTransform = linkedTransform;
    }

    private Vector3 localPosition;
    private Quaternion localRotation;

    public Vector3 GlobalPosition { get; private set; }
    public Vector3? LastPosition { get; set; }

    private Transform linkedTransform;


    /// <summary>
    /// Transforms the local positon of the sample to a global position, based on translation and rotation
    /// </summary>
    public void SetPosition()
    {
        Matrix4x4 m = Matrix4x4.Rotate(linkedTransform.rotation * Quaternion.Inverse(localRotation));
        Vector3 rotatedOffset = m.MultiplyPoint3x4(localPosition);
        this.GlobalPosition = linkedTransform.position + rotatedOffset;
    }
}