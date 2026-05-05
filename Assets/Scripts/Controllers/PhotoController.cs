using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IterGO.Services;
using IterGO.Models;

namespace IterGO.Controllers
{
    public class PhotoController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private RectTransform panelTop;
        [SerializeField] private RectTransform panelBottom;
        [SerializeField] private RectTransform panelLeft;
        [SerializeField] private RectTransform panelRight;

        [Header("Controls")]
        [SerializeField] private GameObject captureButton;
        [SerializeField] private RawImage photoPreviewImage;
        [SerializeField] private GameObject closeButton;

        [Header("Champs obligatoires")]
        [SerializeField] private TMP_InputField titleField;
        [SerializeField] private TMP_InputField descriptionField;
        [SerializeField] private TMP_Text errorText;

        [Header("Boutons Save / Send")]
        [SerializeField] private GameObject saveButton;
        [SerializeField] private GameObject sendButton;

        [Header("Firestore")]
        [SerializeField] private FirestoreService firestoreService;

        // Services
        private ScreenshotService screenshotService;

        // State
        private Texture2D currentPhoto;
        private bool hasPhoto = false;

        void Start()
        {
            screenshotService = gameObject.AddComponent<ScreenshotService>();

            // État initial
            saveButton.SetActive(false);
            sendButton.SetActive(false);
            closeButton.SetActive(false);
            photoPreviewImage.gameObject.SetActive(false);

            if (errorText != null)
                errorText.gameObject.SetActive(false);
        }

        // ─────────────────────────────────────────────
        // VALIDATION
        // ─────────────────────────────────────────────

        private bool ValidateFields()
        {
            string error = "";

            if (string.IsNullOrWhiteSpace(titleField.text))
                error += "Le titre est obligatoire.\n";

            if (string.IsNullOrWhiteSpace(descriptionField.text))
                error += "La description est obligatoire.\n";

            if (!hasPhoto)
                error += "Une photo est obligatoire.";

            if (!string.IsNullOrEmpty(error))
            {
                if (errorText != null)
                {
                    errorText.text = error.Trim();
                    errorText.gameObject.SetActive(true);
                }
                return false;
            }

            if (errorText != null)
                errorText.gameObject.SetActive(false);

            return true;
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
            saveButton.SetActive(false);
            sendButton.SetActive(false);
            captureButton.SetActive(true);

            if (errorText != null)
                errorText.gameObject.SetActive(false);
        }

        // SAVE → POI privé
        public void OnSaveClicked()
        {
            if (!ValidateFields()) return;

            string photoBase64 = FirestoreService.TextureToBase64(currentPhoto, 256);
            string titre = titleField.text;
            string description = descriptionField.text;

            // Reset UI immédiatement
            RemovePhoto();
            titleField.text = "";
            descriptionField.text = "";

            // Lance l'envoi en arrière-plan
            StartCoroutine(GetLocationAndCreate(
                titre,
                description,
                photoBase64,
                isProposition: false,
                isPrivate: true
            ));
        }

        // SEND → PropositionPOI
        public void OnSendClicked()
        {
            if (!ValidateFields()) return;
            string photoBase64 = FirestoreService.TextureToBase64(currentPhoto, 256);
            StartCoroutine(GetLocationAndCreate(
                titleField.text,
                descriptionField.text,
                photoBase64,
                isProposition: true,
                isPrivate: false
            ));
            RemovePhoto();
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
            saveButton.SetActive(true);
            sendButton.SetActive(true);

            // Save to file
            string fileName = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(path, screenshot.EncodeToPNG());

            Debug.Log($"Photo sauvegardée: {path}");
        }

        private IEnumerator GetLocationAndCreate(
            string titre,
            string description,
            string photoBase64,
            bool isProposition,
            bool isPrivate)
        {
            double latitude  = 0;
            double longitude = 0;

            if (Input.location.isEnabledByUser)
            {
                Input.location.Start();
                int maxWait = 10;
                while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
                {
                    yield return new WaitForSeconds(1);
                    maxWait--;
                }

                if (Input.location.status == LocationServiceStatus.Running)
                {
                    latitude  = Input.location.lastData.latitude;
                    longitude = Input.location.lastData.longitude;
                }

                Input.location.Stop();
            }
            else
            {
                Debug.LogWarning("Localisation désactivée, coordonnées à 0.");
            }

            firestoreService.CreateEntry(
                titre,
                description,
                latitude,
                longitude,
                photoBase64,
                UserSession.UserId,
                isProposition,
                isPrivate,
                "",
                1
            );
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