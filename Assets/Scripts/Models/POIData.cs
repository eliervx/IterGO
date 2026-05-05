using System;
using UnityEngine;

/// <summary>
/// Modèle représentant un Point Of Interest (POI)
/// </summary>
[Serializable]
public class POIData
{
    public string id;
    public string nom;
    public float latitude;
    public float longitude;
    public string description;
    public string[] imageURLs;
    public string userId;
    public bool estPrive;
    public bool isProposition;
    public string prefabTag;
    public int sliderValues;

    public POIData(string id, string nom, float latitude, float longitude, string description, bool estPrive, string prefabTag, int sliderValues)
    {
        this.id = id;
        this.nom = nom;
        this.latitude = latitude;
        this.longitude = longitude;
        this.description = description;
        this.estPrive = estPrive;
        this.prefabTag = prefabTag;
        this.sliderValues = sliderValues;
    }
}


