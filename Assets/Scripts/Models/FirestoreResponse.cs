using System;
using UnityEngine;
using System.Collections.Generic;

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
    public StringValue description;
    public ReferenceValue userId;
    public ArrayValueWrapper imageURLs;
    public DoubleValue Latitude;
    public DoubleValue Longitude;
    public BoolValue estPrive;
}

[Serializable]
public class StringValue
{
    public string stringValue;
}


[Serializable]
public class ReferenceValue
{
    public string referenceValue;
}

[Serializable]
public class ArrayValueWrapper
{
    public ArrayValue arrayValue;
}

[Serializable]
public class ArrayValue
{
    public List<StringValue> values;
}

[Serializable]
public class DoubleValue
{
    public double doubleValue;
}

[Serializable]
public class BoolValue
{
    public bool booleanValue;
}