using System;
using UnityEngine;

/// <summary>
/// Modèle représentant un Point Of Interest (POI)
/// </summary>
[Serializable]
public class POIData
{
    public string id;
    public string name;
    public float latitude;
    public float longitude;
    public string description;
    public string photoBase64;
    public string userId;

    public POIData(string id, string name, float latitude, float longitude, string description)
    {
        this.id = id;
        this.name = name;
        this.latitude = latitude;
        this.longitude = longitude;
        this.description = description;
    }
}


