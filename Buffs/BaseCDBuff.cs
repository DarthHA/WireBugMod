using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.System;
using WireBugMod.Utils;

namespace WireBugMod.Buffs
{
    public abstract class BaseCDBuff : ModBuff
    {
        public override string Texture => "WireBugMod/Images/BugIcon";
        public virtual int Index => 0;
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
            if (Index < modplayer.bugs.Count)
            {
                float percentage = (float)modplayer.bugs[Index].Cooldown / modplayer.bugs[Index].MaxTime;
                if (percentage > 0)
                {
                    EasyDraw.AnotherDraw(SpriteSortMode.Immediate);
                    WireBugMod.PieEffect.Parameters["progress"].SetValue(percentage);
                    WireBugMod.PieEffect.CurrentTechnique.Passes["PieEffect"].Apply();
                    spriteBatch.Draw(drawParams.Texture, drawParams.Position, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    EasyDraw.AnotherDraw(SpriteSortMode.Deferred);
                }

                float progress = Math.Abs(modplayer.bugs[Index].ProgressTimer) / 10f;
                float alpha = 1 - progress;
                float scale = 1 + progress * 0.5f;
                Vector2 DrawCenter = drawParams.Position + drawParams.Texture.Size() / 2f;
                spriteBatch.Draw(drawParams.Texture, DrawCenter, null, Color.White * alpha, 0, drawParams.Texture.Size() / 2f, scale, SpriteEffects.None, 0);
            }
            return false;
        }

    }

    public class CDBuff1 : BaseCDBuff
    {
        public override int Index => 0;
    }
    public class CDBuff2 : BaseCDBuff
    {
        public override int Index => 1;
    }
    public class CDBuff3 : BaseCDBuff
    {
        public override int Index => 2;
    }

    public class CDBuff4 : BaseCDBuff
    {
        public override int Index => 3;
    }
}
