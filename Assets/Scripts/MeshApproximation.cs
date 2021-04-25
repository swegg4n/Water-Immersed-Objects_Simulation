using System.Linq;
using UnityEngine;

public class MeshApproximation
{
    public SamplePoint[] Samples { get; private set; }
    public byte[] IsUnderWater { get; private set; }
    public int[] OnHullIndices { get; set; }

    public int SampleCount { get; private set; }



    public MeshApproximation(int[] sampleCounts)
    {
        this.SampleCount = sampleCounts.Sum();

        this.Samples = new SamplePoint[SampleCount];
        this.IsUnderWater = new byte[SampleCount];
    }

    public void Update()
    {
        UpdateSamplesPosition();
        UpdateNormals();
        UpdateUnderWaterSamples();
    }

    public void UpdateSamplesPosition()
    {
        foreach (SamplePoint sp in Samples)
        {
            sp.SetPosition();
        }
    }
    public void UpdateNormals()
    {
        foreach (SamplePoint sp in Samples)
        {
            sp.UpdateNormals();
        }
    }

    private void UpdateUnderWaterSamples()
    {
        try
        {
            for (int i = 0; i < SampleCount; i++)
            {
                IsUnderWater[i] = (Samples[i].GlobalPosition.y <= WaveManager.instance.GetWaveHeight(Samples[i].GlobalPosition)) ? (byte)1 : (byte)0;
            }
        }
        catch (System.Exception)
        {
        }
    }

    public Vector3 AverageSamplePosition()
    {
        Vector3 avg = Vector3.zero;

        foreach (SamplePoint sp in Samples)
        {
            avg += sp.GlobalPosition;
        }

        return avg / SampleCount;
    }
    public Vector3 AverageUnderWaterSamplePosition()
    {
        Vector3 avg = Vector3.zero;

        float underWaterSampleCount = 0;
        for (int i = 0; i < SampleCount; i++)
        {
            if (IsUnderWater[i] == 1)
            {
                avg += Samples[i].GlobalPosition;
                ++underWaterSampleCount;
            }
        }

        return avg / underWaterSampleCount;
    }

}
