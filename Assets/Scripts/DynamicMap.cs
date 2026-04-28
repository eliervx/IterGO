using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using System.Collections;

public class DynamicMap : MonoBehaviour, IDragHandler, IEndDragHandler {
    public RawImage mapDisplay;
    public int zoom = 15;
    
    // On stocke la position actuelle de la carte
    private float currentLat = 48.8584f; 
    private float currentLon = 2.2945f;

    void Start() {
        // Au début, on essaye de chopper le vrai GPS
        if (Input.location.status == LocationServiceStatus.Running) {
            currentLat = Input.location.lastData.latitude;
            currentLon = Input.location.lastData.longitude;
        }
        UpdateMap();
    }

    // Détection du glissement
    public void OnDrag(PointerEventData eventData) {
        // On ajuste la sensibilité selon le zoom
        float sensitivity = 0.00001f * (20 - zoom); 
        currentLon -= eventData.delta.x * sensitivity;
        currentLat += eventData.delta.y * sensitivity;
    }

    // Quand on relâche, on recharge la tuile
    public void OnEndDrag(PointerEventData eventData) {
        UpdateMap();
    }

    public void UpdateMap() {
        StartCoroutine(DownloadTile(currentLat, currentLon));
    }

    IEnumerator DownloadTile(float lat, float lon) {
        Debug.Log("Chargement...");
        int x = (int)((lon + 180.0f) / 360.0f * Mathf.Pow(2, zoom));
        float latRad = lat * Mathf.PI / 180.0f;
        int y = (int)((1.0f - Mathf.Log(Mathf.Tan(latRad) + 1.0f / Mathf.Cos(latRad)) / Mathf.PI) / 2.0f * Mathf.Pow(2, zoom));

        string url = $"https://tile.openstreetmap.org/{zoom}/{x}/{y}.png";
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        request.SetRequestHeader("User-Agent", "Unity-IterGO-POC");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            mapDisplay.texture = DownloadHandlerTexture.GetContent(request);
        }
    }
}