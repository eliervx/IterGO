using System.Collections;
using UnityEngine;

namespace IterGO.Services
{
    public class ScreenshotService : MonoBehaviour
    {
        public Texture2D capturedTexture;

        public void Capture()
        {
            capturedTexture = ScreenCapture.CaptureScreenshotAsTexture();
        }

        public IEnumerator CaptureAsync()
        {
            yield return new WaitForEndOfFrame();
            capturedTexture = ScreenCapture.CaptureScreenshotAsTexture();
        }
    }
}