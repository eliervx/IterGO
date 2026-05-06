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
    public ReferenceValue referenceValue;
}

[Serializable]
public class ReferenceValue
{
    public string referenceValue;

    public string Trim()
    {
        return referenceValue?.Trim();
    }
}

[Serializable]
public class ArrayValue
{
    public FieldValue[] values;
}

[Serializable]
public class DoubleValue
{
    public double doubleValue;
}

[Serializable]
public class RunQueryResponse
{
    public FirestoreDocument document;
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string wrapped = "{\"array\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
        return wrapper.array;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}