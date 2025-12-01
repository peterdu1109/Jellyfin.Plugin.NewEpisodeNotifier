(function () {
    console.log("[NewEpisodeNotifier] Script chargé.");

    let checkInterval = 60000;
    let enableAnimation = true;
    let customMessage = "De nouveaux épisodes ont été ajoutés récemment !";

    async function loadConfig() {
        try {
            if (window.ApiClient) {
                const config = await window.ApiClient.getPluginConfiguration("a2d3e4f5-6789-4b12-8c34-5d6e7f8a9b0c");
                checkInterval = (config.CheckIntervalSeconds || 60) * 1000;
                enableAnimation = config.EnableAnimation !== false;
                customMessage = config.CustomAlertMessage || "De nouveaux épisodes ont été ajoutés récemment !";
                console.log("[NewEpisodeNotifier] Configuration chargée:", { checkInterval, enableAnimation, customMessage });
            }
        } catch (err) {
            console.warn("[NewEpisodeNotifier] Configuration par défaut utilisée:", err);
        }
    }

    function addBellIcon() {
        if (document.getElementById('new-episode-bell')) return;

        const headerRight = document.querySelector('.headerRight') ||
            document.querySelector('.header-right') ||
            document.querySelector('.skinHeader-content .headerRight');

        if (!headerRight) return;

        const bellBtn = document.createElement('button');
        bellBtn.id = 'new-episode-bell';
        bellBtn.className = 'headerButton paper-icon-button-light';
        bellBtn.innerHTML = '<span class="material-icons notification_important" style="font-size: 1.5em;">notifications</span>';
        bellBtn.style.display = 'none';
        bellBtn.style.color = '#e74c3c';
        bellBtn.title = "Nouveaux épisodes disponibles";

        bellBtn.onclick = function () {
            alert(customMessage);
        };

        headerRight.insertBefore(bellBtn, headerRight.firstChild);
        console.log("[NewEpisodeNotifier] Cloche ajoutée au DOM.");
    }

    async function checkNotifications() {
        if (!window.ApiClient) return;
        const userId = window.ApiClient.getCurrentUserId();
        if (!userId) return;

        try {
            const response = await fetch(`/NewEpisodeNotifier/Check?userId=${userId}`);
            if (!response.ok) return;

            const data = await response.json();
            const bell = document.getElementById('new-episode-bell');

            if (bell) {
                if (data.hasNewContent) {
                    bell.style.display = 'inline-flex';
                    if (enableAnimation) {
                        bell.animate([
                            { transform: 'scale(1)' },
                            { transform: 'scale(1.2)' },
                            { transform: 'scale(1)' }
                        ], { duration: 500 });
                    }
                } else {
                    bell.style.display = 'none';
                }
            } else {
                addBellIcon();
            }
        } catch (err) {
            console.error("[NewEpisodeNotifier] Erreur :", err);
        }
    }

    document.addEventListener('DOMContentLoaded', async () => {
        await loadConfig();
        setTimeout(() => {
            addBellIcon();
            checkNotifications();
            setInterval(checkNotifications, checkInterval);
        }, 2000);
    });

    const observer = new MutationObserver(() => {
        if (!document.getElementById('new-episode-bell')) {
            addBellIcon();
        }
    });
    observer.observe(document.body, { childList: true, subtree: true });
})();