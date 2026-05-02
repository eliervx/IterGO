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
    Debug.Log("Le Manager a détecté un changement ! Nombre d'images : " + args.updated.Count);
    
    foreach (var image in args.updated)
    {
        
        Debug.Log("DANS LE FOR : " + args.updated.Count);
        // On ne traite l'image que si elle est actuellement "Tracking" (bien vue par la caméra)
        if (image.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
        {
            Debug.Log("DANS LE IF : " + args.updated.Count);
            CustomEvent.Trigger(gameObject, "MarkerUpdated", image.referenceImage.name, image.transform.position, image.transform.rotation);
        }
    }
}
}
