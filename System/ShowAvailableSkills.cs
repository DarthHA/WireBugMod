using Terraria.ModLoader;

namespace WireBugMod.System
{
    public class ShowAvailableSkills : GlobalItem
    {
        /*
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.GetWeaponType() == WeaponType.None) return;

            string result = "";
            result += Language.GetTextValue("Mods.WireBugMod.SkillInfos.AvailableSkill") + "\n";
            string skill1 = "", skill2 = "";
            foreach (BaseSkill skill in SkillLoader.skills)
            {
                if (SkillUtils.CheckSameSwitchSkillAsPlayer(skill, Main.LocalPlayer.GetModPlayer<WireBugPlayer>()) && SkillUtils.CheckTheSkillOfWeapon(skill, item.GetWeaponType()))
                {
                    if (skill.Keybind == 1)
                    {
                        skill1 = string.Format(Language.GetTextValue("Mods.WireBugMod.SkillInfos.SkillName1"), Language.GetTextValue("Mods.WireBugMod.Skills." + skill.SkillName)) + "\n";
                        skill1 += Language.GetTextValue("Mods.WireBugMod.SkillInfos." + skill.SkillName) + "\n";
                        skill1 += string.Format(Language.GetTextValue("Mods.WireBugMod.SkillInfos.BugRecoverySpeed"), (skill.Cooldown / 60f).ToString()) + "\n";
                        skill1 += string.Format(Language.GetTextValue("Mods.WireBugMod.SkillInfos.BugCost"), skill.UseBugCount.ToString());
                    }
                    else if (skill.Keybind == 2)
                    {
                        skill2 = string.Format(Language.GetTextValue("Mods.WireBugMod.SkillInfos.SkillName2"), Language.GetTextValue("Mods.WireBugMod.Skills." + skill.SkillName)) + "\n";
                        skill2 += Language.GetTextValue("Mods.WireBugMod.SkillInfos." + skill.SkillName) + "\n";
                        skill2 += string.Format(Language.GetTextValue("Mods.WireBugMod.SkillInfos.BugRecoverySpeed"), (skill.Cooldown / 60f).ToString()) + "\n";
                        skill2 += string.Format(Language.GetTextValue("Mods.WireBugMod.SkillInfos.BugCost"), skill.UseBugCount.ToString());
                    }
                }
                if (skill1 != "" && skill2 != "") break;
            }

            result += skill1 + "\n" + skill2;

            tooltips.Add(new TooltipLine(Mod, "Showme", result.ToString()));

        }
        */
    }
}
