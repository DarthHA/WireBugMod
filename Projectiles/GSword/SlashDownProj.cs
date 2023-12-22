using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Projectiles.Weapons;
using WireBugMod.Skills;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.GSword
{
    public enum SlashDownPhase
    {
        Default,
        Shoot1,
        Drag1,
        Pause1,
        Falling,
        Pause2,
        JumpForHit,
        Charging,
        ChargeSlash,
        SlashDown,
    }
    public class SlashDownProj : BaseSkillProj
    {
        public Vector2 TargetPos = Vector2.Zero;
        public Vector2 StartPos = Vector2.Zero;
        public Vector2 DownPos = Vector2.Zero;
        public bool Connected = true;
        public bool Disappear = false;

        public const float HoverY = 50;
        public const float BugWireOffset = 10;
        public const float ShootSpeed = 20;
        public const float DragSpeed = 30;
        public const float DragSpeed2 = 30;
        public const float ReturnSpeed = 20;

        public int SwordProj = -1;

        internal int DamageScale = 0;

        public bool Hit = false;

        public SlashDownPhase Phase = SlashDownPhase.Default;
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
                    SkillUtils.GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(5), 1 + Main.rand.NextFloat() * 0.5f);
                }
            }

            Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 1.5f, 1.5f, 1.5f);

            if (Phase == SlashDownPhase.Shoot1)           //发射翔虫,10帧
            {
                for (int i = 0; i < 2; i++)
                {
                    SkillUtils.GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }

                if (Projectile.ai[1] == 0)
                {
                    GSwordWeaponProj.SummonSword(Projectile, ref SwordProj, -MathHelper.Pi / 4, 0);
                }

                Projectile.ai[1]++;
                Vector2 HoverPos = TargetPos + new Vector2(0, -HoverY);
                int timeNeeded = Math.Clamp((int)((StartPos - HoverPos).Length() / ShootSpeed), 1, 10);
                Projectile.Center = Vector2.Lerp(StartPos, HoverPos, Projectile.ai[1] / timeNeeded);
                Projectile.spriteDirection = Math.Sign(HoverPos.X - StartPos.X);
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);

                Main.projectile[SwordProj].rotation = MathHelper.Lerp(-MathHelper.Pi / 4, -MathHelper.Pi / 6 * 7, Projectile.ai[1] / timeNeeded);

                if (Projectile.ai[1] >= timeNeeded)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        SkillUtils.GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(10), 1 + Main.rand.NextFloat() * 0.5f);
                    }
                    StartPos = owner.Center;
                    Projectile.Center = HoverPos;
                    Phase = SlashDownPhase.Drag1;
                    Projectile.ai[1] = 0;

                }
            }
            else if (Phase == SlashDownPhase.Drag1)      //拉扯
            {
                for (int i = 0; i < 2; i++)
                {
                    SkillUtils.GenDust(owner.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;
                if (Projectile.ai[1] == 1)
                {
                    GSwordWeaponProj.SummonSword(Projectile, ref SwordProj, -MathHelper.Pi / 6 * 7, SkillDamageData.SlashDown.Base1);
                }
                Vector2 HoverPos = TargetPos + new Vector2(0, -HoverY);
                Projectile.Center = HoverPos;
                int timeNeeded = Math.Clamp((int)((StartPos - TargetPos).Length() / DragSpeed), 1, 114514);
                owner.velocity = Vector2.Normalize(TargetPos - owner.Center) * (DragSpeed - 1);
                owner.position += Vector2.Normalize(owner.velocity);
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);

                owner.fullRotationOrigin = owner.Size / 2f;
                owner.fullRotation = Math.Clamp(Projectile.ai[1] / timeNeeded, 0, 1) * MathHelper.TwoPi * 0.25f * owner.direction;

                if ((Main.projectile[SwordProj].ModProjectile as GSwordWeaponProj).HitCount > 0 && !Hit)      //打中人了
                {
                    Hit = true;
                    owner.SetIFrame(120);
                    if (owner.PressLeftInGame())
                    {
                        Projectile.ai[1] = 0;
                        Phase = SlashDownPhase.JumpForHit;
                        StartPos = owner.Center;
                        owner.SetPlayerFallStart(StartPos);
                        Connected = false;
                        Disappear = true;
                        ReturningBug.Summon(owner, Projectile.Center, Projectile.spriteDirection);
                        return;
                    }
                }

                if (Projectile.ai[1] >= timeNeeded || owner.Distance(TargetPos) <= DragSpeed / 1.5f)
                {
                    owner.velocity = new Vector2(0, -8);
                    owner.SetPlayerFallStart(StartPos);
                    Projectile.ai[1] = 0;
                    Phase = SlashDownPhase.Pause1;
                    Connected = false;
                    Disappear = true;
                    ReturningBug.Summon(owner, Projectile.Center, Projectile.spriteDirection);
                }
            }
            else if (Phase == SlashDownPhase.Pause1)       //短暂暂停+旋转,20帧
            {
                Projectile.ai[1]++;

                //owner.velocity = Vector2.Zero;
                if (Projectile.ai[1] < 20)
                {
                    owner.fullRotationOrigin = owner.Size / 2f;
                    //owner.fullRotation = (MathHelper.TwoPi * 0.75f + Projectile.ai[1] / 10f * MathHelper.TwoPi * 0.25f) * owner.direction;
                    owner.fullRotation = (MathHelper.TwoPi * 0.25f + Projectile.ai[1] / 20f * MathHelper.TwoPi * 0.75f) * owner.direction;
                }
                else
                {
                    owner.fullRotationOrigin = owner.Size / 2f;
                    owner.fullRotation = 0;
                }

                if ((Main.projectile[SwordProj].ModProjectile as GSwordWeaponProj).HitCount > 0 && !Hit)      //打中人了
                {
                    Hit = true;
                    owner.SetIFrame(120);
                    if (owner.PressLeftInGame())
                    {
                        Projectile.ai[1] = 0;
                        Phase = SlashDownPhase.JumpForHit;
                        StartPos = owner.Center;
                        return;
                    }
                }

                if (Projectile.ai[1] > 30)         //重新校正下批坐标
                {

                    Vector2 targetVec = Main.MouseWorld - owner.Center;
                    if (targetVec.Y != 0 && Math.Abs(targetVec.X / targetVec.Y) > 0.5f)
                    {
                        targetVec.X = Math.Abs(0.5f * targetVec.Y) * Math.Sign(targetVec.X);
                    }
                    if (targetVec.Length() < 300)
                    {
                        targetVec = Vector2.Normalize(targetVec) * 300;
                    }
                    DownPos = PlayerUtils.SearchForNotBlockedPos(owner.Center, owner.Center + new Vector2(targetVec.X, Math.Abs(targetVec.Y)));

                    Projectile.ai[1] = 0;
                    Phase = SlashDownPhase.Falling;
                    StartPos = owner.Center;
                }
            }
            else if (Phase == SlashDownPhase.Falling)      //劈落
            {
                Projectile.ai[1]++;

                if (Projectile.ai[1] == 1)
                {
                    GSwordWeaponProj.SummonSword(Projectile, ref SwordProj, -MathHelper.Pi / 6 * 7, SkillDamageData.SlashDown.Base2);
                }

                int timeNeeded = Math.Clamp((int)((StartPos - DownPos).Length() / DragSpeed2), 1, 114514);
                owner.velocity = Vector2.Normalize(DownPos - owner.Center) * (DragSpeed2 - 1);
                owner.position += Vector2.Normalize(owner.velocity);
                owner.direction = Math.Sign(DownPos.X - owner.Center.X + 0.01f);

                Main.projectile[SwordProj].rotation = MathHelper.Lerp(-MathHelper.Pi / 6 * 7, MathHelper.Pi / 6, Math.Clamp(Projectile.ai[1] / Math.Min(timeNeeded, 10), 0, 1));

                if ((Main.projectile[SwordProj].ModProjectile as GSwordWeaponProj).HitCount > 0  && !Hit)      //打中人了
                {
                    Hit = true;
                    owner.SetIFrame(120);
                    if (owner.PressLeftInGame())
                    {
                        Projectile.ai[1] = 0;
                        Phase = SlashDownPhase.JumpForHit;
                        StartPos = owner.Center;
                        return;
                    }
                }

                if (Projectile.ai[1] >= timeNeeded || owner.Distance(DownPos) <= DragSpeed2 / 1.5f)
                {
                    owner.SetPlayerFallStart(StartPos);
                    Projectile.ai[1] = 0;
                    Phase = SlashDownPhase.Pause2;
                    StartPos = Projectile.Center;
                }
            }
            else if (Phase == SlashDownPhase.JumpForHit)        //命中目标的上跳,不蓄力就正常下落，蓄力就蓄力后蓄力斩
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] == 1)
                {
                    owner.fullRotation = 0;
                    owner.fullRotationOrigin = owner.Size / 2;
                    GSwordWeaponProj.SummonSword(Projectile, ref SwordProj, MathHelper.Pi / 6, 0);
                }
                if (Projectile.ai[1] < 5)
                {
                    owner.velocity = Vector2.Zero;
                }
                else if (Projectile.ai[1] == 5)
                {
                    owner.velocity = new Vector2(0, -15);
                }
                else if (Projectile.ai[1] < 25)
                {
                    owner.fullRotation = (Projectile.ai[1] - 5) / 20f * MathHelper.TwoPi * owner.direction;
                    owner.fullRotationOrigin = owner.Size / 2;
                    Main.projectile[SwordProj].rotation = MathHelper.Lerp(MathHelper.Pi / 6, -MathHelper.Pi / 3 * 2, (Projectile.ai[1] - 5) / 20);
                }
                else if (Projectile.ai[1] == 25)
                {
                    owner.fullRotation = 0;
                    owner.fullRotationOrigin = owner.Size / 2;
                    Main.projectile[SwordProj].rotation = -MathHelper.Pi / 3 * 2;
                    Projectile.ai[1] = 0;
                    if (owner.PressLeftInGame())
                    {
                        Phase = SlashDownPhase.Charging;
                        if (owner.velocity.Y < 5) owner.velocity.Y = 0;
                    }
                    else
                    {
                        //ReturningBug.Summon(owner, Projectile.Center, Projectile.spriteDirection);
                        Projectile.Kill();
                        return;
                    }
                }
            }
            else if (Phase == SlashDownPhase.Charging)
            {
                Projectile.ai[1]++;

                Main.projectile[SwordProj].rotation = MathHelper.Lerp(-MathHelper.Pi / 3 * 2, -MathHelper.Pi / 6 * 7, Math.Clamp(Projectile.ai[1] / 10, 0, 1));
                if (owner.velocity.Y > 2) owner.velocity.Y = 2;
                if (owner.velocity.Y == 0 && owner.oldVelocity.Y == 0)       //提前落地
                {
                    //ReturningBug.Summon(owner, Projectile.Center, Projectile.spriteDirection);
                    Projectile.Kill();
                    return;
                }

                if (Projectile.ai[1] > 10)        //10为0蓄，30为1蓄，50为2蓄，70为3蓄
                {
                    if (Projectile.ai[1] == 30 || Projectile.ai[1] == 50 || Projectile.ai[1] == 70)
                    {
                        DamageScale++;

                        float start = MathHelper.TwoPi * Main.rand.NextFloat();
                        for (int i = 0; i < 30; i++)
                        {
                            float r = start + MathHelper.TwoPi * i / 30f;
                            int dusttmp = Dust.NewDust(owner.Center, 1, 1, DustID.BubbleBurst_White);
                            Main.dust[dusttmp].position = owner.Center;
                            Main.dust[dusttmp].color = Color.Orange;
                            Main.dust[dusttmp].velocity = r.ToRotationVector2() * 5;
                            Main.dust[dusttmp].scale = 1.5f;
                            Main.dust[dusttmp].noGravity = true;
                            Main.dust[dusttmp].noLight = false;
                        }
                    }
                }

                if (Projectile.ai[1] > 10 && !owner.PressLeftInGame())     //下劈
                {
                    owner.SetIFrame(120);
                    Phase = SlashDownPhase.ChargeSlash;
                    Projectile.ai[1] = 0;

                    StartPos = owner.Center;
                    DownPos = PlayerUtils.SearchForNotBlockedPos(owner.Center, owner.Center + new Vector2(0, 300));
                    if (DownPos.Distance(owner.Center) < 100) DownPos = owner.Center + new Vector2(0, 100);

                }

            }
            else if (Phase == SlashDownPhase.ChargeSlash)       //蓄力劈落
            {
                owner.SetIFrame(120);

                if (SleepTimer > 0)
                {
                    owner.velocity = Vector2.Normalize(DownPos - owner.Center);
                    return;
                }

                Projectile.ai[1]++;
                
                if (Projectile.ai[1] == 1)
                {
                    GSwordWeaponProj.SummonSword(Projectile, ref SwordProj, -MathHelper.Pi / 6 * 7, SkillDamageData.SlashDown.Base * SkillDamageData.SlashDown.Charge[DamageScale], 999, "SlashDown");
                }

                int timeNeeded = Math.Clamp((int)((StartPos - DownPos).Length() / DragSpeed2), 1, 114514);
                owner.velocity = Vector2.Normalize(DownPos - owner.Center) * (DragSpeed2 - 1);
                owner.position += Vector2.Normalize(owner.velocity);

                Main.projectile[SwordProj].rotation = MathHelper.Lerp(-MathHelper.Pi / 6 * 7, MathHelper.Pi / 6, Math.Clamp(Projectile.ai[1] / Math.Min(timeNeeded, 10), 0, 1));

                if (Projectile.ai[1] >= timeNeeded || owner.Distance(DownPos) <= DragSpeed2 / 1.5f)
                {
                    Projectile.ai[1] = 0;
                    Phase = SlashDownPhase.Pause2;
                    StartPos = Projectile.Center;
                }
            }
            else if (Phase == SlashDownPhase.Pause2)       //短暂暂停,20帧
            {
                Projectile.ai[1]++;
                if (owner.velocity.Y == 0)
                {
                    owner.velocity.X = 0f;
                }

                if (Projectile.ai[1] > 20)
                {
                    //ReturningBug.Summon(owner, Projectile.Center, Projectile.spriteDirection);
                    Projectile.Kill();
                    return;
                }
            }


        }




        public override bool PreDraw(ref Color lightColor)
        {
            if (Disappear) return false;

            if (Connected)     //绘制虫丝
            {
                float percentage = 0;
                if (Phase == SlashDownPhase.Shoot1)
                {
                    Vector2 HoverPos = TargetPos + new Vector2(0, -HoverY);
                    percentage = Projectile.Distance(HoverPos) / HoverPos.Distance(StartPos);
                }
                DrawUtils.DrawWire(Main.player[Projectile.owner].Center, Projectile.Center + new Vector2(0, BugWireOffset), percentage, Color.White, 0.01f);
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
        public override void OnKill(int timeLeft)
        {
            Main.player[Projectile.owner].fullRotation = 0;
        }


    }
}