using UnityEngine;
using System;

/// <summary>
/// Service responsable de la gestion de la localisation utilisateur
/// Centralize la logique GPS en un seul endroit
/// </summary>
public class LocationService : MonoBehaviour
{
    private UserData userData;
    private bool isInitialized = false;

    public delegate void OnLocationUpdatedCallback(UserData userData);
    public event OnLocationUpdatedCallback OnLocationUpdated;

    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// Initialise le service de localisation
    /// </summary>
    private void Initialize()
    {
        // Valeurs par défaut (Paris)
        float defaultLat = 48.8584f;
        float defaultLon = 2.2945f;

        if (Input.location.isEnabledByUser)
        {
            Input.location.Start();
            StartCoroutine(WaitForGPS(defaultLat, defaultLon));
        }
        else
        {
            userData = new UserData(defaultLat, defaultLon, false);
            isInitialized = true;
        }
    }

    private System.Collections.IEnumerator WaitForGPS(float defaultLat, float defaultLon)
    {
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Running)
        {
            userData = new UserData(
                (float)Input.location.lastData.latitude,
                (float)Input.location.lastData.longitude,
                true
            );
        }
        else
        {
            userData = new UserData(defaultLat, defaultLon, false);
        }

        isInitialized = true;
    }

    private void Update()
    {
        if (isInitialized && Input.location.status == LocationServiceStatus.Running)
        {
            UserData newData = new UserData(
                (float)Input.location.lastData.latitude,
                (float)Input.location.lastData.longitude,
                true
            );
            OnLocationUpdated?.Invoke(newData);
        }
    }

    public UserData GetCurrentLocation() => userData;
    public bool IsInitialized() => isInitialized;
}
