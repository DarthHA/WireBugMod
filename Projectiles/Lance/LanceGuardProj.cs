using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.System;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.Lance
{
    public enum LanceGuardPhase
    {
        Guard,
        GP,
        Default
    }
    public class LanceGuardProj : BaseSkillProj
    {
        public override string Texture => "WireBugMod/Images/PlaceHolder";

        public LanceGuardPhase Phase = LanceGuardPhase.Default;


        const float ShieldOffsetX = 12f;

        public int LanceProj = -1;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
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
            Projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.IsDead() || owner.HeldItem.GetWeaponType() != WeaponType.Lance || !owner.hasRaisableShield)
            {
                Projectile.Kill();
                return;
            }

            if (Main.rand.NextBool(12))
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(5), 1 + Main.rand.NextFloat() * 0.5f);
                }
            }

            Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 1.5f, 1.5f, 1.5f);

            Projectile.Center = owner.Center;
            owner.velocity.X = 0;


            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                for (int i = 0; i < 4; i++)
                {
                    float radian = 55 + Main.rand.Next(0, 30);
                    float inip = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float vel = Main.rand.NextFloat() * 0.6f + 0.6f;
                    float scale = Main.rand.NextFloat() * 2f + 2f;
                    float rot = 0.6f - 0.2f * i - 0.3f;
                    LanceBugRoundingProj.SummonProj(owner, Vector2.Zero, Color.Cyan, radian, rot, inip, 0.15f, vel, scale, owner.direction);
                }
                owner.ClearIFrame();
                SummonSpear(owner.HeldItem.shoot, (Main.MouseWorld - owner.Center).ToRotation(), 0, 0, 999);
            }
            //60Ö¡¼Ü¶Ü£¬GPºóÇÐ»»Îª40Ö¡


            if (Phase == LanceGuardPhase.Guard)
            {
                Projectile.ai[1]++;
                if (owner.GetModPlayer<MiscEffectPlayer>().JustHit > 0)
                {
                    Projectile.ai[1] = 0;
                    Phase = LanceGuardPhase.GP;
                    return;
                }
                if (Projectile.ai[1] > 60)
                {
                    Phase = LanceGuardPhase.Default;
                }
            }
            else if (Phase == LanceGuardPhase.GP)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] == 1)
                {
                    owner.SetIFrame(120);
                    ActivatingGP = false;
                    ShieldLevel = 0;
                    float rot = (Main.MouseWorld - owner.Center).ToRotation();
                    if (LanceProj != -1)
                    {
                        rot = Main.projectile[LanceProj].rotation;
                    }
                    SummonShoot(owner.HeldItem.shoot, rot, owner.GetWeaponDamage() * 5, owner.GetWeaponKnockback(), 10);

                    for (int i = 0; i < 4; i++)
                    {
                        float radian = 70 + Main.rand.Next(0, 30);
                        float inip = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float vel = Main.rand.NextFloat() * 0.6f + 0.6f;
                        float scale = Main.rand.NextFloat() * 2f + 2f;
                        float rot2 = 0.6f - 0.2f * i - 0.3f;
                        LanceBugRoundingProj.SummonProjFaster(owner, Vector2.Zero, Color.Red, radian, rot2, inip, 0.15f, vel, scale, owner.direction);
                    }


                    Vector2 Center = owner.Center + new Vector2(ShieldOffsetX * owner.direction, -3 * owner.gravDir);
                    int protmp = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Center, Vector2.Zero, ModContent.ProjectileType<GPSpark>(), 0, 0, owner.whoAmI);
                    Main.projectile[protmp].rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
                }


                if (Projectile.ai[1] > 40)
                {
                    Projectile.ai[1] = 0;
                    Phase = LanceGuardPhase.Default;
                }
            }
            else if (Phase == LanceGuardPhase.Default)
            {
                KillSpear();
                Projectile.Kill();
            }


        }


        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            if (Phase == LanceGuardPhase.Guard)
            {
                float percentage = Math.Clamp(Projectile.ai[1] / 15f, 0, 1);
                float YOffest = percentage * -7 + 4;
                Vector2 DrawCenter = owner.Center + new Vector2(ShieldOffsetX * owner.direction, YOffest * owner.gravDir);
                for (float i = 0; i < MathHelper.TwoPi; i += MathHelper.Pi / 5f * 2)
                {
                    Vector2 Pos = i.ToRotationVector2() * 50;
                    Pos.Y /= 4;
                    Pos += owner.Bottom + new Vector2(ShieldOffsetX * owner.direction, 12 * owner.gravDir);
                    DrawUtils.DrawWire(DrawCenter, Pos, 0, Color.Cyan, 0.0075f);
                }
            }
            else
            {
                float YOffest = -2;
                Vector2 DrawCenter = owner.Center + new Vector2(ShieldOffsetX * owner.direction, YOffest * owner.gravDir);
                for (float i = 0; i < MathHelper.TwoPi; i += MathHelper.Pi / 5f * 2)
                {
                    Vector2 Pos = i.ToRotationVector2() * 50;
                    Pos.Y /= 3;
                    Pos += owner.Bottom + new Vector2(ShieldOffsetX * owner.direction, 5 * owner.gravDir);
                    DrawUtils.DrawWire(DrawCenter, Pos, 0, Color.Cyan, 0.0075f);
                }
            }
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
            int protmp = Projectile.NewProjectile(owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, 0), owner.Center, Vector2.Zero, ModContent.ProjectileType<LanceWeaponProj>(), damage, kb, owner.whoAmI);
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

        private void SummonShoot(int type, float rot, int damage, float kb, int hitCooldown = 10)
        {
            if (LanceProj != -1) KillSpear();
            Player owner = Main.player[Projectile.owner];
            int protmp = Projectile.NewProjectile(owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, 0), owner.Center, Vector2.Zero, ModContent.ProjectileType<LanceWeaponProj>(), damage, kb, owner.whoAmI);
            if (protmp >= 0)
            {
                Main.projectile[protmp].rotation = rot;
                Main.projectile[protmp].localNPCHitCooldown = hitCooldown;
                LanceWeaponProj modproj = Main.projectile[protmp].ModProjectile as LanceWeaponProj;
                modproj.ProjOwner = Projectile.whoAmI;
                modproj.ProjType = type;
                modproj.Behavior = 1;
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