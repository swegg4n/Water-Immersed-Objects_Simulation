using UnityEngine;

public class BoundingBox
{
    public Vector3 Center { get; private set; }
    public Vector3 Size { get; private set; }

    public Vector3 MinCorner { get { return Center - Size / 2; } }


    public BoundingBox(Vector3 center, Vector3 size)
    {
        this.Center = center;
        this.Size = size;
    }

    public Vector3 RandomPoint()
    {
        return new Vector3(Random.Range(0, Size.x), Random.Range(0, Size.y), Random.Range(0, Size.z)) + MinCorner;
    }

    public bool Valid()
    {
        float shortestSide = Mathf.Min(Size.x, Size.y, Size.z);
        return Physics.CheckSphere(Center, shortestSide / 2);
    }
}