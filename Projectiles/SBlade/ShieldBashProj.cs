using Humanizer.DateTimeHumanizeStrategy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.System;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.SBlade
{
    public enum ShieldBashPhase
    {
        Default,
        Shoot,
        Drag,
        Pause,
    }
    public class ShieldBashProj : BaseSkillProj
    {
        public Vector2 TargetPos = Vector2.Zero;
        public Vector2 StartPos = Vector2.Zero;
        public bool Connected = true;

        public const float HoverY = 50;
        public const float BugWireOffset = 10;
        public const float ShootSpeed = 20;
        public const float DragSpeed = 25;
        public const float ReturnSpeed = 20;

        private int SwordProj = -1;
        private bool GetHit = false;
        private bool Hit = false;

        public ShieldBashPhase Phase = ShieldBashPhase.Default;
        public override string Texture => "WireBugMod/Images/PlaceHolder";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 2000;
        }
        public override void SetDefaults()              //这个弹幕带伤害的
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.timeLeft = 99999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (owner.IsDead() || !owner.hasRaisableShield)
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

            if (Phase == ShieldBashPhase.Shoot)           //10帧
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;
                if (Projectile.ai[1] == 1)
                {
                    SummonSword(owner.HeldItem.type, 0, 0, 0);
                }
                float TargetRot = (TargetPos - StartPos).ToRotation();
                Main.projectile[SwordProj].rotation = PlayerUtils.GetRotationByDirection(TargetRot, owner.direction);

                Vector2 HoverPos = TargetPos + Vector2.Normalize(TargetPos - StartPos) * HoverY;
                int timeNeeded = Math.Clamp((int)((StartPos - HoverPos).Length() / ShootSpeed), 1, 10);
                Projectile.Center = Vector2.Lerp(StartPos, HoverPos, Projectile.ai[1] / timeNeeded);
                Projectile.spriteDirection = Math.Sign(HoverPos.X - StartPos.X);
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);

                float CurrentRot = MathHelper.Lerp(0, PlayerUtils.GetRotationByDirection(TargetRot, owner.direction), Projectile.ai[1] / timeNeeded);
                owner.GetModPlayer<MiscEffectPlayer>().ShieldRotation = CurrentRot;

                if (Projectile.ai[1] >= timeNeeded)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(10), 1 + Main.rand.NextFloat() * 0.5f);
                    }
                    StartPos = owner.Center;
                    Projectile.Center = HoverPos;
                    Phase = ShieldBashPhase.Drag;
                    Projectile.ai[1] = 0;
                    owner.GetModPlayer<MiscEffectPlayer>().ShieldRotation = PlayerUtils.GetRotationByDirection(TargetRot, owner.direction);
                    ActivatingGP = true;
                    ShieldLevel = 2;
                }
            }
            else if (Phase == ShieldBashPhase.Drag)      //拉扯
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(owner.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;
                Vector2 HoverPos = TargetPos + Vector2.Normalize(TargetPos - StartPos) * HoverY;
                Projectile.Center = HoverPos;
                int timeNeeded = Math.Clamp((int)((StartPos - TargetPos).Length() / DragSpeed), 1, 114514);
                //owner.Center = Vector2.Lerp(StartPos, TargetPos, Projectile.ai[1] / timeNeeded);         位移移动
                owner.velocity = Vector2.Normalize(TargetPos - owner.Center) * (DragSpeed - 1);
                owner.position += Vector2.Normalize(owner.velocity);
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);

                float TargetRot = (TargetPos - StartPos).ToRotation();
                owner.GetModPlayer<MiscEffectPlayer>().ShieldRotation = PlayerUtils.GetRotationByDirection(TargetRot, owner.direction);
                float DeltaRot = MathHelper.Pi / 6 * 5 / Math.Min(5, timeNeeded);
                if (Projectile.ai[1] <= 5)
                {
                    Main.projectile[SwordProj].rotation += DeltaRot;
                }

                if (owner.GetModPlayer<MiscEffectPlayer>().JustHit > 0 && !GetHit)
                {
                    GetHit = true;
                    GPSpark.Summon(owner.Center + TargetRot.ToRotationVector2() * 10);
                    owner.SetIFrame(60);
                }

                if (Projectile.ai[1] >= timeNeeded || owner.Distance(TargetPos) <= DragSpeed / 1.5f || (Projectile.ai[1] >= 5 && Hit))
                {
                    owner.velocity = Vector2.Normalize(TargetPos - StartPos) * 5;
                    owner.SetPlayerFallStart(StartPos);
                    Connected = false;
                    Phase = ShieldBashPhase.Pause;
                    Projectile.ai[1] = 0;
                    ActivatingGP = false;
                    ShieldLevel = 0;
                }
            }
            else if (Phase == ShieldBashPhase.Pause)      //暂停
            {
                Projectile.ai[1]++;

                float TargetRot = (TargetPos - StartPos).ToRotation();
                owner.GetModPlayer<MiscEffectPlayer>().ShieldRotation = PlayerUtils.GetRotationByDirection(TargetRot, owner.direction);

                if (Projectile.ai[1] > 10)
                {
                    KillSword();
                    ReturningBug.Summon(owner, Projectile.Center, Projectile.spriteDirection);
                    Projectile.Kill();
                    return;
                }
            }

        }

        public override bool? CanHitNPC(NPC target)
        {
            if (Phase == ShieldBashPhase.Drag) return null;
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player owner = Main.player[Projectile.owner];
            float point = 0f;
            Vector2 Unit = Vector2.Normalize(TargetPos - StartPos);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), owner.Center - Unit * 12, owner.Center + Unit * 24, 20, ref point);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].SetIFrame(120);
            Hit = true;
        }
        public override bool PreDraw(ref Color lightColor)
        {

            if (Connected)     //绘制虫丝
            {
                float percentage = 0;
                if (Phase == ShieldBashPhase.Shoot)
                {
                    Vector2 HoverPos = TargetPos + new Vector2(0, -HoverY);
                    percentage = Projectile.Distance(HoverPos) / HoverPos.Distance(StartPos);
                }
                DrawUtils.DrawWire(Main.player[Projectile.owner].Center, Projectile.Center + new Vector2(0, BugWireOffset), percentage, Color.Cyan, 0.01f);
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
            int protmp = Projectile.NewProjectile(owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, 0, "WireBug"), owner.Center, Vector2.Zero, ModContent.ProjectileType<SBladeWeaponProj>(), damage, kb, owner.whoAmI);
            if (protmp >= 0)
            {
                Main.projectile[protmp].rotation = rot;
                Main.projectile[protmp].localNPCHitCooldown = hitCooldown;
                SBladeWeaponProj modproj = Main.projectile[protmp].ModProjectile as SBladeWeaponProj;
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