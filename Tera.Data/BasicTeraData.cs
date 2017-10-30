﻿// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Tera.Game;

namespace Tera.Data
{
    public class BasicTeraData
    {
        public string ResourceDirectory { get; }
        public ServerDatabase Servers { get; private set; }
        public IconsDatabase Icons { get; private set; }
        public string Language { get; private set; }
        private readonly Func<string, TeraData> _dataForRegion;
        private readonly string _overridesDirectory;

        public TeraData DataForRegion(string region)
        {
            return _dataForRegion(region);
        }

        public BasicTeraData(string overridesDirectory,string language, bool detectBosses)
        {
            _overridesDirectory = overridesDirectory;
            ResourceDirectory = FindResourceDirectory();
            Language = language;
            Icons=new IconsDatabase(ResourceDirectory);
            _dataForRegion = Helpers.Memoize<string, TeraData>(region => new TeraData(this, region, detectBosses));
            LoadServers();
        }

        private void LoadServers()
        {
            Servers = new ServerDatabase(ResourceDirectory);

            //handle overrides
            var serversOverridePath = Path.Combine(_overridesDirectory, "server-overrides.txt");
            if (!File.Exists(serversOverridePath))//create the default file if it doesn't exist
                File.WriteAllText(serversOverridePath, Properties.Resources.server_overrides);
            var overriddenServers = GetServers(serversOverridePath).ToList();
            Servers.AddOverrides(overriddenServers);

        }

        private string FindResourceDirectory()
        {
            var resourceDirectory = Path.Combine(_overridesDirectory, @"res\");
            if (!Directory.Exists(resourceDirectory))
            {
                //clone git repo if it doesn't already exist
                Repository.Clone(@"git://github.com/neowutran/TeraDpsMeterData.git", resourceDirectory);
            }
            else
            {   //if we already have the repo, just update it
                using (var repo = new Repository(resourceDirectory))
                {
                    Commands.Pull(repo, new Signature("guest", "guest", DateTimeOffset.Now), new PullOptions());
                }
            }

            return resourceDirectory;
        }

        private static IEnumerable<Server> GetServers(string filename)
        {
            return File.ReadAllLines(filename)
                       .Where(s => !s.StartsWith("#") && !string.IsNullOrWhiteSpace(s))
                       .Select(s => s.Split(new[] { ' ' }, 3))
                       .Select(parts => new Server(parts[2], parts[1], parts[0]));
        }
    }
}
