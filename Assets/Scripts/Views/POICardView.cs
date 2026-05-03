using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class POICardView : MonoBehaviour
{
    private RawImage photoImage;
    private TMP_Text titleText;
    private TMP_Text descriptionText;

    void Awake()
    {
        photoImage = transform.Find("PhotoPOI").GetComponent<RawImage>();
        titleText = transform.Find("TextPOI/TitrePOI").GetComponent<TMP_Text>();
        descriptionText = transform.Find("TextPOI/DescriptionScroll/Viewport/Content/DescriptionPOI").GetComponent<TMP_Text>();
    }

    public void Setup(POIData poi)
    {
        if (titleText != null)
            titleText.text = poi.nom;

        if (descriptionText != null)
            descriptionText.text = poi.description;

        if (photoImage != null)
        {
            // Vérifie que c'est bien du base64 valide
            bool hasValidPhoto = !string.IsNullOrEmpty(poi.imageURLs) 
                            && poi.imageURLs != "['']"
                            && poi.imageURLs.Length > 100;

            if (hasValidPhoto)
            {
                try
                {
                    photoImage.texture = Base64ToTexture(poi.imageURLs);
                    photoImage.gameObject.SetActive(true);
                }
                catch
                {
                    photoImage.gameObject.SetActive(false);
                }
            }
            else
            {
                photoImage.gameObject.SetActive(false);
            }
        }
    }

    private Texture2D Base64ToTexture(string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        return tex;
    }

    void OnDestroy()
    {
        if (photoImage != null && photoImage.texture != null)
            Destroy(photoImage.texture);
    }
}