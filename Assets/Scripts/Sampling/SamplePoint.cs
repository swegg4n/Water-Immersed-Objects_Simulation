using UnityEngine;

public class SamplePoint
{
    public SamplePoint(Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Transform linkedTransform)
    {
        this.localPosition = localPosition;
        this.localRotation = localRotation;
        this.localScale = localScale;

        this.linkedTransform = linkedTransform;
        this.Normal = Vector3.zero;
    }

    private Vector3 localPosition;
    private Quaternion localRotation;
    private Vector3 localScale;

    public Vector3 GlobalPosition { get; private set; }
    public Vector3? LastPosition { get; set; }

    private Transform linkedTransform;



    public Vector3 Normal { get; set; }

    private Quaternion lastRotation;


    /// <summary>
    /// Transforms the local positon of the sample to a global position, based on translation and rotation
    /// </summary>
    public void SetPosition()
    {
        Matrix4x4 m = Matrix4x4.Rotate(linkedTransform.rotation * Quaternion.Inverse(localRotation));
        Vector3 rotatedOffset = m.MultiplyPoint3x4(localPosition);
        this.GlobalPosition = linkedTransform.position + rotatedOffset;
    }

    public void UpdateNormals()
    {
        Matrix4x4 m = Matrix4x4.Rotate(linkedTransform.rotation * Quaternion.Inverse(lastRotation));
        Normal = m.MultiplyPoint3x4(Normal).normalized;

        lastRotation = linkedTransform.rotation;
    }

}