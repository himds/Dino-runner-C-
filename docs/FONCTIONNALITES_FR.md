# Dino Runner — Documentation des fonctionnalités (français)

Ce document décrit **comment** les fonctionnalités sont implémentées dans le projet : coopération front/back, persistance des données, emplacement des fichiers clés.

---

## 1. Stack technique et architecture

| Couche | Technologie | Rôle |
|--------|-------------|------|
| Backend | ASP.NET Core 8, contrôleurs MVC | API JSON sous `/api/*` |
| ORM | Entity Framework Core 8 + SQLite | Entités et fichier `dino.db` |
| Frontend | HTML/CSS/JS statiques (`wwwroot`) | Pas de bundler ; `fetch` vers l’API |
| Session locale | `localStorage`, clé `dino_user` | Objet `{ id, coins }` — **pas de cookie ni session serveur** |

Point d’entrée : `Program.cs` — enregistrement des contrôleurs, base de données, CORS, Swagger (développement), fichiers statiques ; au démarrage, `EnsureCreated()` crée le schéma et applique les données initiales (seed). `MapControllers()` est appelé **avant** `MapFallbackToFile("index.html")` pour que les requêtes `/api/*` ne soient pas interceptées par la route de repli.

---

## 2. Inscription et connexion

**Fichiers :** `AuthController.cs`, `login.html`, `Models/User.cs`

- **Inscription** `POST /api/auth/register` : vérifie l’unicité du nom d’utilisateur ; crée un `User` ; le mot de passe est stocké sous forme de hachage **SHA256** sur la chaîne `nomUtilisateur:motDePasse`, en hexadécimal (`PasswordHash`) — **jamais en clair**.
- **Connexion** `POST /api/auth/login` : recherche par nom d’utilisateur, recalcule le hachage et compare ; en cas de succès, renvoie `Id`, `Username`, `Coins`.
- Le frontend enregistre `{ id, coins }` dans `localStorage.setItem('dino_user', ...)` ; chaque page lit cette valeur et redirige vers `login.html` si besoin.

**Erreurs :** les réponses JSON contiennent un champ `message` (en français) affichable tel quel dans l’interface.

---

## 3. Pièces (monnaie du jeu)

**Fichiers :** champ `User.Coins`, `ScoresController`, `ShopController`, `index.html`, `shop.html`

- **Solde compte :** colonne `Coins` de la table `Users` ; valeur par défaut 0 (configurée dans `AppDbContext`).
- **Pièces ramassées en partie :** des pièces apparaissent sur la piste ; les collisions incrémentent un compteur de partie `runCoins`. **À la fin de la partie**, le client envoie `coinsCollected` dans le corps de `POST /api/scores` ; le serveur **ajoute** cette valeur au solde (ce n’est pas un calcul automatique à partir du score seul).
- **Dépense :** l’achat en boutique déduit `ShopItem.Price` de `user.Coins`.

---

## 4. Envoi du score, classement et historique

**Fichiers :** `ScoresController.cs`, `Dtos/ScoreSubmitRequest.cs`, `Dtos/ScoreSubmitResponse.cs`

- **Soumission** `POST /api/scores` (action `SubmitScore`) : vérifie l’utilisateur → insère une ligne dans `Scores` → crédite les pièces selon `CoinsCollected` → évalue les succès → renvoie `ScoreSubmitResponse` (`NewCoins`, `TotalCoins`, `UnlockedAchievementIds`, etc.).
- **Classement** `GET /api/scores/top?limit=` : tri par score décroissant puis par date ; limite par défaut 20, paramètre borné entre 1 et 100.
- **Historique d’un joueur** `GET /api/scores/user/{userId}` : tous les scores de l’utilisateur, du plus récent au plus ancien.

---

## 5. Succès (achievements)

**Fichiers :** seed `Achievement` / `UserAchievement` dans `AppDbContext`, méthode `EvaluateAchievements` dans `ScoresController`

