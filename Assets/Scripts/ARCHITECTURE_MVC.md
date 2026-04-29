# Architecture MVC Refactorisée - Guide d'Intégration

## Vue d'ensemble de la nouvelle architecture

### Structure des dossiers
```
Assets/Scripts/
├── Controllers/
│   └── MapController.cs         (Orchestration & Interactions)
├── Models/
│   └── POIData.cs              (Modèles de données)
├── Views/
│   ├── UserMarkerView.cs       (Affichage marqueur utilisateur)
│   └── POIMarkerView.cs        (Affichage marqueur POI)
└── Services/
    ├── MapService.cs           (Logique de conversion GPS/Tile)
    ├── FirestoreService.cs     (Récupération données Firestore)
    └── LocationService.cs      (Gestion de la localisation)
```

## Principes MVC respectés

### Model (Models/)
- **POIData.cs** : Structures de données pures
  - `POIData` : Représente un POI
  - `UserData` : Représente l'utilisateur
  - Classes de sérialisation Firestore

### View (Views/)
- **UserMarkerView.cs** : Vue du marqueur utilisateur
  - Affiche uniquement
  - S'abonne aux mises à jour de localisation
  - Pas de logique métier

- **POIMarkerView.cs** : Vue d'un marqueur POI
  - Affiche les données du POI
  - Gère les interactions utilisateur (click)
  - Pas de logique métier

### Controller (Controllers/)
- **MapController.cs** : Contrôleur principal
  - Gère les interactions (drag, zoom)
  - Orchestre les services
  - Communique avec les vues
  - Pas de calculs mathématiques (délégués au MapService)

### Services (Services/)
- **MapService.cs** : Service métier
  - Conversion GPS ↔ Tiles
  - Calcul de positions UI
  - Gestion du zoom

- **FirestoreService.cs** : Service de données
  - Récupération asynchrone des POIs
  - Callback avec List<POIData>
  - Aucune instanciation de GameObject

- **LocationService.cs** : Service de localisation
  - Initialisation GPS
  - Mises à jour continues
  - Events publics pour s'abonner

## Points clés de la refactorisation

### Avant (Problèmes)
- UserMarker faisait des calculs GPS à chaque Update
- POIController mélange données et affichage
- MapController faisait trop de choses
- FirestoreReader instanciait des GameObjects

### Après (Solutions)
1. **Séparation claire** : Chaque classe a une responsabilité unique
2. **Services sans GameObject** : Logique métier indépendante de Unity
3. **Événements** : Communication découplée entre composants
4. **Testabilité** : Les services peuvent être testés sans Unity

## Configuration dans l'éditeur Unity

### 1. MapController (sur le GameObject de la carte)
```
- Ajouter le composant FirestoreService
- Assigner les références :
  - Tile Prefab
  - POI Marker Prefab
  - Initial Zoom : 6
  - Display Tile Size : 512
```

### 2. LocationService (GameObject global)
```
- Créer un GameObject "LocationService"
- Ajouter le composant LocationService
- (Sera automatiquement créé s'il n'existe pas)
```

### 3. UserMarkerView (sur le marqueur utilisateur)
```
- Ajouter le composant UserMarkerView
- Dans MapController.Start(), appeler :
  UserMarkerView view = userMarker.GetComponent<UserMarkerView>();
  view.Initialize(locationService, mapService, displayTileSize);
```

### 4. POIMarkerView (sur le préfab de marqueur POI)
```
- Renommer le composant POIController en POIMarkerView
- Assigner les références TextMeshPro:
  - Name Text
  - Description Text
```
