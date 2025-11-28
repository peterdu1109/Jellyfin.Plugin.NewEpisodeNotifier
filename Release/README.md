# New Episode Notifier Plugin

Ce plugin notifie les utilisateurs de Jellyfin lorsqu'un nouvel épisode est ajouté à la bibliothèque.

## Fonctionnalités

- Affiche une icône de cloche dans la barre d'en-tête.
- L'icône s'anime lorsqu'il y a de nouveaux épisodes.
- Les notifications sont persistantes (sauvegardées sur le disque).

## Installation via Repository (Recommandé)

1. Allez dans le **Tableau de bord** de Jellyfin -> **Plugins** -> **Dépôts**.
2. Cliquez sur le bouton `+` pour ajouter un dépôt.
3. Nom : `New Episode Notifier`
4. URL : `https://peterdu1109.github.io/Jellyfin.Plugin.NewEpisodeNotifier/manifest.json`
5. Sauvegardez.
6. Allez dans le catalogue, trouvez "New Episode Notifier" et installez-le.
7. Redémarrez Jellyfin.

## Installation Manuelle (Alternative)

1. Téléchargez la dernière release (fichier `Jellyfin.Plugin.NewEpisodeNotifier.dll`).
2. Arrêtez votre serveur Jellyfin.
3. Copiez le fichier `Jellyfin.Plugin.NewEpisodeNotifier.dll` dans le dossier `plugins` de votre installation Jellyfin (créez un sous-dossier `NewEpisodeNotifier` si vous voulez être propre).
   - Windows : `C:\ProgramData\Jellyfin\Server\plugins\`
   - Linux : `/var/lib/jellyfin/plugins/`
   - Docker : `/config/plugins/`
4. Redémarrez le serveur Jellyfin.

## Activation du Script Client

**Note importante :** L'injection automatique du script n'est pas possible techniquement sur les versions actuelles de Jellyfin via ce plugin. Vous devez suivre cette procédure manuelle une seule fois :

1. Allez dans le **Tableau de bord** de Jellyfin.
2. Allez dans **Général** -> **Code HTML personnalisé**.
3. Ajoutez la ligne suivante dans la section **Body (JS)** (ou équivalent) :

```html
<script src="/NewEpisodeNotifier/ClientScript.js"></script>
```

4. Sauvegardez et rafraîchissez votre page Jellyfin (F5).

## Dépannage

Si la cloche n'apparaît pas :
- Vérifiez la console du navigateur (F12) pour voir si le script est chargé (`[NewEpisodeNotifier] Script chargé.`).
- Assurez-vous que le chemin `/NewEpisodeNotifier/ClientScript.js` est accessible (essayez de l'ouvrir dans un nouvel onglet).
