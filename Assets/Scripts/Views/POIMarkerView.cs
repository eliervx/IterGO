using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private GameObject poiPanelPrefab;
    [SerializeField] private Texture poiPanelTextureEstPrive;
    [SerializeField] private Texture poiPanelTextureEstPublic;

    private MapService mapService;
    private float displayTileSize;
    private MapController mapController;
    private POIData poiData;
    private RectTransform rectTransform;
    private static GameObject activePOIPanel;

    /// <summary>
    /// Initialise la vue avec les données d'un POI
    /// </summary>
    public void SetData(POIData data, MapService mapService, MapController mapController, float displayTileSize)
    {
        this.poiData = data;
        this.mapService = mapService;
        this.mapController = mapController;
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
            nameText.text = "<b>Nom : </b>" + poiData.nom;

        if (descriptionText != null) {
            if (!string.IsNullOrEmpty(poiData.description)) 
            {
                descriptionText.text = "<b>Description : </b>" + poiData.description;
            } 
            else 
            {
                descriptionText.text = "<b>Description : </b><i> Aucune donnée </i>";
            }
        
        }
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

    public static void CloseActivePanel()
    {
        if (activePOIPanel != null)
        {
            Object.Destroy(activePOIPanel);
            activePOIPanel = null;
        }
    }

    public void ShowPOIPanel()
    {

        CloseActivePanel();

        activePOIPanel = Instantiate(poiPanelPrefab, transform);

        activePOIPanel.transform.localPosition = new Vector3(-150, 100, 0);
        activePOIPanel.transform.localScale = Vector3.one;

        nameText = activePOIPanel.transform.Find("ContentContainer/POIName").GetComponent<TextMeshProUGUI>();
        descriptionText = activePOIPanel.transform.Find("ContentContainer/POIDescription").GetComponent<TextMeshProUGUI>();
        UpdateDisplay();

        RawImage visibilityImage = activePOIPanel.transform.Find("POITopPanel/POIVisibilite")?.GetComponent<RawImage>();
        if (visibilityImage != null)
        {
            visibilityImage.texture = poiData.estPrive ? poiPanelTextureEstPrive : poiPanelTextureEstPublic;
        }

        activePOIPanel.transform.SetAsLastSibling();
    }

    /// <summary>
    /// Appelé quand l'utilisateur clique sur le marqueur
    /// </summary>
    public void OnMarkerClicked()
    {
        Debug.Log($"POI cliqué : {poiData.nom}");
        ShowPOIPanel();
        UpdateDisplay();
    }
}