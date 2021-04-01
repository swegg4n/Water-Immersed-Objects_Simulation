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

    /// <summary>
    /// Gets a random point within the bounds.
    /// Straightness ([0,1]) controls how close to the center of bounds the sample must be.
    /// Straightness = 0  means the sample can be anywhere within the bounds.
    /// Staightness = 1  means the sample must be placed in the center of the bounds
    public Vector3 RandomPoint(float straightness = 0.0f)
    {
        return new Vector3(Random.Range(Size.x / 2 * straightness, Size.x - Size.x / 2 * straightness),
                           Random.Range(Size.y / 2 * straightness, Size.y - Size.y / 2 * straightness),
                           Random.Range(Size.z / 2 * straightness, Size.z - Size.z / 2 * straightness)) + MinCorner;
    }

}