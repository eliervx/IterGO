using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using IterGO.Services;
using IterGO.Models;
using System.Collections.Generic;

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
        public string modelName = "Eiffel";
        private int sliderValues = 0;
        private bool sliderActive = false;

        [Header("Model Library")]
        public List<GameObject> sceneModels;
        private GameObject currentActiveModel;

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
            firestoreService = gameObject.AddComponent<FirestoreService>();

            // Subscribe to AR events
            if (arService != null)
            {
                arService.OnMarkerUpdated += OnMarkerUpdated;
            }

            // Setup UI
            SetupSliderUI();

            // Start location service
            StartCoroutine(InitializeLocation());
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

        private IEnumerator InitializeLocation()
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
        }

        private void LoadNearbyModels(System.Action onComplete = null)
        {
            firestoreService.GetPOIs((List<POIData> pois) => {
                if (pois != null && pois.Count > 0)
                {
                    Debug.Log($"Nombre de POIs reçus de Firestore : {pois.Count}");
                    POIData closest = firestoreService.GetClosestPOI(pois, latitude, longitude);
                    if (closest != null)
                    {
                        modelName = closest.prefabTag; 
                        sliderValues = closest.sliderValues;
                        Debug.Log($"POI trouvé : Nom={closest.nom}, Tag={closest.prefabTag}, SliderValues={closest.sliderValues}");
                    }
                    else
                    {
                        modelName = "Eiffel"; 
                        sliderValues = 3;
                        Debug.Log($"Aucun POI à proximité, utilisation du default : {modelName}");
                    }
                }
                else
                {
                    modelName = "Eiffel"; 
                    sliderValues = 3;
                    Debug.Log($"Aucun POI à proximité, utilisation du default : {modelName}");
                }
                onComplete?.Invoke();
            });
        }

        private void OnMarkerUpdated(string markerName, Vector3 position, Quaternion rotation)
        {
            if (sliderActive) return;
            Debug.Log($"Marqueur détecté: {markerName} à position {position}");

            LoadNearbyModels(() => {
                if (!string.IsNullOrEmpty(modelName))
                {
                    GameObject targetModel = sceneModels.Find(m => m.name == modelName);
                    if (targetModel != null)
                    {
                        if (currentActiveModel != null) currentActiveModel.SetActive(false);
                        targetModel.SetActive(true);
                        currentActiveModel = targetModel;
                        ShowUI(targetModel);
                    }
                    else
                    {
                        Debug.LogError($"L'objet '{modelName}' est introuvable dans la liste sceneModels !");
                    }
                }
            });
        }

        void ShowUI(GameObject container)
        {
            Debug.Log("Apparition du slider");
            if (!sliderActive && sliderUI != null && sliderComponent != null && sliderValues > 1)
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
            UpdateModels(currentActiveModel, (int)sliderComponent.value);
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