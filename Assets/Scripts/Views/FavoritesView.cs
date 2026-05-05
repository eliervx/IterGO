using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FavoritesView : MonoBehaviour
{
    public GameObject favoritesPanel;
    public Transform favoritesContent;
    public GameObject favoriteItemPrefab;
    public Button showFavoritesButton;

    private UserDataService userDataService;
    private FirestoreService firestoreService;
    private List<string> userFavorites = new List<string>();
    private Dictionary<string, POIData> poiCache = new Dictionary<string, POIData>();

    void Start()
    {
        userDataService = FindObjectOfType<UserDataService>();
        firestoreService = FindObjectOfType<FirestoreService>();

        if (showFavoritesButton != null)
        {
            showFavoritesButton.onClick.AddListener(ToggleFavoritesPanel);
        }

        // Load favorites when component starts
        LoadUserFavorites();
    }

    private void LoadUserFavorites()
    {
        if (userDataService == null) return;

        userDataService.GetUserFavorites(favorites => {
            userFavorites = favorites;
            RefreshFavoritesUI();
        });
    }

    private void RefreshFavoritesUI()
    {
        // Clear existing items
        foreach (Transform child in favoritesContent)
        {
            Destroy(child.gameObject);
        }

        // Load POI data for favorites
        firestoreService.GetPOIs(allPOIs => {
            poiCache.Clear();
            foreach (POIData poi in allPOIs)
            {
                poiCache[poi.id] = poi;
            }

            // Create UI items for favorites
            foreach (string poiId in userFavorites)
            {
                if (poiCache.ContainsKey(poiId))
                {
                    CreateFavoriteItem(poiCache[poiId]);
                }
            }
        });
    }

    private void CreateFavoriteItem(POIData poi)
    {
        GameObject item = Instantiate(favoriteItemPrefab, favoritesContent);
        TextMeshProUGUI nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = item.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        Button removeButton = item.transform.Find("RemoveButton")?.GetComponent<Button>();

        if (nameText != null)
            nameText.text = poi.nom;

        if (descText != null)
        {
            descText.text = string.IsNullOrEmpty(poi.description) ? "<i>Aucune description</i>" : poi.description;
        }

        if (removeButton != null)
        {
            removeButton.onClick.AddListener(() => RemoveFromFavorites(poi.id));
        }

        // Add click handler to focus on POI
        Button itemButton = item.GetComponent<Button>();
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(() => FocusOnPOI(poi));
        }
    }

    private void RemoveFromFavorites(string poiId)
    {
        userDataService.RemoveFromFavorites(poiId, (success, message) => {
            if (success)
            {
                userFavorites.Remove(poiId);
                RefreshFavoritesUI();
            }
            else
            {
                Debug.LogError("Error removing from favorites: " + message);
            }
        });
    }

    private void FocusOnPOI(POIData poi)
    {
        // TODO: Implement map focusing on POI
        Debug.Log("Focusing on POI: " + poi.nom);
    }

    private void ToggleFavoritesPanel()
    {
        favoritesPanel.SetActive(!favoritesPanel.activeSelf);
        if (favoritesPanel.activeSelf)
        {
            LoadUserFavorites();
        }
    }
}