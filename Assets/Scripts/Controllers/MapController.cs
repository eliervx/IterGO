using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// MapController : Contrôleur responsable de la logique de gestion de la carte
/// - Gère les interactions utilisateur (drag, zoom)
/// - Orchestre les services (MapService, FirestoreService, LocationService)
/// - Communique avec les vues (UserMarkerView, POIMarkerView)
/// </summary>
public class MapController : MonoBehaviour, IDragHandler
{
    [Header("Configuration")]
    public GameObject tilePrefab;
    public GameObject poiMarkerPrefab;
    public int initialZoom = 6;
    public float displayTileSize = 512f;

    [Header("Références")]
    private RectTransform contentTransform;
    private FirestoreService firestoreService;
    private LocationService locationService;
    private MapService mapService;

    private Vector2 lastCenterTile;
    private Dictionary<Vector2, GameObject> spawnedTiles = new Dictionary<Vector2, GameObject>();
    private Dictionary<string, GameObject> spawnedPOIMarkers = new Dictionary<string, GameObject>();

    [SerializeField] private GameObject poiPanel;
    private GameObject activePOIPanel;

    IEnumerator Start()
    {
        // Initialisation des références
        contentTransform = GetComponent<RectTransform>();
        mapService = new MapService(initialZoom);

        // Récupérer ou créer les services
        firestoreService = GetComponent<FirestoreService>() ?? gameObject.AddComponent<FirestoreService>();
        locationService = FindObjectOfType<LocationService>() ?? new GameObject("LocationService").AddComponent<LocationService>();

        // Attendre l'initialisation des services
        yield return new WaitUntil(() => locationService.IsInitialized());

        // Initialiser la carte avec la position utilisateur
        UserData userLocation = locationService.GetCurrentLocation();
        mapService.Initialize(userLocation.latitude, userLocation.longitude);

        lastCenterTile = new Vector2(Mathf.Floor(mapService.GetStartTileCoords().x), Mathf.Floor(mapService.GetStartTileCoords().y));
        contentTransform.anchoredPosition = Vector2.zero;

        // Connection du slider de zoom
        Slider zoomSlider = GameObject.Find("ZoomSlider").GetComponent<Slider>();
        zoomSlider.onValueChanged.AddListener(OnZoomChanged);

        UpdateGrid();

        // Charger les POIs depuis Firestore
        firestoreService.GetPOIs(OnPOIsLoaded);
    }

    /// <summary>
    /// Callback appelé quand les POIs sont chargés depuis Firestore
    /// </summary>
    private void OnPOIsLoaded(List<POIData> pois)
    {
        foreach (var poi in pois)
        {
            SpawnPOIMarker(poi);
        }
    }

    /// <summary>
    /// Crée et affiche un marqueur POI
    /// </summary>
    private void SpawnPOIMarker(POIData poi)
    {
        if (!spawnedPOIMarkers.ContainsKey(poi.id))
        {
            GameObject marker = Instantiate(poiMarkerPrefab, contentTransform);
            Vector2 uiPos = mapService.GetUIPositionFromGPS(poi.latitude, poi.longitude, displayTileSize);
            marker.GetComponent<RectTransform>().anchoredPosition = uiPos;

            POIMarkerView poiView = marker.GetComponent<POIMarkerView>();
            if (poiView != null)
            {
                poiView.SetData(poi, mapService, this, displayTileSize);
            }

            // Placer le marqueur POI au-dessus des tuiles (et du marqueur utilisateur)
            marker.transform.SetAsLastSibling();

            spawnedPOIMarkers[poi.id] = marker;
        }
    }

    public void ShowPOIPanel(POIData poi)
    {
        Debug.Log($"Affichage du panneau pour le POI : {poi.name}");
        
        if (activePOIPanel != null)
            Destroy(activePOIPanel);
        
        // Instancier le panel en tant qu'enfant de MapContent
        activePOIPanel = Instantiate(poiPanel, contentTransform);
        
        activePOIPanel.transform.Find("ContentContainer/POIName").GetComponent<TextMeshProUGUI>().text = "<b>Nom : </b>" + poi.name;
        activePOIPanel.transform.Find("ContentContainer/POIDescription").GetComponent<TextMeshProUGUI>().text = "<b>Description : </b>" + poi.description;
        activePOIPanel.transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        contentTransform.anchoredPosition += eventData.delta;

        // On vérifie si on doit charger de nouvelles tuiles
        Vector2 currentPosTile = GetTileCoordsFromPosition();
        Vector2 discreteCenter = new Vector2(Mathf.Floor(currentPosTile.x), Mathf.Floor(currentPosTile.y));

        // Détruire le POI panel actif lors du déplacement
        if (activePOIPanel != null)
        {
            Destroy(activePOIPanel);
            activePOIPanel = null;
        }

        if (discreteCenter != lastCenterTile)
        {
            lastCenterTile = discreteCenter;
            UpdateGrid();
        }
    }

