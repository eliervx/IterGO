using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using IterGO.Services;

namespace IterGO.Controllers
{
    public class PhotoController : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private RectTransform panelTop;
        [SerializeField] private RectTransform panelBottom;
        [SerializeField] private RectTransform panelLeft;
        [SerializeField] private RectTransform panelRight;

        [Header("UI Controls")]
        [SerializeField] private GameObject captureButton;
        [SerializeField] private RawImage photoPreviewImage;
        [SerializeField] private GameObject closeButton;

        // Services
        private ScreenshotService screenshotService;

        // State
        private Texture2D currentPhoto;
        private bool hasPhoto = false;

        void Start()
        {
            screenshotService = new ScreenshotService();
        }

        // ─────────────────────────────────────────────
        // PUBLIC API
        // ─────────────────────────────────────────────

        public void TakePhoto()
        {
            if (hasPhoto) return;
            StartCoroutine(CaptureCoroutine());
        }

        public void RemovePhoto()
        {
            if (currentPhoto != null)
            {
                Destroy(currentPhoto);
                currentPhoto = null;
            }

            hasPhoto = false;

            photoPreviewImage.gameObject.SetActive(false);
            closeButton.SetActive(false);
            captureButton.SetActive(true);
        }

        // ─────────────────────────────────────────────
        // PRIVATE METHODS
        // ─────────────────────────────────────────────

        private Rect GetCaptureRect()
        {
            Vector3[] top = new Vector3[4];
            Vector3[] bottom = new Vector3[4];
            Vector3[] left = new Vector3[4];
            Vector3[] right = new Vector3[4];

            panelTop.GetWorldCorners(top);
            panelBottom.GetWorldCorners(bottom);
            panelLeft.GetWorldCorners(left);
            panelRight.GetWorldCorners(right);

            float x = left[2].x;
            float y = bottom[1].y;
            float width = right[0].x - x;
            float height = top[0].y - y;

            return new Rect(x, y, width, height);
        }

        private IEnumerator CaptureCoroutine()
        {
            captureButton.SetActive(false);

            yield return new WaitForEndOfFrame();

            Rect zone = GetCaptureRect();

            if (zone.width <= 0 || zone.height <= 0)
                zone = new Rect(0, 0, Screen.width, Screen.height);

            Texture2D screenshot = new Texture2D((int)zone.width, (int)zone.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(zone, 0, 0);
            screenshot.Apply();

            currentPhoto = screenshot;
            hasPhoto = true;

            photoPreviewImage.texture = screenshot;
            photoPreviewImage.gameObject.SetActive(true);
            closeButton.SetActive(true);

            // Save to file
            string fileName = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(path, screenshot.EncodeToPNG());

            Debug.Log($"Photo sauvegardée: {path}");
        }

        // ─────────────────────────────────────────────
        // CLEANUP
        // ─────────────────────────────────────────────

        private void OnDestroy()
        {
            if (currentPhoto != null)
                Destroy(currentPhoto);
        }
    }
}