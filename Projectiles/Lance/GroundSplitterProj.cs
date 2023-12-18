using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Projectiles.Weapons;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.Lance
{
    public enum GroundSplitterPhase
    {
        Default,
        Shoot,
        Drag,
        Swing,
    }
    public class GroundSplitterProj : BaseSkillProj
    {
        public Vector2 TargetPos = Vector2.Zero;
        public Vector2 StartPos = Vector2.Zero;
        public bool Connected = true;

        public const float HoverY = 50;
        public const float BugWireOffset = 10;
        public const float ShootSpeed = 20;
        public const float DragSpeed = 20;
        public const float ReturnSpeed = 20;

        private int SpearProj = -1;

        public GroundSplitterPhase Phase = GroundSplitterPhase.Default;
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

            if (Phase == GroundSplitterPhase.Shoot)           //10帧
            {
                for (int i = 0; i < 2; i++)
                {
                    SkillUtils.GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;

                if (Projectile.ai[1] == 1)
                {
                    LanceWeaponProj.SummonSpear(Projectile, ref SpearProj, PlayerUtils.GetRotationByDirection(0, owner.direction), 2, 999);
                }

                float TargetRot = (TargetPos - StartPos).ToRotation();
                Vector2 HoverPos = TargetPos + TargetRot.ToRotationVector2() * HoverY;
                int timeNeeded = Math.Clamp((int)((StartPos - HoverPos).Length() / ShootSpeed), 1, 10);
                Projectile.Center = Vector2.Lerp(StartPos, HoverPos, Projectile.ai[1] / timeNeeded);
                Projectile.spriteDirection = Math.Sign(HoverPos.X - StartPos.X);
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);

                Main.projectile[SpearProj].rotation = MathHelper.Lerp(PlayerUtils.GetRotationByDirection(0, owner.direction), PlayerUtils.GetRotationByDirection(MathHelper.Pi / 6 * 7, owner.direction), Projectile.ai[1] / timeNeeded);

                if (Projectile.ai[1] >= timeNeeded)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        SkillUtils.GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(10), 1 + Main.rand.NextFloat() * 0.5f);
                    }
                    StartPos = owner.Center;
                    Projectile.Center = HoverPos;
                    Phase = GroundSplitterPhase.Drag;
                    Projectile.ai[1] = 0;
                    Main.projectile[SpearProj].rotation = PlayerUtils.GetRotationByDirection(MathHelper.Pi / 6 * 7, owner.direction);
                }
            }
            else if (Phase == GroundSplitterPhase.Drag)      //拉扯
            {
                for (int i = 0; i < 2; i++)
                {
                    SkillUtils.GenDust(owner.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;
                float TargetRot = (TargetPos - StartPos).ToRotation();
                Vector2 HoverPos = TargetPos + TargetRot.ToRotationVector2() * HoverY;
                Projectile.Center = HoverPos;
                int timeNeeded = Math.Clamp((int)((StartPos - TargetPos).Length() / DragSpeed), 1, 114514);
                //owner.Center = Vector2.Lerp(StartPos, TargetPos, Projectile.ai[1] / timeNeeded);         位移移动
                owner.velocity = Vector2.Normalize(TargetPos - owner.Center) * (DragSpeed - 1);
                if (!Collision.SolidCollision(owner.position + Vector2.Normalize(owner.velocity), owner.width, owner.height))
                {
                    owner.position += Vector2.Normalize(owner.velocity);
                }
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);

                //获取矛尖坐标
                Vector2 SpearTip = (Main.projectile[SpearProj].ModProjectile as LanceWeaponProj).GetTipPos();
                bool Collided = Projectile.ai[1] < timeNeeded * 0.25f || Collision.SolidTiles(SpearTip - new Vector2(5, 5), 10, 10);

                if (Collided)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Dust dust = Dust.NewDustDirect(SpearTip - new Vector2(3, 3), 6, 6, DustID.CopperCoin);
                        dust.velocity = new Vector2(Main.rand.Next(-5, 6), -4);
                        dust.noGravity = false;
                        dust.scale = 1;
                    }
                }

                if (Projectile.ai[1] < timeNeeded * 0.25f)
                {
                    Main.projectile[SpearProj].rotation = MathHelper.Lerp(PlayerUtils.GetRotationByDirection(MathHelper.Pi / 6 * 7, owner.direction), PlayerUtils.GetRotationByDirection(MathHelper.Pi / 4 * 3, owner.direction), Projectile.ai[1] / (timeNeeded * 0.25f));
                }
                else
                {
                    Main.projectile[SpearProj].rotation = PlayerUtils.GetRotationByDirection(MathHelper.Pi / 4 * 3, owner.direction);
                }


                if (Projectile.ai[1] >= timeNeeded || owner.Distance(TargetPos) <= DragSpeed / 1.5f || !Collided)
                {
                    Projectile.ai[1] = 0;
                    Phase = GroundSplitterPhase.Swing;
                    owner.SetPlayerFallStart(StartPos);
                    Connected = false;
                }
            }
            else if (Phase == GroundSplitterPhase.Swing)         //旋转,20帧复位，20帧停顿
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] == 1)
                {
                    owner.velocity.X = 0;
                }
                if (Projectile.ai[1] <= 10)
                {
                    float t = (float)Math.Pow(Projectile.ai[1] / 10f, 2);
                    if (owner.direction >= 0)          //方向算法的优劣弧修正
                    {
                        Main.projectile[SpearProj].rotation = MathHelper.Lerp(MathHelper.Pi / 4 * 3, -MathHelper.Pi / 4, t);
                    }
                    else
                    {
                        Main.projectile[SpearProj].rotation = MathHelper.Lerp(MathHelper.Pi / 4, MathHelper.Pi / 4 * 5, t);
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 SpearTip = (Main.projectile[SpearProj].ModProjectile as LanceWeaponProj).GetTipPos();
                        Dust dust = Dust.NewDustDirect(SpearTip - new Vector2(3, 3), 6, 6, DustID.CopperCoin);
                        dust.velocity = new Vector2(Main.rand.Next(-5, 6), -4);
                        dust.noGravity = false;
                        dust.scale = 1;
                    }
                }
                else
                {
                    Main.projectile[SpearProj].rotation = PlayerUtils.GetRotationByDirection(-MathHelper.Pi / 4, owner.direction);
                }
                if (Projectile.ai[1] >= 40)
                {
                    ReturningBug.Summon(owner, Projectile.Center, Projectile.spriteDirection);
                    Projectile.Kill();
                    return;
                }
            }

        }


        public override bool PreDraw(ref Color lightColor)
        {

            if (Connected)     //绘制虫丝
            {
                float percentage = 0;
                if (Phase == GroundSplitterPhase.Shoot)
                {
                    Vector2 HoverPos = TargetPos + Vector2.Normalize(TargetPos - StartPos) * HoverY;
                    percentage = Projectile.Distance(HoverPos) / HoverPos.Distance(StartPos);
                }
                DrawUtils.DrawWire(Main.player[Projectile.owner].Center, Projectile.Center + new Vector2(0, BugWireOffset), percentage, Color.White, 0.01f);
                //Terraria.Utils.DrawLine(Main.spriteBatch, Main.player[Projectile.owner].Center, Projectile.Center + new Vector2(0, 5), Color.Cyan, Color.Cyan, 2);
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