using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Projectiles.Lance;
using WireBugMod.Projectiles.Weapons;
using WireBugMod.System;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.Lance
{
    public enum ReverseBlastPhase
    {
        Default,
        PrePare,
        Moving,
        Stop,
        Drag,
        Hover,
    }
    public class ReverseBlastProj : BaseSkillProj
    {
        public float MovingRotation = 0;

        public bool Connected = false;
        public bool Disappear = true;

        public const float HoverY = 100;
        public const float BugWireOffset = 10;
        public const float MoveSpeed = 20;

        public int SpearProj = -1;

        private Vector2 StartPos = Vector2.Zero;

        public ReverseBlastPhase Phase = ReverseBlastPhase.Default;
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

            if (Phase == ReverseBlastPhase.PrePare)       //5帧准备时间
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] == 1)
                {
                    Vector2 RotVec = MovingRotation.ToRotationVector2();
                    LanceWeaponProj.SummonSpear(Projectile, ref SpearProj, (-RotVec).ToRotation(), 3);
                    (Main.projectile[SpearProj].ModProjectile as LanceWeaponProj).OffSet = 20;
                }
                owner.direction = Math.Sign(MovingRotation.ToRotationVector2().X);
                owner.velocity *= 0.99f;
                Projectile.Center = owner.Center;
                if (Projectile.ai[1] > 5)
                {
                    Projectile.ai[1] = 0;
                    Phase = ReverseBlastPhase.Moving;
                }
            }
            else if (Phase == ReverseBlastPhase.Moving)          //反冲移动
            {
                Projectile.ai[1]++;

                float speed = MathHelper.Lerp(15, MoveSpeed, Math.Clamp(Projectile.ai[1] / 15, 0, 1));

                owner.velocity = MovingRotation.ToRotationVector2() * (speed - 1);
                bool Collided = Collision.SolidTiles(owner.position + Vector2.Normalize(owner.velocity) * speed, owner.width, owner.height);
                if (!Collided)
                {
                    owner.position += Vector2.Normalize(owner.velocity);
                }
                owner.direction = Math.Sign(MovingRotation.ToRotationVector2().X);


                Projectile.Center = owner.Center;

                if (Projectile.ai[1] % 10 == 2)
                {
                    float ShootLength = DrawUtils.GetProjTexture(owner.HeldItem.shoot).Size().Distance(Vector2.Zero) * owner.GetAdjustedItemScale(owner.HeldItem) / 2 - 20;
                    int protmp = Projectile.NewProjectile(owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, 0, "WireBug"), owner.Center - MovingRotation.ToRotationVector2() * ShootLength, (-MovingRotation.ToRotationVector2()) * owner.HeldItem.shootSpeed * 3, 343, owner.GetWeaponDamage() * 5, owner.GetWeaponKnockback());
                    Main.projectile[protmp].usesLocalNPCImmunity = true;
                    Main.projectile[protmp].localNPCHitCooldown = 10;
                }

                if (Projectile.ai[1] > 10)
                {
                    if (Collided || Projectile.ai[1] > 60 || (owner.GetSkillKeyJPStatus("ReverseBlast").HasValue && owner.GetSkillKeyJPStatus("ReverseBlast").Value))              //超过1秒,撞墙或者再次按下按键时
                    {
                        Phase = ReverseBlastPhase.Stop;
                        Projectile.ai[1] = 0;
                        owner.SetPlayerFallStart(owner.Center);
                        owner.velocity = MovingRotation.ToRotationVector2() * 5;
                        StartPos = owner.Center;
                        Connected = true;
                        Disappear = false;
                        KillSpear();
                    }
                }
            }
            if (Phase == ReverseBlastPhase.Stop)           //召虫急停，5帧
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;
                owner.velocity = MovingRotation.ToRotationVector2() * 5;
                owner.direction = Math.Sign(owner.velocity.X);
                Vector2 HoverPos = StartPos + owner.velocity * 5 - MovingRotation.ToRotationVector2() * HoverY;
                Projectile.Center = Vector2.Lerp(StartPos, HoverPos, Projectile.ai[1] / 5f);
                Projectile.spriteDirection = Math.Sign(HoverPos.X - StartPos.X + 0.01f);
                if (Projectile.ai[1] >= 5)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(10), 1 + Main.rand.NextFloat() * 0.5f);
                    }
                    Projectile.Center = HoverPos;
                    Phase = ReverseBlastPhase.Hover;
                    Projectile.ai[1] = 0;
                    owner.velocity = Vector2.Zero;
                }
            }
            else if (Phase == ReverseBlastPhase.Hover)       //暂时悬挂
            {
                Projectile.ai[1]++;
                float dist = Math.Clamp(owner.Distance(Projectile.Center), 0, HoverY);
                owner.Center = Projectile.Center + Vector2.Normalize(owner.Center - Projectile.Center) * dist;  //位置锁定

                Vector2 Unit = Vector2.Normalize(Projectile.Center - owner.Center);//速度锁定
                if (PlayerUtils.PointMulti(Unit, owner.velocity) < 0)  //存在反向速度分量
                {
                    owner.velocity -= Unit * PlayerUtils.PointMulti(Unit, owner.velocity);
                }
                if (Projectile.ai[1] > 20)
                {
                    if (!owner.GetModPlayer<WireBugPlayer>().PressingWireSkill1)
                    {
                        ReturningBug.Summon(owner, Projectile.Center, Projectile.spriteDirection);
                        Projectile.Kill();
                        return;
                    }
                }
            }

        }


        public override bool PreDraw(ref Color lightColor)
        {
            if (Connected)     //绘制虫丝
            {
                float percentage = 0;
                if (Phase == ReverseBlastPhase.Stop)
                {
                    Vector2 HoverPos = StartPos + Main.player[Projectile.owner].velocity * 5 - MovingRotation.ToRotationVector2() * HoverY;
                    percentage = Math.Clamp(Projectile.Distance(HoverPos) / (25 + HoverY), 0, 1);
                }
                DrawUtils.DrawWire(Main.player[Projectile.owner].Center, Projectile.Center + new Vector2(0, BugWireOffset), percentage, Color.White, 0.01f);
                //Terraria.Utils.DrawLine(Main.spriteBatch, Main.player[Projectile.owner].Center, Projectile.Center + new Vector2(0, 5), Color.Cyan, Color.Cyan, 2);
            }

            if (!Disappear)
            {
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


        private void KillSpear()
        {
            if (SpearProj == -1) return;
            Main.projectile[SpearProj].Kill();
            SpearProj = -1;
        }
    }
}