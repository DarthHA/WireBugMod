

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WireBugMod.System;
using WireBugMod.UI;

namespace WireBugMod.Buffs
{
    public class SwitchSkillBuff : ModBuff
    {
        public override string Texture => "WireBugMod/Images/SwitchSkill1";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;

        }

        public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
        {
            WireBugPlayer modplayer = Main.LocalPlayer.GetModPlayer<WireBugPlayer>();
            UIPlayer modplayer2 = Main.LocalPlayer.GetModPlayer<UIPlayer>();
            if (modplayer2.ProgressTimer < 10)
            {
                Texture2D texture1 = ModContent.Request<Texture2D>("WireBugMod/Images/SwitchSkill1").Value;
                Texture2D texture2 = ModContent.Request<Texture2D>("WireBugMod/Images/SwitchSkill2").Value;
                if (modplayer.SwitchSkill)
                {
                    texture1 = ModContent.Request<Texture2D>("WireBugMod/Images/SwitchSkill2").Value;
                    texture2 = ModContent.Request<Texture2D>("WireBugMod/Images/SwitchSkill1").Value;
                }
                float progress = modplayer2.ProgressTimer / 10f;
                float alpha1 = progress;
                float alpha2 = 1 - progress;
                float scale1 = 1 + (1 - progress) * 0.5f;
                float scale2 = 1 + progress * 0.5f;
                spriteBatch.Draw(texture1, drawParams.Position + texture1.Size() / 2, null, Color.White * alpha1, 0, texture1.Size() / 2, scale1, SpriteEffects.None, 0);
                spriteBatch.Draw(texture2, drawParams.Position + texture2.Size() / 2, null, Color.White * alpha2, 0, texture2.Size() / 2, scale2, SpriteEffects.None, 0);
            }
            else
            {
                Texture2D texture = ModContent.Request<Texture2D>("WireBugMod/Images/SwitchSkill1").Value;
                if (modplayer.SwitchSkill)
                {
                    texture = ModContent.Request<Texture2D>("WireBugMod/Images/SwitchSkill2").Value;
                }
                float scale = 1;
                if (drawParams.MouseRectangle.Contains((Main.MouseWorld - Main.screenPosition).ToPoint())) scale = 1.2f;
                spriteBatch.Draw(texture, drawParams.Position + texture.Size() / 2, null, Color.White, 0, texture.Size() / 2, scale, SpriteEffects.None, 0);
            }

            return false;
        }
        public override bool RightClick(int buffIndex)
        {
            WireBugPlayer modplayer = Main.LocalPlayer.GetModPlayer<WireBugPlayer>();
            bool AllSet = true;
            foreach (BugUnit bug in modplayer.bugs)
            {
                if (!bug.IsReady())
                {
                    AllSet = false;
                    break;
                }
            }
            if (AllSet && !Main.LocalPlayer.ItemAnimationActive)
            {
                UIManager.Visible = !UIManager.Visible;
            }
            return false;
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            WireBugPlayer modplayer = Main.LocalPlayer.GetModPlayer<WireBugPlayer>();
            UIPlayer modplayer2 = Main.LocalPlayer.GetModPlayer<UIPlayer>();

            if (modplayer.SwitchSkill)
            {
                rare = ItemRarityID.Cyan;
                buffName = Language.GetTextValue("Mods.WireBugMod.SkillInfos.SwitchBookBlue");
                tip = string.Format(Language.GetTextValue("Mods.WireBugMod.SkillInfos.SwitchBookDesc"), Language.GetTextValue("Mods.WireBugMod.Skills." + modplayer2.SwitchSkillName1), Language.GetTextValue("Mods.WireBugMod.Skills." + modplayer2.SwitchSkillName2));
            }
            else
            {
                rare = ItemRarityID.Orange;
                buffName = Language.GetTextValue("Mods.WireBugMod.SkillInfos.SwitchBookRed");
                tip = string.Format(Language.GetTextValue("Mods.WireBugMod.SkillInfos.SwitchBookDesc"), Language.GetTextValue("Mods.WireBugMod.Skills." + modplayer2.SkillName1), Language.GetTextValue("Mods.WireBugMod.Skills." + modplayer2.SkillName2));
            }

        }
    }
}