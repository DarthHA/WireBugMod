using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.SBlade
{
    public enum SwingBladePhase
    {
        Begin,
        Swing,
        End,
        Default
    }

    public class SwingBladeProj : BaseSkillProj        //特殊：这个弹幕会造成伤害
    {
        public override string Texture => "WireBugMod/Images/PlaceHolder";

        public SwingBladePhase Phase = SwingBladePhase.Default;

        private float StringLen = 0;

        private const float Length = 100;
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

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.IsDead())
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
            /*
            开始旋转
            伸出铁虫丝
            长度最大时加速变为残影
            最后一圈半圈减速半圈加速
            转完后收回
            */
            Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 1.5f, 1.5f, 1.5f);

            owner.velocity.X = 0;

            owner.heldProj = Projectile.whoAmI;
            Projectile.Center = owner.Center;
            owner.itemLocation = Vector2.Zero;
            owner.itemTime = owner.itemAnimation = 2;


            float rotdir = GetRotByDir(Projectile.rotation, owner.direction);
            owner.ChangeItemRotation(rotdir, false);


            float ModifiedLength = Length + DrawUtils.GetItemTexture(owner.HeldItem.type).Size().Length() * 0.4f;
            if (Phase == SwingBladePhase.Begin)
            {
                Projectile.ai[1]++;
                Projectile.rotation = MathHelper.Lerp(MathHelper.Pi / 6, MathHelper.Pi / 6 - MathHelper.Pi, Projectile.ai[1] / 15f);
                StringLen = MathHelper.Lerp(0, ModifiedLength, Projectile.ai[1] / 15f);
                if (Projectile.ai[1] >= 15)
                {
                    Projectile.ai[1] = 0;
                    Phase = SwingBladePhase.Swing;

                    for (int i = 0; i < 3; i++)
                    {
                        float radian = StringLen + 10 + 20 * Main.rand.NextFloat();
                        float iniPhase = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float vel = Main.rand.NextFloat() * 0.6f + 0.6f;
                        float scale = Main.rand.NextFloat() * 1.5f + 1.5f;
                        float rotateRadian = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float yModifier = Main.rand.NextFloat() * 0.1f + 0.9f;
                        SwingBladeRoundingProj.SummonProj(owner, Vector2.Zero, Color.Cyan, radian, rotateRadian, iniPhase, yModifier, -vel, scale, owner.direction);
                    }
                }
            }
            else if (Phase == SwingBladePhase.Swing)
            {
                Projectile.ai[1]++;
                Projectile.rotation -= MathHelper.TwoPi / 9.67f;
                if (Projectile.ai[1] >= 120)
                {
                    Projectile.ai[1] = 0;
                    Phase = SwingBladePhase.End;
                }
            }
            else if (Phase == SwingBladePhase.End)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] < 20)
                {
                    Projectile.rotation -= MathHelper.Pi / 14f;
                    StringLen = ModifiedLength;
                }
                else if (Projectile.ai[1] < 25)
                {
                    float factor = (25 - Projectile.ai[1]) / 5f + 0.5f;
                    Projectile.rotation -= MathHelper.Pi / 5f * factor;
                    StringLen = MathHelper.Lerp(ModifiedLength, 1, (Projectile.ai[1] - 20) / 5f);
                }
                else if (Projectile.ai[1] <= 50)
                {
                    StringLen = 1;
                }
                else
                {
                    Projectile.ai[1] = 0;
                    Phase = SwingBladePhase.Default;
                }
            }
            else if (Phase == SwingBladePhase.Default)
            {
                Projectile.Kill();
                return;
            }

        }


        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];

            Texture2D tex = DrawUtils.GetItemTexture(owner.HeldItem.type);

            Vector2 origin = new Vector2(owner.direction >= 0 ? 0 : tex.Size().X, tex.Size().Y);

            SpriteEffects spriteEffects = owner.direction >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            float rot = GetRotByDir(Projectile.rotation, owner.direction);
            rot += owner.direction >= 0 ? MathHelper.Pi / 4 : MathHelper.Pi / 4 * 3;

            Vector2 EndPos = Projectile.Center + GetRotByDir(Projectile.rotation, owner.direction).ToRotationVector2() * StringLen;

            Terraria.Utils.DrawLine(Main.spriteBatch, Projectile.Center, EndPos, Color.Cyan, Color.Cyan, 2);


            Main.spriteBatch.Draw(tex,
                EndPos - Main.screenPosition,
                null,
                lightColor * (1 - owner.immuneAlpha / 255f),
                rot,
                origin,
                Projectile.scale * owner.GetAdjustedItemScale(owner.HeldItem),
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


        private float GetRotByDir(float rot, int dir)
        {
            if (dir > 0)
            {
                return rot;
            }
            else
            {
                Vector2 temp = rot.ToRotationVector2();
                temp.X = -temp.X;
                rot = temp.ToRotation();
                return rot;
            }
        }
    }
}