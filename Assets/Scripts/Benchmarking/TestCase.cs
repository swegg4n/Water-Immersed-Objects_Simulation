using UnityEngine;


public enum TypeOfTest { Performance, Correctness }


[CreateAssetMenu(fileName = "TestCase", menuName = "Benchmark/TestCase", order = 1)]
public class TestCase : ScriptableObject
{
    public int testLength = 1000;  //number of frames to test
    public TypeOfTest typeOfTest;

    public GameObject prefab;
    public Vector3 position = new Vector3(0, 0, 0);
    public int[] sampleCounts = new int[] { 10, 20, 40, 80, 160, 320, 640, 1280, 2560, 5120, 10240, 20480 };

    public float density = 700.0f;

    public float coefficientOfDrag = 1.0f;
    public float coefficientOfLift = 1.0f;

    public float amplitude = 0.0f;
    public float ordinaryFrequency = 1.5f;
    public float angluarFrequency = 1.0f;
}
