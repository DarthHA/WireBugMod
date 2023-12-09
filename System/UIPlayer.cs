using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using WireBugMod.UI;

namespace WireBugMod.System
{
    public class UIPlayer : ModPlayer
    {
        public string SkillName1 = "HuntingEdge";
        public string SkillName2 = "PowerSheathe";
        public string SwitchSkillName1 = "SerenePose";
        public string SwitchSkillName2 = "SakuraSlash";

        public int ProgressTimer = 10;

        public static Vector2 TopLeft = new Vector2(300, 300);

        public override void SaveData(TagCompound tag)
        {
            tag.Add("SkillName1", SkillName1);
            tag.Add("SkillName2", SkillName2);
            tag.Add("SwitchSkillName1", SwitchSkillName1);
            tag.Add("SwitchSkillName2", SwitchSkillName2);
        }

        public override void LoadData(TagCompound tag)
        {
            SkillName1 = tag.GetString("SkillName1");
            SkillName2 = tag.GetString("SkillName2");
            SwitchSkillName1 = tag.GetString("SwitchSkillName1");
            SwitchSkillName2 = tag.GetString("SwitchSkillName2");
        }

        public void SetSkillName(string name, int index)
        {
            switch (index)
            {
                case 0:
                    SkillName1 = name;
                    break;
                case 1:
                    SkillName2 = name;
                    break;
                case 2:
                    SwitchSkillName1 = name;
                    break;
                case 3:
                    SwitchSkillName2 = name;
                    break;
            }
        }

        public string GetSkillName(int index)
        {
            switch (index)
            {
                case 0:
                    return SkillName1;
                case 1:
                    return SkillName2;
                case 2:
                    return SwitchSkillName1;
                case 3:
                    return SwitchSkillName2;
                default:
                    return "";
            }
        }

        public override void OnEnterWorld()
        {
            UIManager.ShouldUpdate = true;
            UIManager.Visible = false;
        }

        public override void PostUpdateMiscEffects()
        {
            if (ProgressTimer < 10) ProgressTimer++;
        }
    }
}
