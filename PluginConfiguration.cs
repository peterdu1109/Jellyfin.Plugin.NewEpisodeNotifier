using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.NewEpisodeNotifier
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Intervalle de vérification des notifications en secondes
        /// </summary>
        public int CheckIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Activer l'animation de la cloche
        /// </summary>
        public bool EnableAnimation { get; set; } = true;

        /// <summary>
        /// Message d'alerte personnalisé
        /// </summary>
        public string CustomAlertMessage { get; set; } = "De nouveaux épisodes ont été ajoutés récemment !";
    }
}
