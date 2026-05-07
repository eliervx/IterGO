using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public class FirestoreService : MonoBehaviour
{
    private static string projectId = FirebaseConfig.PROJECT_ID;
    private static string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents";

    public delegate void OnPOIsLoadedCallback(List<POIData> pois);

    private AuthService authService;

    void Start()
    {
        authService = FindObjectOfType<AuthService>();
    }

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
                string currentUserId = authService?.GetCurrentUserId();

                foreach (var doc in data.documents)
                {
                    bool estPrive = doc.fields.estPrive?.booleanValue ?? false;
                    string userId = doc.fields.userId?.stringValue ?? "";

                    // Afficher POI public OU POI privé de l'utilisateur connecté
                    if (!estPrive || (currentUserId != null && userId == currentUserId))
                    {
                        string nom = doc.fields.nom?.stringValue ?? "";
                        float lat = (float)(doc.fields.lat?.doubleValue ?? 0);
                        float lon = (float)(doc.fields.lon?.doubleValue ?? 0);
                        string description = doc.fields.description?.stringValue ?? "";
                        string prefabTag = doc.fields.prefabTag?.stringValue ?? "";
                        int sliderValues = int.TryParse(doc.fields.sliderValues?.integerValue ?? "1", out int val) ? val : 1;

                        POIData poi = new POIData(doc.name, nom, lat, lon, description, estPrive, prefabTag, sliderValues);
                        poi.userId = userId;
                        pois.Add(poi);
                    }
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
        StartCoroutine(GetUserEntriesAsync(callback));
    }

    private IEnumerator GetUserEntriesAsync(OnPOIsLoadedCallback callback)
    {
        // Attendre que l'authentification soit prête
        while (authService == null || !authService.IsUserLoggedIn())
        {
            Debug.Log("Attente de l'authentification...");
            yield return new WaitForSeconds(0.5f);
        }

        string currentUserId = authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogError("Utilisateur non connecté après attente, impossible de récupérer les entrées utilisateur");
            callback?.Invoke(new List<POIData>());
            yield break;
        }

        string userReference = $"projects/{FirebaseConfig.PROJECT_ID}/databases/(default)/documents/Utilisateur/{currentUserId}";
        Debug.Log($"Récupération des entrées pour l'utilisateur: {currentUserId}, référence: {userReference}");

        // Attendre que le token soit disponible
        string token = authService.GetIdToken();
        int attempts = 0;
        while (string.IsNullOrEmpty(token) && attempts < 10) // Timeout après 5 secondes
        {
            Debug.Log("Attente du token d'authentification...");
            yield return new WaitForSeconds(0.5f);
            token = authService.GetIdToken();
            attempts++;
        }

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Token d'authentification non disponible après attente");
            callback?.Invoke(new List<POIData>());
            yield break;
        }

        Debug.Log("Token d'authentification récupéré avec succès");
        List<POIData> allEntries = new List<POIData>();

        yield return StartCoroutine(FetchCollection("POI", userReference, token, allEntries));
        yield return StartCoroutine(FetchCollection("PropositionPOI", userReference, token, allEntries));

        Debug.Log($"{allEntries.Count} entrées trouvées pour userReference={userReference}");
        callback?.Invoke(allEntries);
    }

    private IEnumerator FetchCollection(string collection, string userReference, string token, List<POIData> results)
    {
        string queryUrl = $"{url}:runQuery";
        string userPath = $"projects/{FirebaseConfig.PROJECT_ID}/databases/(default)/documents/Utilisateur/{userId}";

        string queryJson = $@"{{
            ""structuredQuery"": {{
                ""from"": [{{ ""collectionId"": ""{collection}"" }}],
                ""where"": {{
                    ""fieldFilter"": {{
                        ""field"": {{ ""fieldPath"": ""userId"" }},
                        ""op"": ""EQUAL"",
                        ""value"": {{ ""referenceValue"": ""{userPath}"" }}
                    }}
                }}
            }}
        }}";
        Debug.Log($"Query JSON pour {collection}: {queryJson}");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(queryJson);

        UnityWebRequest request = new UnityWebRequest(queryUrl, "POST");
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + token);
        Debug.Log($"Envoi de requête runQuery pour {collection} avec token d'authentification");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // runQuery retourne un tableau JSON
            string response = request.downloadHandler.text;
            
            // Parse manuel car JsonUtility ne gère pas les tableaux racine
            RunQueryResponse[] queryResults = JsonHelper.FromJson<RunQueryResponse>(response);

            if (queryResults != null && queryResults.Length > 0)
            {
                foreach (var result in queryResults)
                {
                    if (result == null) continue;
                    if (result.document == null) continue;
                    if (result.document.name == null) continue;
                    if (result.document.fields == null) continue;

                    var doc = result.document;
                    Debug.Log($"Le doc trouvé : {doc.name} {doc.fields}");
                    string docUserId = doc.fields.userId?.referenceValue?.Trim() ?? "";

                    POIData poi = new POIData(
                        doc.name,
                        doc.fields.nom?.stringValue ?? "Sans titre",
                        (float)(doc.fields.lat?.doubleValue ?? 0),
                        (float)(doc.fields.lon?.doubleValue ?? 0),
                        doc.fields.description?.stringValue ?? "",
                        doc.fields.estPrive?.booleanValue ?? false,
                        doc.fields.prefabTag?.stringValue ?? "",
                        int.TryParse(doc.fields.sliderValues?.integerValue, out int val) ? val : 1
                    );

                    poi.imageURLs = doc.fields.imageURLs?.arrayValue?.values != null
                        ? System.Array.ConvertAll(doc.fields.imageURLs.arrayValue.values,
                            v => v?.stringValue ?? "")
                        : new string[0];
                    poi.userId        = docUserId;
                    poi.estPrive      = doc.fields.estPrive?.booleanValue ?? false;
                    poi.isProposition = collection == "PropositionPOI";

                    Debug.Log($"[{collection}] trouvé : {poi.nom}");
                    results.Add(poi);
                }
            }
        }
        else
        {
            Debug.LogError($"Erreur {collection} : {request.error}\n{request.downloadHandler.text}");
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

        string lat = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string lon = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

        string userPath = $"projects/{FirebaseConfig.PROJECT_ID}/databases/(default)/documents/Utilisateur/{userId}";

        string json = $@"{{
            ""fields"": {{
                ""id"":          {{ ""stringValue"": ""{docId}"" }},
                ""nom"":         {{ ""stringValue"": ""{nom}"" }},
                ""description"": {{ ""stringValue"": ""{description}"" }},
                ""lat"":         {{ ""doubleValue"": {lat} }},
                ""lon"":         {{ ""doubleValue"": {lon} }},
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

        string authToken = authService?.GetIdToken();
        if (!string.IsNullOrEmpty(authToken))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
        }

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