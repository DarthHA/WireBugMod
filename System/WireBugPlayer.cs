using Microsoft.Xna.Framework;
using rail;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using WireBugMod.Buffs;
using WireBugMod.Projectiles;
using WireBugMod.Projectiles.Weapons;
using WireBugMod.System.Skill;
using WireBugMod.UI;
using WireBugMod.Utils;

namespace WireBugMod.System
{
    public class BugUnit
    {
        public int Cooldown = 60;
        public int MaxTime = 60;
        public bool CanUse = true;

        public int ProgressTimer = 10;
        public void Update()
        {
            if (Cooldown > MaxTime)
            {
                Cooldown = MaxTime;
            }
            if (CanUse)
            {
                if (Cooldown > 0)
                {
                    Cooldown--;
                    if (Cooldown == 0)
                    {
                        ProgressTimer = -10;
                    }
                }
            }

            if (ProgressTimer > 0 && ProgressTimer < 10)
            {
                ProgressTimer++;
            }
            if (ProgressTimer < 0)
            {
                ProgressTimer++;
            }
        }
        public bool IsReady()
        {
            return Cooldown <= 0 && CanUse;
        }
        public void SetCD(int time, float modifier)
        {
            ProgressTimer = 1;
            CanUse = false;
            time = (int)(time * modifier);
            MaxTime = time;
            Cooldown = time;
        }
        public void Reset()
        {
            ProgressTimer = 10;
            CanUse = true;
            Cooldown = 60;
            MaxTime = 60;
        }
    }

    public class WireBugPlayer : ModPlayer
    {
        public bool HasWireBug = false;

        /// <summary>
        /// 操作锁定
        /// </summary>
        public bool LockInput = false;

        public bool PressingWireDash = false;
        public bool JustPressedWireDash = false;
        public bool JustPressedWireSkill1 = false;
        public bool PressingWireSkill1 = false;
        public bool JustPressedWireSkill2 = false;
        public bool JustPressedSwitchSkill = false;


        public bool SwitchSkill = false;


        public List<BugUnit> bugs = new();

        public float BugRecoveryStat = 0;

        public int SkillSwitchCooldown = 0;

        public int ImmunityTimer = 0;
        public int ForbiddenUseItemTimer = 0;
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            PressingWireDash = WireBugMod.FlyBugKey.Current;
            PressingWireSkill1 = WireBugMod.WireSkillKey1.Current;
            JustPressedWireDash = WireBugMod.FlyBugKey.JustPressed;
            JustPressedWireSkill1 = WireBugMod.WireSkillKey1.JustPressed;
            JustPressedWireSkill2 = WireBugMod.WireSkillKey2.JustPressed;
            JustPressedSwitchSkill = WireBugMod.SwitchSkillKey.JustPressed;
        }

        public override void SetControls()
        {
            if (LockInput)
            {
                Player.controlDown = false;
                Player.controlHook = false;
                Player.controlJump = false;
                Player.controlLeft = false;
                Player.controlUp = false;
                Player.controlThrow = false;
                Player.controlUseTile = false;
                Player.controlUseItem = false;
                Player.controlTorch = false;
                Player.controlRight = false;
                Player.controlMount = false;
            }
            if (ForbiddenUseItemTimer > 0)
            {
                Player.controlUseTile = false;
                Player.controlUseItem = false;
                Player.controlTorch = false;
            }
        }

