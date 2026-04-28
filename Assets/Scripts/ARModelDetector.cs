using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ARModelDetector : MonoBehaviour
{
    public ARTrackedImageManager imageManager;
    public string sliderTag = "Slider";
    public string modelContainerTag;

    public GameObject sliderUI;
    public Slider sliderComponent;
    private bool sliderActive = false;
    private int sliderValues = 0;

    void OnEnable() => imageManager.trackedImagesChanged += OnChanged;
    void OnDisable() => imageManager.trackedImagesChanged -= OnChanged;

    void returnModel() {
        // Là, il faudra faire un appel à la base pour récupérer le point le plus proche,
        // Ça c'est les paramètres pour la Tour Eiffel
        modelContainerTag = "ARObject";
        sliderValues = 3;
    }

    void Start() {
        returnModel();
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

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        foreach (var newImage in eventArgs.added)
        {
            ShowUI(newImage.gameObject);
        }
    }

    void ShowUI(GameObject container) {
        if (!sliderActive && sliderUI != null) {
            sliderUI.SetActive(true);
            sliderActive = true;
            sliderComponent.maxValue = sliderValues - 1;
        }
        
        sliderComponent.value = 2; 
        UpdateModels(container, 2);
    }

    public void OnSliderMove() {
        GameObject container = GameObject.FindWithTag("ARObject");
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