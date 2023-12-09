using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.GSword
{

    public class BackGSwordProj : ModProjectile
    {
        public int ItemType = -1;

        public int ProjOwner = -1;

        public int Behavior = 0;

        public override string Texture => "WireBugMod/Images/PlaceHolder";
        public override void SetStaticDefaults()
        {

        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.timeLeft = 999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;
            Projectile.Opacity = 0;
        }
        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                //Projectile.rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            }
            if (ItemType == -1)
            {
                Projectile.Kill();
                return;
            }
            if (Behavior == 0)
            {
                if (ProjOwner == -1 || !Main.projectile[ProjOwner].active)
                {
                    Projectile.Kill();
                    return;
                }
            }

            Player owner = Main.player[Projectile.owner];
            if (owner.IsDead())
            {
                Projectile.Kill();
                return;
            }
            if (owner.ItemAnimationActive || owner.HeldItem.type != ItemType)
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = owner.Center;


            if (Behavior == 1)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] < 15)
                {
                    Projectile.Center = owner.Center + new Vector2(owner.direction, -owner.gravDir) * Projectile.ai[1];
                }
                else if (Projectile.ai[1] < 20)
                {
                    Projectile.Center = owner.Center + new Vector2(owner.direction, -owner.gravDir) * (20 - Projectile.ai[1]);
                }

                if (Projectile.ai[1] >= 15)          //15-35
                {
                    if (Projectile.ai[1] <= 20)
                    {
                        Projectile.Opacity = (Projectile.ai[1] - 15) / 5f;
                    }
                    else if (Projectile.ai[1] < 25)
                    {
                        Projectile.Opacity = 1;
                    }
                    else
                    {
                        Projectile.Opacity = (35 - Projectile.ai[1]) / 10f;
                    }
                }

                if (Projectile.ai[1] >= 35)
                {
                    Projectile.Kill();
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];

            Texture2D texWeapon = DrawUtils.GetItemTexture(ItemType);
            SpriteEffects sp = owner.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally; //反向用于适配
            Main.spriteBatch.Draw(texWeapon,
                Projectile.Center - Main.screenPosition,
                null,
                Lighting.GetColor((int)owner.Center.X / 16, (int)owner.Center.Y / 16) * (1 - owner.immuneAlpha / 255f),
                owner.fullRotation - MathHelper.Pi / 2 * owner.direction,
                texWeapon.Size() / 2,
                owner.GetAdjustedItemScale(owner.HeldItem),
                sp,
                0);

            if (Projectile.Opacity > 0)
            {
                Vector2 DrawPos = owner.Center + new Vector2(15 * owner.direction, -15 * owner.gravDir) - Main.screenPosition;
                Texture2D tex = ModContent.Request<Texture2D>("WireBugMod/Images/Slashing").Value;
                EasyDraw.AnotherDraw(BlendState.Additive);
                Main.spriteBatch.Draw(tex,
                    DrawPos,
                    null,
                    Color.White * Projectile.Opacity,
                    Projectile.rotation,
                    tex.Size() / 2,
                    Projectile.scale * 0.6f,
                    SpriteEffects.None, 0);

                Main.spriteBatch.Draw(tex,
                DrawPos,
                null,
                Color.White * Projectile.Opacity,
                Projectile.rotation + MathHelper.Pi / 2,
                tex.Size() / 2,
                Projectile.scale * 0.3f,
                SpriteEffects.None, 0);

                EasyDraw.AnotherDraw(BlendState.AlphaBlend);
            }
            return false;
        }

    }
}