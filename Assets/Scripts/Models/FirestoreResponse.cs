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
    public BoolValue estPrive;
    public ReferenceValue userId;
    public StringArray imageURLs;
    public DoubleValue lat;
    public DoubleValue lon;
    public StringValue prefabTag;
    public IntegerValue sliderValues;
}

[Serializable]
public class StringValue
{
    public string stringValue;
}

[Serializable]
public class StringArray
{
    public ArrayValue arrayValue;
}

[Serializable]
public class ArrayValue
{
    public List<StringValue> values;
}

[Serializable]
public class IntegerValue
{
    public string integerValue;
}


[Serializable]
public class ReferenceValue
{
    public string referenceValue;
}

[Serializable]
public class DoubleValue
{
    public double doubleValue;
}

[Serializable]
public class BoolValue
{
    public bool boolValue;
}