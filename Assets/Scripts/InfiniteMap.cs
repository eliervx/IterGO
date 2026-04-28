using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

public class InfiniteMap : MonoBehaviour, IDragHandler {
    public GameObject tilePrefab; 
    public int zoom = 6;
    private float displayTileSize = 512f;
    
    public float currentLat = 48.8584f;
    public float currentLon = 2.2945f;
    
    [HideInInspector] public Vector2 startTileCoords; // Coordonnées flottantes de la tuile centrale

    private Vector2 lastCenterTile;
    private RectTransform contentTransform;
    private Dictionary<Vector2, GameObject> spawnedTiles = new Dictionary<Vector2, GameObject>();

    IEnumerator Start() {
        contentTransform = GetComponent<RectTransform>();
        
        // Initialisation GPS
        if (Input.location.isEnabledByUser) {
            Input.location.Start();
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
                yield return new WaitForSeconds(1);
                maxWait--;
            }
            if (Input.location.status == LocationServiceStatus.Running) {
                currentLat = Input.location.lastData.latitude;
                currentLon = Input.location.lastData.longitude;
            }
        }

        // Calcul du point de référence au lancement
        startTileCoords = GPSToTile(currentLat, currentLon); 
        lastCenterTile = new Vector2(Mathf.Floor(startTileCoords.x), Mathf.Floor(startTileCoords.y));
        
        contentTransform.anchoredPosition = Vector2.zero; 
        UpdateGrid();
    }

    public void OnDrag(PointerEventData eventData) {
        contentTransform.anchoredPosition += eventData.delta;

        // On vérifie si on doit charger de nouvelles tuiles
        Vector2 currentPosTile = GetTileCoordsFromPosition();
        Vector2 discreteCenter = new Vector2(Mathf.Floor(currentPosTile.x), Mathf.Floor(currentPosTile.y));

        if (discreteCenter != lastCenterTile) {
            lastCenterTile = discreteCenter;
            UpdateGrid();
        }
    }

    public void OnZoomChanged(float newValue) {
        int newZoom = Mathf.RoundToInt(newValue);
        if (newZoom < 5) newZoom = 5;

        if (newZoom != zoom) {
            // 1. Sauvegarder la position GPS centrale actuelle avant de zoomer
            Vector2 currentCenterGPS = TileToGPS(GetTileCoordsFromPosition());
            
            zoom = newZoom;
            currentLat = currentCenterGPS.x;
            currentLon = currentCenterGPS.y;

            // 2. Reset
            startTileCoords = GPSToTile(currentLat, currentLon);
            lastCenterTile = new Vector2(Mathf.Floor(startTileCoords.x), Mathf.Floor(startTileCoords.y));

            foreach (var tile in spawnedTiles.Values) Destroy(tile);
            spawnedTiles.Clear();
            
            contentTransform.anchoredPosition = Vector2.zero;
            UpdateGrid();
        }
    }

    void UpdateGrid() {
        int range = 2; // Grille 5x5
        for (int x = (int)lastCenterTile.x - range; x <= (int)lastCenterTile.x + range; x++) {
            for (int y = (int)lastCenterTile.y - range; y <= (int)lastCenterTile.y + range; y++) {
                Vector2 target = new Vector2(x, y);
                if (!spawnedTiles.ContainsKey(target)) {
                    CreateTile(target);
                }
            }
        }
        CleanupTiles();
    }

    void CreateTile(Vector2 coords) {
        GameObject go = Instantiate(tilePrefab, transform);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(displayTileSize, displayTileSize);

        // Positionnement relatif à startTileCoords
        float posX = (coords.x - startTileCoords.x) * displayTileSize;
        float posY = (coords.y - startTileCoords.y) * displayTileSize;
        
        // Comme le pivot est à 0.5, (0,0) est le centre. Pas besoin d'offset manuel ici, 
        //posX et posY sont déjà des écarts par rapport au centre.
        rt.anchoredPosition = new Vector2(posX, -posY);
        
        spawnedTiles.Add(coords, go);

        // Maintenir le marqueur au dessus
        Transform userMarker = transform.Find("UserLocationMarker");
        if (userMarker != null) userMarker.SetAsLastSibling();

        StartCoroutine(DownloadTile(coords, go.GetComponent<RawImage>()));
    }

    IEnumerator DownloadTile(Vector2 c, RawImage img) {
        // On utilise Floor pour l'URL car OSM veut des entiers
        string url = $"https://tile.openstreetmap.org/{zoom}/{(int)c.x}/{(int)c.y}.png";
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        request.SetRequestHeader("User-Agent", "Unity-IterGO-POC");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            img.texture = DownloadHandlerTexture.GetContent(request);
        }
    }

    void CleanupTiles() {
        List<Vector2> toDestroy = new List<Vector2>();
        foreach (var tile in spawnedTiles) {
            if (Vector2.Distance(tile.Key, lastCenterTile) > 4) toDestroy.Add(tile.Key);
        }
        foreach (Vector2 key in toDestroy) {
            Destroy(spawnedTiles[key]);
            spawnedTiles.Remove(key);
        }
    }

    // --- Fonctions Mathématiques ---

    Vector2 GPSToTile(float lat, float lon) {
        float n = Mathf.Pow(2, zoom);
        float x = (lon + 180.0f) / 360.0f * n;
        float latRad = lat * Mathf.PI / 180.0f;
        float y = (1.0f - Mathf.Log(Mathf.Tan(latRad) + 1.0f / Mathf.Cos(latRad)) / Mathf.PI) / 2.0f * n;
        return new Vector2(x, y);
    }

    Vector2 TileToGPS(Vector2 tile) {
        float n = Mathf.Pow(2, zoom);
        float lon = tile.x / n * 360.0f - 180.0f;
        float latRad = Mathf.Atan((float)Math.Sinh(Mathf.PI * (1 - 2 * tile.y / n)));
        float lat = latRad * 180.0f / Mathf.PI;
        return new Vector2(lat, lon);
    }

    Vector2 GetTileCoordsFromPosition() {
        float offsetX = -contentTransform.anchoredPosition.x / displayTileSize;
        float offsetY = contentTransform.anchoredPosition.y / displayTileSize;
        return new Vector2(startTileCoords.x + offsetX, startTileCoords.y + offsetY);
    }
}