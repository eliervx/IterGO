using System;
using UnityEngine;

/// <summary>
/// Modèle représentant les données de l'utilisateur
/// </summary>
[Serializable]
public class UserData
{
    public float latitude;
    public float longitude;
    public bool isGPSValid;

    public UserData(float latitude, float longitude, bool isGPSValid)
    {
        this.latitude = latitude;
        this.longitude = longitude;
        this.isGPSValid = isGPSValid;
    }
}