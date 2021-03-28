using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class Benchmarking : MonoBehaviour
{
    private string benchmarkPath;

    private const int REFERENCE_BOAT_SAMPLES = 20000;

    [SerializeField] private TestCase[] testCases;


    private bool testComplete = false;


    public static Vector3[][] referenceVertices;




    private void Awake()
    {
        benchmarkPath = Application.dataPath + "/TestResults/";
#if !UNITY_EDITOR       //Creates the Test_Results folder for builds
        if (!Directory.Exists(benchmarkPath))   
            Directory.CreateDirectory(benchmarkPath);
#endif
    }

    private void Start()
    {
        StartCoroutine(RunAllBenchmarks());
    }


    //public void Run()
    //{
    //    StartCoroutine(RunAllBenchmarks());
    //}


    //    private IEnumerator RunAllBenchmarks()
    //    {
    //        for (int i = 0; i < testCases.Length; i++)
    //        {
    //            string testName = testCases[i].name;
    //#if UNITY_EDITOR
    //            testName += "-EDITOR"; //Prefixes the file name so we can differentiate between the tests
    //#else
    //        testName += "-BUILD";
    //#endif
    //            switch (testCases[i].typeOfTest)
    //            {
    //                case TypeOfTest.Performance:
    //                    testResult = new PerformanceTestResult(testCases[i], testCases[i].sampleCounts.Length);
    //                    break;

    //                case TypeOfTest.Correctness:
    //                    testResult = new CorrectnessTestResult(testCases[i], testCases[i].sampleCounts.Length);
    //                    break;
    //            }

    //            string filePath = benchmarkPath + testName + ".txt";
    //            using (StreamWriter writer = new StreamWriter(filePath))
    //            {
    //                for (int j = 0; j < testResult.Header().Count; j++)
    //                {
    //                    writer.WriteLine(testResult.Header()[j]);
    //                }
    //            }


    //            for (int j = 0; j < testCases[i].sampleCounts.Length; j++)
    //            {
    //                testComplete = false;
    //                StartCoroutine(RunBenchmark(testCases[i], testCases[i].sampleCounts[j], filePath));

    //                while (testComplete == false) { yield return new WaitForSeconds(1.0f); }  //spin wait
    //            }
    //        }

    //        Debug.Log("End of test, closing...");
    //        yield return new WaitForSeconds(2.0f);
    //#if UNITY_EDITOR
    //        UnityEditor.EditorApplication.isPlaying = false;
    //#else
    //        Application.Quit();
    //#endif
    //    }



    //    private IEnumerator RunBenchmark(TestCase testCase, int samples, string filePath)
    //    {

    //        /*Set the wave manager settings according to the test case*/
    //        this.waterInstance.GetComponent<WaveManager>().Set(testCase.amplitude, testCase.ordinaryFrequency, testCase.angluarFrequency);


    //        /*Instantiate a new boat to test with*/
    //        boatInstance = Instantiate(testCase.prefab, testCase.position, Quaternion.identity);
    //        boatInstance.GetComponent<WaterImmersedRigidbody>().Set(samples/*, testCase.stratifiedDivisions*/, testCase.density, testCase.viscosity);

    //        if (testCase.typeOfTest == TypeOfTest.Correctness)  //If we aim to test correctness => instantiate one more boat with high sample count, to test against.
    //        {
    //            referenceBoatInstance = Instantiate(testCase.prefab, testCase.position, Quaternion.identity);
    //            referenceBoatInstance.GetComponent<WaterImmersedRigidbody>().Set(REFERENCE_BOAT_SAMPLES/*, testCase.stratifiedDivisions*/, testCase.density, testCase.viscosity);

    //            referenceBoatInstance.layer = 6;     //Set layer to "Reference", non-colliding layer
    //            for (int i = 0; i < referenceBoatInstance.transform.childCount; i++)
    //                referenceBoatInstance.transform.GetChild(i).gameObject.layer = 6;
    //        }


    //        yield return new WaitForSeconds(Time.deltaTime);    //Delay to not have instantiation manipulate test results. (instantiate is computationally heavy)


    //        using (StreamWriter writer = File.AppendText(filePath))
    //        {
    //            Debug.Log($"Running benchmark: {testCase.name}_s{samples}");

    //            int framesCounter = 0;
    //            while (framesCounter < testCase.testLength)
    //            {
    //                if (framesCounter % 10 == 0)
    //                    Debug.Log($"Progress:  {(framesCounter * 100) / testCase.testLength}%");  //DEBUG progress

    //                testResult.SaveFrame(framesCounter);    //Update the test result values with data from this frame

    //                yield return new WaitForSeconds(Time.deltaTime);    // Wait for next frame
    //                ++framesCounter;
    //            }

    //            #region Write/Log results
    //            Debug.Log($"Benchmark \"{testCase.name}_s{samples}\" - Completed");

    //            writer.WriteLine(testResult.Data());
    //            Debug.Log(testResult.Data());
    //            #endregion
    //        }

    //        Destroy(boatInstance);
    //        Destroy(referenceBoatInstance);

    //        testComplete = true;
    //    }



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
            string filePath = benchmarkPath + testName + ".txt";

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
                StartCoroutine(GenerateReferenceData(testCases[i]));
            }
            testComplete = true;

            for (int j = 0; j < testCases[i].sampleCounts.Length; j++)
            {
                while (testComplete == false) yield return new WaitForSecondsRealtime(1.0f);    //Spin wait

                StartCoroutine(RunBenchmark(testCases[i], testCases[i].sampleCounts[j], testResult, benchmarkPath));
                testComplete = false;
            }
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


        Mesh[] meshes = referenceInstance.GetComponent<WaterImmersedRigidbody>().meshList.ToArray();

        int meshVertexCount = 0;
        for (int i = 0; i < meshes.Length; i++)
        {
            meshVertexCount += meshes[i].vertexCount;
        }
        referenceVertices = new Vector3[testCase.testLength][];


        int framesCounter = 0;
        while (framesCounter < testCase.testLength)
        {
            if (framesCounter % 10 == 0)
                Debug.Log($"Progress:  {(framesCounter * 100) / testCase.testLength}%");  //DEBUG progress

            Time.timeScale = 0.0f;

            referenceVertices[framesCounter] = new Vector3[meshVertexCount];

            int vertCounter = 0;
            for (int i = 0; i < meshes.Length; i++)
            {
                for (int j = 0; j < meshes[i].vertexCount; j++, vertCounter++)
                {
                    referenceVertices[framesCounter][vertCounter] = meshes[i].vertices[j];
                }
            }

            Time.timeScale = 1.0f;

            yield return new WaitForSecondsRealtime(Time.deltaTime);   // Wait for next frame
            ++framesCounter;
        }

        Destroy(referenceInstance);
    }


    private IEnumerator RunBenchmark(TestCase testCase, int sampleCount, TestResult testResult, string filePath)
    {
        Debug.Log("RunBenchmark: " + testCase.name + "_s" + sampleCount);


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
                if (framesCounter % 10 == 0)
                    Debug.Log($"Progress:  {(framesCounter * 100) / testCase.testLength}%");  //DEBUG progress

                switch (testCase.typeOfTest)        //Update the test result values with data from this frame
                {
                    case TypeOfTest.Performance:
                        testResult.SaveFrame(framesCounter);
                        break;

                    case TypeOfTest.Correctness:
                        testResult.SaveFrame(framesCounter, boatInstance.GetComponent<WaterImmersedRigidbody>().meshList.ToArray());
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
