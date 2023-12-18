using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.System.Skill
{
    public class SkillSystem : ModSystem
    {
        public override void Load()
        {
            SkillLoader.Load(Mod);
        }
        public override void Unload()
        {
            SkillLoader.Unload();
        }


        public static void SelectAndUseSkill(WireBugPlayer modplayer)
        {
            foreach (BaseSkill skill in SkillLoader.skills)
            {
                if (CheckUseCondition(modplayer, skill))
                {
                    int UseBugCount = skill.UseBugCount;
                    List<int> BugAvailable = new();
                    for (int i = 0; i < modplayer.bugs.Count; i++)
                    {
                        if (modplayer.bugs[i].IsReady())
                        {
                            BugAvailable.Add(i);
                        }
                    }
                    if (BugAvailable.Count >= UseBugCount)
                    {
                        int bug2 = BugAvailable.Count > 1 ? BugAvailable[1] : -1;
                        if (skill.OnUse(modplayer, BugAvailable[0], bug2))           //被成功使用
                        {
                            for (int i = 0; i < UseBugCount; i++)
                            {
                                modplayer.bugs[BugAvailable[i]].SetCD(skill.Cooldown, 1 - modplayer.BugRecoveryStat);
                            }
                            modplayer.Player.direction = Math.Sign(Main.MouseWorld.X - modplayer.Player.Center.X + 0.01f);
                            modplayer.Player.RemoveAllGrapplingHooks();
                            modplayer.Player.mount.Dismount(modplayer.Player);
                            modplayer.Player.ClearBuff(BuffID.Gravitation);
                            break;
                        }
                    }

                }
            }
        }

        public static void ForceUseWireDash(WireBugPlayer modplayer, float PunishmentModifier = 1)
        {
            foreach (BaseSkill skill in SkillLoader.skills)
            {
                if (!skill.NotWireDash && skill.UseCondition(modplayer))
                {
                    int UseBugCount = skill.UseBugCount;
                    List<int> BugAvailable = new();
                    for (int i = 0; i < modplayer.bugs.Count; i++)
                    {
                        if (modplayer.bugs[i].IsReady())
                        {
                            BugAvailable.Add(i);
                        }
                    }
                    if (BugAvailable.Count >= UseBugCount)
                    {
                        int bug2 = BugAvailable.Count > 1 ? BugAvailable[1] : -1;
                        if (skill.OnUse(modplayer, BugAvailable[0], bug2))           //被成功使用
                        {
                            for (int i = 0; i < UseBugCount; i++)
                            {
                                modplayer.bugs[BugAvailable[i]].SetCD(skill.Cooldown, PunishmentModifier - modplayer.BugRecoveryStat);
                            }
                            modplayer.Player.direction = Math.Sign(Main.MouseWorld.X - modplayer.Player.Center.X + 0.01f);
                            modplayer.Player.RemoveAllGrapplingHooks();
                            modplayer.Player.mount.Dismount(modplayer.Player);
                            modplayer.Player.ClearBuff(BuffID.Gravitation);
                            break;
                        }
                    }

                }
            }
        }

        private static bool CheckUseCondition(WireBugPlayer modplayer, BaseSkill skill)
        {
            Player player = modplayer.Player;
            bool firstcheck = false;
            if (modplayer.JustPressedWireDash)
            {
                if (!skill.NotWireDash) firstcheck = true;
            }
            else
            {
                if (SkillUtils.CheckAvailableSkill(skill, modplayer) && SkillUtils.CheckTheSkillOfWeapon(skill, player.HeldItem.GetWeaponType()))
                {
                    firstcheck = true;
                }
            }

            if (firstcheck)
            {
                return skill.UseCondition(modplayer);
            }
            return false;
        }

    }
}
