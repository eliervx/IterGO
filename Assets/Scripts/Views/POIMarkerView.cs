using UnityEngine;
using TMPro;

/// <summary>
/// POIMarkerView : Vue responsable de l'affichage d'un marqueur POI
/// - Affiche les données d'un POI (nom, description)
/// - Aucune logique métier
/// - Communique avec le controller pour les interactions
/// </summary>
public class POIMarkerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    private MapService mapService;
    private float displayTileSize;
    
    private POIData poiData;
    private RectTransform rectTransform;

    /// <summary>
    /// Initialise la vue avec les données d'un POI
    /// </summary>
    public void SetData(POIData data, MapService mapService, float displayTileSize)
    {
        this.poiData = data;
        this.mapService = mapService;
        this.displayTileSize = displayTileSize;
        
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        UpdateDisplay();
    }

    /// <summary>
    /// Met à jour l'affichage avec les données du POI
    /// </summary>
    private void UpdateDisplay()
    {
        if (poiData == null) return;

        if (nameText != null)
            nameText.text = poiData.name;

        if (descriptionText != null)
            descriptionText.text = poiData.description;
    }

    /// <summary>
    /// Met à jour la position du marqueur POI
    /// Utilise anchoredPosition comme le reste du code
    /// </summary>
    private void UpdatePosition()
    {
        if (poiData == null || mapService == null || rectTransform == null)
            return;

        // Calculer la position UI relative aux coordonnées de départ
        Vector2 uiPos = mapService.GetUIPositionFromGPS(
            poiData.latitude, 
            poiData.longitude, 
            displayTileSize
        );
        
        rectTransform.anchoredPosition = uiPos;
    }

    /// <summary>
    /// Retourne les données du POI
    /// </summary>
    public POIData GetData()
    {
        return poiData;
    }

    /// <summary>
    /// Appelé quand l'utilisateur clique sur le marqueur
    /// </summary>
    public void OnMarkerClicked()
    {
        Debug.Log($"POI cliqué : {poiData.name}");
        // Implémentez ici l'interaction avec le POI (afficher un détail, etc.)
    }
}