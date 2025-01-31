﻿using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using GameServerCore.Domain;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Inventory;
using LeagueSandbox.GameServer.Logging;
using log4net;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// Class that contains basic game information which is used to decide how the game will function after starting, such as players, their spawns,
    /// the packages which control the functionality of their champions/abilities, and lastly whether basic game mechanics such as 
    /// cooldowns/mana costs/minion spawns should be enabled/disabled.
    /// </summary>
    public class Config
    {
        public const string VERSION_STRING = "Version 4.20.0.315 [PUBLIC]";

        public List<PlayerConfig> Players { get; private set; }
        public GameConfig GameConfig { get; private set; }
        public ContentManager ContentManager { get; private set; }
        public FeatureFlags GameFeatures { get; private set; }
        public static readonly Version VERSION = new Version(4, 20, 0, 315);
        internal string[] AssemblyNames { get; private set; } = [];
        public bool ChatCheatsEnabled { get; private set; }
        public string ContentPath { get; private set; }
        public bool IsDamageTextGlobal { get; private set; }

        public float ForcedStart { get; private set; }

        private Config()
        {
        }

        public static Config LoadFromJson(string json)
        {
            var result = new Config();
            result.LoadConfig(json);
            return result;
        }

        public static Config LoadFromFile(string path)
        {
            var result = new Config();
            result.LoadConfig(File.ReadAllText(path));
            return result;
        }

        private void LoadConfig(string json)
        {
            var data = JObject.Parse(json);

            var gameInfo = data.SelectToken("gameInfo");
            SetGameFeatures(FeatureFlags.EnableCooldowns, (bool)gameInfo.SelectToken("COOLDOWNS_ENABLED"));
            SetGameFeatures(FeatureFlags.EnableManaCosts, (bool)gameInfo.SelectToken("MANACOSTS_ENABLED"));
            SetGameFeatures(FeatureFlags.EnableLaneMinions, (bool)gameInfo.SelectToken("MINION_SPAWNS_ENABLED"));

            // Read if chat commands are enabled
            ChatCheatsEnabled = (bool)gameInfo.SelectToken("CHEATS_ENABLED");

            // Read where the content is
            ContentPath = (string)gameInfo.SelectToken("CONTENT_PATH");

            // Evaluate if content path is correct, if not try to path traversal to find it
            if (!Directory.Exists(ContentPath))
            {
                ContentPath = GetContentPath();
            }

            // Read global damage text setting
            IsDamageTextGlobal = (bool)gameInfo.SelectToken("IS_DAMAGE_TEXT_GLOBAL");

            // Read the game configuration
            var gameToken = data.SelectToken("game");
            GameConfig = new GameConfig(gameToken);

            Players = new List<PlayerConfig>();

            // Read the player configuration
            var playerConfigurations = data.SelectToken("players");
            foreach (var player in playerConfigurations)
            {
                var playerConfig = new PlayerConfig(player);
                Players.Add(playerConfig);
            }

            ForcedStart = (float)(data.SelectToken("forcedStart") ?? 0) * 1000;
            AssemblyNames = gameInfo?.SelectToken("scriptAssemblies")?.Values<string>().ToArray() as string[] ?? [];
        }

        public void LoadContent(Game game)
        {
            // Load data package
            ContentManager = new(game);
            foreach (var player in Players)
            {
                player.LoadTalentsAndRunes();
            }
        }

        private string GetContentPath()
        {
            string result = null;

            var executionDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = new DirectoryInfo(executionDirectory ?? Directory.GetCurrentDirectory());

            while (result == null)
            {
                if (path == null)
                {
                    break;
                }

                var directory = path.GetDirectories().Where(c => c.Name.Equals("Content")).ToArray();

                if (directory.Length == 1)
                {
                    result = directory[0].FullName;
                }
                else
                {
                    path = path.Parent;
                }
            }

            return result;
        }

        public void SetGameFeatures(FeatureFlags flag, bool enabled)
        {
            // Toggle the flag on.
            if (enabled)
            {
                GameFeatures |= flag;
            }
            // Toggle off.
            else
            {
                GameFeatures &= ~flag;
            }
        }

        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> GetMapSpawns()
        {
            //Temp Hack
            Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> toReturn = new()
            {
                [TeamId.TEAM_BLUE] = new()
                {
                    [1] = new()
                    {
                        [1] = new(5000, 5000)
                    }
                }
            };
            return toReturn;
        }
    }
}

