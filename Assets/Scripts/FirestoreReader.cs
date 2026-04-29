using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;

[Serializable] public class FirestoreResponse { public List<FirestoreDocument> documents; }
[Serializable] public class FirestoreDocument { public string name; public Fields fields; }
[Serializable] public class Fields { 
    public StringValue name; 
    public DoubleValue latitude; 
    public DoubleValue longitude; 
    public StringValue description;
}
[Serializable] public class StringValue { public string stringValue; }
[Serializable] public class DoubleValue { public double doubleValue; }

public class FirestoreReader : MonoBehaviour
{   
    static string id = "itergo-fd8aa";
    static List<string> collections = new List<string> {"POI", "PropositionsPOI", "Utilisateur"};
    static string url = $"https://firestore.googleapis.com/v1/projects/{id}/databases/(default)/documents/POI";
    
    public GameObject markerPrefab; 
    public RectTransform mapContent; 
    public MapManager mapManager; 

    void Start()
    {
        // On lance la coroutine (fonction asynchrone) au démarrage
        StartCoroutine(GetPOI());
    }

    IEnumerator GetPOI()
    {
        Debug.Log("Connexion à Firestore en cours...");
        UnityWebRequest request = UnityWebRequest.Get(url);
        
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("DONNÉES REÇUES : " + request.downloadHandler.text);
            string jsonResponse = request.downloadHandler.text;
            FirestoreResponse data = JsonUtility.FromJson<FirestoreResponse>(jsonResponse);

            if (data != null && data.documents != null)
            {
                foreach (var doc in data.documents) {
                    float lat = (float)doc.fields.latitude.doubleValue;
                    float lon = (float)doc.fields.longitude.doubleValue;
                    string poiName = doc.fields.name.stringValue;

                    Vector2 uiPos = mapManager.GetRelativePosition(lat, lon);

                    GameObject marker = Instantiate(markerPrefab, mapContent);
                    marker.GetComponent<RectTransform>().anchoredPosition = uiPos;
                    
                    var controller = marker.GetComponent<POIController>();
                    if(controller != null) {
                        controller.Setup(poiName, doc.fields.description.stringValue);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("ERREUR FIRESTORE : " + request.error);
        }
    }

    void SpawnPOIMarker(string name, float lat, float lon) {
        Vector2 tileCoords = mapManager.GetTileCoords(lat, lon);
        
        float x = (tileCoords.x - mapManager.startTileCoords.x) * 512f;
        float y = (tileCoords.y - mapManager.startTileCoords.y) * 512f;

        GameObject marker = Instantiate(markerPrefab, mapContent);
        RectTransform rect = marker.GetComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(x, -y);

        marker.name = "POI_" + name;
    }
}