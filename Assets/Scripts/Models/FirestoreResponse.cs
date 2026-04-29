using System;
using UnityEngine;

/// <summary>
/// Classes de sérialisation pour Firestore
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
    public StringValue nom;
    public DoubleValue lat;
    public DoubleValue lon;
    public StringValue description;
}

[Serializable]
public class StringValue
{
    public string stringValue;
}

[Serializable]
public class DoubleValue
{
    public double doubleValue;
}