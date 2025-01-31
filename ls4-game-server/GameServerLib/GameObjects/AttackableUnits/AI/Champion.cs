﻿using System.Numerics;
using GameServerCore.NetInfo;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.Inventory;
using LeagueSandbox.GameServer.API;
using LeaguePackets.Game.Events;
using GameServerLib.GameObjects.AttackableUnits;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Logging;
using log4net;
using LeagueSandbox.GameServer.Content;
using GameServerLib.Handlers;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class Champion : ObjAIBase
    {
        private float _championHitFlagTimer;
        private static ILog _logger = LoggerProvider.GetLogger();
        private float _currentShield = 0;
        /// <summary>
        /// Player number ordered by the config file.
        /// </summary>
        public int ClientId { get; private set; }
        private uint _playerHitId;
        private List<ToolTipData> _tipsChanged;
        public Shop Shop { get; protected set; }
        public float RespawnTimer { get; private set; }
        public int DeathSpree { get; set; } = 0;
        public int KillSpree { get; set; } = 0;
        public float GoldFromMinions { get; set; }
        public RuneCollection RuneList { get; }
        public TalentInventory TalentInventory { get; set; }
        public ChampionStats ChampStats { get; private set; } = new ChampionStats();

        public byte SkillPoints { get; set; }

        public override bool SpawnShouldBeHidden => false;

        public List<EventHistoryEntry> EventHistory { get; } = new List<EventHistoryEntry>();

        public Champion(Game game,
                        string model,
                        RuneCollection runeList,
                        TalentInventory talentInventory,
                        ClientInfo clientInfo,
                        uint netId = 0,
                        TeamId team = TeamId.TEAM_BLUE,
                        Stats stats = null)
            : base(game, model, clientInfo.Name, 30, new Vector2(), 1200, clientInfo.SkinNo, netId, team, stats)
        {
            //TODO: Champion.ClientInfo?
            ClientId = clientInfo.ClientId;
            RuneList = runeList;

            TalentInventory = talentInventory;
            Shop = Shop.CreateShop(this, game);

            AddGold(null, GlobalData.ObjAIBaseVariables.StartingGold, false);
            Stats.GoldPerGoldTick.BaseValue = GlobalData.ChampionVariables.AmbientGoldAmount;
            Stats.IsGeneratingGold = false;

            //TODO: automaticaly rise spell levels with CharData.SpellLevelsUp

            Spells[(int)SpellSlotType.SummonerSpellSlots] = new Spell(game, this, clientInfo.SummonerSkills[0], (int)SpellSlotType.SummonerSpellSlots);
            Spells[(int)SpellSlotType.SummonerSpellSlots].LevelUp();
            Spells[(int)SpellSlotType.SummonerSpellSlots + 1] = new Spell(game, this, clientInfo.SummonerSkills[1], (int)SpellSlotType.SummonerSpellSlots + 1);
            Spells[(int)SpellSlotType.SummonerSpellSlots + 1].LevelUp();

            Spells[(int)SpellSlotType.BluePillSlot] = new Spell(game, this,
            _game.ItemManager.GetItemType(_game.Map.MapScript.MapScriptMetadata.RecallSpellItemId).SpellName, (int)SpellSlotType.BluePillSlot);
            Stats.SetSpellEnabled((byte)SpellSlotType.BluePillSlot, true);
            SkillPoints++;

            Replication = new ReplicationHero(this);

            _tipsChanged = new List<ToolTipData>();

            if (clientInfo.PlayerId == -1)
            {
                IsBot = true;
            }
        }

        public void ApplyShield(AttackableUnit unit, float shieldAmount, bool isUnique, bool isMagical, bool isPhysical)
        {
            var shieldDelta = shieldAmount - _currentShield;
            if (shieldDelta != 0)
            {
                _currentShield = shieldAmount;
                unit.Stats.CurrentShield += shieldDelta;
                _game.PacketNotifier.NotifyShield(unit, shieldAmount, isUnique, isMagical, isPhysical);
            }
        }

        public void AddGold(AttackableUnit source, float gold, bool notify = true)
        {
            Stats.Gold += gold;
            if (notify && source != null)
            {
                _game.PacketNotifier.NotifyUnitAddGold(this, source, gold);
            }
        }

        public override void OnAdded()
        {
            _game.ObjectManager.AddChampion(this);
            base.OnAdded();
            TalentInventory.Initialize(this);

            var bluePill = _itemManager.GetItemType(_game.Map.MapScript.MapScriptMetadata.RecallSpellItemId);
            Inventory.SetExtraItem(7, bluePill);

            // Runes
            byte runeItemSlot = 14;
            foreach (var rune in RuneList.Runes)
            {
                var runeItem = _itemManager.GetItemType(rune.Value);
                var newRune = Inventory.SetExtraItem(runeItemSlot, runeItem);
                AddStatModifier(runeItem);
                runeItemSlot++;
            }
            Stats.SetSummonerSpellEnabled(0, true);
            Stats.SetSummonerSpellEnabled(1, true);

            //Change this to send only a single LevelUp call in case of multiple levels.
            while (Stats.Level < _game.Map.MapScript.MapScriptMetadata.InitialLevel)
            {
                LevelUp(true);
            }
        }

        protected override void OnSpawn(int userId, TeamId team, bool doVision)
        {
            var peerInfo = _game.PlayerManager.GetClientInfoByChampion(this);
            _game.PacketNotifier.NotifyS2C_CreateHero(peerInfo, userId, doVision);
            _game.PacketNotifier.NotifyAvatarInfo(peerInfo, userId);

            bool ownChamp = peerInfo.ClientId == userId;
            if (ownChamp)
            {
                // Buy blue pill
                var itemInstance = Inventory.GetItem(7);
                _game.PacketNotifier.NotifyBuyItem(this, itemInstance);

                // Set spell levels
                foreach (var spell in Spells)
                {
                    var castInfo = spell.Value.CastInfo;
                    if (castInfo.SpellLevel > 0)
                    {
                        // NotifyNPC_UpgradeSpellAns has no effect here
                        _game.PacketNotifier.NotifyS2C_SetSpellLevel(userId, NetId, castInfo.SpellSlot, castInfo.SpellLevel);

                        float currentCD = spell.Value.CurrentCooldown;
                        float totalCD = spell.Value.GetCooldown();
                        if (currentCD > 0)
                        {
                            _game.PacketNotifier.NotifyCHAR_SetCooldown(this, castInfo.SpellSlot, currentCD, totalCD, userId);
                        }
                    }
                }
            }
        }

        public override void OnRemoved()
        {
            base.OnRemoved();
            _game.ObjectManager.RemoveChampion(this);
        }

        public int GetTeamSize()
        {
            var teamSize = 0;
            foreach (var player in _game.Config.Players)
            {
                if (player.Team == Team)
                {
                    teamSize++;
                }
            }
            return teamSize;
        }

        public Vector2 GetSpawnPosition(int index)
        {
            var teamSize = GetTeamSize();

            if (_game.Map.PlayerSpawnPoints[Team].ContainsKey(teamSize))
            {
                return _game.Map.PlayerSpawnPoints[Team][teamSize][index];
            }

            if (_game.Map.PlayerSpawnPoints[Team].ContainsKey(1) && _game.Map.PlayerSpawnPoints[Team][1].ContainsKey(1))
            {
                return _game.Map.PlayerSpawnPoints[Team][1][1];
            }
            //TODO: wrap in try {} catch
            return _game.Map.MapScript.GetFountainPosition(Team);
        }

        public Vector2 GetRespawnPosition()
        {
            //TODO: wrap in try {} catch
            return _game.Map.MapScript.GetFountainPosition(Team);
        }

        public override Spell LevelUpSpell(byte slot)
        {
            if (SkillPoints == 0)
            {
                return null;
            }

            SkillPoints--;

            return base.LevelUpSpell(slot);
        }

        public void AddToolTipChange(ToolTipData data)
        {
            if (!_tipsChanged.Contains(data))
            {
                _tipsChanged.Add(data);
            }
        }

        public void ClearToolTipsChanged()
        {
            _tipsChanged.Clear();
        }

        float _goldTimer;
        float _EXPTimer;
        public override void Update(float diff)
        {
            base.Update(diff);

            if (Stats.IsGeneratingGold && Stats.GoldPerGoldTick.Total > 0)
            {
                _goldTimer -= diff;

                if (_goldTimer <= 0)
                {
                    AddGold(null, Stats.GoldPerGoldTick.Total, false);
                    _goldTimer = GlobalData.ChampionVariables.AmbientGoldInterval;
                }
            }
            else if (!Stats.IsGeneratingGold && _game.GameTime >= GlobalData.ObjAIBaseVariables.AmbientGoldDelay)
            {
                Stats.IsGeneratingGold = true;
                _logger.Debug("Generating Gold!");
            }

            if (_game.GameTime >= GlobalData.ChampionVariables.AmbientXPDelay)
            {
                _EXPTimer -= diff;
                if (_EXPTimer <= 0)
                {
                    AddExperience(GlobalData.ChampionVariables.AmbientXPAmount, false);
                    _EXPTimer = GlobalData.ChampionVariables.AmbientXPInterval;
                }
            }



            if (RespawnTimer > 0)
            {
                RespawnTimer -= diff;
                if (RespawnTimer <= 0)
                {
                    Respawn();
                }
            }

            if (_championHitFlagTimer > 0)
            {
                _championHitFlagTimer -= diff;
                if (_championHitFlagTimer <= 0)
                {
                    _championHitFlagTimer = 0;
                }
            }

            // TODO: Find out the best way to bulk send these for all champions (tool tip handler?).
            // League sends a single packet detailing every champion's tool tip changes.
            if (_tipsChanged.Count > 0)
            {
                _game.PacketNotifier.NotifyS2C_ToolTipVars(_tipsChanged);
                ClearToolTipsChanged();
            }
        }

        public void Respawn()
        {
            var spawnPos = GetRespawnPosition();
            SetPosition(spawnPos);
            float parToRestore = 0;
            // TODO: Find a better way to do this, perhaps through scripts. Otherwise, make sure all types are accounted for.
            if (Stats.ParType == PrimaryAbilityResourceType.MANA || Stats.ParType == PrimaryAbilityResourceType.Energy || Stats.ParType == PrimaryAbilityResourceType.Wind)
            {
                parToRestore = Stats.ManaPoints.Total;
            }
            Stats.CurrentMana = parToRestore;
            _game.PacketNotifier.NotifyHeroReincarnateAlive(this, parToRestore);
            Stats.CurrentHealth = Stats.HealthPoints.Total;
            IsDead = false;
            RespawnTimer = -1;
            SetDashingState(false, MoveStopReason.HeroReincarnate);
            ApiEventManager.OnResurrect.Publish(this);
        }

        public bool OnDisconnect()
        {
            this.StopMovement();
            this.SetWaypoints(_game.Map.PathingHandler.GetPath(Position, GetRespawnPosition(), PathfindingRadius));
            this.UpdateMoveOrder(OrderType.MoveTo, true);

            return true;
        }

        public void Recall()
        {
            var spawnPos = GetRespawnPosition();
            TeleportTo(spawnPos.X, spawnPos.Y);
        }

        public void AddExperience(float experience, bool notify = true)
        {
            if (experience > 0)
            {
                Stats.Experience += experience;

                if (notify)
                {
                    _game.PacketNotifier.NotifyUnitAddEXP(this, experience);
                }

                while (Stats.Experience >= _game.Map.MapData.ExpCurve[Stats.Level - 1] && LevelUp()) ;
            }
        }

        public override bool LevelUp(bool force = false)
        {
            var stats = Stats;
            var expMap = _game.Map.MapData.ExpCurve;

            if (force && stats.Level > 0)
            {
                Stats.Experience = expMap[Stats.Level - 1];
            }

            if (stats.Level < _game.Map.MapScript.MapScriptMetadata.MaxLevel && (stats.Level < 1 || (stats.Experience >= expMap[stats.Level - 1])))
            {
                if (stats.Level <= 18)
                {
                    SkillPoints++;
                }
                base.LevelUp(force);
                _logger.Debug($"Player {Name} leveled up to {stats.Level}");

                return true;
            }

            return false;
        }

        public void OnKill(DeathData deathData)
        {
            ApiEventManager.OnKillUnit.Publish(deathData.Killer, deathData);

            if (deathData.Unit is Minion)
            {
                ChampStats.MinionsKilled += 1;
                if (deathData.Unit.Team == TeamId.TEAM_NEUTRAL)
                {
                    ChampStats.NeutralMinionsKilled += 1;
                }

                var gold = deathData.Unit.Stats.GoldGivenOnDeath.Total;
                if (gold <= 0)
                {
                    return;
                }

                AddGold(deathData.Unit, gold);

                if (DeathSpree > 0)
                {
                    GoldFromMinions += gold;
                }

                if (GoldFromMinions >= 1000)
                {
                    GoldFromMinions -= 1000;
                    DeathSpree -= 1;
                }
            }
        }

        public override void Die(DeathData data)
        {
            IsDead = true;
            RespawnTimer = _game.Map.MapData.DeathTimes[Stats.Level] * 1000.0f;
            ChampStats.Deaths++;

            //Check order of operations
            _game.ObjectManager.StopTargeting(this);
            SetDashingState(false, MoveStopReason.Death);
            ApiEventManager.OnDeath.Publish(data.Unit, data);

            //TODO: Check this
            if (data.Killer is Champion)
            {
                ChampionDeathHandler.ProcessKill(data);
            }
            else if (EnemyAssistMarkers.LastOrDefault()?.Source is Champion ch)
            {
                data.Killer = ch;
                ChampionDeathHandler.ProcessKill(data);
            }

            _game.PacketNotifier.NotifyNPC_Hero_Die(data);
            EventHistory.Clear();
        }

        private T CreateEventForHistory<T>(AttackableUnit source, IEventSource sourceScript) where T : ArgsForClient, new()
        {
            if (source == null || sourceScript == null)
            {
                return null;
            }

            var entry = new EventHistoryEntry();
            entry.Timestamp = _game.GameTime / 1000f; // ?
            entry.Count = 1; //TODO: stack?
            entry.Source = source.NetId;
            var e = new T();
            entry.Event = (IEvent)e;

            e.ParentCasterNetID = entry.Source;
            e.OtherNetID = this.NetId;

            e.ScriptNameHash = 1;
            e.ParentScriptNameHash = sourceScript.ScriptNameHash;
            if (sourceScript.ParentScript != null)
            {
                e.ScriptNameHash = sourceScript.ScriptNameHash;
                e.ParentScriptNameHash = sourceScript.ParentScript.ScriptNameHash;
            }
            else if (sourceScript is Buff b && b.OriginSpell != null)
            {
                e.ScriptNameHash = sourceScript.ScriptNameHash;
                e.ParentScriptNameHash = (uint)b.OriginSpell.GetId();
            }

            e.EventSource = 0; // ?
            e.Unknown = 0; // ?
            e.SourceObjectNetID = 0;
            e.Bitfield = 0; // ?

            EventHistory.Add(entry);

            return e;
        }

        public override bool AddBuff(Buff b)
        {
            if (base.AddBuff(b))
            {
                CreateEventForHistory<OnBuff>(b.SourceUnit, b);
                return true;
            }
            return false;
        }

        public override void TakeHeal(AttackableUnit caster, float amount, IEventSource sourceScript = null)
        {
            base.TakeHeal(caster, amount, sourceScript);

            var e = CreateEventForHistory<OnCastHeal>(caster, sourceScript);
            if (e != null)
            {
                e.HealAmmount = amount;
            }
        }

        public override void TakeDamage(DamageData damageData, DamageResultType damageText, IEventSource sourceScript = null)
        {
            base.TakeDamage(damageData, damageText, sourceScript);

            _championHitFlagTimer = 15 * 1000; //15 seconds timer, so when you get executed the last enemy champion who hit you gets the gold
            _playerHitId = damageData.Attacker.NetId;
            //CORE_INFO("15 second execution timer on you. Do not get killed by a minion, turret or monster!");

            var e = CreateEventForHistory<OnDamageGiven>(damageData.Attacker, sourceScript);
            if (e != null)
            {
                if (damageData.DamageType == DamageType.DAMAGE_TYPE_MAGICAL)
                {
                    e.MagicalDamage = damageData.Damage;
                }
                else if (damageData.DamageType == DamageType.DAMAGE_TYPE_PHYSICAL)
                {
                    e.PhysicalDamage = damageData.Damage;
                }
                else if (damageData.DamageType == DamageType.DAMAGE_TYPE_TRUE)
                {
                    e.TrueDamage = damageData.Damage;
                }
                //TODO: handle mixed damage?
            }
        }

        public void UpdateSkin(int skinNo)
        {
            SkinID = skinNo;
        }

        public void IncrementScore(float points, ScoreCategory scoreCategory, ScoreEvent scoreEvent, bool doCallOut, bool notifyText = true)
        {
            Stats.Points += points;
            var scoreData = new ScoreData(this, points, scoreCategory, scoreEvent, doCallOut);
            _game.PacketNotifier.NotifyS2C_IncrementPlayerScore(scoreData);

            if (notifyText)
            {
                //TODO: Figure out what "Params" is exactly
                _game.PacketNotifier.NotifyDisplayFloatingText(new FloatingTextData(this, $"+{(int)points} Points", FloatTextType.Score, 1073741833), Team);
            }

            ApiEventManager.OnIncrementChampionScore.Publish(scoreData.Owner, scoreData);
        }
    }
}
