using System.Linq;
using UnityEngine;

public class MeshApproximation
{
    public SamplePoint[] Samples { get; private set; }
    public int[] IsUnderWater { get; private set; }

    public int SampleCount { get; set; }


    public MeshApproximation(int[] sampleCounts)
    {
        this.SampleCount = sampleCounts.Sum();

        this.Samples = new SamplePoint[SampleCount];
        this.IsUnderWater = new int[SampleCount];
    }

    public void Update()
    {
        UpdateSamplesPosition();
        UpdateUnderWaterSamples();
    }

    private void UpdateSamplesPosition()
    {
        foreach (SamplePoint sp in Samples)
        {
            sp.SetPosition();
        }
    }
    private void UpdateUnderWaterSamples()
    {
        for (int i = 0; i < SampleCount; i++)
        {
            IsUnderWater[i] = (Samples[i].GlobalPosition.y <= WaveManager.instance.GetWaveHeight(Samples[i].GlobalPosition)) ? 1 : 0;
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
