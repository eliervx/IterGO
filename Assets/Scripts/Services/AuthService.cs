using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

[Serializable]
public class FirebaseAuthResponse
{
    public string kind;
    public string localId;
    public string email;
    public string displayName;
    public string idToken;
    public bool registered;
    public string refreshToken;
    public string expiresIn;
}

[Serializable]
public class FirebaseError
{
    public FirebaseErrorDetails error;
}

[Serializable]
public class FirebaseErrorDetails
{
    public int code;
    public string message;
}

public class AuthService : MonoBehaviour
{
    private const string API_KEY = FirebaseConfig.API_KEY;
    private const string AUTH_URL = "https://identitytoolkit.googleapis.com/v1/accounts:";

    public delegate void OnAuthStateChanged(bool isLoggedIn, string userId, string email);
    public event OnAuthStateChanged AuthStateChanged;

    private string currentUserId;
    private string currentUserEmail;
    private string idToken;

    void Start()
    {
        // Load saved auth data
        currentUserId = PlayerPrefs.GetString("userId", "");
        currentUserEmail = PlayerPrefs.GetString("userEmail", "");
        idToken = PlayerPrefs.GetString("idToken", "");

        if (!string.IsNullOrEmpty(currentUserId))
        {
            AuthStateChanged?.Invoke(true, currentUserId, currentUserEmail);
        }
    }

    public void SignUp(string email, string password, Action<bool, string> callback)
    {
        StartCoroutine(SignUpCoroutine(email, password, callback));
    }

    private IEnumerator SignUpCoroutine(string email, string password, Action<bool, string> callback)
    {
        string jsonData = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";
        UnityWebRequest request = new UnityWebRequest(AUTH_URL + "signUp?key=" + API_KEY, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            FirebaseAuthResponse response = JsonUtility.FromJson<FirebaseAuthResponse>(request.downloadHandler.text);
            currentUserId = response.localId;
            currentUserEmail = response.email;
            idToken = response.idToken;

            // Save auth data
            PlayerPrefs.SetString("userId", currentUserId);
            PlayerPrefs.SetString("userEmail", currentUserEmail);
            PlayerPrefs.SetString("idToken", idToken);
            PlayerPrefs.Save();

            // Créer le document utilisateur dans Firestore
            yield return StartCoroutine(CreateUserDocumentCoroutine(currentUserId, email, callback));
        }
        else
        {
            FirebaseError error = JsonUtility.FromJson<FirebaseError>(request.downloadHandler.text);
            callback(false, error?.error?.message ?? "Unknown error");
        }
    }

    private IEnumerator CreateUserDocumentCoroutine(string userId, string email, Action<bool, string> callback)
    {
        string projectId = FirebaseConfig.PROJECT_ID;
        string firestoreUrl = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{userId}";

        string userDocJson = $@"{{
            ""fields"": {{
                ""email"": {{""stringValue"": ""{email}""}},
                ""userId"": {{""stringValue"": ""{userId}""}},
                ""favorites"": {{""arrayValue"": {{}}}},
                ""createdAt"": {{""timestamp"": ""{System.DateTime.UtcNow:o}""}}
            }}
        }}";

        UnityWebRequest createRequest = new UnityWebRequest(firestoreUrl, "PATCH");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(userDocJson);
        createRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        createRequest.downloadHandler = new DownloadHandlerBuffer();
        createRequest.SetRequestHeader("Content-Type", "application/json");

        yield return createRequest.SendWebRequest();

        if (createRequest.result == UnityWebRequest.Result.Success)
        {
            AuthStateChanged?.Invoke(true, currentUserId, currentUserEmail);
            callback(true, "");
            Debug.Log($"User document created in Firestore for {email}");
        }
        else
        {
            Debug.LogError($"Failed to create user document: {createRequest.downloadHandler.text}");
            callback(false, "Failed to create user profile");
        }
    }

    public void SignIn(string email, string password, Action<bool, string> callback)
    {
        StartCoroutine(SignInCoroutine(email, password, callback));
    }

    private IEnumerator SignInCoroutine(string email, string password, Action<bool, string> callback)
    {
        string jsonData = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";
        UnityWebRequest request = new UnityWebRequest(AUTH_URL + "signInWithPassword?key=" + API_KEY, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            FirebaseAuthResponse response = JsonUtility.FromJson<FirebaseAuthResponse>(request.downloadHandler.text);
            currentUserId = response.localId;
            currentUserEmail = response.email;
            idToken = response.idToken;

            // Save auth data
            PlayerPrefs.SetString("userId", currentUserId);
            PlayerPrefs.SetString("userEmail", currentUserEmail);
            PlayerPrefs.SetString("idToken", idToken);
            PlayerPrefs.Save();

            AuthStateChanged?.Invoke(true, currentUserId, currentUserEmail);
            callback(true, "");
        }
        else
        {
            FirebaseError error = JsonUtility.FromJson<FirebaseError>(request.downloadHandler.text);
            callback(false, error?.error?.message ?? "Unknown error");
        }
    }

    public void SignOut()
    {
        currentUserId = "";
        currentUserEmail = "";
        idToken = "";

        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.DeleteKey("userEmail");
        PlayerPrefs.DeleteKey("idToken");
        PlayerPrefs.Save();

        AuthStateChanged?.Invoke(false, "", "");
    }

    public bool IsUserLoggedIn()
    {
        return !string.IsNullOrEmpty(currentUserId);
    }

    public string GetCurrentUserId()
    {
        return currentUserId;
    }

    public string GetCurrentUserEmail()
    {
        return currentUserEmail;
    }

    public string GetIdToken()
    {
        return idToken;
    }
}