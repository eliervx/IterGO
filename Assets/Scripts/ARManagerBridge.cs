using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.VisualScripting;

public class ARManagerBridge : MonoBehaviour
{
    private ARTrackedImageManager _manager;

    void Awake()
    {
        _manager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        _manager.trackedImagesChanged += OnImagesChanged;
    }

    void OnDisable()
    {
        _manager.trackedImagesChanged -= OnImagesChanged;
    }

    void OnImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var image in args.updated)
        {
            // On envoie le nom de l'image et l'objet image au Visual Scripting
            CustomEvent.Trigger(gameObject, "MarkerUpdated", image.referenceImage.name, image.transform.position, image.transform.rotation);
        }
    }
}
