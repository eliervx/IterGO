using System;
using System.Collections.Generic;
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
    public int likesCount;
    public bool isLikedByCurrentUser;
    public List<CommentData> comments;

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
        this.likesCount = 0;
        this.isLikedByCurrentUser = false;
        this.comments = new List<CommentData>();
    }
}

[Serializable]
public class CommentData
{
    public string id;
    public string userId;
    public string userName;
    public string text;
    public DateTime timestamp;
    public int likesCount;

    public CommentData(string id, string userId, string userName, string text)
    {
        this.id = id;
        this.userId = userId;
        this.userName = userName;
        this.text = text;
        this.timestamp = DateTime.Now;
        this.likesCount = 0;
    }
}