public class MapData
{
    public int Id { get; private set; }
    public Dictionary<string, float> MapConstants { get; private set; }
    /// <summary>
    /// Collection of MapObjects present within a map's room file, with the key being the name present in the room file. Refer to <see cref="MapObject"/>.
    /// </summary>
    public Dictionary<string, MapObject> MapObjects { get; private set; }
    /// <summary>
    /// Collection of MapObjects which represent lane minion spawn positions.
    /// Not present within the room file, therefor it is split into its own collection.
    /// </summary>
    public Dictionary<string, MapObject> SpawnBarracks { get; private set; }
    /// <summary>
    /// Experience required to level, ordered from 2 and up.
    /// </summary>
    public List<float> ExpCurve { get; private set; }
    public float BaseExpMultiple { get; set; }
    public float LevelDifferenceExpMultiple { get; set; }
    public float MinimumExpMultiple { get; set; }
    /// <summary>
    /// Amount of time death should last depending on level.
    /// </summary>
    public List<float> DeathTimes { get; private set; }
    /// <summary>
    /// Potential progression of stats per-level of jungle monsters.
    /// </summary>
    /// TODO: Figure out what this is and how to implement it.
    public List<float> StatsProgression { get; private set; }

    public MapData(int mapId)
    {
        Id = mapId;
        MapConstants = new Dictionary<string, float>();
        MapObjects = new Dictionary<string, MapObject>();
        SpawnBarracks = new Dictionary<string, MapObject>();
        ExpCurve = new List<float>();
        DeathTimes = new List<float>();
        StatsProgression = new List<float>();
    }
}

public class GameConfig
{
    public int Map => (int)_gameData.SelectToken("map");
    public string GameMode => _gameData.SelectToken("gameMode").ToString().ToUpper().Replace(" ", string.Empty);
    public string DataPackage => (string)_gameData.SelectToken("dataPackage");

    private JToken _gameData;

    public GameConfig(JToken gameData)
    {
        _gameData = gameData;
    }
}

public class PlayerConfig
{
    public long PlayerID { get; private set; }
    public string Rank { get; private set; }
    public string Name { get; private set; }
    public string Champion { get; private set; }
    public TeamId Team { get; private set; }
    public short Skin { get; private set; }
    public string Summoner1 { get; private set; }
    public string Summoner2 { get; private set; }
    public short Ribbon { get; private set; }
    public int Icon { get; private set; }
    public string BlowfishKey { get; private set; }
    public RuneCollection Runes { get; private set; }
    public TalentInventory Talents { get; private set; }

    private JToken _playerData;

    private static ILog _logger = LoggerProvider.GetLogger();
    public PlayerConfig(JToken playerData)
    {
        _playerData = playerData;
        PlayerID = (long)playerData.SelectToken("playerId");
        Rank = (string)playerData.SelectToken("rank");
        Name = (string)playerData.SelectToken("name");
        Champion = (string)playerData.SelectToken("champion");

        Team = TeamId.TEAM_PURPLE;
        if (((string)playerData.SelectToken("team")).ToLower().Equals("blue"))
        {
            Team = TeamId.TEAM_BLUE;
        }

        Skin = (short)playerData.SelectToken("skin");
        Summoner1 = (string)playerData.SelectToken("summoner1");
        Summoner2 = (string)playerData.SelectToken("summoner2");
        Ribbon = (short)playerData.SelectToken("ribbon");
        Icon = (int)playerData.SelectToken("icon");
        BlowfishKey = (string)playerData.SelectToken("blowfishKey");
    }

    public void LoadTalentsAndRunes()
    {
        Runes = new RuneCollection();
        var runes = _playerData.SelectToken("runes");
        if (runes != null)
        {
            foreach (JProperty runeCategory in runes)
            {
                Runes.Add(Convert.ToInt32(runeCategory.Name), Convert.ToInt32(runeCategory.Value));
            }
        }
        else
        {
            _logger.Warn($"No runes found for player {PlayerID}!");
        }

        Talents = new TalentInventory();
        var talents = _playerData.SelectToken("talents");
        if (talents != null)
        {
            foreach (JProperty talent in talents)
            {
                byte level = 1;
                try
                {
                    level = talent.Value.Value<byte>();
                }
                catch
                {
                    _logger.Warn($"Invalid Talent Rank for Talent {talent.Name}! Please use ranks between 1 and {byte.MaxValue}! Defaulting to Rank 1...");
                }
                Talents.Add(talent.Name, level);
            }
        }
    }
}
