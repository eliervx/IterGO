using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using IterGO.Services;
using IterGO.Models;

namespace IterGO.Controllers
{
    public class ARController : MonoBehaviour
    {
        [Header("AR Components")]
        public ARService arService;

        [Header("UI Components")]
        public string sliderTag = "Slider";
        public GameObject sliderUI;
        public Slider sliderComponent;

        [Header("Model Management")]
        public string modelContainerTag;
        private int sliderValues = 0;
        private bool sliderActive = false;

        // Services
        private LocationService locationService;
        private FirestoreService firestoreService;

        // Data
        private float latitude;
        private float longitude;

        void Start()
        {
            // Initialize services
            locationService = new LocationService();
            firestoreService = new FirestoreService();

            // Subscribe to AR events
            if (arService != null)
            {
                arService.OnMarkerUpdated += OnMarkerUpdated;
            }

            // Setup UI
            SetupSliderUI();

            // Start location service
            StartCoroutine(InitializeLocationAndLoadModels());
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            if (arService != null)
            {
                arService.OnMarkerUpdated -= OnMarkerUpdated;
            }
            StopAllCoroutines();
        }

        private void SetupSliderUI()
        {
            GameObject slider = GameObject.FindWithTag(sliderTag);
            if (slider != null)
            {
                sliderUI = slider;
                sliderComponent = slider.GetComponentInChildren<Slider>();

                if (sliderComponent != null)
                {
                    sliderComponent.onValueChanged.AddListener(delegate { OnSliderMove(); });
                }

                sliderUI.SetActive(false);
            }
            else
            {
                Debug.LogError($"Attention : Aucun objet avec le tag {sliderTag} n'a été trouvé !");
            }
        }

        private IEnumerator InitializeLocationAndLoadModels()
        {
            // Initialize GPS
            locationService.Initialize();

            // Wait for GPS to be ready (LocationService handles its own coroutine internally)
            yield return new WaitUntil(() => locationService.IsInitialized());

            if (locationService.IsInitialized())
            {
                UserData userLocation = locationService.GetCurrentLocation();
                latitude = userLocation.latitude;
                longitude = userLocation.longitude;
                Debug.Log($"GPS Initialisé : Lat {latitude} / Long {longitude}");
            }
            else
            {
                Debug.Log("GPS non disponible, utilisation des valeurs par défaut");
                latitude = 48.8584f; // Paris
                longitude = 2.2945f;
            }

            // Load nearby models
            LoadNearbyModels();
        }

        private void LoadNearbyModels()
        {
            // TODO: Implement POI loading based on location
            // For now, use hardcoded values
            modelContainerTag = "Eiffel";
            sliderValues = 3;

            Debug.Log($"Modèle chargé: {modelContainerTag} avec {sliderValues} variantes");
        }

        private void OnMarkerUpdated(string markerName, Vector3 position, Quaternion rotation)
        {
            Debug.Log($"Marqueur détecté: {markerName} à position {position}");

            // Find the container for this marker and show UI
            GameObject container = GameObject.FindWithTag(modelContainerTag);
            if (container != null)
            {
                ShowUI(container);
            }
        }

        void ShowUI(GameObject container)
        {
            if (!sliderActive && sliderUI != null && sliderValues > 1)
            {
                sliderUI.SetActive(true);
                sliderActive = true;
                sliderComponent.maxValue = sliderValues - 1;
                sliderComponent.value = 0;
                UpdateModels(container, 0);
            }
        }

        public void OnSliderMove()
        {
            GameObject container = GameObject.FindWithTag(modelContainerTag);
            if (container != null)
            {
                UpdateModels(container, (int)sliderComponent.value);
            }
        }

        void UpdateModels(GameObject container, int index)
        {
            for (int i = 0; i < container.transform.childCount; i++)
            {
                container.transform.GetChild(i).gameObject.SetActive(i == index);
            }
        }
    }
}