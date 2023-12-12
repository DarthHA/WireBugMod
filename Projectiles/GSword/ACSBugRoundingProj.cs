using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.GSword
{
    public class ACSBugRoundingProj : ModProjectile
    {

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
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            float r = Projectile.ai[1] / 20f * MathHelper.TwoPi * RotateDir * VelocityModifier + IniPhase;
            r -= RotateDir * MathHelper.Pi / 8f * LengthModifier;
            if (r.ToRotationVector2().Y > 0)
            {
                overPlayers.Add(index);
            }
            else
            {
                behindProjectiles.Add(index);
            }
        }

        public override void AI()
        {
            if (Projectile.localAI[1] == 1 || Projectile.localAI[0] - 1 == -1 || !Main.projectile[(int)Projectile.localAI[0] - 1].active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.ai[1]++;
            float r = Projectile.ai[1] / 20f * MathHelper.TwoPi * RotateDir * VelocityModifier + IniPhase; ;
            Projectile.rotation = GetRot(r);
            Projectile.Center = Main.player[Main.projectile[(int)Projectile.localAI[0] - 1].owner].Center;
        }

        private float GetRot(float r)
        {
            Vector2 Rot = (r + MathHelper.Pi / 2 * RotateDir).ToRotationVector2();
            Rot.Y *= YModifier;
            return Rot.ToRotation() + RotateRadian;
        }

        private Vector2 GetCirclePos(float r)
        {
            float radian = Radian;
            Vector2 CirclePos = r.ToRotationVector2() * radian;
            CirclePos.Y *= YModifier;
            CirclePos = (CirclePos.ToRotation() + RotateRadian).ToRotationVector2() * CirclePos.Length();
            return CirclePos;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float r = Projectile.ai[1] / 20f * MathHelper.TwoPi * RotateDir * VelocityModifier + IniPhase;

            Vector2 Center = Projectile.Center + Offset;     //环绕中心

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

            return false;
        }

        public static void SummonProj(int projOwner,Vector2 Pos, Vector2 offset, Color color, float radian = 40, float rotateRadian = 0, float iniPhise = 0, float yModifier = 0.6f, float velocityModifier = 1, float lengthModifier = 1, int rotateDir = 1)
        {
            int protmp = Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Pos, Vector2.Zero, ModContent.ProjectileType<ACSBugRoundingProj>(), 0, 0, Main.myPlayer);
            if (protmp > -1)
            {
                Main.projectile[protmp].localAI[0] = projOwner + 1;
                ACSBugRoundingProj modproj = Main.projectile[protmp].ModProjectile as ACSBugRoundingProj;
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
