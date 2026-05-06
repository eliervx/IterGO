using UnityEngine;

public class UserSession
{
    // Pour l'instant chemin hardcodé, à remplacer par le vrai ID après login
    // public static string UserId => PlayerPrefs.GetString("userId", "projects/itergo-dev/databases/(default)/documents/Utilisateur/Ix7nu6dpm0qucauBtTO1");
    public static string UserId => PlayerPrefs.GetString("userId", 
    $"projects/{FirebaseConfig.PROJECT_ID}/databases/(default)/documents/Utilisateur/Ix7nu6dpm0qucauBtTO1");

    public static string UserDocPath => UserId; // alias explicite

    public static void SetUserId(string firestorePath)
    {
        PlayerPrefs.SetString("userId", firestorePath);
        PlayerPrefs.Save();
    }

    public static bool IsGuest => UserId == "projects/itergo-dev/databases/(default)/documents/Utilisateur/test_user";
}