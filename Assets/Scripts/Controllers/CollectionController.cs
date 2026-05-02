using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// CollectionController : Affiche tous les POIs créés par un utilisateur
/// </summary>
public class CollectionController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string userId; // ID de l'utilisateur connecté

    [Header("Références")]
    [SerializeField] private Transform poiListContainer; // le Content du ScrollView
    [SerializeField] private GameObject poiCardPrefab;   // prefab carte POI

    private FirestoreService firestoreService;

    void Start()
    {
        firestoreService = GetComponent<FirestoreService>() 
            ?? gameObject.AddComponent<FirestoreService>();

        LoadUserPOIs();
    }

    public void LoadUserPOIs()
    {
        // Vide la liste avant de recharger
        foreach (Transform child in poiListContainer)
            Destroy(child.gameObject);

        firestoreService.GetPOIsByUser(userId, OnPOIsLoaded);
    }

    private void OnPOIsLoaded(List<POIData> pois)
    {
        Debug.Log($"{pois.Count} POIs trouvés pour l'utilisateur {userId}");

        foreach (var poi in pois)
        {
            GameObject card = Instantiate(poiCardPrefab, poiListContainer);
            POICardView cardView = card.GetComponent<POICardView>();
            if (cardView != null)
                cardView.Setup(poi);
        }
    }
}