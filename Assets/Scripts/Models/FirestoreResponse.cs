using System;
using UnityEngine;

/// <summary>
/// Classes simplifiées pour Firestore API REST
/// </summary>
[Serializable]
public class FirestoreResponse
{
    public FirestoreDocument[] documents;
}

[Serializable]
public class FirestoreDocument
{
    public string name;
    public Fields fields;
}

[Serializable]
public class Fields
{
    public FieldValue nom;
    public FieldValue description;
    public FieldValue estPrive;
    public FieldValue userId;
    public FieldValue imageURLs;
    public FieldValue lat;
    public FieldValue lon;
    public FieldValue prefabTag;
    public FieldValue sliderValues;
    public FieldValue favorites;
    public FieldValue userName;
    public FieldValue text;
}

[Serializable]
public class FieldValue
{
    public string stringValue;
    public ArrayValue arrayValue;
    public string integerValue;
    public bool booleanValue;
    public double doubleValue;
}

[Serializable]
public class ArrayValue
{
    public FieldValue[] values;
}