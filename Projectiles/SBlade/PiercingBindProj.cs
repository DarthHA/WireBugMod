﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.SBlade
{
    public enum PiercingBindPhase
    {
        Pierce,
        PierceButNoDamage,
        Stay,
        Boom,
        Default
    }

    public class PiercingBindProj : BaseSkillProj        //特殊：这个弹幕会造成伤害
    {
        public override string Texture => "WireBugMod/Images/PlaceHolder";

        public PiercingBindPhase Phase = PiercingBindPhase.Default;

        bool Connected = false;

        public int Target = -1;
        private Vector2 SavedRelaPos = Vector2.Zero;
        private int SavedDir = 1;
        private float SavedRot = 0;

        public const int Cooldown = 5;

        public override void SetStaticDefaults()
        {
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

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.ownerHitCheck = true;
        }



        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.IsDead())
            {
                Projectile.Kill();
                return;
            }


            Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 1.5f, 1.5f, 1.5f);

            if (Phase == PiercingBindPhase.Pierce)
            {
                Projectile.ai[1]++;
                Projectile.rotation = Projectile.velocity.ToRotation();
                owner.itemLocation = Vector2.Zero;
                owner.itemTime = owner.itemAnimation = 2;
                owner.direction = Math.Sign(Projectile.rotation.ToRotationVector2().X + 0.01f);
                owner.ChangeItemRotation(Projectile.rotation);
                owner.heldProj = Projectile.whoAmI;
                float len = MathHelper.Lerp(0, 30, Projectile.ai[1] / 10);
                Projectile.Center = owner.Center + Projectile.rotation.ToRotationVector2() * len;
                if (Projectile.ai[1] >= 10)
                {
                    Phase = PiercingBindPhase.PierceButNoDamage;
                    Projectile.ai[1] = 0;
                    Projectile.friendly = false;
                }
            }
            else if (Phase == PiercingBindPhase.PierceButNoDamage)         //仅为视觉特效，不造成伤害
            {
                Projectile.ai[1]++;
                Projectile.rotation = Projectile.velocity.ToRotation();
                owner.itemLocation = Vector2.Zero;
                owner.itemTime = owner.itemAnimation = 2;
                owner.direction = Math.Sign(Projectile.rotation.ToRotationVector2().X + 0.01f);
                owner.ChangeItemRotation(Projectile.rotation);
                owner.heldProj = Projectile.whoAmI;
                Projectile.Center = owner.Center + Projectile.rotation.ToRotationVector2() * 30;
                if (Projectile.ai[1] >= 15)
                {
                    Projectile.Kill();
                    return;
                }
            }
            else if (Phase == PiercingBindPhase.Stay)
            {
                if (Main.rand.NextBool(12))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(5), 1 + Main.rand.NextFloat() * 0.5f);
                    }
                }
                if (Target == -1 || !Main.npc[Target].active || !Main.npc[Target].CanBeChasedBy() || Main.npc[Target].Distance(owner.Center) > 1500)
                {
                    Phase = PiercingBindPhase.Boom;
                    Projectile.ai[1] = 0;
                    return;
                }
                Vector2 RelaPos = SavedRelaPos;
                if (SavedDir == Main.npc[Target].spriteDirection)
                {
                    Projectile.Center = Main.npc[Target].Center + RelaPos.RotatedBy(Main.npc[Target].rotation);
                    Projectile.rotation = SavedRot + Main.npc[Target].rotation;
                }
                else
                {
                    Projectile.Center = Main.npc[Target].Center + new Vector2(-RelaPos.X, RelaPos.Y).RotatedBy(Main.npc[Target].rotation);
                    Projectile.rotation = PlayerUtils.GetRotationByDirection(SavedRot, -1) + Main.npc[Target].rotation;
                }

                Projectile.ai[1]++;
                if (Projectile.ai[1] > 600)
                {
                    Phase = PiercingBindPhase.Boom;
                    Projectile.ai[1] = 0;
                }

            }
            else if (Phase == PiercingBindPhase.Boom)            //造成一次总伤害
            {
                if (Target != -1 && Main.npc[Target].active && Main.npc[Target].CanBeChasedBy())
                {
                    owner.StrikeNPCDirect(Main.npc[Target],
                        Main.npc[Target].CalculateHitInfo((int)Projectile.localAI[0] + 30, Math.Sign(Main.npc[Target].Center.X - owner.Center.X), true, 0, DamageClass.Melee));

                    SlashProj.Summon(owner, Projectile.Center, 0, 0);
                }
                if (Projectile.localAI[0] > 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        float radian = 60 + Main.rand.Next(0, 20);
                        float inip = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float vel = Main.rand.NextFloat() * 0.6f + 0.6f;
                        float scale = Main.rand.NextFloat() * 2f + 2f;
                        float rot2 = 0.45f - 0.15f * i;
                        Vector2 OffSet = new(0, Main.rand.Next(-12, 12));
                        PiercingBindBugRoundingProj2.SummonProj(Projectile.Center, OffSet, Color.Cyan, radian, rot2, inip, 0.15f, vel, scale, Main.rand.Next(2) * 2 - 1);
                    }
                }

                Projectile.Kill();
            }
            else if (Phase == PiercingBindPhase.Default)
            {
                Projectile.Kill();
                return;
            }

        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];

            Texture2D tex = ModContent.Request<Texture2D>("WireBugMod/Images/ThrowingKnife").Value;

            if (Connected)
            {
                Vector2 DrawEnd = Projectile.Center - Projectile.rotation.ToRotationVector2() * 12;
                Terraria.Utils.DrawLine(Main.spriteBatch, DrawEnd, owner.Center, Color.White, Color.Transparent, 2);
            }
            Main.spriteBatch.Draw(tex,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                tex.Size() / 2f,
                Projectile.scale,
                SpriteEffects.None,
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

        public override bool? CanHitNPC(NPC target)
        {
            if (Phase == PiercingBindPhase.Pierce && Target == -1 && target.CanBeChasedBy())
            {
                return null;
            }
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)        //48,20
        {
            float point = 0;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Projectile.Center,
                Projectile.Center + Projectile.rotation.ToRotationVector2() * 48,
                20,
                ref point);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Target = target.whoAmI;
            Phase = PiercingBindPhase.Stay;
            Projectile.ai[1] = 0;

            SavedDir = target.spriteDirection;
            SavedRot = Projectile.rotation - target.rotation;
            SavedRelaPos = ((Projectile.Center + target.Center) / 2f - target.Center).RotatedBy(-target.rotation);
            DisableMeleeEffect = false;
            LockBug = false;
            Connected = true;

            for (int i = 0; i < 5; i++)
            {
                float radian = 50 + Main.rand.Next(0, 20);
                float inip = Main.rand.NextFloat() * MathHelper.TwoPi;
                float vel = Main.rand.NextFloat() * 0.6f + 0.6f;
                float scale = Main.rand.NextFloat() * 2f + 2f;
                float rot2 = 0.3f - 0.15f * i;
                Vector2 OffSet = new(0, Main.rand.Next(-12, 12));
                PiercingBindBugRoundingProj.SummonProj(Projectile, OffSet, Color.Cyan, radian, rot2, inip, 0.15f, vel, scale, Main.rand.Next(2) * 2 - 1);
            }
        }
    }
}