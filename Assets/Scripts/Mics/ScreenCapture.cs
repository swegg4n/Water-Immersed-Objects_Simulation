using UnityEngine;

public class ScreenCapture : MonoBehaviour
{
    static int id = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            UnityEngine.ScreenCapture.CaptureScreenshot($"{Application.dataPath}/Images/Screenshots/capture_{++id:000}.png", 1);
        }
    }
}
