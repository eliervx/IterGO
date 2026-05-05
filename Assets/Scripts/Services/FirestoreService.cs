using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public class FirestoreService : MonoBehaviour
{
    // private static string projectId = "itergo-fd8aa";
    private static string projectId = "itergo-dev";
    private static string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents";

    public delegate void OnPOIsLoadedCallback(List<POIData> pois);

    // ─────────────────────────────────────────────
    // LECTURE — tous les POIs publics
    // ─────────────────────────────────────────────

    public void GetPOIs(OnPOIsLoadedCallback callback)
    {
        StartCoroutine(GetPOIsCoroutine(callback));
    }

    private IEnumerator GetPOIsCoroutine(OnPOIsLoadedCallback callback)
    {
        Debug.Log("Chargement des POIs...");
        UnityWebRequest request = UnityWebRequest.Get(url + "/POI");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            FirestoreResponse data = JsonUtility.FromJson<FirestoreResponse>(request.downloadHandler.text);
            List<POIData> pois = new List<POIData>();

            if (data != null && data.documents != null)
            {
                foreach (var doc in data.documents)
                {
                    POIData poi = new POIData(
                        doc.name,
                        doc.fields.nom.stringValue,
                        (float)doc.fields.lat.doubleValue,
                        (float)doc.fields.lon.doubleValue,
                        doc.fields.description.stringValue,
                        doc.fields.estPrive.boolValue,
                        doc.fields.prefabTag?.stringValue ?? "",
                        int.TryParse(doc.fields.sliderValues?.integerValue, out int val) ? val : 1
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

    // ─────────────────────────────────────────────
    // LECTURE — POIs + PropositionPOIs d'un utilisateur
    // ─────────────────────────────────────────────

    public void GetUserEntries(string userId, OnPOIsLoadedCallback callback)
    {
        StartCoroutine(GetUserEntriesCoroutine(userId, callback));
    }

    private IEnumerator GetUserEntriesCoroutine(string userId, OnPOIsLoadedCallback callback)
    {
        List<POIData> allEntries = new List<POIData>();

        yield return StartCoroutine(FetchCollection("POI", userId, allEntries));
        yield return StartCoroutine(FetchCollection("PropositionPOI", userId, allEntries));

        Debug.Log($"{allEntries.Count} entrées trouvées pour userId={userId}");
        callback?.Invoke(allEntries);
    }

    private IEnumerator FetchCollection(string collection, string userId, List<POIData> results)
    {
        UnityWebRequest request = UnityWebRequest.Get($"{url}/{collection}?pageSize=100");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            FirestoreResponse data = JsonUtility.FromJson<FirestoreResponse>(request.downloadHandler.text);

            if (data != null && data.documents != null)
            {
                foreach (var doc in data.documents)
                {
                    string docUserId = doc.fields.userId?.referenceValue?.Trim() ?? "";
                    
                    if (docUserId != userId) continue;

                    POIData poi = new POIData(
                        doc.name,
                        doc.fields.nom?.stringValue ?? "Sans titre",
                        (float)(doc.fields.lat?.doubleValue ?? 0),
                        (float)(doc.fields.lon?.doubleValue ?? 0),
                        doc.fields.description?.stringValue ?? "",
                        doc.fields.estPrive?.boolValue ?? false,
                        doc.fields.prefabTag?.stringValue ?? "",
                        int.Parse(doc.fields.sliderValues?.integerValue ?? "1")
                    );

                    poi.imageURLs = doc.fields.imageURLs?.arrayValue?.values != null
                        ? doc.fields.imageURLs.arrayValue.values.ConvertAll(v => v.stringValue).ToArray()
                        : new string[0];
                    poi.userId         = doc.fields.userId?.referenceValue ?? "";
                    poi.estPrive       = doc.fields.estPrive?.boolValue ?? false;
                    poi.isProposition  = collection == "PropositionPOI";

                    Debug.Log($"Un {collection}: {poi.nom}");
                    results.Add(poi);
                }
            }
        }
        else
        {
            Debug.LogError($"Erreur {collection} : {request.error}");
        }
    }

    public POIData GetClosestPOI(List<POIData> pois, float userLat, float userLon)
    {
        POIData closest = null;
        float minDistance = float.MaxValue;

        foreach (var poi in pois)
        {
            float dist = CalculateDistance(userLat, userLon, poi.latitude, poi.longitude);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = poi;
            }
        }
        return closest;
    }

    private float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
    {
        float R = 6371e3f;
        float phi1 = lat1 * Mathf.Deg2Rad;
        float phi2 = lat2 * Mathf.Deg2Rad;
        float deltaPhi = (lat2 - lat1) * Mathf.Deg2Rad;
        float deltaLambda = (lon2 - lon1) * Mathf.Deg2Rad;

        float a = Mathf.Sin(deltaPhi / 2) * Mathf.Sin(deltaPhi / 2) +
                    Mathf.Cos(phi1) * Mathf.Cos(phi2) *
                    Mathf.Sin(deltaLambda / 2) * Mathf.Sin(deltaLambda / 2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        return R * c;
    }

    // ─────────────────────────────────────────────
    // ÉCRITURE
    // ─────────────────────────────────────────────

    public void CreateEntry(
        string nom,
        string description,
        double latitude,
        double longitude,
        string imageURLs,
        string userId,
        bool isProposition,
        bool estPrive,
        string prefabTag,
        int sliderValues)
    {
        StartCoroutine(PostEntry(nom, description, latitude, longitude, imageURLs, userId, isProposition, estPrive, prefabTag, sliderValues));
    }

    private IEnumerator PostEntry(
        string nom,
        string description,
        double latitude,
        double longitude,
        string imageURLs,
        string userId,
        bool isProposition,
        bool estPrive,
        string prefabTag,
        int sliderValues)
    {
        string docId      = Guid.NewGuid().ToString();
        string collection = isProposition ? "PropositionPOI" : "POI";
        string endpoint   = $"{url}/{collection}/{docId}";

        string Latitude = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string Longitude = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

        string userPath = userId;

        string json = $@"{{
            ""fields"": {{
                ""id"":          {{ ""stringValue"": ""{docId}"" }},
                ""nom"":         {{ ""stringValue"": ""{nom}"" }},
                ""description"": {{ ""stringValue"": ""{description}"" }},
                ""Latitude"":         {{ ""doubleValue"": {Latitude} }},
                ""Longitude"":         {{ ""doubleValue"": {Longitude} }},
                ""imageURLs"":   {{ ""arrayValue"": {{ ""values"": [ {{ ""stringValue"": ""{imageURLs}"" }} ] }} }},
                ""userId"":      {{ ""referenceValue"": ""{userPath}"" }},
                ""estPrive"":    {{ ""booleanValue"": {estPrive.ToString().ToLower()} }},
                ""majAt"":   {{ ""stringValue"": ""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}"" }},
                ""prefabTag"":   {{ ""stringValue"": ""{prefabTag}"" }},
                ""sliderValues"": {{ ""integerValue"": {sliderValues} }}
            }}
        }}";

        Debug.Log($"JSON envoyé : {json}");
        Debug.Log($"URL : {endpoint}");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(endpoint, "PATCH");
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log($"{collection} créé ! ID : {docId}");
        else
            Debug.LogError($"Erreur : {request.error}\n{request.downloadHandler.text}");
    }

    // ─────────────────────────────────────────────
    // UTILITAIRE
    // ─────────────────────────────────────────────

    public static string TextureToBase64(Texture2D texture, int maxSize = 256)
    {
        Texture2D resized = ResizeTexture(texture, maxSize, maxSize);
        byte[] bytes = resized.EncodeToJPG(50);
        Destroy(resized);
        return Convert.ToBase64String(bytes);
    }

    private static Texture2D ResizeTexture(Texture2D source, int maxWidth, int maxHeight)
    {
        float ratio   = Mathf.Min((float)maxWidth / source.width, (float)maxHeight / source.height);
        int newWidth  = Mathf.RoundToInt(source.width * ratio);
        int newHeight = Mathf.RoundToInt(source.height * ratio);

        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        Graphics.Blit(source, rt);
        RenderTexture.active = rt;

        Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }
}