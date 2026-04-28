using UnityEngine;
using UnityEngine.UI;

public class MiniatureItem : MonoBehaviour
{
    [SerializeField] private Button deleteButton;
    private Texture2D texture;
    private LiveFeed LiveFeed;

    public void Init(Texture2D tex, LiveFeed liveFeed)
    {
        texture = tex;
        LiveFeed = liveFeed;
        deleteButton.onClick.AddListener(OnDelete);

        RawImage rawImage = GetComponent<RawImage>();
        if (rawImage != null)
            rawImage.texture = tex;
    }

    private void OnDelete()
    {
        LiveFeed.RemoveThumbnail(this, texture);
    }
}