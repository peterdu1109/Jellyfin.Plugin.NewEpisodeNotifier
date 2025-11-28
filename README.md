# New Episode Notifier Plugin

Ce plugin notifie les utilisateurs de Jellyfin lorsqu'un nouvel épisode est ajouté à la bibliothèque.

## Fonctionnalités

- Affiche une icône de cloche dans la barre d'en-tête.
- L'icône s'anime lorsqu'il y a de nouveaux épisodes.
- Les notifications sont persistantes (sauvegardées sur le disque).

## Installation

1. Compilez le projet et placez la DLL dans le dossier `plugins` de votre serveur Jellyfin.
2. Redémarrez le serveur Jellyfin.

## Activation du Script Client

Depuis les versions récentes de Jellyfin, l'injection automatique de scripts est plus restreinte. Pour que la cloche apparaisse, vous devez ajouter le script manuellement :

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
