using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.System;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.SBlade
{
    public enum RisingDragonPhase
    {
        Default,
        Shoot,
        GPPause,
        Drag,
        GPDrag,
        Pause1,
        JumpForHit,
        SlashDown,
        Pause2,
    }
    public class RisingDragonProj : BaseSkillProj
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

        public int SwordProj = -1;

        public bool Hit = false;

        public int OwnerDefaultDirection = 1;

        public RisingDragonPhase Phase = RisingDragonPhase.Default;
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
            if (owner.IsDead() || !owner.hasRaisableShield)
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

            if (Phase == RisingDragonPhase.Shoot)           //发射翔虫,10帧，期间GP判定，GP成功转升龙拉扯
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }

                if (Projectile.ai[1] == 0)
                {
                    owner.ClearIFrame();
                    SummonSword(owner.HeldItem.type, MathHelper.Pi / 4, 0, 0);
                }

                Projectile.ai[1]++;
                Vector2 HoverPos = TargetPos + new Vector2(0, -HoverY);
                int timeNeeded = Math.Clamp((int)((StartPos - HoverPos).Length() / ShootSpeed), 1, 10);
                Projectile.Center = Vector2.Lerp(StartPos, HoverPos, Projectile.ai[1] / timeNeeded);
                Projectile.spriteDirection = OwnerDefaultDirection;
                owner.direction = OwnerDefaultDirection;

                float t = Math.Clamp(Projectile.ai[1] / timeNeeded * 2, 0, 1);
                Main.projectile[SwordProj].rotation = MathHelper.Lerp(MathHelper.Pi / 4, -MathHelper.Pi / 8 * 5, t);
                Main.projectile[SwordProj].localAI[0] = MathHelper.Lerp(MathHelper.Pi / 2, MathHelper.Pi / 8, t);
                float bladeOffsetX = MathHelper.Lerp(0, 25, t);
                owner.ChangeItemRotation(PlayerUtils.GetRotationByDirection(MathHelper.Lerp(MathHelper.Pi / 8, 0, t), owner.direction));
                (Main.projectile[SwordProj].ModProjectile as RisingDragonWeaponProj).OffSet = new Vector2(bladeOffsetX, 0);

                owner.GetModPlayer<MiscEffectPlayer>().ShieldRotation = MathHelper.Lerp(0, -MathHelper.Pi / 3, t);

                if (owner.GetModPlayer<MiscEffectPlayer>().JustHit > 0 && !Hit)
                {
                    Hit = true;
                    owner.SetIFrame(120);
                    Vector2 Center = owner.Center + new Vector2(10 * owner.direction, 0);
                    int protmp = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Center, Vector2.Zero, ModContent.ProjectileType<GPSpark>(), 0, 0, owner.whoAmI);
                    Main.projectile[protmp].rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
                }



                if (Projectile.ai[1] >= timeNeeded)
                {
                    if (Hit)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(10), 1 + Main.rand.NextFloat() * 0.5f);
                        }
                        StartPos = owner.Center;
                        Projectile.Center = HoverPos;
                        Phase = RisingDragonPhase.GPDrag;
                        Projectile.ai[1] = 0;
                        DamageProj.Summon(owner, owner.Center, 50, 50, owner.GetWeaponDamage() * 5, owner.GetWeaponKnockback());
                    }
                    else
                    {
                        StartPos = owner.Center;
                        Projectile.Center = HoverPos;
                        Phase = RisingDragonPhase.GPPause;
                        Projectile.ai[1] = 0;
                    }
                }
            }
            else if (Phase == RisingDragonPhase.GPPause)        //延长一段GP判定试试
            {
                for (int i = 0; i < 2; i++)
                {
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }

                Projectile.ai[1]++;

                owner.direction = OwnerDefaultDirection;
                owner.ChangeItemRotation(PlayerUtils.GetRotationByDirection(0, owner.direction));
                owner.GetModPlayer<MiscEffectPlayer>().ShieldRotation = -MathHelper.Pi / 3;

                if (owner.GetModPlayer<MiscEffectPlayer>().JustHit > 0 && !Hit)
                {
                    Hit = true;
                    owner.SetIFrame(120);
                    Vector2 Center = owner.Center + new Vector2(10 * owner.direction, 0);
                    int protmp = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Center, Vector2.Zero, ModContent.ProjectileType<GPSpark>(), 0, 0, owner.whoAmI);
                    Main.projectile[protmp].rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
                }

                if (Projectile.ai[1] >= 10)
                {
                    if (Hit)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(10), 1 + Main.rand.NextFloat() * 0.5f);
                        }
                        StartPos = owner.Center;
                        Phase = RisingDragonPhase.GPDrag;
                        Projectile.ai[1] = 0;
                        DamageProj.Summon(owner, owner.Center, 50, 50, owner.GetWeaponDamage() * 5, owner.GetWeaponKnockback());
                    }
                    else
                    {
                        StartPos = owner.Center;
                        Phase = RisingDragonPhase.Drag;
                        Projectile.ai[1] = 0;
                        DamageProj.Summon(owner, owner.Center, 50, 50, owner.GetWeaponDamage() * 5, owner.GetWeaponKnockback());
                    }
                }
            }
            else if (Phase == RisingDragonPhase.Drag)      //拉扯
            {
                ActivatingGP = false;
                ShieldLevel = 0;

                for (int i = 0; i < 2; i++)
                {
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }

                Projectile.ai[1]++;
                Vector2 HoverPos = TargetPos + new Vector2(0, -HoverY);
                Projectile.Center = HoverPos;
                int timeNeeded = Math.Clamp((int)((StartPos - TargetPos).Length() / DragSpeed), 1, 114514);
                owner.velocity = Vector2.Normalize(TargetPos - owner.Center) * (DragSpeed - 1);
                owner.position += Vector2.Normalize(owner.velocity);
                owner.direction = OwnerDefaultDirection;


                float t = Math.Clamp(Projectile.ai[1] / Math.Min(10, timeNeeded), 0, 1);
                Main.projectile[SwordProj].rotation = MathHelper.Lerp(-MathHelper.Pi / 8 * 5, -MathHelper.Pi / 4 * 3, t);
                Main.projectile[SwordProj].localAI[0] = MathHelper.Lerp(MathHelper.Pi / 8, -MathHelper.Pi / 2, t);
                float bladeOffsetX = MathHelper.Lerp(25, 0, t);
                (Main.projectile[SwordProj].ModProjectile as RisingDragonWeaponProj).OffSet = new Vector2(bladeOffsetX, 0);
                owner.ChangeItemRotation(PlayerUtils.GetRotationByDirection(MathHelper.Lerp(0, MathHelper.Pi / 4 * 3, t), owner.direction));

                owner.GetModPlayer<MiscEffectPlayer>().ShieldRotation = -MathHelper.Pi / 3f;
                owner.GetModPlayer<MiscEffectPlayer>().ShieldOffset = new Vector2(0, -5);// new Vector2(0, MathHelper.Lerp(0, -20, t));

                if (Projectile.ai[1] >= timeNeeded || owner.Distance(TargetPos) <= DragSpeed / 1.5f)
                {
                    owner.velocity = new Vector2(0, -8);
                    owner.SetPlayerFallStart(StartPos);
                    Projectile.ai[1] = 0;
                    Phase = RisingDragonPhase.Pause1;
                    Connected = false;
                }
            }
            else if (Phase == RisingDragonPhase.GPDrag)      //GP拉扯，带伤害判定
            {
                ActivatingGP = false;
                ShieldLevel = 0;

                for (int i = 0; i < 2; i++)
                {
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }

                Projectile.ai[1]++;
                Vector2 HoverPos = TargetPos + new Vector2(0, -HoverY);
                Projectile.Center = HoverPos;
                int timeNeeded = Math.Clamp((int)((StartPos - TargetPos).Length() / DragSpeed), 1, 114514);
                owner.velocity = Vector2.Normalize(TargetPos - owner.Center) * (DragSpeed - 1);
                owner.position += Vector2.Normalize(owner.velocity);
                owner.direction = OwnerDefaultDirection;

                if (Projectile.ai[1] % 7 <= 3)
                {
                    owner.direction = OwnerDefaultDirection;
                }
                else
                {
                    owner.direction = -OwnerDefaultDirection;
                }

                float t = Math.Clamp(Projectile.ai[1] / Math.Min(10, timeNeeded), 0, 1);
                Main.projectile[SwordProj].rotation = MathHelper.Lerp(-MathHelper.Pi / 8 * 5, -MathHelper.Pi, t);
                Main.projectile[SwordProj].localAI[0] = MathHelper.Lerp(MathHelper.Pi / 8, -MathHelper.Pi / 2, t);
                float bladeOffsetX = MathHelper.Lerp(25, 0, t);
                (Main.projectile[SwordProj].ModProjectile as RisingDragonWeaponProj).OffSet = new Vector2(bladeOffsetX, 0);
                //owner.ChangeItemRotation(PlayerUtils.GetRotationByDirection(MathHelper.Lerp(0, MathHelper.Pi / 4 * 3, t), owner.direction));

                owner.GetModPlayer<MiscEffectPlayer>().ShieldRotation = -MathHelper.Pi / 3f;
                owner.GetModPlayer<MiscEffectPlayer>().ShieldOffset = new Vector2(0, -5);// new Vector2(0, MathHelper.Lerp(0, -20, t));

                if (Projectile.ai[1] >= timeNeeded || owner.Distance(TargetPos) <= DragSpeed / 1.5f)
                {
                    owner.velocity = new Vector2(0, -12);
                    owner.SetPlayerFallStart(StartPos);
                    Projectile.ai[1] = 0;
                    Phase = RisingDragonPhase.Pause1;
                    Connected = false;
                }
            }
            else if (Phase == RisingDragonPhase.Pause1)       //短暂暂停,30帧
            {
                Projectile.ai[1]++;

                owner.GetModPlayer<MiscEffectPlayer>().ShieldRotation = -MathHelper.Pi / 3f;
                owner.GetModPlayer<MiscEffectPlayer>().ShieldOffset = new Vector2(0, -5);
                owner.direction = OwnerDefaultDirection;

                if (Projectile.ai[1] > 10)     //劈落派生
                {
                    if (owner.PressLeftInGame())
                    {
                        Projectile.ai[1] = 0;
                        Phase = RisingDragonPhase.JumpForHit;
                        StartPos = owner.Center;
                        owner.velocity = new Vector2(0, -8);
                        KillSword();
                        return;
                    }
                }

                if (Projectile.ai[1] > 30 || (owner.velocity.Y == 0 && owner.oldVelocity.Y == 0))     //时间过长或者落地
                {
                    KillSword();
                    ReturningBug.Summon(owner, Projectile.Center, Projectile.spriteDirection);
                    Projectile.Kill();
                    return;
                }
            }
            else if (Phase == RisingDragonPhase.JumpForHit)        //劈落派生的上跳
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] == 1)
                {
                    SummonSword(owner.HeldItem.type, Hit ? MathHelper.Pi : MathHelper.Pi / 4 * 3, owner.GetWeaponDamage() * 5, owner.GetWeaponKnockback(), 10);
                    Main.projectile[SwordProj].localAI[0] = MathHelper.Pi / 2f;
                }

                Main.projectile[SwordProj].rotation = MathHelper.Lerp(Hit ? MathHelper.Pi : MathHelper.Pi / 4 * 3, 0, Projectile.ai[1] / 10f);
                owner.GetModPlayer<MiscEffectPlayer>().ShieldRotation = MathHelper.Lerp(-MathHelper.Pi / 3f, 0, Projectile.ai[1] / 10);
                owner.GetModPlayer<MiscEffectPlayer>().ShieldOffset = new Vector2(0, MathHelper.Lerp(-5, 0, Projectile.ai[1] / 10));
                //owner.ChangeItemRotation(PlayerUtils.GetRotationByDirection(MathHelper.Lerp(0, MathHelper.Pi / 4 * 3, t), owner.direction));


                if (Projectile.ai[1] > 10)
                {
                    Projectile.ai[1] = 0;
                    StartPos = owner.Center;
                    Phase = RisingDragonPhase.SlashDown;
                }
            }
            else if (Phase == RisingDragonPhase.SlashDown)       //劈落
            {
                owner.SetIFrame(120);
                for (int i = 0; i < 2; i++)
                {
                    GenDust(Projectile.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }
                Projectile.ai[1]++;

                int timeNeeded = Math.Clamp((int)((StartPos - DownPos).Length() / DragSpeed2), 1, 114514);
                owner.velocity = Vector2.Normalize(DownPos - owner.Center) * (DragSpeed2 - 1);
                owner.position += Vector2.Normalize(owner.velocity);


                if (Projectile.ai[1] >= timeNeeded || owner.Distance(DownPos) <= DragSpeed2 / 1.5f)
                {
                    owner.SetPlayerFallStart(StartPos);
                    Projectile.ai[1] = 0;
                    Phase = RisingDragonPhase.Pause2;
                    StartPos = Projectile.Center;
                }
            }
            else if (Phase == RisingDragonPhase.Pause2)       //短暂暂停,20帧
            {
                Projectile.ai[1]++;
                owner.velocity.X = 0f;

                if (Projectile.ai[1] > 20)
                {
                    KillSword();
                    ReturningBug.Summon(owner, Projectile.Center, Projectile.spriteDirection);
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
                if (Phase == RisingDragonPhase.Shoot)
                {
                    Vector2 HoverPos = TargetPos + new Vector2(0, -HoverY);
                    percentage = Projectile.Distance(HoverPos) / HoverPos.Distance(StartPos);
                }
                DrawUtils.DrawWire(Main.player[Projectile.owner].Center, Projectile.Center + new Vector2(0, BugWireOffset), percentage, Color.Cyan, 0.01f);
                //Terraria.Utils.DrawLine(Main.spriteBatch, Main.player[Projectile.owner].Center, Projectile.Center + new Vector2(0, 5), Color.Cyan, Color.Cyan, 2);
            }

            if (BecomeTrail)        //绘制拖尾
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
        public override void OnKill(int timeLeft)
        {
            Main.player[Projectile.owner].fullRotation = 0;
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
            int protmp = Projectile.NewProjectile(owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, 0), owner.Center, Vector2.Zero, ModContent.ProjectileType<RisingDragonWeaponProj>(), damage, kb, owner.whoAmI);
            if (protmp >= 0)
            {
                Main.projectile[protmp].rotation = rot;
                Main.projectile[protmp].localNPCHitCooldown = hitCooldown;
                RisingDragonWeaponProj modproj = Main.projectile[protmp].ModProjectile as RisingDragonWeaponProj;
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