﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Projectiles.Lance;
using WireBugMod.Projectiles.Weapons;
using WireBugMod.System;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.BugDefend
{
    public enum BugDefendPhase
    {
        Guard,
        GP,
        Default
    }
    public class BugDefendProj : BaseSkillProj
    {
        public override string Texture => "WireBugMod/Images/PlaceHolder";

        public BugDefendPhase Phase = BugDefendPhase.Default;

        const float ShieldOffsetX = 12f;

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
            if (owner.IsDead() || !owner.hasRaisableShield)
            {
                Projectile.Kill();
                return;
            }

            if (Main.rand.NextBool(12))
            {
                for (int i = 0; i < 2; i++)
                {
                    SkillUtils.GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(5), 1 + Main.rand.NextFloat() * 0.5f);
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

            }
            //60帧架盾，GP后切换为40帧


            if (Phase == BugDefendPhase.Guard)
            {
                Projectile.ai[1]++;
                if (owner.GetModPlayer<MiscEffectPlayer>().JustHit > 0)
                {
                    Projectile.ai[1] = 0;
                    Phase = BugDefendPhase.GP;
                    return;
                }
                if (Projectile.ai[1] > 60)
                {
                    Phase = BugDefendPhase.Default;
                }
            }
            else if (Phase == BugDefendPhase.GP)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] == 1)
                {
                    owner.SetIFrame(120);
                    ActivatingGP = false;
                    ShieldLevel = 0;

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
                    GPSpark.Summon(Center);
                }


                if (Projectile.ai[1] > 20)
                {
                    Projectile.ai[1] = 0;
                    Phase = BugDefendPhase.Default;
                }
            }
            else if (Phase == BugDefendPhase.Default)
            {
                Projectile.Kill();
            }
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            if (Phase == BugDefendPhase.Guard)
            {
                float percentage = Math.Clamp(Projectile.ai[1] / 15f, 0, 1);
                float YOffest = percentage * -7 + 4;
                Vector2 DrawCenter = owner.Center + new Vector2(ShieldOffsetX * owner.direction, YOffest * owner.gravDir);
                for (float i = 0; i < MathHelper.TwoPi; i += MathHelper.Pi / 5f * 2)
                {
                    Vector2 Pos = i.ToRotationVector2() * 50;
                    Pos.Y /= 4;
                    Pos += owner.Bottom + new Vector2(ShieldOffsetX * owner.direction, 12 * owner.gravDir);
                    DrawUtils.DrawWire(DrawCenter, Pos, 0, Color.White, 0.0075f);
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
                    DrawUtils.DrawWire(DrawCenter, Pos, 0, Color.White, 0.0075f);
                }
            }
            return false;
        }

    }
}