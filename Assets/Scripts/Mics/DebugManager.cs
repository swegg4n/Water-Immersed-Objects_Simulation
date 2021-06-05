using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }


    [SerializeField] private bool debugAirSamples = false;
    [SerializeField] private bool debugUnderWaterSamples = false;
    [SerializeField] private bool debugDrag = false;
    [SerializeField] private bool debugLift = false;
    [SerializeField] private bool debugOnHull = false;

    [SerializeField] private bool debugSampleNormals = false;
    [SerializeField] private bool debugBounds = false;


    public bool DebugAirSamples { get { return debugAirSamples; } }
    public bool DebugUnderWaterSamples { get { return debugUnderWaterSamples; } }
    public bool DebugDrag { get { return debugDrag; } }
    public bool DebugLift { get { return debugLift; } }
    public bool DebugOnHull { get { return debugOnHull; } }

    public bool DebugSampleNormals { get { return debugSampleNormals; } }
    public bool DebugBounds { get { return debugBounds; } }
}
