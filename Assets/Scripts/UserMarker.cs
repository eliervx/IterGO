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
        // On vérifie si le service de localisation est actif
        if (Input.location.status == LocationServiceStatus.Running)
        {
            float uLat = Input.location.lastData.latitude;
            float uLon = Input.location.lastData.longitude;

            // 1. Conversion GPS -> Coordonnées de tuile "World"
            float worldX = (uLon + 180.0f) / 360.0f * Mathf.Pow(2, mapScript.zoom);
            float latRad = uLat * Mathf.PI / 180.0f;
            float worldY = (1.0f - Mathf.Log(Mathf.Tan(latRad) + 1.0f / Mathf.Cos(latRad)) / Mathf.PI) / 2.0f * Mathf.Pow(2, mapScript.zoom);

            // 2. Calcul de la position relative par rapport à la tuile de départ
            // On utilise displayTileSize pour que ça colle à tes tuiles agrandies
            float localX = (worldX - mapScript.startTileCoords.x) * displayTileSize;
            float localY = (worldY - mapScript.startTileCoords.y) * displayTileSize;

            // 3. Application de la position
            rectTransform.anchoredPosition = new Vector2(localX, -localY);
        }
    }
}