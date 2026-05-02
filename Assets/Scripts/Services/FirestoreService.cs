using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Service responsable du chargement des données depuis Firestore
/// Retourne uniquement les données, sans créer de GameObjects
/// </summary>
public class FirestoreService : MonoBehaviour
{
    private static string id = "itergo-fd8aa";
    private static string url = $"https://firestore.googleapis.com/v1/projects/{id}/databases/(default)/documents";

    public delegate void OnPOIsLoadedCallback(List<POIData> pois);

    /// <summary>
    /// Récupère les POIs depuis Firestore de manière asynchrone
    /// </summary>
    public void GetPOIs(OnPOIsLoadedCallback callback)
    {
        StartCoroutine(GetPOIsCoroutine(callback));
    }

    private IEnumerator GetPOIsCoroutine(OnPOIsLoadedCallback callback)
    {
        Debug.Log("Connexion à Firestore en cours...");
        UnityWebRequest request = UnityWebRequest.Get(url+"/POI");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("DONNÉES REÇUES : " + request.downloadHandler.text);
            string jsonResponse = request.downloadHandler.text;
            FirestoreResponse data = JsonUtility.FromJson<FirestoreResponse>(jsonResponse);

            List<POIData> pois = new List<POIData>();

            if (data != null && data.documents != null)
            {
                foreach (var doc in data.documents)
                {
                    float lat = (float)doc.fields.lat.doubleValue;
                    float lon = (float)doc.fields.lon.doubleValue;
                    
                    // Debug.Log($"POI parsé: '{doc.fields.nom.stringValue}' -> lat={lat}, lon={lon}");
                    
                    POIData poi = new POIData(
                        doc.name,
                        doc.fields.nom.stringValue,
                        lat,
                        lon,
                        doc.fields.description.stringValue
                    );
                    pois.Add(poi);
                }
            }

            callback?.Invoke(pois);
        }
        else
        {
            Debug.LogError("ERREUR FIRESTORE : " + request.error);
            callback?.Invoke(new List<POIData>());
        }
    }
}
