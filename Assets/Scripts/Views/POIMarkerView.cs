using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private Button favoriteButton;
    [SerializeField] private Button commentButton;
    [SerializeField] private Button reportButton;
    private MapService mapService;
    private float displayTileSize;
    private MapController mapController;
    private POIData poiData;
    private RectTransform rectTransform;
    private static GameObject activePOIPanel;
    private AuthService authService;
    private UserDataService userDataService;

    public void SetData(POIData data, MapService mapService, MapController mapController, float displayTileSize)
    {
        this.poiData = data;
        this.mapService = mapService;
        this.mapController = mapController;
        this.displayTileSize = displayTileSize;
        this.authService = FindObjectOfType<AuthService>();
        this.userDataService = FindObjectOfType<UserDataService>();
        
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
        if (poiPanelPrefab == null)
        {
            Debug.LogWarning("POI panel prefab is not assigned on POIMarkerView.");
            return;
        }

        CloseActivePanel();

        if (transform.parent == null)
        {
            Debug.LogWarning("Cannot instantiate POI panel because marker has no parent transform.");
            return;
        }

        activePOIPanel = Instantiate(poiPanelPrefab, transform.parent, false);
        RectTransform panelRect = activePOIPanel.GetComponent<RectTransform>();

        if (panelRect != null && rectTransform != null)
        {
            Canvas rootCanvas = activePOIPanel.GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
            if (rootCanvas != null)
            {
                activePOIPanel.transform.SetParent(rootCanvas.transform, false);
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(rootCanvas.worldCamera, rectTransform.position);
                RectTransform canvasRect = rootCanvas.transform as RectTransform;
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, rootCanvas.worldCamera, out localPoint);

                Vector2 preferredPosition = localPoint + new Vector2(0, rectTransform.rect.height * 1.5f);
                panelRect.anchoredPosition = ClampToRect(preferredPosition, panelRect, canvasRect);
            }
            else
            {
                panelRect.anchoredPosition = rectTransform.anchoredPosition + new Vector2(0, rectTransform.rect.height * 1.5f);
            }
        }

        activePOIPanel.transform.Find("ContentContainer/Viewport/Content/POIName").GetComponent<TextMeshProUGUI>().text = "<b>Nom : </b>" + poiData.nom;
        activePOIPanel.transform.Find("ContentContainer/Viewport/Content/POIDescription").GetComponent<TextMeshProUGUI>().text = "<b>Description : </b>" + poiData.description;

        RawImage visibilityImage = activePOIPanel.transform.Find("POITopPanel/POIVisibilite")?.GetComponent<RawImage>();
        if (visibilityImage != null)
        {
            visibilityImage.texture = poiData.estPrive ? poiPanelTextureEstPrive : poiPanelTextureEstPublic;
        }

        // Configure buttons
        if (favoriteButton != null)
        {
            favoriteButton.onClick.AddListener(OnFavoriteClick);
            UpdateFavoriteButton();
        }

        if (commentButton != null)
        {
            commentButton.onClick.AddListener(OnCommentClick);
        }

        if (reportButton != null)
        {
            reportButton.onClick.AddListener(OnReportClick);
        }

        activePOIPanel.transform.SetAsLastSibling();
    }

    private Vector2 ClampToRect(Vector2 position, RectTransform childRect, RectTransform parentRect)
    {
        Vector2 minPosition = new Vector2(
            parentRect.rect.xMin + childRect.rect.width * childRect.pivot.x,
            parentRect.rect.yMin + childRect.rect.height * childRect.pivot.y
        );

        Vector2 maxPosition = new Vector2(
            parentRect.rect.xMax - childRect.rect.width * (1 - childRect.pivot.x),
            parentRect.rect.yMax - childRect.rect.height * (1 - childRect.pivot.y)
        );

        return new Vector2(
            Mathf.Clamp(position.x, minPosition.x, maxPosition.x),
            Mathf.Clamp(position.y, minPosition.y, maxPosition.y)
        );
    }

    private void OnFavoriteClick()
    {
        if (userDataService == null) return;

        if (poiData.isLikedByCurrentUser)
        {
            userDataService.RemoveFromFavorites(poiData.id, (success, message) => {
                if (success)
                {
                    poiData.isLikedByCurrentUser = false;
                    poiData.likesCount--;
                    UpdateFavoriteButton();
                }
                else
                {
                    Debug.LogError("Error removing from favorites: " + message);
                }
            });
        }
        else
        {
            userDataService.AddToFavorites(poiData.id, (success, message) => {
                if (success)
                {
                    poiData.isLikedByCurrentUser = true;
                    poiData.likesCount++;
                    UpdateFavoriteButton();
                }
                else
                {
                    Debug.LogError("Error adding to favorites: " + message);
                }
            });
        }
    }

    private void UpdateFavoriteButton()
    {
        if (favoriteButton == null) return;

        TextMeshProUGUI buttonText = favoriteButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = poiData.isLikedByCurrentUser ? "❤️ Favoris" : "🤍 Ajouter aux favoris";
        }
    }

    private void OnCommentClick()
    {
        Debug.Log("Comment button clicked - TODO: Open comments panel");
        // TODO: Implement comments UI
    }

    private void OnReportClick()
    {
        if (userDataService == null) return;

        userDataService.ReportPOI(poiData.id, "Contenu inapproprié", (success, message) => {
            if (success)
            {
                Debug.Log("POI reported successfully");
                // TODO: Show success message to user
            }
            else
            {
                Debug.LogError("Error reporting POI: " + message);
            }
        });
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