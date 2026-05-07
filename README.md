# IterGO

IterGO est une application mobile de réalité augmentée permettant de visualiser le modèle 3D de monuments en France. Les utilisateurs peuvent photographier des lieux, les géolocaliser et les rendre accessibles à tous via une carte interactive.

## Prérequis

- Téléphone Android (iOS non pris en charge)
- Accès à la caméra et à la localisation

Pour le build :
- Unity 6000.4.4f1
- Android SDK API 36
- ARCore XR Plugin

---

## I. Lancement de l'application

Pour lancer l'application sur téléphone Android :

1. Télécharger le fichier `IterGO-V0.9.apk` sur votre téléphone
2. Ouvrir le fichier APK — Android peut demander d'autoriser l'installation depuis des sources inconnues, acceptez
3. Une fois installé, lancez **IterGO**
4. Accordez les permissions suivantes si demandées :
   - Accès à l'appareil photo
   - Accès à la localisation

---

## II. Build de l'application

Pour compiler l'application en APK (compatible Android) :

1. Ouvrir le projet dans **Unity 6000.4.4f1**
2. Aller dans **File → Build Profiles**
3. Dans l'onglet **Scene List**, vérifier que les scènes sont dans cet ordre :

| Scène | Index |
|---|---|
| Scenes/MapScene | 0 |
| Scenes/ScanScene | 1 |
| Scenes/CollectionScene | 2 |
| Scenes/SpotScene | 3 |

4. Dans l'onglet **Android™** : vérifier la Scene List puis cliquer sur **Build**
5. Installer l'APK généré sur le téléphone (voir section I)

## III. Fonctionnalités

### Carte interactive
- Affichage des POIs géolocalisés sur une carte OpenStreetMap
- Zoom et navigation
- Marqueurs cliquables avec titre et description

### Capture de POI (SpotScene)
- Flux caméra en temps réel
- Prise de photo dans une zone délimitée
- Saisie du titre et de la description (obligatoires)
- **Icone sauvegarder** : crée un POI privé visible uniquement par l'utilisateur
- **Icone envoyer** : soumet une proposition de POI pour validation

### Collection personnelle
- Affichage de tous les POIs et propositions créés par l'utilisateur
- Visualisation de la photo, du titre et de la description

### Visualisation AR (ScanScene)
- Superposition de contenus AR sur les lieux géolocalisés

---

## IV. Base de données

Le projet utilise **Firebase Firestore** avec les collections suivantes :

| Collection | Description |
|---|---|
| `POI` | Points d'intérêt validés et publics |
| `PropositionPOI` | Propositions soumises par les utilisateurs en attente de validation |
| `Utilisateur` | Données des utilisateurs |

---

## V. Technologies utilisées

- **Unity 6000.4.4f1** — moteur de jeu et rendu AR
- **AR Foundation + ARCore** — réalité augmentée sur Android
- **Firebase Firestore** — base de données temps réel
- **OpenStreetMap** — tuiles cartographiques

