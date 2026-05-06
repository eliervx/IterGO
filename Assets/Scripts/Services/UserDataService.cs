using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class UserDataService : MonoBehaviour
{
    private static string projectId = FirebaseConfig.PROJECT_ID;
    private static string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents";

    private AuthService authService;

    void Start()
    {
        authService = FindObjectOfType<AuthService>();
    }

    // ─────────────────────────────────────────────
    // FAVORIS
    // ─────────────────────────────────────────────

    public void AddToFavorites(string poiId, Action<bool, string> callback)
    {
        if (!authService.IsUserLoggedIn())
        {
            callback(false, "User not logged in");
            return;
        }

        StartCoroutine(AddToFavoritesCoroutine(poiId, callback));
    }

    private IEnumerator AddToFavoritesCoroutine(string poiId, Action<bool, string> callback)
    {
        string userId = authService.GetCurrentUserId();
        string userDocUrl = $"{url}/users/{userId}";

        // First get current favorites
        UnityWebRequest getRequest = UnityWebRequest.Get(userDocUrl);
        yield return getRequest.SendWebRequest();

        List<string> currentFavorites = new List<string>();
        if (getRequest.result == UnityWebRequest.Result.Success)
        {
            FirestoreDocument doc = JsonUtility.FromJson<FirestoreDocument>(getRequest.downloadHandler.text);
            if (doc != null && doc.fields != null && doc.fields.favorites != null)
            {
                // Parse existing favorites
                var favoritesField = doc.fields.favorites;
                if (favoritesField.arrayValue != null && favoritesField.arrayValue.values != null)
                {
                    foreach (var fav in favoritesField.arrayValue.values)
                    {
                        if (fav != null && !string.IsNullOrEmpty(fav.stringValue))
                        {
                            currentFavorites.Add(fav.stringValue);
                        }
                    }
                }
            }
        }

        // Add new favorite if not already present
        if (!currentFavorites.Contains(poiId))
        {
            currentFavorites.Add(poiId);
        }

        // Update document
        string jsonData = CreateFavoritesJson(currentFavorites);
        UnityWebRequest patchRequest = UnityWebRequest.Put(userDocUrl, jsonData);
        patchRequest.SetRequestHeader("Content-Type", "application/json");
        yield return patchRequest.SendWebRequest();

        if (patchRequest.result == UnityWebRequest.Result.Success)
        {
            callback(true, "");
            Debug.Log("Added to favorites: " + poiId);
        }
        else
        {
            callback(false, patchRequest.error);
        }
    }

    public void RemoveFromFavorites(string poiId, Action<bool, string> callback)
    {
        if (!authService.IsUserLoggedIn())
        {
            callback(false, "User not logged in");
            return;
        }

        StartCoroutine(RemoveFromFavoritesCoroutine(poiId, callback));
    }

    private IEnumerator RemoveFromFavoritesCoroutine(string poiId, Action<bool, string> callback)
    {
        string userId = authService.GetCurrentUserId();
        string userDocUrl = $"{url}/users/{userId}";

        // Get current favorites
        UnityWebRequest getRequest = UnityWebRequest.Get(userDocUrl);
        yield return getRequest.SendWebRequest();

        List<string> currentFavorites = new List<string>();
        if (getRequest.result == UnityWebRequest.Result.Success)
        {
            FirestoreDocument doc = JsonUtility.FromJson<FirestoreDocument>(getRequest.downloadHandler.text);
            if (doc != null && doc.fields != null && doc.fields.favorites != null)
            {
                var favoritesField = doc.fields.favorites;
                if (favoritesField.arrayValue != null && favoritesField.arrayValue.values != null)
                {
                    foreach (var fav in favoritesField.arrayValue.values)
                    {
                        if (fav.stringValue != null)
                        {
                            currentFavorites.Add(fav.stringValue);
                        }
                    }
                }
            }
        }

        // Remove favorite
        currentFavorites.Remove(poiId);

        // Update document
        string jsonData = CreateFavoritesJson(currentFavorites);
        UnityWebRequest patchRequest = UnityWebRequest.Put(userDocUrl, jsonData);
        patchRequest.SetRequestHeader("Content-Type", "application/json");
        yield return patchRequest.SendWebRequest();

        if (patchRequest.result == UnityWebRequest.Result.Success)
        {
            callback(true, "");
            Debug.Log("Removed from favorites: " + poiId);
        }
        else
        {
            callback(false, patchRequest.error);
        }
    }

    private string CreateFavoritesJson(List<string> favorites)
    {
        StringBuilder json = new StringBuilder();
        json.Append("{\"fields\":{\"favorites\":{\"arrayValue\":{\"values\":[");
        for (int i = 0; i < favorites.Count; i++)
        {
            json.Append($"{{\"stringValue\":\"{favorites[i]}\"}}");
            if (i < favorites.Count - 1) json.Append(",");
        }
        json.Append("]}}}}");
        return json.ToString();
    }

    public void GetUserFavorites(Action<List<string>> callback)
    {
        if (!authService.IsUserLoggedIn())
        {
            callback(new List<string>());
            return;
        }

        StartCoroutine(GetUserFavoritesCoroutine(callback));
    }

    private IEnumerator GetUserFavoritesCoroutine(Action<List<string>> callback)
    {
        string userId = authService.GetCurrentUserId();
        string userDocUrl = $"{url}/users/{userId}";

        UnityWebRequest request = UnityWebRequest.Get(userDocUrl);
        yield return request.SendWebRequest();

        List<string> favorites = new List<string>();
        if (request.result == UnityWebRequest.Result.Success)
        {
            FirestoreDocument doc = JsonUtility.FromJson<FirestoreDocument>(request.downloadHandler.text);
            if (doc != null && doc.fields != null && doc.fields.favorites != null)
            {
                var favoritesField = doc.fields.favorites;
                if (favoritesField.arrayValue != null && favoritesField.arrayValue.values != null)
                {
                    foreach (var fav in favoritesField.arrayValue.values)
                    {
                        if (fav != null && !string.IsNullOrEmpty(fav.stringValue))
                        {
                            favorites.Add(fav.stringValue);
                        }
                    }
                }
            }
        }

        callback(favorites);
    }

    // ─────────────────────────────────────────────
    // COMMENTAIRES
    // ─────────────────────────────────────────────

    public void AddComment(string poiId, string text, Action<bool, string> callback)
    {
        if (!authService.IsUserLoggedIn())
        {
            callback(false, "User not logged in");
            return;
        }

        StartCoroutine(AddCommentCoroutine(poiId, text, callback));
    }

    private IEnumerator AddCommentCoroutine(string poiId, string text, Action<bool, string> callback)
    {
        string userId = authService.GetCurrentUserId();
        string userName = authService.GetCurrentUserEmail();
        string commentsUrl = $"{url}/POI/{poiId}/comments";

        string jsonData = $"{{\"fields\":{{\"userId\":{{\"stringValue\":\"{userId}\"}},\"userName\":{{\"stringValue\":\"{userName}\"}},\"text\":{{\"stringValue\":\"{text}\"}},\"timestamp\":{{\"timestampValue\":\"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")}\"}},\"likesCount\":{{\"integerValue\":\"0\"}}}}}}";

        UnityWebRequest request = UnityWebRequest.PostWwwForm(commentsUrl, jsonData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            callback(true, "");
            Debug.Log("Comment added to POI: " + poiId);
        }
        else
        {
            callback(false, request.error);
        }
    }

    public void GetComments(string poiId, Action<List<CommentData>> callback)
    {
        StartCoroutine(GetCommentsCoroutine(poiId, callback));
    }

    private IEnumerator GetCommentsCoroutine(string poiId, Action<List<CommentData>> callback)
    {
        string commentsUrl = $"{url}/POI/{poiId}/comments";
        UnityWebRequest request = UnityWebRequest.Get(commentsUrl);
        yield return request.SendWebRequest();

        List<CommentData> comments = new List<CommentData>();
        if (request.result == UnityWebRequest.Result.Success)
        {
            FirestoreResponse data = JsonUtility.FromJson<FirestoreResponse>(request.downloadHandler.text);
            if (data != null && data.documents != null)
            {
                foreach (var doc in data.documents)
                {
                    string userId = doc.fields.userId?.stringValue ?? "";
                    string userName = doc.fields.userName?.stringValue ?? "";
                    string text = doc.fields.text?.stringValue ?? "";

                    CommentData comment = new CommentData(doc.name, userId, userName, text);
                    comments.Add(comment);
                }
            }
        }

        callback(comments);
    }

    // ─────────────────────────────────────────────
    // SIGNALEMENTS
    // ─────────────────────────────────────────────

    public void ReportPOI(string poiId, string reason, Action<bool, string> callback)
    {
        if (!authService.IsUserLoggedIn())
        {
            callback(false, "User not logged in");
            return;
        }

        StartCoroutine(ReportPOICoroutine(poiId, reason, callback));
    }

    private IEnumerator ReportPOICoroutine(string poiId, string reason, Action<bool, string> callback)
    {
        string userId = authService.GetCurrentUserId();
        string reportsUrl = $"{url}/reports";

        string jsonData = $"{{\"fields\":{{\"poiId\":{{\"stringValue\":\"{poiId}\"}},\"userId\":{{\"stringValue\":\"{userId}\"}},\"reason\":{{\"stringValue\":\"{reason}\"}},\"timestamp\":{{\"timestampValue\":\"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")}\"}},\"status\":{{\"stringValue\":\"pending\"}}}}}}";

        UnityWebRequest request = UnityWebRequest.PostWwwForm(reportsUrl, jsonData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            callback(true, "");
            Debug.Log("POI reported: " + poiId);
        }
        else
        {
            callback(false, request.error);
        }
    }
}