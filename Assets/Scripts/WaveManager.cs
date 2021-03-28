using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    [SerializeField] private float amplitude = 0.0f;
    [Range(0.025f, 100.0f)] [SerializeField] private float ordinaryFrequency = 1.5f;
    [SerializeField] private float angluarFrequency = 1.0f;
    private float phase = 0.0f;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void Set(float amplitude, float ordinaryFrequency, float angluarFrequency)
    {
        this.amplitude = amplitude;
        this.ordinaryFrequency = ordinaryFrequency;
        this.angluarFrequency = angluarFrequency;
        this.phase = 0.0f;
    }


    private void Update()
    {
        phase += angluarFrequency * Time.deltaTime;
    }


    public float GetWaveHeight(Vector3 point)
    {
        return amplitude * Mathf.Sin(point.x / ordinaryFrequency + phase);
    }

}
