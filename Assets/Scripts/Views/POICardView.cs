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
        descriptionText = transform.Find("TextPOI/DescriptionPOI").GetComponent<TMP_Text>();
    }

    public void Setup(POIData poi)
    {
        titleText.text = poi.name;
        descriptionText.text = poi.description;

        if (!string.IsNullOrEmpty(poi.photoBase64))
        {
            photoImage.gameObject.SetActive(true);
            photoImage.texture = Base64ToTexture(poi.photoBase64);
        }
        else
        {
            photoImage.gameObject.SetActive(false);
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