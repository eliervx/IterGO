using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CaptureManager : MonoBehaviour
{
    [Header("Les 4 panels noirs")]
    [SerializeField] private RectTransform panelTop;
    [SerializeField] private RectTransform panelBottom;
    [SerializeField] private RectTransform panelLeft;
    [SerializeField] private RectTransform panelRight;

    [Header("Miniatures")]
    [SerializeField] private Transform thumbnailContainer;
    [SerializeField] private GameObject thumbnailPrefab;
    [SerializeField] private int maxThumbnails = 10;

    private List<Texture2D> capturedTextures = new List<Texture2D>();

    // ─────────────────────────────────────────────
    // BOUTON
    // ─────────────────────────────────────────────

    public void TakePhoto()
    {
        Debug.Log("TakePhoto appelé !");
        StartCoroutine(CaptureCoroutine());
    }

    // ─────────────────────────────────────────────
    // CALCUL DE LA ZONE DU CARRÉ
    // ─────────────────────────────────────────────

    private Rect GetCaptureRect()
    {
        Vector3[] cornersTop    = new Vector3[4];
        Vector3[] cornersBottom = new Vector3[4];
        Vector3[] cornersLeft   = new Vector3[4];
        Vector3[] cornersRight  = new Vector3[4];

        panelTop.GetWorldCorners(cornersTop);
        panelBottom.GetWorldCorners(cornersBottom);
        panelLeft.GetWorldCorners(cornersLeft);
        panelRight.GetWorldCorners(cornersRight);

        // La zone visible est l'espace entre les 4 panels
        float x      = cornersLeft[2].x;
        float y      = cornersBottom[1].y;
        float width  = cornersRight[0].x - x;
        float height = cornersTop[0].y - y;

        Debug.Log($"Zone capture : x={x} y={y} w={width} h={height}");

        return new Rect(x, y, width, height);
    }

    // ─────────────────────────────────────────────
    // CAPTURE
    // ─────────────────────────────────────────────

    private IEnumerator CaptureCoroutine()
    {
        yield return new WaitForEndOfFrame();

        Rect zone = GetCaptureRect();

        // Sécurité : si la zone est invalide on capture tout l'écran
        if (zone.width <= 0 || zone.height <= 0)
        {
            Debug.LogWarning("Zone invalide, capture de tout l'écran.");
            zone = new Rect(0, 0, Screen.width, Screen.height);
        }

        Texture2D screenshot = new Texture2D((int)zone.width, (int)zone.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(zone, 0, 0);
        screenshot.Apply();

        // Sauvegarde sur disque
        string fileName = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string path = Path.Combine(Application.persistentDataPath, fileName);
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Photo sauvegardée : {path}");

        // Affiche la miniature
        AddThumbnail(screenshot);
    }

    // ─────────────────────────────────────────────
    // MINIATURES
    // ─────────────────────────────────────────────

    private void AddThumbnail(Texture2D texture)    
    {
        // Supprime la plus ancienne si on dépasse la limite
        if (capturedTextures.Count >= maxThumbnails)
        {
            Texture2D oldest = capturedTextures[0];
            capturedTextures.RemoveAt(0);

            if (thumbnailContainer.childCount > 0)
            {
                GameObject oldThumb = thumbnailContainer.GetChild(0).gameObject;
                RawImage oldImage = oldThumb.GetComponent<RawImage>();
                if (oldImage != null && oldImage.texture != null)
                    Destroy(oldImage.texture);
                Destroy(oldThumb);
            }

            Destroy(oldest);
            Resources.UnloadUnusedAssets();
        }

        // Crée la miniature
        GameObject thumb = Instantiate(thumbnailPrefab, thumbnailContainer);
        RawImage rawImage = thumb.GetComponent<RawImage>();
        if (rawImage != null)
        {
            rawImage.texture = texture;
        }

        capturedTextures.Add(texture);
    }

    // ─────────────────────────────────────────────
    // NETTOYAGE MÉMOIRE
    // ─────────────────────────────────────────────

    void OnDestroy()
    {
        foreach (Texture2D tex in capturedTextures)
            Destroy(tex);
        capturedTextures.Clear();
    }
}