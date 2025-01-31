﻿using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    class ZedWShadowBuff : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Buff ThisBuff;
        Minion Shadow;
        Particle currentIndicator;
        int previousIndicatorState;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            ThisBuff = buff;
            Shadow = unit as Minion;
            buff.SetStatusEffect(StatusFlags.Targetable, false);
            buff.SetStatusEffect(StatusFlags.Ghosted, true);
            AddParticleTarget(Shadow.Owner, Shadow, "zed_base_w_tar", Shadow);
            ApiEventManager.OnSpellCast.AddListener(this, Shadow.Owner.GetSpell("ZedShuriken"), QOnSpellCast);
            ApiEventManager.OnSpellPostCast.AddListener(this, Shadow.Owner.GetSpell("ZedShuriken"), QOnSpellPostCast);
            ApiEventManager.OnSpellCast.AddListener(this, Shadow.Owner.GetSpell("ZedPBAOEDummy"), EOnSpellCast);
            currentIndicator = AddParticleTarget(Shadow.Owner, Shadow.Owner, "zed_shadowindicatorfar", Shadow, buff.Duration, flags: FXFlags.TargetDirection);
            if (Shadow.Owner.HasBuff("ZedShuriken"))
            {
                QOnSpellCast(ownerSpell);
            }
            if (Shadow.Owner.HasBuff("ZedPBAOEDummy"))
            {
                EOnSpellCast(ownerSpell);
            }
        }
        public void QOnSpellCast(Spell spell)
        {
            if (Shadow != null && !Shadow.IsDead)
            {
                PlayAnimation(Shadow, "Spell1");
                var targetPos = new Vector2(Shadow.Owner.GetSpell("ZedShuriken").CastInfo.TargetPositionEnd.X, Shadow.Owner.GetSpell("ZedShuriken").CastInfo.TargetPositionEnd.Z);
                FaceDirection(targetPos, Shadow, true);
            }
        }

        public void QOnSpellPostCast(Spell spell)
        {
            if (Shadow != null && !Shadow.IsDead)
            {
                var targetPos = GetPointFromUnit(Shadow, 950f);
                FaceDirection(targetPos, Shadow, true);
                SpellCast(Shadow.Owner, 1, SpellSlotType.ExtraSlots, targetPos, Vector2.Zero, true, Shadow.Position);
            }
        }
        public void EOnSpellCast(Spell spell)
        {
            if (Shadow != null && !Shadow.IsDead)
            {
                PlayAnimation(Shadow, "Spell3", 0.5f);
                AddParticle(Shadow.Owner, null, "Zed_Base_E_cas.troy", Shadow.Position);
                SpellCast(Shadow.Owner, 2, SpellSlotType.ExtraSlots, true, Shadow, Vector2.Zero);
            }
        }
        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            if (Shadow != null && !Shadow.IsDead)
            {
                if (currentIndicator != null)
                {
                    currentIndicator.SetToRemove();
                }

                SetStatus(Shadow, StatusFlags.NoRender, true);
                AddParticle(Shadow.Owner, null, "zed_base_clonedeath", Shadow.Position);
                Shadow.TakeDamage(Shadow.Owner, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
            }
        }

        public int GetIndicatorState()
        {
            var dist = Vector2.Distance(Shadow.Owner.Position, Shadow.Position);
            var state = 0;

            if (!Shadow.Owner.HasBuff("ZedW2"))
            {
                return state;
            }

            if (dist >= 1000.0f)
            {
                state = 0;
            }
            else if (dist >= 800.0f)
            {
                state = 1;
            }
            else if (dist >= 0f)
            {
                state = 2;
            }

            return state;
        }

        public string GetIndicatorName(int state)
        {
            switch (state)
            {
                case 0:
                    {
                        return "zed_shadowindicatorfar";
                    }
                case 1:
                    {
                        return "zed_shadowindicatormed";
                    }
                case 2:
                    {
                        return "zed_shadowindicatornearbloop";
                    }
                default:
                    {
                        return "zed_shadowindicatorfar";
                    }
            }
        }

        public void OnUpdate(float diff)
        {
            if (Shadow != null && !Shadow.IsDead)
            {
                int state = GetIndicatorState();
                if (state != previousIndicatorState)
                {
                    previousIndicatorState = state;
                    if (currentIndicator != null)
                    {
                        currentIndicator.SetToRemove();
                    }

                    currentIndicator = AddParticleTarget(Shadow.Owner, Shadow.Owner, GetIndicatorName(state), Shadow, ThisBuff.Duration - ThisBuff.TimeElapsed, flags: FXFlags.TargetDirection);
                }
            }
        }
    }
}