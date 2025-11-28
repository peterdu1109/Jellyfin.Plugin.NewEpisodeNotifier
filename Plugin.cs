using System;
using System.Collections.Generic;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.NewEpisodeNotifier
{
    public class Plugin : BasePlugin
    {
        // --- ID UNIQUE DU PLUGIN ---
        public override Guid Id => new Guid("a2d3e4f5-6789-4b12-8c34-5d6e7f8a9b0c");

        public override string Name => "New Episode Notifier";
        public override string Description => "Affiche une cloche quand un épisode est ajouté.";

        public static Plugin Instance { get; private set; }
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<Plugin> _logger;
        
        private readonly object _lock = new object();
        private HashSet<Guid> _seriesWithNewEpisodes = new HashSet<Guid>();
        public HashSet<Guid> SeriesWithNewEpisodes 
        {
            get 
            { 
                lock(_lock) { return new HashSet<Guid>(_seriesWithNewEpisodes); }
            }
        }

        // --- CONSTRUCTEUR CORRIGÉ POUR COMPILER ---
        // On demande juste ce dont on a besoin, et on laisse base() vide.
        public Plugin(ILibraryManager libraryManager, ILogger<Plugin> logger) 
            : base() 
        {
            Instance = this;
            _libraryManager = libraryManager;
            _logger = logger;
            
            LoadData();

            // On s'abonne aux événements
            if (_libraryManager != null)
            {
                _libraryManager.ItemAdded += OnItemAdded;
            }
        }

        private void OnItemAdded(object sender, ItemChangeEventArgs e)
        {
            if (e.Item == null) return;

            if (e.Item is Episode episode)
            {
                try 
                {
                    var seriesId = episode.SeriesId;
                    _logger.LogInformation($"[NewEpisodeNotifier] Nouvel épisode : {episode.Name}");
                    
                    lock(_lock)
                    {
                        if (_seriesWithNewEpisodes.Add(seriesId))
                        {
                            SaveData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur plugin notification");
                }
            }
        }
        
        public bool HasNewContent() 
        {
            lock(_lock)
            {
                return _seriesWithNewEpisodes.Count > 0;
            }
        }

        private void SaveData()
        {
            try
            {
                if (string.IsNullOrEmpty(DataFolderPath))
                {
                    _logger.LogWarning("[NewEpisodeNotifier] DataFolderPath is null, cannot save data.");
                    return;
                }
                
                var dataFilePath = System.IO.Path.Combine(DataFolderPath, "new_episodes.json");
                var json = System.Text.Json.JsonSerializer.Serialize(_seriesWithNewEpisodes);
                System.IO.File.WriteAllText(dataFilePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde des notifications.");
            }
        }

        private void LoadData()
        {
            try
            {
                if (string.IsNullOrEmpty(DataFolderPath))
                {
                    _logger.LogWarning("[NewEpisodeNotifier] DataFolderPath is null, cannot load data.");
                    return;
                }
                
                var dataFilePath = System.IO.Path.Combine(DataFolderPath, "new_episodes.json");
                if (System.IO.File.Exists(dataFilePath))
                {
                    var json = System.IO.File.ReadAllText(dataFilePath);
                    var data = System.Text.Json.JsonSerializer.Deserialize<HashSet<Guid>>(json);
                    if (data != null)
                    {
                        lock(_lock)
                        {
                            _seriesWithNewEpisodes = data;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des notifications.");
            }
        }
    }
}