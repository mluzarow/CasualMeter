﻿using System;
using System.Collections.Generic;
using System.IO;
using CasualMeter.Core.Entities;
using Newtonsoft.Json;
using Tera.Data;
using Tera.Game;

namespace CasualMeter.Core.Helpers
{
    public sealed class SettingsHelper
    {
        private static readonly Lazy<SettingsHelper> Lazy = new Lazy<SettingsHelper>(() => new SettingsHelper());
        
        public readonly BasicTeraData BasicTeraData;
        private readonly Dictionary<PlayerClass, string> _classIcons;
        
        public static SettingsHelper Instance => Lazy.Value;

        private static readonly string TempFolderPath = Path.Combine(Path.GetTempPath(), "CasualMeter");
        private static readonly string DocumentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CasualMeter");
        private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CasualMeter");
        private static readonly string ConfigFilePath = Path.Combine(SettingsPath, "settings.json");

        private readonly JsonSerializerSettings _jsonSerializerSettings ;

        public Settings Settings { get; set; }
        public string Version { get; set; } = "debug";

        private SettingsHelper()
        {
            Directory.CreateDirectory(SettingsPath);//ensure settings directory is created
            _classIcons = new Dictionary<PlayerClass, string>();

            _jsonSerializerSettings = new JsonSerializerSettings {DefaultValueHandling = DefaultValueHandling.Populate};

            Load();
            BasicTeraData = new BasicTeraData(SettingsPath, Settings.Language, Settings.DetectBosses);
            LoadClassIcons();
        }

        public void Initialize()
        {
            //empty method to ensure initialization
        }

        private void LoadClassIcons()
        {
            var directory = Path.Combine(BasicTeraData.ResourceDirectory, @"class-icons");
            foreach (var playerClass in (PlayerClass[]) Enum.GetValues(typeof (PlayerClass)))
            {
                var filename = Path.Combine(directory, playerClass.ToString().ToLowerInvariant() + ".png");
                _classIcons.Add(playerClass, filename);
            }
        }

        private void Load()
        {
            if (File.Exists(ConfigFilePath))
            {   //load settings if the file exists
                try
                {
                    Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(ConfigFilePath),
                        _jsonSerializerSettings);
                }
                catch (JsonSerializationException)
                {
                    //someone fucked up their settings...
                    Settings = new Settings();
                }
            }
            else
            {   //no saved settings found
                Settings = new Settings();
            }
            Save();
        }

        public void Save()
        {
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(Settings, Formatting.Indented, _jsonSerializerSettings));
        }

        public string GetImage(PlayerClass @class)
        {
            return _classIcons[@class];
        }

        public string GetSettingsPath()
        {
            return SettingsPath;
        }

        public string GetDocumentsPath()
        {
            return DocumentsPath;
        }

        public string GetTempFolderPath()
        {
            return TempFolderPath;
        }
    }
}
