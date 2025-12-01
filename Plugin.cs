using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.NewEpisodeNotifier
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
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

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILibraryManager libraryManager, ILogger<Plugin> logger) 
            : base(applicationPaths, xmlSerializer) 
        {
            Instance = this;
            _libraryManager = libraryManager;
            _logger = logger;
            
            LoadData();

            if (_libraryManager != null)
            {
                _libraryManager.ItemAdded += OnItemAdded;
            }

            RegisterFileTransformation();
        }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = Name,
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
                }
            };
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

        private void RegisterFileTransformation()
        {
            try
            {
                Assembly? fileTransformationAssembly = System.Runtime.Loader.AssemblyLoadContext.All
                    .SelectMany(x => x.Assemblies)
                    .FirstOrDefault(x => x.FullName?.Contains(".FileTransformation") ?? false);

                if (fileTransformationAssembly != null)
                {
                    Type? pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");
                    if (pluginInterfaceType != null)
                    {
                        // Chercher JObject dans tous les assemblies chargés car on ne le référence pas directement
                        Type? jObjectType = null;
                        foreach (var context in System.Runtime.Loader.AssemblyLoadContext.All)
                        {
                            foreach (var asm in context.Assemblies)
                            {
                                jObjectType = asm.GetType("Newtonsoft.Json.Linq.JObject");
                                if (jObjectType != null) break;
                            }
                            if (jObjectType != null) break;
                        }

                        if (jObjectType == null)
                        {
                            _logger.LogWarning("[NewEpisodeNotifier] Type 'Newtonsoft.Json.Linq.JObject' introuvable dans les assemblies chargés.");
                            return;
                        }

                        var payloadData = new
                        {
                            id = "a2d3e4f5-6789-4b12-8c34-5d6e7f8a9b0d",
                            fileNamePattern = "index.html",
                            callbackAssembly = Assembly.GetExecutingAssembly().FullName,
                            callbackClass = typeof(Plugin).FullName,
                            callbackMethod = nameof(TransformFile)
                        };

                        var jsonString = System.Text.Json.JsonSerializer.Serialize(payloadData);
                        _logger.LogDebug("[NewEpisodeNotifier] Payload JSON: {Json}", jsonString);

                        var parseMethod = jObjectType.GetMethod("Parse", new[] { typeof(string) });
                        if (parseMethod == null)
                        {
                            _logger.LogWarning("[NewEpisodeNotifier] Méthode 'Parse' introuvable sur JObject.");
                            return;
                        }

                        var payload = parseMethod.Invoke(null, new object[] { jsonString });
                        if (payload == null)
                        {
                            _logger.LogWarning("[NewEpisodeNotifier] Le payload JObject est null après Parse.");
                            return;
                        }

                        pluginInterfaceType.GetMethod("RegisterTransformation")?.Invoke(null, new object?[] { payload });
                        _logger.LogInformation("[NewEpisodeNotifier] Transformation de fichier enregistrée avec succès.");
                    }
                    else
                    {
                        _logger.LogWarning("[NewEpisodeNotifier] Type 'Jellyfin.Plugin.FileTransformation.PluginInterface' introuvable.");
                    }
                }
                else
                {
                    _logger.LogWarning("[NewEpisodeNotifier] Assembly 'FileTransformation' introuvable. Assurez-vous que le plugin est installé.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NewEpisodeNotifier] Erreur lors de l'enregistrement de la transformation de fichier.");
            }
        }

        public static object TransformFile(object data)
        {
            try
            {
                var type = data.GetType();
                var contentsProp = type.GetProperty("contents");
                
                if (contentsProp != null)
                {
                    var content = contentsProp.GetValue(data) as string;
                    if (content != null)
                    {
                        var scriptTag = "<script src=\"/NewEpisodeNotifier/ClientScript.js\" defer></script>";
                        
                        if (content.Contains("</body>"))
                        {
                            var newContent = content.Replace("</body>", $"{scriptTag}</body>");
                            return new { contents = newContent };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NewEpisodeNotifier] Erreur dans TransformFile: {ex.Message}");
            }
            return data; 
        }
    }
}