        public override void PostUpdateMiscEffects()
        {
            if (SkillSwitchCooldown > 0)
            {
                SkillSwitchCooldown--;
            }
            if (ForbiddenUseItemTimer > 0 && !LockInput)
            {
                ForbiddenUseItemTimer--;
            }

            if (ImmunityTimer > 0)
            {
                Player.buffImmune[BuffID.Stoned] = true;
                Player.buffImmune[BuffID.Webbed] = true;
                Player.buffImmune[BuffID.Frozen] = true;
                ImmunityTimer--;
            }

            if (HasWireBug)
            {
                CheckAndUpdateBugCount();
                CheckLock();

                bool PressAnyKey = PressingWireDash ||
                PressingWireSkill1 ||
                JustPressedWireDash ||
                JustPressedWireSkill1 ||
                JustPressedWireSkill2 ||
                JustPressedSwitchSkill;

                bool PressOnlyWireDash = PressingWireDash && !(
                PressingWireSkill1 ||
                JustPressedWireDash ||
                JustPressedWireSkill1 ||
                JustPressedWireSkill2 ||
                JustPressedSwitchSkill);

                if (PressAnyKey)
                {
                    if (!LockInput && !Player.shimmerWet && !UIManager.Visible)        //可以操作,没有在微光里,没有被定住,且没有同时使用物品，没有打开翔虫UI
                    {
                        if (!Player.CCed)
                        {
                            if (!Player.ItemAnimationActive)
                            {
                                if (JustPressedSwitchSkill)         //优先判定迅速切换
                                {
                                    PressSwitchSkillKey();
                                }
                                else
                                {
                                    //判断先后：可以操作，检查是否有虫可用,按下按键
                                    SkillSystem.SelectAndUseSkill(this);
                                }
                            }
                            else
                            {
                                if (PressOnlyWireDash)
                                {
                                    Player.itemTime = Player.itemTimeMax;
                                    Player.itemAnimation = Player.itemAnimationMax;
                                    Player.controlUseItem = false;
                                    SkillSystem.ForceUseWireDash(this, 1.25f);
                                    ForbiddenUseItemTimer = 30;
                                }
                            }
                        }
                        else
                        {
                            if (PressOnlyWireDash)         //Debuff受身
                            {
                                Player.frozen = false;
                                Player.webbed = false;
                                Player.stoned = false;
                                Player.ClearBuff(BuffID.Frozen);
                                Player.ClearBuff(BuffID.Webbed);
                                Player.ClearBuff(BuffID.Stoned);

                                SkillSystem.ForceUseWireDash(this, 3);

                                Player.SetIFrame(60);
                                ImmunityTimer = 60;
                                ForbiddenUseItemTimer = 30;

                                for (int i = 0; i < 20; i++)
                                {
                                   SkillUtils.GenDust(Player.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), Main.rand.Next(8), 1 + Main.rand.NextFloat() * 0.5f);
                                }
                            }
                        }
                    }
                }

                ModifyLockPlayer();
                UpdateBugs();
                BugStatusShow();

            }
            else         //清除弹幕
            {
                foreach(Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == Player.whoAmI && proj.ModProjectile != null)
                    {
                        if (proj.ModProjectile is BaseSkillProj || proj.ModProjectile is BaseWeaponProj)
                        {
                            proj.Kill();
                        }
                    }
                }
            }


        }

        public override void ResetEffects()
        {
            BugRecoveryStat = 0;
            HasWireBug = false;
            LockInput = false;
        }

        public override void UpdateDead()
        {
            HasWireBug = false;
            LockInput = false;
            bugs.Clear();
        }

        public override void LoadData(TagCompound tag)
        {
            SwitchSkill = tag.GetBool("SwitchSkill");
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("SwitchSkill", SwitchSkill);
        }

        private void CheckAndUpdateBugCount()
        {
            int Count = 3;          //TODO:可以更改

            if (bugs.Count > Count)
            {
                int RemoveCount = bugs.Count - Count;
                for (int i = 0; i < RemoveCount; i++)        //移除最后的翔虫
                {
                    bugs.RemoveAt(bugs.Count - 1);
                }
            }
            else if (bugs.Count < Count)
            {
                int AddCount = Count - bugs.Count;
                for (int i = 0; i < AddCount; i++)
                {
                    bugs.Add(new BugUnit());
                }
            }
        }
        private void UpdateBugs()
        {
            foreach (BugUnit bug in bugs)
            {
                bug.Update();
            }
        }
        private void ModifyLockPlayer()
        {
            if (LockInput)
            {
                Player.noKnockback = true;     //免疫击退，增加下坠速度，取消水中阻力,取消反重力,取消潜行
                Player.maxFallSpeed += 30;
                Player.merman = true;
                Player.shroomiteStealth = false;
                Player.vortexStealthActive = false;
                Player.ClearBuff(BuffID.Gravitation);
                if (!Player.accMerman && !Player.forceMerman)
                {
                    Player.hideMerman = true;
                }
            }
        }
        private void CheckLock()
        {
            foreach (BugUnit bug in bugs)
            {
                bug.CanUse = true;
            }
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.ModProjectile != null && proj.ModProjectile is BaseSkillProj)
                {
                    BaseSkillProj modproj = proj.ModProjectile as BaseSkillProj;
                    LockInput = LockInput || modproj.LockInput;
                    if (modproj.LockBug)
                    {
                        if (modproj.UsedBugID1 >= 0 && modproj.UsedBugID1 < bugs.Count)
                        {
                            bugs[modproj.UsedBugID1].CanUse = false;
                        }
                        if (modproj.UsedBugID2 >= 0 && modproj.UsedBugID2 < bugs.Count)
                        {
                            bugs[modproj.UsedBugID2].CanUse = false;
                        }
                    }

                    if (modproj.LockAllBug)
                    {
                        foreach (BugUnit bug in bugs)
                        {
                            bug.CanUse = false;
                        }
                        break;
                    }
                }
            }

        }
        private void BugStatusShow()
        {
            Player.AddBuff(ModContent.BuffType<CDBuff1>(), 2);
            Player.AddBuff(ModContent.BuffType<CDBuff2>(), 2);
            if (bugs.Count > 2)
            {
                Player.AddBuff(ModContent.BuffType<CDBuff3>(), 2);
                if (bugs.Count > 3)
                {
                    Player.AddBuff(ModContent.BuffType<CDBuff4>(), 2);
                }
            }

            Player.AddBuff(ModContent.BuffType<SwitchSkillBuff>(), 2);
        }

        private void PressSwitchSkillKey()
        {
            if (SkillSwitchCooldown == 0)
            {
                SkillSwitchCooldown = 60;
                Player.SetIFrame(10);
                SwitchSkill = !SwitchSkill;
                Player.GetModPlayer<UIPlayer>().ProgressTimer = 0;
                Color color = Color.Yellow;
                if (SwitchSkill)
                {
                    color = Color.Cyan;
                }
                float start = MathHelper.TwoPi * Main.rand.NextFloat();
                for (int i = 0; i < 30; i++)
                {
                    float r = start + MathHelper.TwoPi * i / 30f;
                    int dusttmp = Dust.NewDust(Player.Center, 1, 1, DustID.BubbleBurst_White);
                    Main.dust[dusttmp].position = Player.Center;
                    Main.dust[dusttmp].color = color;
                    Main.dust[dusttmp].velocity = r.ToRotationVector2() * 5;
                    Main.dust[dusttmp].scale = 1.5f;
                    Main.dust[dusttmp].noGravity = true;
                    Main.dust[dusttmp].noLight = false;
                }
            }
        }
    }
}