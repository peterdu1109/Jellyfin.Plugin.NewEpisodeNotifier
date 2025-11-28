(function () {
    console.log("[NewEpisodeNotifier] Script chargé.");

    const checkInterval = 60000; // Vérification toutes les 60 secondes

    function addBellIcon() {
        // Si la cloche existe déjà, on arrête
        if (document.getElementById('new-episode-bell')) return;

        // Cible la barre d'en-tête de Jellyfin
        // Support de plusieurs sélecteurs pour compatibilité versions
        const headerRight = document.querySelector('.headerRight') ||
            document.querySelector('.header-right') ||
            document.querySelector('.skinHeader-content .headerRight');

        // Si l'interface n'est pas encore chargée, on réessaiera plus tard
        if (!headerRight) {
            // console.log("[NewEpisodeNotifier] Header non trouvé, nouvel essai bientôt...");
            return;
        }

        const bellBtn = document.createElement('button');
        bellBtn.id = 'new-episode-bell';
        bellBtn.className = 'headerButton paper-icon-button-light';
        // Icône "Notifications" (Material Design utilisé par Jellyfin)
        bellBtn.innerHTML = '<span class="material-icons notification_important" style="font-size: 1.5em;">notifications</span>';
        bellBtn.style.display = 'none'; // Caché par défaut
        bellBtn.style.color = '#e74c3c'; // Rouge
        bellBtn.title = "Nouveaux épisodes disponibles";

        bellBtn.onclick = function () {
            alert("De nouveaux épisodes ont été ajoutés récemment !");
            // Optionnel : Cacher la cloche après le clic
            // this.style.display = 'none';
        };

        // Insérer au début de la zone droite (avant l'avatar/cast)
        headerRight.insertBefore(bellBtn, headerRight.firstChild);
        console.log("[NewEpisodeNotifier] Cloche ajoutée au DOM.");
    }

    async function checkNotifications() {
        // Récupération ID utilisateur via l'objet global Jellyfin
        if (!window.ApiClient) return;
        const userId = window.ApiClient.getCurrentUserId();

        if (!userId) return;

        try {
            // Appel au contrôleur C#
            const response = await fetch(`/NewEpisodeNotifier/Check?userId=${userId}`);
            if (!response.ok) return;

            const data = await response.json();
            const bell = document.getElementById('new-episode-bell');

            if (bell) {
                if (data.hasNewContent) {
                    bell.style.display = 'inline-flex';
                    // Animation simple
                    bell.animate([
                        { transform: 'scale(1)' },
                        { transform: 'scale(1.2)' },
                        { transform: 'scale(1)' }
                    ], { duration: 500 });
                } else {
                    bell.style.display = 'none';
                }
            } else {
                // Si la cloche n'est pas là (changement de page), on la remet
                addBellIcon();
            }
        } catch (err) {
            console.error("[NewEpisodeNotifier] Erreur :", err);
        }
    }

    // Initialisation
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(() => {
            addBellIcon();
            checkNotifications();
            setInterval(checkNotifications, checkInterval);
        }, 2000);
    });

    // Observer pour gérer la navigation (SPA)
    const observer = new MutationObserver(() => {
        if (!document.getElementById('new-episode-bell')) {
            addBellIcon();
        }
    });
    observer.observe(document.body, { childList: true, subtree: true });

})();