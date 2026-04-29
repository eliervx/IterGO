using System.Collections;
using UnityEngine;

public class ScreenshotHelper : MonoBehaviour
{
    public Texture2D capturedTexture;

    public void Capture()
    {
        StartCoroutine(CaptureRoutine());
    }

    private IEnumerator CaptureRoutine()
    {
        yield return new WaitForEndOfFrame();

        capturedTexture = ScreenCapture.CaptureScreenshotAsTexture();
    }
}