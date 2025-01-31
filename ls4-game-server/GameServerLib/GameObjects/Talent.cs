﻿using GameServerCore.Scripting.CSharp;
using static GameServerCore.Content.HashFunctions;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class Talent
    {
        public string Name { get; }
        public byte Rank { get; }
        public ITalentScript Script { get; }
        public uint ScriptNameHash { get; private set; }
        public IEventSource ParentScript => null;

        public Talent(string name, byte level)
        {
            Name = name;
            Rank = Math.Min(level, (byte)3);
            Script = Game.ScriptEngine.CreateObject<ITalentScript>("Talents", $"Talent_{name}") ?? new EmptyTalentScript();
            ScriptNameHash = HashString(name);
        }
    }
}
