using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Projectiles.SBlade;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.GSword
{
    public enum ACSlashPhase
    {
        Default,
        Shoot,
        Drag,
        Charge,
        Slash,
        Pause
    }
    public class ACSlashProj : BaseSkillProj
    {
        public Vector2 TargetPos = Vector2.Zero;
        public Vector2 StartPos = Vector2.Zero;
        public bool Connected = true;

        public const float HoverY = 50;
        public const float BugWireOffset = 10;
        public const float ShootSpeed = 20;
        public const float DragSpeed = 10;

        private int SwordProj = -1;

        public ACSlashPhase Phase = ACSlashPhase.Default;
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
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(5), 1 + Main.rand.NextFloat() * 0.5f);
                }
            }

            Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 1.5f, 1.5f, 1.5f);

            if (Phase == ACSlashPhase.Shoot)           //10帧
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;

                if (Projectile.ai[1] == 1)
                {
                    SummonSword(owner.HeldItem.type, -MathHelper.Pi / 4f, 0, 0);
                    for (int i = 0; i < 7; i++)
                    {
                        float radian = 40 + Main.rand.Next(0, 30);
                        float inip = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float vel = Main.rand.NextFloat() * 0.6f + 0.6f;
                        float scale = Main.rand.NextFloat() * 2f + 2f;
                        float rot2 = 0.45f - 0.15f * i;
                        Vector2 OffSet = new(0, Main.rand.Next(-12, 12));
                        ACSBugRoundingProj.SummonProj(Projectile.whoAmI, Projectile.Center, OffSet, Color.Cyan, radian, rot2, inip, 0.15f, vel, scale, Main.rand.Next(2) * 2 - 1);
                    }
                }

                float TargetRot = (TargetPos - StartPos).ToRotation();
                Vector2 HoverPos = TargetPos + TargetRot.ToRotationVector2() * HoverY;
                int timeNeeded = Math.Clamp((int)((StartPos - HoverPos).Length() / ShootSpeed), 1, 10);
                Projectile.Center = Vector2.Lerp(StartPos, HoverPos, Projectile.ai[1] / timeNeeded);
                Projectile.spriteDirection = Math.Sign(HoverPos.X - StartPos.X);
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);

                Main.projectile[SwordProj].rotation = MathHelper.Lerp(-MathHelper.Pi / 4f, -MathHelper.Pi / 4 * 5, Projectile.ai[1] / timeNeeded);

                if (Projectile.ai[1] >= timeNeeded)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(10), 1 + Main.rand.NextFloat() * 0.5f);
                    }
                    StartPos = owner.Center;
                    Projectile.Center = HoverPos;
                    Phase = ACSlashPhase.Drag;
                    Projectile.ai[1] = 0;
                    Main.projectile[SwordProj].rotation = -MathHelper.Pi / 4f * 5f;
                }
            }
            else if (Phase == ACSlashPhase.Drag)      //拉扯
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
                if (!Collision.SolidCollision(owner.position + Vector2.Normalize(owner.velocity), owner.width, owner.height))
                {
                    owner.position += Vector2.Normalize(owner.velocity);
                }
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);

                if (Projectile.ai[1] < (timeNeeded * 0.25f))
                {
                    Main.projectile[SwordProj].rotation = MathHelper.Lerp(-MathHelper.Pi / 4f * 5, -MathHelper.Pi / 4 * 3, Projectile.ai[1] / (timeNeeded * 0.25f));
                }
                else
                {
                    Main.projectile[SwordProj].rotation = -MathHelper.Pi / 4 * 3;
                }

                if (Projectile.ai[1] >= timeNeeded || owner.Distance(TargetPos) <= DragSpeed / 1.5f)
                {
                    owner.velocity.X = owner.direction * 5;
                    Projectile.ai[1] = 0;
                    Phase = ACSlashPhase.Charge;
                    owner.SetPlayerFallStart(StartPos);
                    Connected = false;

                }
            }
            else if (Phase == ACSlashPhase.Charge)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] < 30)
                {
                    Main.projectile[SwordProj].rotation = MathHelper.Lerp(-MathHelper.Pi / 4f * 3, -MathHelper.Pi / 4f * 5, Projectile.ai[1] / 30f);
                }
                else
                {
                    Main.projectile[SwordProj].rotation = -MathHelper.Pi / 4f * 5;
                }

                if (Projectile.ai[1] > 40)
                {
                    if (Projectile.ai[1] > 120 || !owner.PressLeftInGame())
                    {
                        SummonSword(owner.HeldItem.type, -MathHelper.Pi / 4f * 5, owner.GetWeaponDamage() * 10, owner.GetWeaponKnockback(), 999);
                        Phase = ACSlashPhase.Slash;
                        Projectile.ai[1] = 0;

                        foreach (Projectile proj in Main.projectile)
                        {
                            if (proj.active && proj.type == ModContent.ProjectileType<ACSBugRoundingProj>() && proj.localAI[0] == Projectile.whoAmI + 1)
                            {
                                proj.localAI[1] = 1;
                            }
                        }
                    }
                }
            }
            else if (Phase == ACSlashPhase.Slash)
            {
                Projectile.ai[1]++;
                Main.projectile[SwordProj].rotation = MathHelper.Lerp(-MathHelper.Pi / 4f * 5, MathHelper.Pi / 12f, (float)Math.Pow(Projectile.ai[1] / 15f, 2));
                if (Projectile.ai[1] >= 15)
                {
                    Phase = ACSlashPhase.Pause;
                    Projectile.ai[1] = 0;
                }
            }
            else if (Phase == ACSlashPhase.Pause)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] > 20)
                {
                    KillSword();
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
                if (Phase == ACSlashPhase.Shoot)
                {
                    float TargetRot = (TargetPos - StartPos).ToRotation();
                    Vector2 HoverPos = TargetPos + TargetRot.ToRotationVector2() * HoverY;
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

        private void SummonSword(int type, float rot, int damage, float kb, int hitCooldown = 999)
        {
            if (SwordProj != -1) KillSword();
            Player owner = Main.player[Projectile.owner];
            int protmp = Projectile.NewProjectile(owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, 0, "WireBug"), owner.Center, Vector2.Zero, ModContent.ProjectileType<GSwordWeaponProj>(), damage, kb, owner.whoAmI);
            if (protmp >= 0)
            {
                Main.projectile[protmp].rotation = rot;
                Main.projectile[protmp].localNPCHitCooldown = hitCooldown;
                GSwordWeaponProj modproj = Main.projectile[protmp].ModProjectile as GSwordWeaponProj;
                modproj.ProjOwner = Projectile.whoAmI;
                modproj.ItemType = type;
                SwordProj = protmp;
            }
        }

        private void KillSword()
        {
            if (SwordProj == -1) return;
            Main.projectile[SwordProj].Kill();
            SwordProj = -1;
        }
    }
}