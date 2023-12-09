using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles
{
    public class SlashProj : ModProjectile
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

            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.damage = 10;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 999;
        }


        public override void AI()
        {
            Projectile.ai[1]++;
            if (Projectile.ai[1] < 4)
            {
                Projectile.Opacity = Projectile.ai[1] / 4f;
            }
            else if (Projectile.ai[1] < 8)
            {
                Projectile.Opacity = 1;
            }
            else if (Projectile.ai[1] < 14)
            {
                Projectile.Opacity = (14 - Projectile.ai[1]) / 6f;
            }
            else
            {
                Projectile.Kill();
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
                Projectile.scale * 0.8f,
                SpriteEffects.None, 0);


            EasyDraw.AnotherDraw(BlendState.AlphaBlend);
            return false;
        }
    }
}
