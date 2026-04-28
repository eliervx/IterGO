using UnityEngine;

public class UserMarker : MonoBehaviour
{
    private InfiniteMap mapScript;
    private RectTransform rectTransform;
    private float displayTileSize = 512f; 

    void Start()
    {
        mapScript = GetComponentInParent<InfiniteMap>();
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // if (Input.location.status == LocationServiceStatus.Running)
        // {
        //     float uLat = Input.location.lastData.latitude;
        //     float uLon = Input.location.lastData.longitude;

        //     float worldX = (uLon + 180.0f) / 360.0f * Mathf.Pow(2, mapScript.zoom);
        //     float latRad = uLat * Mathf.PI / 180.0f;
        //     float worldY = (1.0f - Mathf.Log(Mathf.Tan(latRad) + 1.0f / Mathf.Cos(latRad)) / Mathf.PI) / 2.0f * Mathf.Pow(2, mapScript.zoom);

        //     float localX = (worldX - mapScript.startTileCoords.x) * displayTileSize;
        //     float localY = (worldY - mapScript.startTileCoords.y) * displayTileSize;

        //     rectTransform.anchoredPosition = new Vector2(localX, -localY);
        // }
        // TEST 
        float uLat = 48.8584f;
        float uLon = 2.2945f;

        // 2. Calcul du worldX/Y en FLOAT (très important pour ne pas saccader)
        Vector2 startTileCoords = GetTileCoords(uLat, uLon);

        // 3. Écart par rapport à la tuile de référence (startTileCoords)
        float deltaX = (startTileCoords.x - mapScript.startTileCoords.x) * displayTileSize;
        float deltaY = (startTileCoords.y - mapScript.startTileCoords.y) * displayTileSize;

        rectTransform.anchoredPosition = new Vector2(deltaX, -deltaY);
    }

    Vector2 GetTileCoords(float lat, float lon) {
        float n = Mathf.Pow(2, mapScript.zoom);
        float x = (lon + 180.0f) / 360.0f * n;
        float latRad = lat * Mathf.PI / 180.0f;
        float y = (1.0f - Mathf.Log(Mathf.Tan(latRad) + 1.0f / Mathf.Cos(latRad)) / Mathf.PI) / 2.0f * n;
        return new Vector2(x, y);
    }
}