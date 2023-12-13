using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.Lance
{
    public enum LanceDownPhase
    {
        Default,
        Shoot1,
        Drag1,
        Pause1,
        Shoot2,
        Drag2,
        Pause2,
    }
    public class LanceDownProj : BaseSkillProj
    {
        public Vector2 TargetPos = Vector2.Zero;
        public Vector2 StartPos = Vector2.Zero;
        public Vector2 DownPos = Vector2.Zero;
        public bool Connected = true;
        public bool BecomeTrail = false;
        public bool Disappear = false;

        public const float HoverY = 50;
        public const float BugWireOffset = 10;
        public const float ShootSpeed = 20;
        public const float DragSpeed = 20;
        public const float DragSpeed2 = 30;
        public const float ReturnSpeed = 20;

        public int LanceProj = -1;


        public LanceDownPhase Phase = LanceDownPhase.Default;
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

            if (Phase == LanceDownPhase.Shoot1)           //��һ�η������,10֡
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;
                Vector2 HoverPos = TargetPos + new Vector2(0, -HoverY);
                int timeNeeded = Math.Clamp((int)((StartPos - HoverPos).Length() / ShootSpeed), 1, 10);
                Projectile.Center = Vector2.Lerp(StartPos, HoverPos, Projectile.ai[1] / timeNeeded);
                Projectile.spriteDirection = Math.Sign(HoverPos.X - StartPos.X);
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);
                if (Projectile.ai[1] >= timeNeeded)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(10), 1 + Main.rand.NextFloat() * 0.5f);
                    }
                    StartPos = owner.Center;
                    Projectile.Center = HoverPos;
                    Phase = LanceDownPhase.Drag1;
                    Projectile.ai[1] = 0;

                    SummonSpear(owner.HeldItem.shoot, (TargetPos - owner.Center).ToRotation(), owner.GetWeaponDamage(owner.HeldItem) * 20, owner.GetWeaponKnockback(owner.HeldItem), 999);
                }
            }
            else if (Phase == LanceDownPhase.Drag1)      //��һ������
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(owner.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;
                Vector2 HoverPos = TargetPos + new Vector2(0, -HoverY);
                Projectile.Center = HoverPos;
                int timeNeeded = Math.Clamp((int)((StartPos - TargetPos).Length() / DragSpeed), 1, 114514);
                owner.velocity = Vector2.Normalize(TargetPos - owner.Center) * (DragSpeed - 1);
                owner.position += Vector2.Normalize(owner.velocity);
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);


                if (Projectile.ai[1] >= timeNeeded || owner.Distance(TargetPos) <= DragSpeed / 1.5f)
                {
                    owner.velocity = new Vector2(0, -5);
                    owner.SetPlayerFallStart(StartPos);
                    Projectile.ai[1] = 0;
                    Phase = LanceDownPhase.Pause1;
                    Connected = false;
                    StartPos = Projectile.Center;
                    KillSpear();
                }
            }
            else if (Phase == LanceDownPhase.Pause1)       //������ͣ,20֡
            {
                Projectile.ai[1]++;
                //owner.velocity = Vector2.Zero;

                if (Projectile.ai[1] >= 10)
                {
                    Disappear = true;
                    Projectile.Center = owner.Center;
                }
                else if (Projectile.ai[1] >= 5)    //���ع�
                {
                    BecomeTrail = true;
                    Projectile.velocity = Vector2.Normalize(owner.Center - Projectile.Center);     //Ϊ�˼�����β
                    Projectile.Center = Vector2.Lerp(StartPos, owner.Center, (Projectile.ai[1] - 5f) / 5f);
                }

                if (Projectile.ai[1] > 20)
                {
                    Projectile.ai[1] = 0;
                    Phase = LanceDownPhase.Shoot2;
                    StartPos = owner.Center;
                    Connected = true;
                    BecomeTrail = false;
                    Disappear = false;
                }
            }
            else if (Phase == LanceDownPhase.Shoot2)        //�ڶ��η������,10֡
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(owner.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;
                Vector2 HoverPos = DownPos + new Vector2(0, HoverY);
                int timeNeeded = Math.Clamp((int)((StartPos - HoverPos).Length() / ShootSpeed), 1, 10);
                Projectile.Center = Vector2.Lerp(StartPos, HoverPos, Projectile.ai[1] / timeNeeded);
                Projectile.spriteDirection = Math.Sign(HoverPos.X - StartPos.X + 0.01f);
                owner.direction = Math.Sign(DownPos.X - owner.Center.X + 0.01f);
                if (Projectile.ai[1] >= timeNeeded)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(10), 1 + Main.rand.NextFloat() * 0.5f);
                    }
                    StartPos = owner.Center;
                    Projectile.Center = HoverPos;
                    Phase = LanceDownPhase.Drag2;
                    Projectile.ai[1] = 0;

                    SummonSpear(owner.HeldItem.shoot, (DownPos - owner.Center).ToRotation(), owner.GetWeaponDamage(owner.HeldItem) * 20, owner.GetWeaponKnockback(owner.HeldItem));
                }
            }
            else if (Phase == LanceDownPhase.Drag2)      //�ڶ�������
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(owner.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;
                Vector2 HoverPos = DownPos + new Vector2(0, HoverY);
                Projectile.Center = HoverPos;
                int timeNeeded = Math.Clamp((int)((StartPos - DownPos).Length() / DragSpeed2), 1, 114514);
                owner.velocity = Vector2.Normalize(DownPos - owner.Center) * (DragSpeed2 - 1);
                owner.position += Vector2.Normalize(owner.velocity);
                owner.direction = Math.Sign(DownPos.X - owner.Center.X + 0.01f);


                if (Projectile.ai[1] >= timeNeeded || owner.Distance(DownPos) <= DragSpeed2 / 1.5f)
                {
                    owner.SetPlayerFallStart(StartPos);
                    Projectile.ai[1] = 0;
                    Phase = LanceDownPhase.Pause2;
                    Connected = false;
                    StartPos = Projectile.Center;
                }
            }
            else if (Phase == LanceDownPhase.Pause2)       //������ͣ,20֡
            {
                Projectile.ai[1]++;

                owner.velocity.X = 0f;

                if (Projectile.ai[1] > 20)
                {
                    KillSpear();
                    ReturningBug.Summon(owner, Projectile.Center, Projectile.spriteDirection);
                    Projectile.Kill();
                    return;
                }
            }


        }




        public override bool PreDraw(ref Color lightColor)
        {
            if (Disappear) return false;

            if (Connected)     //���Ƴ�˿
            {
                float percentage = 0;
                if (Phase == LanceDownPhase.Shoot1)
                {
                    Vector2 HoverPos = TargetPos + new Vector2(0, -HoverY);
                    percentage = Projectile.Distance(HoverPos) / HoverPos.Distance(StartPos);
                }
                else if (Phase == LanceDownPhase.Shoot2)
                {
                    Vector2 HoverPos = DownPos + new Vector2(0, HoverY);
                    percentage = Projectile.Distance(HoverPos) / HoverPos.Distance(StartPos);
                }
                DrawUtils.DrawWire(Main.player[Projectile.owner].Center, Projectile.Center + new Vector2(0, BugWireOffset), percentage, Color.White, 0.01f);
                //Terraria.Utils.DrawLine(Main.spriteBatch, Main.player[Projectile.owner].Center, Projectile.Center + new Vector2(0, 5), Color.Cyan, Color.Cyan, 2);
            }

            if (BecomeTrail)        //������β
            {
                EasyDraw.AnotherDraw(BlendState.Additive);
                Texture2D texTrail = ModContent.Request<Texture2D>("WireBugMod/Images/BlobGlow").Value;
                Vector2 origin = new Vector2(texTrail.Width * 0.75f, texTrail.Height / 2f);
                Vector2 scale = new Vector2(Projectile.scale * 0.3f, Projectile.scale * 0.2f);
                Main.spriteBatch.Draw(texTrail,
                    Projectile.Center - Main.screenPosition,
                    null,
                    Color.Cyan * 0.75f,
                    Projectile.velocity.ToRotation(),
                    origin,
                    scale,
                    SpriteEffects.None,
                    0);

                Main.spriteBatch.Draw(texTrail,
                    Projectile.Center - Main.screenPosition,
                    null,
                    Color.LightBlue * 0.5f,
                    Projectile.velocity.ToRotation(),
                    origin,
                    scale * 0.75f,
                    SpriteEffects.None,
                    0);

                Main.spriteBatch.Draw(texTrail,
                    Projectile.Center - Main.screenPosition,
                    null,
                    Color.White * 0.75f,
                    Projectile.velocity.ToRotation(),
                    origin,
                    scale * 0.6f,
                    SpriteEffects.None,
                    0);
                EasyDraw.AnotherDraw(BlendState.AlphaBlend);
                return false;
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