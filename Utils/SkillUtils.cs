using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using WireBugMod.Projectiles.SBlade;
using WireBugMod.System;
using WireBugMod.System.Skill;

namespace WireBugMod.Utils
{
    public static class SkillUtils
    {
        /// <summary>
        /// 获取武器种类
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static WeaponType GetWeaponType(this Item item)
        {
            if (item.IsAir) return WeaponType.None;
            if (WeaponSkillData.WeaponDictionary.TryGetValue(item.type, out WeaponType type)) return type;
            return WeaponType.None;
        }

        /// <summary>
        /// 判断是否为当前可用的技能
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="modplayer"></param>
        /// <returns></returns>
        public static bool CheckAvailableSkill(BaseSkill skill, WireBugPlayer modplayer)
        {
            UIPlayer modplayer2 = modplayer.Player.GetModPlayer<UIPlayer>();
            if (modplayer.SwitchSkill)
            {
                if ((skill.SkillName == modplayer2.SwitchSkillName1 && modplayer.JustPressedWireSkill1) || (skill.SkillName == modplayer2.SwitchSkillName2 && modplayer.JustPressedWireSkill2)) return true;
            }
            else
            {
                if ((skill.SkillName == modplayer2.SkillName1 && modplayer.JustPressedWireSkill1) || (skill.SkillName == modplayer2.SkillName2 && modplayer.JustPressedWireSkill2)) return true;
            }
            return false;
        }

        /// <summary>
        /// 判断某技能是否属于某种武器
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="weaponType"></param>
        /// <returns></returns>
        public static bool CheckTheSkillOfWeapon(BaseSkill skill, WeaponType weaponType)
        {
            if (weaponType == WeaponType.None) return false;
            if (skill.weaponType.Contains(weaponType))
            {
                return true;
            }
            return false;
        }

        public static bool CheckSilkbindProj(int type)
        {
            return type == ModContent.ProjectileType<PiercingBindProj>() || type== ModContent.ProjectileType<TwinVineProj>();
        }
    }
}
