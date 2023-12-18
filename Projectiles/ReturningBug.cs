using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles
{

    public class ReturningBug : BaseSkillProj
    {
        public Vector2 StartPos = Vector2.Zero;
        public bool BecomeTrail = false;
        public int WaitTime = 15;

        public const float ReturnSpeed = 20;

        public override string Texture => "WireBugMod/Images/PlaceHolder";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 2000;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.timeLeft = 99999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.IsDead())
            {
                Projectile.Kill();
                return;
            }

            if (++Projectile.frameCounter > 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            if (Main.rand.NextBool(12))
            {
                for (int i = 0; i < 2; i++)
                {
                    SkillUtils.GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(5), 1 + Main.rand.NextFloat() * 0.5f);
                }
            }

            Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 1.5f, 1.5f, 1.5f);

            if (Projectile.ai[0] == 0)        //HoveringForReturn,暂停15帧
            {
                Projectile.ai[1]++;
                Projectile.velocity *= 0.9f;
                if (Projectile.ai[1] >= WaitTime)
                {
                    Projectile.ai[1] = 0;
                    Projectile.ai[0] = 1;
                    Projectile.velocity = Vector2.Normalize(owner.Center - Projectile.Center) * -10f;     //为了兼容拖尾
                    BecomeTrail = true;

                    for (int i = 0; i < 20; i++)
                    {
                        SkillUtils.GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), Main.rand.Next(8), 1 + Main.rand.NextFloat() * 0.5f);
                    }
                }
            }
            else if (Projectile.ai[0] == 1)           //Returning,最多60帧
            {
                Projectile.ai[1]++;
                float speed = ReturnSpeed + owner.velocity.Length() / 2f;

                Vector2 MoveVel = Vector2.Normalize(owner.Center - Projectile.Center) * speed;
                if (Projectile.Distance(owner.Center) < 120)
                {
                    Projectile.velocity = MoveVel;
                }
                else
                {
                    Projectile.velocity = Projectile.velocity * 0.8f + MoveVel * 0.3f;
                }
                if (Projectile.velocity.Length() > 6) Projectile.velocity = Vector2.Normalize(Projectile.velocity) * speed;

                Projectile.spriteDirection = Math.Sign(Projectile.velocity.X + 0.01f);
                if (Projectile.Distance(owner.Center) <= 20 || Projectile.Distance(owner.Center) > 2400)
                {
                    Projectile.Kill();
                }
            }

        }

        public static void Summon(Player owner, Vector2 Pos, int dir, int WaitTime = 15)
        {
            int protmp = Projectile.NewProjectile(owner.GetSource_FromThis("WireBug"), Pos, Vector2.Zero, ModContent.ProjectileType<ReturningBug>(), 0, 0, owner.whoAmI);
            if (protmp >= 0)
            {
                (Main.projectile[protmp].ModProjectile as ReturningBug).WaitTime = WaitTime;
                (Main.projectile[protmp].ModProjectile as ReturningBug).LockInput = false;
                Main.projectile[protmp].spriteDirection = dir;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {

            if (BecomeTrail)        //绘制拖尾
            {
                EasyDraw.AnotherDraw(BlendState.Additive);
                Texture2D texTrail = ModContent.Request<Texture2D>("WireBugMod/Images/BlobGlow").Value;
                Vector2 origin = new Vector2(texTrail.Width * 0.75f, texTrail.Height / 2f);
                Vector2 scale = new Vector2(Projectile.scale * 0.3f, Projectile.scale * 0.2f);
                Main.spriteBatch.Draw(texTrail,
                    Projectile.Center - Main.screenPosition,
                    null,
                    Color.Cyan * 0.75f,
                    Projectile.velocity.ToRotation(),
                    origin,
                    scale,
                    SpriteEffects.None,
                    0);

                Main.spriteBatch.Draw(texTrail,
                    Projectile.Center - Main.screenPosition,
                    null,
                    Color.LightBlue * 0.5f,
                    Projectile.velocity.ToRotation(),
                    origin,
                    scale * 0.75f,
                    SpriteEffects.None,
                    0);

                Main.spriteBatch.Draw(texTrail,
                    Projectile.Center - Main.screenPosition,
                    null,
                    Color.White * 0.75f,
                    Projectile.velocity.ToRotation(),
                    origin,
                    scale * 0.6f,
                    SpriteEffects.None,
                    0);
                EasyDraw.AnotherDraw(BlendState.AlphaBlend);
                return false;
            }



            Texture2D tex = ModContent.Request<Texture2D>("WireBugMod/Images/WireBug").Value;
            Texture2D glow = ModContent.Request<Texture2D>("WireBugMod/Images/WireBug_Glow").Value;

            SpriteEffects spriteEffects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Rectangle frame = new(0, tex.Height / Main.projFrames[Projectile.type] * Projectile.frame, tex.Width, tex.Height / Main.projFrames[Projectile.type]);


            Main.spriteBatch.Draw(tex,
                Projectile.Center - Main.screenPosition,
                frame,
                lightColor,
                Projectile.rotation,
                frame.Size() / 2,
                Projectile.scale * 0.75f,
                spriteEffects,
                0);

            Main.spriteBatch.Draw(glow,
                Projectile.Center - Main.screenPosition,
                frame,
                Color.White,
                Projectile.rotation,
                frame.Size() / 2,
                Projectile.scale * 0.75f,
                spriteEffects,
                0);
            return false;
        }

    }
}