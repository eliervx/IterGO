using UnityEngine;
using System;

/// <summary>
/// Service responsable de la logique de conversion GPS et tiles
/// Aucune dépendance aux GameObjects Unity
/// </summary>
public class MapService
{
    private int zoom;
    private Vector2 startTileCoords;

    public MapService(int initialZoom)
    {
        this.zoom = initialZoom;
    }

    /// <summary>
    /// Initialise les coordonnées de tile de départ
    /// </summary>
    public void Initialize(float latitude, float longitude)
    {
        startTileCoords = GPSToTile(latitude, longitude);
    }

    /// <summary>
    /// Convertit des coordonnées GPS en coordonnées de tuiles
    /// </summary>
    public Vector2 GPSToTile(float latitude, float longitude)
    {
        double n = Mathf.Pow(2, zoom);
        double latRad = latitude * Mathf.Deg2Rad;

        double x = n * ((longitude + 180) / 360);
        double y = n * (1 - (Mathf.Log(Mathf.Tan((float)latRad) + 1f / Mathf.Cos((float)latRad)) / Mathf.PI)) / 2;

        return new Vector2((float)x, (float)y);
    }

    public Vector2 TileToGPS(Vector2 tileCoords)
    {
        double n = Mathf.Pow(2, zoom);
        double lon = (tileCoords.x / n) * 360 - 180;
        double latRad = Mathf.Atan((float)System.Math.Sinh(Mathf.PI * (1 - 2 * tileCoords.y / n)));
        double lat = latRad * Mathf.Rad2Deg;

        return new Vector2((float)lat, (float)lon);
    }

    /// <summary>
    /// Obtient la position UI à partir de coordonnées GPS
    /// </summary>
    public Vector2 GetUIPositionFromGPS(float latitude, float longitude, float displayTileSize)
    {
        Vector2 tileCoords = GPSToTile(latitude, longitude);
        float deltaX = (tileCoords.x - startTileCoords.x) * displayTileSize;
        float deltaY = (tileCoords.y - startTileCoords.y) * displayTileSize;
        return new Vector2(deltaX, -deltaY);
    }

    /// <summary>
    /// Change le niveau de zoom
    /// </summary>
    public void SetZoom(int newZoom)
    {
        if (newZoom < 5) newZoom = 5;
        zoom = newZoom;
    }

    public int GetZoom() => zoom;
    public Vector2 GetStartTileCoords() => startTileCoords;
}
