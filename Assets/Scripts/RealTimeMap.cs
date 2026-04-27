using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class RealTimeMap : MonoBehaviour {
    public RawImage mapDisplay;
    public int zoom = 15;

    IEnumerator Start() {
        // 1. Activation du service de localisation
        // if (!Input.location.isEnabledByUser) {
        //     Debug.Log("Impossible d'initialiser le GPS");
        //     Input.location.Start();
        //     yield break ;
        // };

        // int maxWait = 20;
        // while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
        //     yield return new WaitForSeconds(1);
        //     maxWait--;
        // }

        // if (Input.location.status == LocationServiceStatus.Failed || maxWait < 1) {
        //     Debug.Log("Impossible d'initialiser le GPS");
        //     yield break;
        // }

        // 2. Récupération des coordonnées réelles
        // float lat = (float)Input.location.lastData.latitude;
        // float lon = (float)Input.location.lastData.longitude;
        // FORCE LA POSITION POUR LE TEST PC
        float lat = 48.8584f; // Coordonnées de la Tour Eiffel
        float lon = 2.2945f;

        // // 3. Conversion Lat/Lon vers système de tuiles OSM
        int x = (int)((lon + 180.0f) / 360.0f * Mathf.Pow(2, zoom));
        int y = (int)((1.0f - Mathf.Log(Mathf.Tan(lat * Mathf.PI / 180.0f) + 1.0f / Mathf.Cos(lat * Mathf.PI / 180.0f)) / Mathf.PI) / 2.0f * Mathf.Pow(2, zoom));

        // 4. Téléchargement de la carte (Gratuit, pas de clé API nécessaire)
        string url = $"https://tile.openstreetmap.org/{zoom}/{x}/{y}.png";
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        request.SetRequestHeader("User-Agent", "Unity-IterGO-POC-StudentProject");
        Debug.Log($"Tentative d'accès à : {url}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            mapDisplay.texture = DownloadHandlerTexture.GetContent(request);
        } else {
            Debug.LogError("Erreur téléchargement map : " + request.error);
        }
    }
}