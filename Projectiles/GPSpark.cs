using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles
{
    public class GPSpark : ModProjectile
    {
        public override string Texture => "WireBugMod/Images/PlaceHolder";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.timeLeft = 99999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
        public override void AI()
        {
            Projectile.ai[1]++;
            if (Projectile.ai[1] < 4)
            {
                Projectile.Opacity = Projectile.ai[1] / 4f;
            }
            else
            {
                Projectile.Opacity = (14 - Projectile.ai[1]) / 10f;
                if (Projectile.ai[1] >= 14)
                {
                    Projectile.Kill();
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("WireBugMod/Images/Slashing").Value;
            EasyDraw.AnotherDraw(BlendState.Additive);
            Main.spriteBatch.Draw(tex,
                Projectile.Center - Main.screenPosition,
                null,
                Color.White * Projectile.Opacity,
                Projectile.rotation,
                tex.Size() / 2,
                Projectile.scale * 0.75f,
                SpriteEffects.None, 0);
            Main.spriteBatch.Draw(tex,
                Projectile.Center - Main.screenPosition,
                null,
                Color.White * Projectile.Opacity,
                Projectile.rotation + MathHelper.Pi / 2,
                tex.Size() / 2,
                Projectile.scale * 0.75f,
                SpriteEffects.None, 0);

            EasyDraw.AnotherDraw(BlendState.AlphaBlend);
            return false;
        }
    }
}
