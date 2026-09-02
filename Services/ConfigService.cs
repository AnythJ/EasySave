using EasySave.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.Services
{
    public class ConfigService
    {
        private readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "config.json");

        public List<GameConfig> Load()
        {
            if (!File.Exists(_configPath))
                return new List<GameConfig>();

            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<List<GameConfig>>(json) ?? new List<GameConfig>();
        }

        public void Save(List<GameConfig> games)
        {
            var json = JsonSerializer.Serialize(games, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
    }
}
