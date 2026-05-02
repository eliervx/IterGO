using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.VisualScripting;

namespace IterGO.Services
{
    public class ARService : MonoBehaviour
    {
        [SerializeField] private ARTrackedImageManager _manager;

        public delegate void MarkerUpdatedHandler(string markerName, Vector3 position, Quaternion rotation);
        public event MarkerUpdatedHandler OnMarkerUpdated;

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
                    OnMarkerUpdated?.Invoke(image.referenceImage.name, image.transform.position, image.transform.rotation);
                }
            }
        }
    }
}