﻿using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.API;
using GameServerLib.GameObjects.AttackableUnits;

namespace Buffs
{
    internal class VayneSilveredBolts : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.DAMAGE,
            BuffAddType = BuffAddType.STACKS_AND_RENEWS,
            MaxStacks = 3
        };
        public StatsModifier StatsModifier { get; private set; }

        Particle p;
        AttackableUnit Unit;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            Unit = unit;
            switch (buff.StackCount)
            {
                case 1:
                    p = AddParticleTarget(ownerSpell.CastInfo.Owner, null, "vayne_W_ring1.troy", unit, float.MaxValue);
                    break;
                case 2:
                    RemoveParticle(p);
                    p = AddParticleTarget(ownerSpell.CastInfo.Owner, null, "vayne_W_ring2.troy", unit, float.MaxValue);
                    break;
                case 3:
                    AddParticleTarget(ownerSpell.CastInfo.Owner, null, "vayne_W_tar.troy", unit);
                    TargetTakeDamage(ownerSpell);
                    buff.DeactivateBuff();
                    break;
            }
        }
        public void TargetTakeDamage(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            float percentHealthDMG = Unit.Stats.HealthPoints.Total * (0.04f + 0.1f * (owner.GetSpell("VayneSilveredBolts").CastInfo.SpellLevel - 1));
            float flatDMG = 20 + 10f * (owner.GetSpell("VayneSilveredBolts").CastInfo.SpellLevel - 1);
            float damage = flatDMG + percentHealthDMG;

            Unit.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            RemoveParticle(p);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}