# New Episode Notifier Plugin

Ce plugin notifie les utilisateurs de Jellyfin lorsqu'un nouvel épisode est ajouté à la bibliothèque.

## Fonctionnalités

- Affiche une icône de cloche dans la barre d'en-tête.
- L'icône s'anime lorsqu'il y a de nouveaux épisodes.
- Les notifications sont persistantes (sauvegardées sur le disque).

## Pré-requis

- **Jellyfin Plugin File Transformation** : Ce plugin est nécessaire pour l'injection automatique du script.
  1. Ajoutez le dépôt : `https://www.iamparadox.dev/jellyfin/plugins/manifest.json`
  2. Installez le plugin "File Transformation".

## Installation via Repository (Recommandé)

1. Allez dans le **Tableau de bord** de Jellyfin -> **Plugins** -> **Dépôts**.
2. Cliquez sur le bouton `+` pour ajouter un dépôt.
3. Nom : `New Episode Notifier`
4. URL : `https://peterdu1109.github.io/Jellyfin.Plugin.NewEpisodeNotifier/manifest.json`
5. Sauvegardez.
6. Allez dans le catalogue, trouvez "New Episode Notifier" et installez-le.
7. Redémarrez Jellyfin.

Une fois installé (et le plugin File Transformation présent), la cloche de notification apparaîtra automatiquement. Plus besoin de modification manuelle du HTML !

## Installation Manuelle (Alternative)

1. Téléchargez la dernière release (fichier `Jellyfin.Plugin.NewEpisodeNotifier.dll`).
2. Arrêtez votre serveur Jellyfin.
3. Copiez le fichier `Jellyfin.Plugin.NewEpisodeNotifier.dll` dans le dossier `plugins` de votre installation Jellyfin.
4. Assurez-vous d'avoir également installé le plugin "File Transformation".
5. Redémarrez le serveur Jellyfin.

## Dépannage

Si la cloche n'apparaît pas :
- Vérifiez la console du navigateur (F12) pour voir si le script est chargé (`[NewEpisodeNotifier] Script chargé.`).
- Assurez-vous que le chemin `/NewEpisodeNotifier/ClientScript.js` est accessible (essayez de l'ouvrir dans un nouvel onglet).