    public void OnZoomChanged(float newValue)
    {
        int newZoom = Mathf.RoundToInt(newValue);

        if (newZoom != mapService.GetZoom())
        {
            // 1. Sauvegarder la position GPS centrale actuelle avant de zoomer
            Vector2 currentCenterGPS = mapService.TileToGPS(GetTileCoordsFromPosition());

            // 2. Appliquer le zoom et réinitialiser la carte
            mapService.SetZoom(newZoom);
            mapService.Initialize(currentCenterGPS.x, currentCenterGPS.y);

            lastCenterTile = new Vector2(Mathf.Floor(mapService.GetStartTileCoords().x), Mathf.Floor(mapService.GetStartTileCoords().y));

            foreach (var tile in spawnedTiles.Values) Destroy(tile);
            spawnedTiles.Clear();

            contentTransform.anchoredPosition = Vector2.zero;
            UpdateGrid();

            // Réinitialiser le marqueur utilisateur avec les nouvelles coordonnées
            Transform userMarkerTransform = contentTransform.Find("UserLocationMarker");
            if (userMarkerTransform != null)
            {
                UserMarkerView userMarkerView = userMarkerTransform.GetComponent<UserMarkerView>();
                if (userMarkerView != null && locationService != null)
                {
                    userMarkerView.Initialize(locationService, mapService, displayTileSize);
                }
            }

            // Mettre à jour les positions des POIs
            foreach (var poiMarker in spawnedPOIMarkers.Values)
            {
                var poiView = poiMarker.GetComponent<POIMarkerView>();
                if (poiView != null)
                {
                    Vector2 newPos = mapService.GetUIPositionFromGPS(poiView.GetData().latitude, poiView.GetData().longitude, displayTileSize);
                    poiMarker.GetComponent<RectTransform>().anchoredPosition = newPos;
                    
                    // Assurer que les POIs restent au-dessus des tuiles
                    poiMarker.transform.SetAsLastSibling();
                }
            }
        }
    }

    private void UpdateGrid()
    {
        int range = 2; // Grille 5x5
        Vector2 startTileCoords = mapService.GetStartTileCoords();
        
        for (int x = (int)lastCenterTile.x - range; x <= (int)lastCenterTile.x + range; x++)
        {
            for (int y = (int)lastCenterTile.y - range; y <= (int)lastCenterTile.y + range; y++)
            {
                Vector2 target = new Vector2(x, y);
                if (!spawnedTiles.ContainsKey(target))
                {
                    CreateTile(target);
                }
            }
        }
        CleanupTiles();
    }

    private void CreateTile(Vector2 coords)
    {
        GameObject go = Instantiate(tilePrefab, contentTransform);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(displayTileSize, displayTileSize);

        // Positionnement relatif à startTileCoords
        Vector2 startTileCoords = mapService.GetStartTileCoords();
        float posX = (coords.x - startTileCoords.x) * displayTileSize;
        float posY = (coords.y - startTileCoords.y) * displayTileSize;

        rt.anchoredPosition = new Vector2(posX, -posY);
        spawnedTiles.Add(coords, go);

        // Maintenir l'ordre de superposition correct :
        // 1. Tuiles (en bas)
        // 2. UserMarker
        // 3. POIs (en haut)
        Transform userMarker = contentTransform.Find("UserLocationMarker");
        if (userMarker != null) userMarker.SetAsLastSibling();

        // Remettre les POIs au-dessus
        foreach (var poiMarker in spawnedPOIMarkers.Values)
        {
            poiMarker.transform.SetAsLastSibling();
        }

        StartCoroutine(DownloadTile(coords, go.GetComponent<RawImage>()));
    }

    private IEnumerator DownloadTile(Vector2 c, RawImage img)
    {
        // On utilise Floor pour l'URL car OSM veut des entiers
        string url = $"https://tile.openstreetmap.org/{mapService.GetZoom()}/{(int)c.x}/{(int)c.y}.png";
        // Debug.Log(url);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        request.SetRequestHeader("User-Agent", "Unity-IterGO-POC");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            img.texture = DownloadHandlerTexture.GetContent(request);
        }
    }

    private void CleanupTiles()
    {
        List<Vector2> toDestroy = new List<Vector2>();
        foreach (var tile in spawnedTiles)
        {
            if (Vector2.Distance(tile.Key, lastCenterTile) > 4)
                toDestroy.Add(tile.Key);
        }
        foreach (Vector2 key in toDestroy)
        {
            Destroy(spawnedTiles[key]);
            spawnedTiles.Remove(key);
        }
    }

    private Vector2 GetTileCoordsFromPosition()
    {
        Vector2 startTileCoords = mapService.GetStartTileCoords();
        float offsetX = -contentTransform.anchoredPosition.x / displayTileSize;
        float offsetY = contentTransform.anchoredPosition.y / displayTileSize;
        return new Vector2(startTileCoords.x + offsetX, startTileCoords.y + offsetY);
    }
}