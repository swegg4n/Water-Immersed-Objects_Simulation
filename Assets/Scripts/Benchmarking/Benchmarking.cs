using System.Collections;
using System.IO;
using UnityEngine;

public class Benchmarking : MonoBehaviour
{
    private string benchmarkPath;

    private const int REFERENCE_BOAT_SAMPLES = 20480;

    [SerializeField] private TestCase[] testCases;


    private bool testComplete = true;


    public static Vector3[][] referenceVertices;




    private void Awake()
    {
        benchmarkPath = Path.Combine(Application.dataPath, "TestResults");
#if !UNITY_EDITOR       //Creates the Test_Results folder for builds
        if (!Directory.Exists(benchmarkPath))   
            Directory.CreateDirectory(benchmarkPath);
#endif
    }

    private void Start()
    {
        StartCoroutine(RunAllBenchmarks());
    }



    private IEnumerator RunAllBenchmarks()
    {
        if (testCases == null || testCases.Length <= 0)
        {
            Debug.LogError("No benchmarks assigned");
            yield break;
        }

        for (int i = 0; i < testCases.Length; i++)
        {
            string testName = testCases[i].name;
            string filePath = Path.Combine(benchmarkPath, testName + ".txt");

            TestResult testResult = new TestResult(testCases[i]);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int j = 0; j < testResult.Header().Count; j++)
                {
                    writer.WriteLine(testResult.Header()[j]);
                }
            }

            if (testCases[i].typeOfTest == TypeOfTest.Correctness)
            {
                testComplete = false;
                StartCoroutine(GenerateReferenceData(testCases[i]));
            }

            for (int j = 0; j < testCases[i].sampleCounts.Length; j++)
            {
                while (testComplete == false) yield return new WaitForSecondsRealtime(1.0f);    //Spin wait

                StartCoroutine(RunBenchmark(testCases[i], testCases[i].sampleCounts[j], testResult, filePath));
                testComplete = false;
            }
            while (testComplete == false) yield return new WaitForSecondsRealtime(1.0f);    //Spin wait
        }

        Debug.Log("End of test, closing...");
        yield return new WaitForSecondsRealtime(2.0f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
    }

    private IEnumerator GenerateReferenceData(TestCase testCase)
    {
        Debug.Log($"Generating reference data (s_{REFERENCE_BOAT_SAMPLES})");

        WaveManager.instance.Set(testCase.amplitude, testCase.ordinaryFrequency, testCase.angluarFrequency);
        GameObject referenceInstance = Instantiate(testCase.prefab, testCase.position, Quaternion.identity);
        referenceInstance.GetComponent<WaterImmersedRigidbody>().Set(REFERENCE_BOAT_SAMPLES, testCase.density, testCase.viscosity);

        Mesh[] meshes = referenceInstance.GetComponent<WaterImmersedRigidbody>().meshes;
        Transform[] transforms = referenceInstance.GetComponent<WaterImmersedRigidbody>().transforms;

        referenceVertices = new Vector3[testCase.testLength][];

        int framesCounter = 0;
        while (framesCounter < testCase.testLength)
        {
            if (framesCounter % 10 == 0)
                Debug.Log($"Progress:  {(framesCounter * 100) / testCase.testLength}%");  //DEBUG progress

            Time.timeScale = 0.0f;
            referenceVertices[framesCounter] = BenchmarkHelper.MeshArrayToVerticesArray(meshes, transforms);
            Time.timeScale = 1.0f;

            yield return new WaitForSecondsRealtime(Time.deltaTime);   // Wait for next frame
            ++framesCounter;
        }

        Destroy(referenceInstance);

        testComplete = true;
    }


    private IEnumerator RunBenchmark(TestCase testCase, int sampleCount, TestResult testResult, string filePath)
    {
        /*Instantiate a new boat to test with*/
        GameObject boatInstance = Instantiate(testCase.prefab, testCase.position, Quaternion.identity);
        boatInstance.GetComponent<WaterImmersedRigidbody>().Set(sampleCount, testCase.density, testCase.viscosity);


        yield return new WaitForSeconds(Time.deltaTime);    //Delay to not have instantiation manipulate test results. (instantiate is computationally heavy)


        using (StreamWriter writer = File.AppendText(filePath))
        {
            Debug.Log($"Running benchmark: {testCase.name}_s{sampleCount}");

            int framesCounter = 0;
            while (framesCounter < testCase.testLength)
            {
                if (framesCounter % 100 == 0)
                    Debug.Log($"Progress:  {(framesCounter * 100) / testCase.testLength}%");  //DEBUG progress

                switch (testCase.typeOfTest)        //Update the test result values with data from this frame
                {
                    case TypeOfTest.Performance:
                        testResult.SaveFrame(framesCounter);
                        break;

                    case TypeOfTest.Correctness:
                        testResult.SaveFrame(framesCounter, boatInstance.GetComponent<WaterImmersedRigidbody>().meshes, boatInstance.GetComponent<WaterImmersedRigidbody>().transforms);
                        break;
                }

                yield return new WaitForSeconds(Time.deltaTime);    // Wait for next frame
                ++framesCounter;
            }

            #region Write/Log results
            Debug.Log($"Benchmark \"{testCase.name}_s{sampleCount}\" - Completed");

            writer.WriteLine(testResult.Data(sampleCount));
            Debug.Log(testResult.Data(sampleCount));
            #endregion
        }

        Destroy(boatInstance);

        testComplete = true;
    }

};
