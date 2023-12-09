using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.System;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.LSword
{
    public enum SernePosePhase
    {
        Guard,
        GP,
        Default
    }

    public class SernePoseProj : BaseSkillProj
    {
        public override string Texture => "WireBugMod/Images/PlaceHolder";

        public SernePosePhase Phase = SernePosePhase.Default;

        public int SwordProj = -1;

        const int LineCount = 15;
        public Vector2[] LineBegin = new Vector2[LineCount];
        public Vector2[] LineEnd = new Vector2[LineCount];
        public override void SetStaticDefaults()
        {

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
            Projectile.Opacity = 0;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.IsDead() || owner.HeldItem.GetWeaponType() != WeaponType.GreatSword)
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
                owner.ClearIFrame();
                SummonSword(owner.HeldItem.type, (Main.MouseWorld - owner.Center).ToRotation(), 0, 0, 999);
                //初始化丝线
                for (int i = 0; i < LineCount; i++)
                {
                    LineBegin[i] = new Vector2(Main.rand.Next(-50, 50), Main.rand.Next(-50, 50));
                    LineEnd[i] = new Vector2(Main.rand.Next(-50, 50), Main.rand.Next(-50, 50));
                }


            }
            //60帧架盾，GP后切换为40帧


            if (Phase == SernePosePhase.Guard)
            {
                Projectile.ai[1]++;
                if (owner.GetModPlayer<MiscEffectPlayer>().JustHit > 0 && Projectile.ai[1] <= 170)
                {
                    Projectile.ai[1] = 0;
                    Phase = SernePosePhase.GP;
                    return;
                }
                if (Projectile.ai[1] <= 10)
                {
                    Projectile.Opacity = Projectile.ai[1] / 10f;
                }
                if (Projectile.ai[1] <= 40)
                {
                    Main.projectile[SwordProj].localAI[0] = MathHelper.Pi / 2;
                    Main.projectile[SwordProj].rotation = MathHelper.Lerp(0, -MathHelper.Pi / 6 * 7, Projectile.ai[1] / 40f);
                }

                if (Projectile.ai[1] > 170)
                {
                    Projectile.Opacity = (180 - Projectile.ai[1]) / 10f;
                }
                if (Projectile.ai[1] > 180)
                {
                    Phase = SernePosePhase.Default;
                }
            }
            else if (Phase == SernePosePhase.GP)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] == 1)
                {
                    owner.SetIFrame(240);
                    ActivatingGP = false;
                    ShieldLevel = 0;
                    Vector2 Center = owner.Center + new Vector2(10 * owner.direction, 0 * owner.gravDir);
                    int protmp = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Center, Vector2.Zero, ModContent.ProjectileType<GPSpark>(), 0, 0, owner.whoAmI);
                    Main.projectile[protmp].rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
                    SummonSword(owner.HeldItem.type, (Main.MouseWorld - owner.Center).ToRotation(), owner.GetWeaponDamage() * 5, owner.GetWeaponKnockback(), 999);
                    Main.projectile[SwordProj].friendly = false;
                }
                if (Projectile.ai[1] == 10)
                {
                    Main.projectile[SwordProj].friendly = true;
                }
                if (Projectile.ai[1] == 3)
                {
                    (Main.projectile[SwordProj].ModProjectile as LSwordWeaponProj).HasTrail = true;
                }
                if (Projectile.ai[1] <= 10)
                {
                    Projectile.Opacity = (10 - Projectile.ai[1]) / 10f;
                }
                if (Projectile.ai[1] <= 3)
                {
                    Main.projectile[SwordProj].localAI[0] = MathHelper.Lerp(MathHelper.Pi / 2, MathHelper.Pi / 6, Projectile.ai[1] / 3f);
                    Main.projectile[SwordProj].rotation = -MathHelper.Pi / 6 * 7;
                }
                else if (Projectile.ai[1] <= 8)
                {
                    Main.projectile[SwordProj].rotation = MathHelper.Lerp(-MathHelper.Pi / 6 * 7, MathHelper.Pi / 6, (Projectile.ai[1] - 3) / 5f);
                }
                else if (Projectile.ai[1] <= 13)
                {
                    Main.projectile[SwordProj].rotation = MathHelper.Lerp(MathHelper.Pi / 6, MathHelper.Pi / 6 * 7, (Projectile.ai[1] - 8) / 5f);
                }
                else if (Projectile.ai[1] <= 23)
                {
                    Main.projectile[SwordProj].rotation = MathHelper.Lerp(MathHelper.Pi / 6 * 7, MathHelper.Pi / 6 * 13, (Projectile.ai[1] - 13) / 10f);
                }

                if (Projectile.ai[1] > 40)
                {
                    Projectile.ai[1] = 0;
                    Phase = SernePosePhase.Default;
                }
            }
            else if (Phase == SernePosePhase.Default)
            {
                KillSword();
                Projectile.Kill();
            }


        }


        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            if (Projectile.localAI[0] != 0)
            {
                Vector2 LineCenter = owner.Center + new Vector2(20 * owner.direction, 0);
                for (int i = 0; i < LineCount; i++)
                {
                    Vector2 Start = LineCenter + LineBegin[i];
                    Vector2 End = LineCenter + LineEnd[i];
                    Vector2 center = (Start + End) / 2f;

                    Terraria.Utils.DrawLine(Main.spriteBatch, center + Vector2.Normalize(Start - center) * 70, center + Vector2.Normalize(End - center) * 70, Color.Cyan * Projectile.Opacity, Color.Cyan * Projectile.Opacity, 1);
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


        private void SummonSword(int type, float rot, int damage, float kb, int hitCooldown = 999)
        {
            if (SwordProj != -1) KillSword();
            Player owner = Main.player[Projectile.owner];
            int protmp = Projectile.NewProjectile(owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, 0), owner.Center, Vector2.Zero, ModContent.ProjectileType<LSwordWeaponProj>(), damage, kb, owner.whoAmI);
            if (protmp >= 0)
            {
                Main.projectile[protmp].rotation = rot;
                Main.projectile[protmp].localNPCHitCooldown = hitCooldown;
                LSwordWeaponProj modproj = Main.projectile[protmp].ModProjectile as LSwordWeaponProj;
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