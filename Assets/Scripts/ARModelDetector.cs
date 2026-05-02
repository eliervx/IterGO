using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using System.Collections;

public class ARModelDetector : MonoBehaviour
{
    public ARTrackedImageManager imageManager;
    public string sliderTag = "Slider";
    public string modelContainerTag;

    public GameObject sliderUI;
    public Slider sliderComponent;
    private bool sliderActive = false;
    private int sliderValues = 0;

    private float latitude;
    private float longitude;

    void OnEnable() => imageManager.trackedImagesChanged += OnChanged;
    void OnDisable() => imageManager.trackedImagesChanged -= OnChanged;
    void OnDestroy() => StopAllCoroutines();

    void returnModel() {
        /*
        Là, il faut faire un appel à la base pour récupérer tous les POI publics et nous appartenant
        requestResult =

        foreach (var poi in requestResult) {
            float poiLat = float.Parse(poi.Child("lat").Value.ToString());
            float poiLon = float.Parse(poi.Child("lon").Value.ToString());
            float distance = CalculateDistance(latitude, longitude, poiLat, poiLon);

            if (distance < 500 && distance < closestDistance) {
                closestDistance = distance;
                modelContainerTag = poi.Child("prefabTag").Value.ToString();
                sliderValues = int.Parse(poi.Child("sliderValues").Value.ToString());
            }
        }
        */

        modelContainerTag = "Eiffel";
        sliderValues = 3;
    }

    void Start() {
        StartCoroutine(StartLocationService());
        GameObject slider = GameObject.FindWithTag(sliderTag);
        if (slider != null) {
            sliderUI = slider;
            sliderComponent = slider.GetComponentInChildren<Slider>();
            
            if (sliderComponent != null) {
                sliderComponent.onValueChanged.AddListener(delegate { OnSliderMove(); });
            }

            sliderUI.SetActive(false);
        }
        else {
            Debug.LogError($"Attention : Aucun objet avec le tag {sliderTag} n'a été trouvé !");
        }
    }

    IEnumerator StartLocationService() {
        Input.location.Start(10f, 10f);
        int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        if (maxWait < 1) {
            Debug.Log("Délai d'attente GPS dépassé.");
            yield break;
        } else if (Input.location.status == LocationServiceStatus.Failed) {
            Debug.Log("Impossible de déterminer la position GPS (Service Failed).");
        } else if (Input.location.status == LocationServiceStatus.Running) {
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
            Debug.Log($"GPS Initialisé : Lat {latitude} / Long {longitude}");
        }
        returnModel();
    }

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        foreach (var newImage in eventArgs.added) {
            ShowUI(newImage.gameObject);
        }
    }

    void ShowUI(GameObject container) {
        if (!sliderActive && sliderUI != null && sliderValues > 1) {
            sliderUI.SetActive(true);
            sliderActive = true;
            sliderComponent.maxValue = sliderValues - 1;
            sliderComponent.value = 0; 
            UpdateModels(container, 0);
        }
    }

    public void OnSliderMove() {
        GameObject container = GameObject.FindWithTag(modelContainerTag);
        if (container != null) {
            UpdateModels(container, (int)sliderComponent.value);
        }
    }

    void UpdateModels(GameObject container, int index) {
        for (int i = 0; i < container.transform.childCount; i++) {
            container.transform.GetChild(i).gameObject.SetActive(i == index);
        }
    }
}