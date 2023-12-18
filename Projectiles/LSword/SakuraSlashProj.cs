﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Projectiles.Weapons;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.LSword
{
    public enum SakuraSlashPhase
    {
        Default,
        Shoot,
        Drag,
    }
    public class SakuraSlashProj : BaseSkillProj
    {
        public Vector2 TargetPos = Vector2.Zero;
        public Vector2 StartPos = Vector2.Zero;
        public bool Connected = true;

        public const float HoverY = 50;
        public const float BugWireOffset = 10;
        public const float ShootSpeed = 20;
        public const float DragSpeed = 20;
        public const float ReturnSpeed = 20;

        private int SwordProj = -1;

        public SakuraSlashPhase Phase = SakuraSlashPhase.Default;
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

            if (Phase == SakuraSlashPhase.Shoot)           //10帧
            {
                for (int i = 0; i < 2; i++)
                {
                    SkillUtils.GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;
                if (Projectile.ai[1] == 1)
                {
                    LSwordWeaponProj.SummonSword(Projectile, ref SwordProj, 0);
                    Main.projectile[SwordProj].localAI[1] = PlayerUtils.GetRotationByDirection((TargetPos - StartPos).ToRotation(), owner.direction);
                }
                Vector2 HoverPos = TargetPos + Vector2.Normalize(TargetPos - StartPos) * HoverY;
                int timeNeeded = Math.Clamp((int)((StartPos - HoverPos).Length() / ShootSpeed), 1, 10);
                Projectile.Center = Vector2.Lerp(StartPos, HoverPos, Projectile.ai[1] / timeNeeded);
                Projectile.spriteDirection = Math.Sign(HoverPos.X - StartPos.X);
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);

                Main.projectile[SwordProj].rotation = MathHelper.Lerp(0, -MathHelper.Pi / 6 * 5, Projectile.ai[1] / timeNeeded);
                Main.projectile[SwordProj].localAI[0] = MathHelper.Lerp(MathHelper.Pi / 2, MathHelper.Pi / 6, Projectile.ai[1] / timeNeeded);

                if (Projectile.ai[1] >= timeNeeded)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        SkillUtils.GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(10), 1 + Main.rand.NextFloat() * 0.5f);
                    }
                    StartPos = owner.Center;
                    Projectile.Center = HoverPos;
                    Phase = SakuraSlashPhase.Drag;
                    Projectile.ai[1] = 0;
                }
            }
            else if (Phase == SakuraSlashPhase.Drag)      //拉扯，可派生悬挂
            {
                if (SleepTimer > 0)
                {
                    owner.velocity = Vector2.Normalize(TargetPos - owner.Center) * 1;
                    return;
                }
                for (int i = 0; i < 2; i++)
                {
                    SkillUtils.GenDust(owner.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;
                Vector2 HoverPos = TargetPos + Vector2.Normalize(TargetPos - StartPos) * HoverY;
                Projectile.Center = HoverPos;
                int timeNeeded = Math.Clamp((int)((StartPos - TargetPos).Length() / DragSpeed), 1, 114514);
                //owner.Center = Vector2.Lerp(StartPos, TargetPos, Projectile.ai[1] / timeNeeded);         位移移动
                owner.velocity = Vector2.Normalize(TargetPos - owner.Center) * (DragSpeed - 1);
                if (!Collision.SolidCollision(owner.position + Vector2.Normalize(owner.velocity), owner.width, owner.height))
                {
                    owner.position += Vector2.Normalize(owner.velocity);
                }

                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);
                if (Projectile.ai[1] == 1)
                {
                    LSwordWeaponProj.SummonSword(Projectile, ref SwordProj, -MathHelper.Pi / 6 * 5, 2, 10, "Sakura");
                    Main.projectile[SwordProj].localAI[0] = MathHelper.Pi / 6f;
                    Main.projectile[SwordProj].localAI[1] = PlayerUtils.GetRotationByDirection((TargetPos - StartPos).ToRotation(), owner.direction);
                }
                Main.projectile[SwordProj].rotation = MathHelper.Lerp(-MathHelper.Pi / 6 * 5, -MathHelper.Pi / 6 * 5 + MathHelper.TwoPi * 2, Projectile.ai[1] / timeNeeded);


                if (Projectile.ai[1] >= timeNeeded || owner.Distance(TargetPos) <= DragSpeed / 1.5f)
                {
                    owner.velocity = Vector2.Normalize(TargetPos - StartPos) * 5;
                    owner.SetPlayerFallStart(StartPos);
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
                if (Phase == SakuraSlashPhase.Shoot)
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