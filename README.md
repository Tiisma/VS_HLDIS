# How Long Did I Sleep? (HLDIS)

Mod Vintage Story client + serveur. Quand un joueur entre en jeu, il reçoit dans le chat :

- le temps de jeu écoulé depuis sa dernière présence :
  - moins de 1 jour : « moins d'une journée » ;
  - moins de 7 jours : nombre de jours entiers (« 3 jours ») ;
  - 7 jours ou plus : semaines arrondies à la demi-semaine (« 1 semaine et demie », « 3 semaines ») ;
- la période de la saison à sa position : début, mi ou fin de printemps / été / automne / hiver, sans mois précis.

Le message est traduit côté client (`assets/hldis/lang/fr.json`, `en.json`) selon la langue du joueur.

## Versions

| Élément | Version |
|---|---|
| Vintage Story (API de compilation) | 1.22.7 |
| Optimum / Stratum ciblés | 0.3.19 / v1.22.7-stratum.2 (non testés) |
| Framework | net10.0 (identique à `VintagestoryAPI.dll` 1.22.7) |
| SDK | .NET 10 (`global.json` : 10.0.100, `latestFeature`) |

## Fonctionnement

- **Serveur** : à l'entrée en jeu (`PlayerNowPlaying`), compare `Calendar.TotalDays` à la date mémorisée pour le joueur, puis envoie un paquet réseau (canal `hldis`). La date est mise à jour à la déconnexion et à chaque sauvegarde du monde (`GameWorldSave`) pour les joueurs connectés. Elle est stockée dans la sauvegarde du monde (clé `hldis-lastseen`).
- **Client** : choisit les textes traduits et les affiche dans le chat.
- **Saison** : calculée à partir de `GetSeasonRel` à la position du joueur (hémisphère et `SeasonOverride` inclus). Les limites des saisons reprennent celles de `GameCalendar.GetSeason` du jeu 1.22.7 ; chaque saison est découpée en trois tiers.
- Une semaine compte 7 jours de jeu, quel que soit le réglage « jours par mois » du monde.

Le mod est requis des deux côtés (`requiredOnClient` et `requiredOnServer`).

## Limites

- En solo, le temps de jeu ne s'écoule pas pendant l'absence : le message indiquera presque toujours « moins d'une journée ».
- Première connexion à un monde : seule la saison est affichée.
- Si le serveur plante, l'absence est calculée depuis la dernière sauvegarde du monde.
- Pas de passage aux mois ou aux années pour les longues absences : la durée reste exprimée en semaines.

## Compilation

Prérequis : SDK .NET 10 et une installation de Vintage Story 1.22.7 (les DLL du jeu ne sont pas incluses dans ce dépôt).

```bash
dotnet build HLDIS/HLDIS.csproj -c Release -p:VintageStoryPath="C:/chemin/vers/Vintagestory-1.22.7"
```

Sans `-p:VintageStoryPath`, la variable d'environnement `VINTAGE_STORY` est utilisée. L'archive installable est produite dans `dist/hldis_1.0.0.zip` ; la copier dans le dossier `Mods` du client et du serveur.

## État des vérifications

- Compilation contre l'API 1.22.7 : voir l'historique Git / la PR.
- Tests en jeu (client, serveur dédié, Optimum, Stratum) : **non effectués**.
