using UnityEngine;
using System.Collections.Generic;

public class CollectionController : MonoBehaviour
{
    [SerializeField] private Transform poiListContainer;
    [SerializeField] private GameObject poiCardPrefab;

    private FirestoreService firestoreService;

    void Start()
    {
        firestoreService = GetComponent<FirestoreService>()
            ?? gameObject.AddComponent<FirestoreService>();

        LoadUserEntries();
    }

    public void LoadUserEntries()
    {
        foreach (Transform child in poiListContainer)
            Destroy(child.gameObject);

        // UserSession.UserId retourne "1" pour l'instant
        // Quand tu auras le login, ça retournera automatiquement le bon ID
        firestoreService.GetUserEntries(UserSession.UserId, OnEntriesLoaded);
    }

    private void OnEntriesLoaded(List<POIData> entries)
    {
        foreach (var poi in entries)
        {
            GameObject card = Instantiate(poiCardPrefab, poiListContainer);
            POICardView cardView = card.GetComponent<POICardView>();
            if (cardView != null)
                cardView.Setup(poi);
        }
    }
}