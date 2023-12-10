﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.SBlade
{
    public class PiercingBindBugRoundingProj : ModProjectile
    {

        /// <summary>
        /// 母弹幕
        /// </summary>
        public int ProjOwner = -1;
        /// <summary>
        /// 环绕半径
        /// </summary>
        public float Radian = 40;
        /// <summary>
        /// 环绕方向
        /// </summary>
        public int RotateDir = 1;
        /// <summary>
        /// 轨道倾角
        /// </summary>
        public float RotateRadian = 0;
        /// <summary>
        /// 初相
        /// </summary>
        public float IniPhase = 0;

        /// <summary>
        /// Y轴压缩参数
        /// </summary>
        public float YModifier = 0.6f;

        /// <summary>
        /// 速度参数
        /// </summary>
        public float VelocityModifier = 1;

        /// <summary>
        /// 长度
        /// </summary>
        public float LengthModifier = 1;

        /// <summary>
        /// 颜色
        /// </summary>
        public Color color = Color.Cyan;

        /// <summary>
        /// 偏移
        /// </summary>
        public Vector2 Offset = Vector2.Zero;

        public override string Texture => "WireBugMod/Images/PlaceHolder";
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 9999;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
        }


        public override void AI()
        {
            if (ProjOwner == -1 || !Main.projectile[ProjOwner].active)
            {
                Projectile.Kill();
                return;
            }
            Projectile.ai[1]++;
            float r = Projectile.ai[1] / 20f * MathHelper.TwoPi * RotateDir * VelocityModifier + IniPhase;
            Projectile owner = Main.projectile[ProjOwner];
            Vector2 Center = owner.Center + Offset;
            Projectile.Center = Center + GetCirclePos(r);
            Projectile.rotation = GetRot(r);

            if (Projectile.ai[1] > 50)
            {
                Projectile.Kill();
            }

        }

        private float GetRot(float r)
        {
            Vector2 Rot = (r + MathHelper.Pi / 2 * RotateDir).ToRotationVector2();
            Rot.Y *= YModifier;
            return Rot.ToRotation() + RotateRadian;
        }

        private Vector2 GetCirclePos(float r)
        {
            float radian = Radian * MathHelper.Lerp(1, 0.4f, Math.Clamp(Projectile.ai[1] / 40f, 0, 1));
            Vector2 CirclePos = r.ToRotationVector2() * radian;
            CirclePos.Y *= YModifier;
            CirclePos = (CirclePos.ToRotation() + RotateRadian).ToRotationVector2() * CirclePos.Length();
            return CirclePos;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float r = Projectile.ai[1] / 20f * MathHelper.TwoPi * RotateDir * VelocityModifier + IniPhase;
            Projectile owner = Main.projectile[ProjOwner];

            Vector2 Center = owner.Center + Offset;     //环绕中心

            Texture2D texExtra = ModContent.Request<Texture2D>("WireBugMod/Images/BlobGlow").Value;

            List<CustomVertexInfo> vertexInfos = new();
            List<CustomVertexInfo> vertexInfos2 = new();
            Vector2 UnitY = (GetRot(r + RotateDir * MathHelper.Pi / 48f) + MathHelper.Pi / 2).ToRotationVector2();
            vertexInfos.Add(new CustomVertexInfo(Center + GetCirclePos(r + RotateDir * MathHelper.Pi / 48f) + UnitY * 1.5f, Color.White, new Vector3(0, 0f, 1)));
            vertexInfos.Add(new CustomVertexInfo(Center + GetCirclePos(r + RotateDir * MathHelper.Pi / 48f) - UnitY * 1.5f, Color.White, new Vector3(0, 1f, 1)));

            vertexInfos2.Add(new CustomVertexInfo(Center + GetCirclePos(r + RotateDir * MathHelper.Pi / 48f) + UnitY * 0.6f, Color.White, new Vector3(0, 0f, 1)));
            vertexInfos2.Add(new CustomVertexInfo(Center + GetCirclePos(r + RotateDir * MathHelper.Pi / 48f) - UnitY * 0.6f, Color.White, new Vector3(0, 1f, 1)));

            for (int i = 0; i < 12; i++)
            {
                float progress = 0.25f + i / 12f * 0.75f;
                UnitY = (GetRot(r) + MathHelper.Pi / 2).ToRotationVector2();
                vertexInfos.Add(new CustomVertexInfo(Center + GetCirclePos(r) + UnitY * 1.5f, Color.White, new Vector3(progress, 0f, 1)));
                vertexInfos.Add(new CustomVertexInfo(Center + GetCirclePos(r) - UnitY * 1.5f, Color.White, new Vector3(progress, 1f, 1)));

                vertexInfos2.Add(new CustomVertexInfo(Center + GetCirclePos(r) + UnitY * 0.6f, Color.White, new Vector3(progress, 0f, 1)));
                vertexInfos2.Add(new CustomVertexInfo(Center + GetCirclePos(r) - UnitY * 0.6f, Color.White, new Vector3(progress, 1f, 1)));
                r -= RotateDir * MathHelper.Pi / 48f * LengthModifier;
            }
            DrawUtils.DrawTrail(texExtra, vertexInfos, Main.spriteBatch, color, BlendState.Additive);
            DrawUtils.DrawTrail(texExtra, vertexInfos2, Main.spriteBatch, Color.White, BlendState.Additive);

            //Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public static void SummonProj(Projectile owner, Vector2 offset, Color color, float radian = 40, float rotateRadian = 0, float iniPhise = 0, float yModifier = 0.6f, float velocityModifier = 1, float lengthModifier = 1, int rotateDir = 1)
        {
            int protmp = Projectile.NewProjectile(owner.GetSource_FromThis(), owner.Center, Vector2.Zero, ModContent.ProjectileType<PiercingBindBugRoundingProj>(), 0, 0, Main.myPlayer);
            if (protmp > -1)
            {
                PiercingBindBugRoundingProj modproj = Main.projectile[protmp].ModProjectile as PiercingBindBugRoundingProj;
                modproj.ProjOwner = owner.whoAmI;
                modproj.Radian = radian;
                modproj.RotateRadian = rotateRadian;
                modproj.IniPhase = iniPhise;
                modproj.YModifier = yModifier;
                modproj.VelocityModifier = velocityModifier;
                modproj.LengthModifier = lengthModifier;
                modproj.RotateDir = rotateDir;
                modproj.color = color;
                modproj.Offset = offset;
            }
        }

    }
}
