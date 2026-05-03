using UnityEngine;

public class UserSession
{
    // Point d'entrée unique pour l'ID utilisateur
    // Pour ajouter le login plus tard : remplace "1" par l'ID récupéré depuis Firebase Auth
    public static string UserId => PlayerPrefs.GetString("userId", "1");

    public static void SetUserId(string id)
    {
        PlayerPrefs.SetString("userId", id);
        PlayerPrefs.Save();
    }

    public static bool IsGuest => UserId == "1";
}