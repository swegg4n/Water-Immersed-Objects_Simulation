using System.Collections.Generic;
using UnityEngine;



public class TestResult
{
    TypeOfTest typeOfTest;

    public TestResult(TestCase testCase)
    {
        this.typeOfTest = testCase.typeOfTest;

        this.testLength = testCase.testLength;
        this.testType = testCase.typeOfTest.ToString();
        this.prefabName = testCase.prefab.name;

        this.density = testCase.density;
        this.viscosity = testCase.viscosity;

        this.amplitude = testCase.amplitude;
        this.ordinaryFrequency = testCase.ordinaryFrequency;
        this.angluarFrequency = testCase.angluarFrequency;


        this.fps = new float[testCase.testLength];
        this.memoryUsage = new long[testCase.testLength];
        this.verticesSqrError = new float[testCase.testLength];
    }

    public int testLength;
    public string testType;
    public string prefabName;

    public int stratifiedDivisions;
    public float density;
    public float viscosity;

    public float amplitude;
    public float ordinaryFrequency;
    public float angluarFrequency;


    public float[] fps;
    public long[] memoryUsage;
    public float[] verticesSqrError;



    public void SaveFrame(int frame, Mesh[] meshes = null)
    {
        switch (typeOfTest)
        {
            case TypeOfTest.Performance:
                this.fps[frame] = BenchmarkHelper.GetFPS();
                this.memoryUsage[frame] = BenchmarkHelper.GetMemoryUsage();
                break;
            case TypeOfTest.Correctness:
                this.verticesSqrError[frame] = BenchmarkHelper.VerticesSqrError(meshes, Benchmarking.referenceVertices[frame]);
                break;
        }
    }


    public List<string> Header()
    {
        List<string> result = new List<string>()
            {
                "Test length:  " + testLength.ToString(),
                "Type of test:  " + testType,
                "CPU: " + SystemInfo.processorType + " (" + SystemInfo.processorFrequency + ")",
                "",
                "Prefab name:  " + prefabName,
                "Stratified divisions:  " + stratifiedDivisions,
                "",
                "Amplitude:  " + amplitude,
                "Ordinary frequency  " + ordinaryFrequency,
                "Angular frequency  " + angluarFrequency,
                "",
            };

        switch (typeOfTest)
        {
            case TypeOfTest.Performance:
                result.Add("\nSampleCount \t Avg. FPS \t Avg. Memory usage (bytes)");
                break;

            case TypeOfTest.Correctness:
                result.Add("\nSampleCount \t Mean square vertices error");
                break;
        }

        return result;
    }

    public string Data(int sampleCount)
    {
        string data = $"{sampleCount} \t ";
        switch (typeOfTest)
        {
            case TypeOfTest.Performance:
                return data + $"{BenchmarkHelper.AverageValue(fps)} \t {BenchmarkHelper.AverageValue(memoryUsage)}";

            case TypeOfTest.Correctness:
                return data + $"{BenchmarkHelper.AverageValue(verticesSqrError)}";

            default:
                return data;
        }
    }

};


//public class PerformanceTestResult : TestResult
//{
//    public PerformanceTestResult(TestCase testCase, int samples) : base(testCase, samples)
//    {
//        this.fps = new float[testCase.testLength];
//        this.memoryUsage = new long[testCase.testLength];
//    }

//    public float[] fps;
//    public long[] memoryUsage;


//    public override void SaveFrame(int frame)
//    {
//        this.fps[frame] = BenchmarkHelper.GetFPS();
//        this.memoryUsage[frame] = BenchmarkHelper.GetMemoryUsage();
//    }

//    public override List<string> Header()
//    {
//        List<string> result = base.Header();

//        result.Add("\nAvg. FPS\tAvg. Memory usage (bytes)");

//        return result;
//    }

//    public override string Data()
//    {
//        return BenchmarkHelper.AverageValue(fps).ToString() + "\t" + BenchmarkHelper.AverageValue(memoryUsage).ToString();
//    }
//};


//public class CorrectnessTestResult : TestResult
//{
//    public CorrectnessTestResult(TestCase testCase, int samples) : base(testCase, samples)
//    {
//        this.verticesSquareError = new float[testCase.testLength];
//    }

//    public float[] verticesSquareError;

//    public override void SaveFrame(int frame)
//    {
//        this.verticesSquareError[frame] = BenchmarkHelper.VerticesSqrError();
//    }

//    public override List<string> Header()
//    {
//        List<string> result = base.Header();

//        result.Add("Mean");

//        return result;
//    }

//    public override string Data()
//    {
//        return BenchmarkHelper.AverageValue(verticesSquareError).ToString();
//    }
//};