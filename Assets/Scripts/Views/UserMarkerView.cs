using UnityEngine;

/// <summary>
/// UserMarkerView : Vue responsable de l'affichage du marqueur utilisateur
/// - Affiche uniquement la position basée sur les données reçues
/// - Aucune logique métier
/// - Communique avec le controller pour les mises à jour
/// </summary>
public class UserMarkerView : MonoBehaviour
{
    private RectTransform rectTransform;
    private LocationService locationService;
    private MapService mapService;
    private float displayTileSize;

    public void Initialize(LocationService locationService, MapService mapService, float displayTileSize)
    {
        this.locationService = locationService;
        this.mapService = mapService;
        this.displayTileSize = displayTileSize;

        rectTransform = GetComponent<RectTransform>();

        // S'abonner aux mises à jour de localisation
        locationService.OnLocationUpdated += UpdatePosition;

        // Position initiale
        if (locationService.GetCurrentLocation() != null)
        {
            UpdatePosition(locationService.GetCurrentLocation());
        }
    }

    /// <summary>
    /// Met à jour la position du marqueur utilisateur
    /// </summary>
    private void UpdatePosition(UserData userData)
    {
        if (userData != null && rectTransform != null)
        {
            Vector2 uiPos = mapService.GetUIPositionFromGPS(userData.latitude, userData.longitude, displayTileSize);
            rectTransform.anchoredPosition = uiPos;
        }
    }

    private void OnDestroy()
    {
        if (locationService != null)
        {
            locationService.OnLocationUpdated -= UpdatePosition;
        }
    }
}