- **Définitions :** table `Achievements` ; le seed définit des chaînes `Condition`, ex. `score>=1000`, `games>=10`.
- **Moment du déblocage :** après chaque soumission de score réussie ; pour chaque succès non encore obtenu :
  - `score>=X` : compare le **score de la manche en cours** (`request.Value`) au seuil ;
  - `games>=X` : compare le **nombre de parties enregistrées** (`user.Scores.Count`, incluant la manche qui vient d’être ajoutée) au seuil.
- **Enregistrement :** insertion dans `UserAchievements` (utilisateur, succès, horodatage).
- **Frontend :** `achievements.html` charge la liste des définitions et celle des succès débloqués pour l’utilisateur, puis fusionne pour l’affichage verrouillé / débloqué.

---

## 6. Boutique et objets achetés

**Fichiers :** `ShopController.cs`, entités `ShopItem` / `UserItem`, `shop.html`, seed dans `AppDbContext`

- **Catalogue** `GET /api/shop/items` : liste des `ShopItem`.
- **Possessions** `GET /api/shop/user/{userId}` : liste des achats avec `shopItemId` pour savoir ce que le joueur possède.
- **Achat** `POST /api/shop/purchase` : contrôles d’existence utilisateur et article ; conflit si déjà possédé ; 400 si pièces insuffisantes ; sinon débit et insertion dans `UserItems`.

Correspondance des **IDs du seed** avec la logique **dans `index.html`** (codée en dur) :

| ShopItemId | Effet en jeu (résumé) |
|------------|------------------------|
| 1 | Double saut (barre d’espace, second saut dans les airs) |
| 2 | Vitesse des obstacles multipliée par ~0,7 |
| 3 | Bouclier : une collision consomme le bouclier et retire l’obstacle sans game over |
| 4 | Le seed décrit un boost de score ; **la boucle de jeu actuelle dans `index.html` ne lit pas l’id 4** (pas de touche E ni doublement implémenté dans ce flux) — achat possible à des fins d’extension ultérieure |

---

## 7. Boucle de jeu (Canvas)

**Fichier :** script inline dans `wwwroot/index.html`

- **Canvas 2D** : fond, sol, rectangle du dinosaure, obstacles, pièces sur la piste.
- **Boucle** `requestAnimationFrame(loop)` : incrémentation de `frame` et du score ; apparition périodique d’obstacles ; vitesse modulée par `speedReduction` ; détection de collision AABB ; logique bouclier décrite ci-dessus.
- **Saut :** écoute de `Space` ; si l’objet id 1 est possédé, `maxJumps = 2`.
- **Fin de partie :** `gameOver()` arrête la boucle, appelle `POST /api/scores` avec `userId`, `value`, `coinsCollected`, met à jour `localStorage` pour `coins`, puis recharge les objets via `loadOwnedItems()` (bouclier, etc.).

Le fichier optionnel `wwwroot/item.js` factorise le chargement des objets vers un module ES (`gameState`) ; **la page jeu principale utilise encore la logique inline**, en parallèle, pour faciliter une refactorisation future.

---

## 8. Base de données et données initiales

**Fichiers :** `Data/AppDbContext.cs`, chaîne de connexion dans `appsettings.json`

- `EnsureCreated()` : création du schéma **sans fichiers de migrations EF** dans le dépôt ; `HasData` insère succès et articles de boutique.
- Si la base locale existe déjà, **modifier le seed ne met pas à jour automatiquement les lignes existantes** ; en développement, on supprime souvent `dino.db` pour recréer (perte des comptes et achats locaux).

---

## 9. CORS et documentation API

- **CORS :** politique `AllowAll` dans `Program.cs` (adaptée au développement local).
- **Swagger :** en environnement Development, UI sous `/swagger`.

---

## 10. Index des fichiers principaux

| Fonctionnalité | Backend | Frontend |
|----------------|---------|----------|
| Inscription / connexion | `AuthController.cs` | `login.html` |
| Jeu | `ScoresController.cs` | `index.html` |
| Classement / historique | `ScoresController.cs` | appels API selon besoin |
| Succès | `AchievementsController.cs` + logique dans `ScoresController` | `achievements.html` |
| Boutique | `ShopController.cs` | `shop.html` |
| Styles | — | `style.css` |

---

*Document rédigé en parallèle du code ; en cas d’écart, le code source fait foi.*
