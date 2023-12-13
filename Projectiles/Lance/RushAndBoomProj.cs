using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.Lance
{
    public enum RushAndBoomPhase
    {
        Default,
        Shoot,
        Drag,
        Attack,
        Backward
    }
    public class RushAndBoomProj : BaseSkillProj
    {
        public Vector2 TargetPos = Vector2.Zero;
        public Vector2 StartPos = Vector2.Zero;
        public bool Connected = true;
        public bool Disappear = false;

        public const float HoverY = 50;
        public const float BugWireOffset = 10;
        public const float ShootSpeed = 20;
        public const float DragSpeed = 20;
        public const float ReturnSpeed = 20;

        private int LanceProj = -1;

        public RushAndBoomPhase Phase = RushAndBoomPhase.Default;
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

            if (!Disappear && Main.rand.NextBool(12))
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(5), 1 + Main.rand.NextFloat() * 0.5f);
                }
            }

            Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 1.5f, 1.5f, 1.5f);

            if (Phase == RushAndBoomPhase.Shoot)           //10帧
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;
                float TargetRot = (TargetPos - StartPos).ToRotation();
                if (Projectile.ai[1] == 1)
                {
                    SummonSpear(owner.HeldItem.shoot, TargetRot, 0, 0);
                }

                Vector2 HoverPos = TargetPos + TargetRot.ToRotationVector2() * HoverY;
                int timeNeeded = Math.Clamp((int)((StartPos - HoverPos).Length() / ShootSpeed), 1, 10);
                Projectile.Center = Vector2.Lerp(StartPos, HoverPos, Projectile.ai[1] / timeNeeded);
                Projectile.spriteDirection = Math.Sign(HoverPos.X - StartPos.X);
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);

                Main.projectile[LanceProj].rotation = TargetRot + MathHelper.Lerp(0, -MathHelper.Pi * owner.direction, Projectile.ai[1] / timeNeeded);

                if (Projectile.ai[1] >= timeNeeded)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(10), 1 + Main.rand.NextFloat() * 0.5f);
                    }
                    StartPos = owner.Center;
                    Projectile.Center = HoverPos;
                    Phase = RushAndBoomPhase.Drag;
                    Projectile.ai[1] = 0;
                    Main.projectile[LanceProj].rotation = TargetRot - MathHelper.Pi * owner.direction;
                }
            }
            else if (Phase == RushAndBoomPhase.Drag)      //拉扯
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(owner.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;
                float TargetRot = (TargetPos - StartPos).ToRotation();
                Vector2 HoverPos = TargetPos + TargetRot.ToRotationVector2() * HoverY;
                Projectile.Center = HoverPos;
                int timeNeeded = Math.Clamp((int)((StartPos - TargetPos).Length() / DragSpeed), 1, 114514);
                //owner.Center = Vector2.Lerp(StartPos, TargetPos, Projectile.ai[1] / timeNeeded);         位移移动
                owner.velocity = Vector2.Normalize(TargetPos - owner.Center) * (DragSpeed - 1);
                owner.position += Vector2.Normalize(owner.velocity);
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);
                if (Projectile.ai[1] == 1)
                {
                    SummonSpear(owner.HeldItem.shoot, TargetRot, owner.GetWeaponDamage() * 2, owner.GetWeaponKnockback(), 999);
                    Main.projectile[LanceProj].rotation = TargetRot - MathHelper.Pi * owner.direction;
                }


                bool OnHit = (Main.projectile[LanceProj].ModProjectile as LanceWeaponProj).Hit;              //中途击中目标

                if (Projectile.ai[1] >= timeNeeded || owner.Distance(TargetPos) <= DragSpeed / 1.5f || OnHit)
                {
                    Projectile.ai[1] = 0;
                    Phase = RushAndBoomPhase.Attack;
                    owner.SetPlayerFallStart(StartPos);
                    Connected = false;
                    Disappear = true;
                    ReturningBug.Summon(owner, Projectile.Center, Projectile.spriteDirection);
                    Vector2 TargetVel = (TargetPos - StartPos) * 10;
                    StartPos = owner.Center;
                    TargetPos = StartPos + TargetVel;
                }
            }
            else if (Phase == RushAndBoomPhase.Attack)         //全弹发射,10帧复位，10帧停顿
            {
                Projectile.ai[1]++;
                //owner.Center = StartPos;    //会出现错位bug，所以删了
                owner.velocity = Vector2.Zero;

                float TargetRot = (TargetPos - StartPos).ToRotation();
                if (Projectile.ai[1] <= 10)
                {
                    Main.projectile[LanceProj].rotation = TargetRot + MathHelper.Lerp(-MathHelper.Pi * owner.direction, 0, Projectile.ai[1] / 10);
                }
                else
                {
                    Main.projectile[LanceProj].rotation = TargetRot;
                }
                if (Projectile.ai[1] == 24)
                {
                    for (float i = -MathHelper.Pi / 3; i <= MathHelper.Pi / 3; i += MathHelper.Pi / 12f)
                    {
                        float ShootRot = i + TargetRot;
                        float ShootLength = DrawUtils.GetProjTexture(owner.HeldItem.shoot).Size().Distance(Vector2.Zero) * owner.GetAdjustedItemScale(owner.HeldItem) / 2 - 20;
                        int protmp = Projectile.NewProjectile(owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, 0, "WireBug"), owner.Center + TargetRot.ToRotationVector2() * ShootLength, ShootRot.ToRotationVector2() * owner.HeldItem.shootSpeed * 3, 343, owner.GetWeaponDamage() * 5, owner.GetWeaponKnockback());
                        Main.projectile[protmp].usesLocalNPCImmunity = true;
                        Main.projectile[protmp].localNPCHitCooldown = 10;
                    }
                    //发射
                }
                if (Projectile.ai[1] >= 25)
                {
                    Projectile.ai[1] = 0;
                    Phase = RushAndBoomPhase.Backward;
                }
            }
            else if (Phase == RushAndBoomPhase.Backward)      //后坐力后退
            {
                Projectile.ai[1]++;
                float TargetRot = (TargetPos - StartPos).ToRotation();
                Vector2 BackwardVel = Vector2.Normalize(StartPos - TargetPos) * MathHelper.Lerp(30f, 10f, Projectile.ai[1] / 20f);
                owner.velocity = BackwardVel;
                if (Projectile.ai[1] <= 10)
                {
                    Main.projectile[LanceProj].rotation = TargetRot + MathHelper.Lerp(0, -MathHelper.Pi / 2 * owner.direction, Projectile.ai[1] / 10);
                }
                else
                {
                    Main.projectile[LanceProj].rotation = TargetRot - MathHelper.Pi / 2 * owner.direction;
                }
                (Main.projectile[LanceProj].ModProjectile as LanceWeaponProj).OffSet = MathHelper.Lerp(0, -30, Projectile.ai[1] / 20f);
                if (Projectile.ai[1] > 20)
                {
                    KillSpear();
                    Projectile.Kill();
                    return;
                }
            }

        }


        public override bool PreDraw(ref Color lightColor)
        {
            if (Disappear) return false;

            if (Connected)     //绘制虫丝
            {
                float percentage = 0;
                if (Phase == RushAndBoomPhase.Shoot)
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

        private void GenDust(Vector2 Pos, float Speed, float scale)
        {
            Dust dust = Dust.NewDustDirect(Pos, 1, 1, DustID.WhiteTorch);
            dust.color = Color.Cyan;
            dust.velocity = (MathHelper.TwoPi * Main.rand.NextFloat()).ToRotationVector2() * Speed;
            dust.position = Pos;
            dust.noGravity = true;
            dust.scale = scale;
        }

        private void SummonSpear(int type, float rot, int damage, float kb, int hitCooldown = 10)
        {
            if (LanceProj != -1) KillSpear();
            Player owner = Main.player[Projectile.owner];
            int protmp = Projectile.NewProjectile(owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, 0, "WireBug"), owner.Center, Vector2.Zero, ModContent.ProjectileType<LanceWeaponProj>(), damage, kb, owner.whoAmI);
            if (protmp >= 0)
            {
                Main.projectile[protmp].rotation = rot;
                Main.projectile[protmp].localNPCHitCooldown = hitCooldown;
                LanceWeaponProj modproj = Main.projectile[protmp].ModProjectile as LanceWeaponProj;
                modproj.ProjOwner = Projectile.whoAmI;
                modproj.ProjType = type;
                LanceProj = protmp;
            }
        }

        private void KillSpear()
        {
            if (LanceProj == -1) return;
            Main.projectile[LanceProj].Kill();
            LanceProj = -1;
        }
    }
}