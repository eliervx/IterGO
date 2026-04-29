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
    // Coordonnées GPS de départ (ex: Paris pour le test)
    public float currentLat = 48.8584f;
    public float currentLon = 2.2945f;
    public Vector2Int startTileCoords;

    private Vector2Int lastCenterTile;
    private RectTransform contentTransform;
    private Dictionary<Vector2Int, GameObject> spawnedTiles = new Dictionary<Vector2Int, GameObject>();

    IEnumerator Start() {
        Debug.Log("Démarrage de la map...");
        contentTransform = GetComponent<RectTransform>();
        if (!Input.location.isEnabledByUser) {
            Debug.Log("GPS désactivé par l'utilisateur");
        } else {
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

        startTileCoords = GetTileCoords(currentLat, currentLon); // Point de reference
        lastCenterTile = startTileCoords;
        contentTransform.anchoredPosition = Vector2.zero; // Remise à zéro de la position 

        // Debug.Log($"Tuile centrale calculée : X={lastCenterTile.x}, Y={lastCenterTile.y}");

        UpdateGrid();
    }

    // Gère le déplacement à la main
    public void OnDrag(PointerEventData eventData) {
        contentTransform.anchoredPosition += eventData.delta * 2.0f;

        int offsetX = Mathf.RoundToInt(-contentTransform.anchoredPosition.x / displayTileSize);
        int offsetY = Mathf.RoundToInt(contentTransform.anchoredPosition.y / displayTileSize);
        
        Vector2Int newCenter = new Vector2Int(startTileCoords.x + offsetX, startTileCoords.y + offsetY);

        if (newCenter != lastCenterTile) {
            lastCenterTile = newCenter;
            UpdateGrid();
        }
    }

    public void OnZoomChanged(float newValue) {
        int newZoom = Mathf.RoundToInt(newValue);
        
        if (newZoom < 5) newZoom = 5;

        if (newZoom != zoom) {
            float offsetX = -contentTransform.anchoredPosition.x / displayTileSize;
            float offsetY = contentTransform.anchoredPosition.y / displayTileSize;
            
            float exactTileX = startTileCoords.x + offsetX;
            float exactTileY = startTileCoords.y + offsetY;
            
            Vector2 gpsCoords = TileToGPSExact(exactTileX, exactTileY, zoom);
            zoom = newZoom;
            
            currentLat = gpsCoords.x;
            currentLon = gpsCoords.y;
            startTileCoords = GetTileCoords(currentLat, currentLon);
            lastCenterTile = startTileCoords;

            foreach (var tile in spawnedTiles.Values) {
                Destroy(tile);
            }
            spawnedTiles.Clear();
            contentTransform.anchoredPosition = Vector2.zero;

            // Debug.Log($"Zoom {zoom} | GPS précis: {currentLat:F6}, {currentLon:F6}");
            UpdateGrid();
        }
    }

    void UpdateGrid() {
        // On vérifie une zone de 5x5 autour de la tuile centrale actuelle
        for (int x = lastCenterTile.x - 2; x <= lastCenterTile.x + 2; x++) {
            for (int y = lastCenterTile.y - 2; y <= lastCenterTile.y + 2; y++) {
                Vector2Int target = new Vector2Int(x, y);
                if (!spawnedTiles.ContainsKey(target)) {
                    CreateTile(target);
                }
            }
        }
        CleanupTiles();
    }

    void CleanupTiles()
    {
        // Liste temp pour stocker les clés à supprimer
        List<Vector2Int> toDestroy = new List<Vector2Int>();

        foreach (var tile in spawnedTiles)
        {
            if (Vector2Int.Distance(tile.Key, lastCenterTile) > 3)
            {
                toDestroy.Add(tile.Key);
            }
        }

        foreach (Vector2Int key in toDestroy)
        {
            Destroy(spawnedTiles[key]); // Détruit l'objet dans la scène
            spawnedTiles.Remove(key);
        }
    }

    void CreateTile(Vector2Int coords) {
        GameObject go = Instantiate(tilePrefab, transform);
        RectTransform rt = go.GetComponent<RectTransform>();
        
        rt.sizeDelta = new Vector2(displayTileSize, displayTileSize);

        float posX = (coords.x - startTileCoords.x) * displayTileSize;
        float posY = (coords.y - startTileCoords.y) * displayTileSize;
        
        rt.anchoredPosition = new Vector2(posX, -posY);
        
        spawnedTiles.Add(coords, go);
        StartCoroutine(DownloadTile(coords, go.GetComponent<RawImage>()));
    }

    IEnumerator DownloadTile(Vector2Int c, RawImage img) {
        string url = $"https://tile.openstreetmap.org/{zoom}/{c.x}/{c.y}.png";
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        // Debug.Log(url);
        request.SetRequestHeader("User-Agent", "Unity-IterGO-POC");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            img.texture = DownloadHandlerTexture.GetContent(request);
        }
    }

    // --- Fonctions utilitaires ---
    Vector2Int GetTileCoords(float lat, float lon) {
        int x = (int)((lon + 180.0f) / 360.0f * Mathf.Pow(2, zoom));
        int y = (int)((1.0f - Mathf.Log(Mathf.Tan(lat * Mathf.PI / 180.0f) + 1.0f / Mathf.Cos(lat * Mathf.PI / 180.0f)) / Mathf.PI) / 2.0f * Mathf.Pow(2, zoom));
        return new Vector2Int(x, y);
    }

    Vector2Int GetTileCoordsFromPosition() {
        // On calcule le décalage actuel par rapport à la tuile de départ
        int offsetX = Mathf.RoundToInt(-contentTransform.anchoredPosition.x / displayTileSize);
        int offsetY = Mathf.RoundToInt(contentTransform.anchoredPosition.y / displayTileSize);
        
        return new Vector2Int(startTileCoords.x + offsetX, startTileCoords.y + offsetY);
    }

    Vector2 TileToGPSExact(float tileX, float tileY, int zoomLevel) {
        float n = Mathf.Pow(2, zoomLevel);
        
        float lon = tileX / n * 360.0f - 180.0f;
        
        float lat_rad = Mathf.Atan((float)Math.Sinh(Mathf.PI * (1 - 2 * tileY / n)));
        float lat = lat_rad * 180.0f / Mathf.PI;
        
        return new Vector2(lat, lon);
    }
}