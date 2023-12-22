using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.System;
using WireBugMod.UI;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.SBlade
{
    public enum TwinVinePhase
    {
        Pierce,
        PierceButNoDamage,
        Stay,
        MoveForward,
        Default
    }

    public class TwinVineProj : BaseSkillProj        //特殊：这个弹幕会造成伤害
    {
        public override string Texture => "WireBugMod/Images/PlaceHolder";

        public TwinVinePhase Phase = TwinVinePhase.Default;

        bool Connected = false;

        public int ItemType = 1;
        public int OriginalDir = 1;

        public int Target = -1;
        private Vector2 SavedRelaPos = Vector2.Zero;
        private int SavedDir = 1;
        private float SavedRot = 0;

        private int Cooldown = 0;

        public const float DragSpeed = 30;

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
            if (owner.IsDead() || UIManager.Visible)
            {
                Projectile.Kill();
                return;
            }


            Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 1.5f, 1.5f, 1.5f);

            if (Phase == TwinVinePhase.Pierce)
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
                    Phase = TwinVinePhase.PierceButNoDamage;
                    Projectile.ai[1] = 0;
                    Projectile.friendly = false;
                }
            }
            else if (Phase == TwinVinePhase.PierceButNoDamage)         //仅为视觉特效，不造成伤害
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
            else if (Phase == TwinVinePhase.Stay)
            {
                if (Main.rand.NextBool(12))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        SkillUtils.GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(5), 1 + Main.rand.NextFloat() * 0.5f);
                    }
                }
                if (Target == -1 || !Main.npc[Target].active || !Main.npc[Target].CanBeChasedBy() || Main.npc[Target].Distance(owner.Center) > 1500)
                {
                    Projectile.Kill();
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

                if (Cooldown == 0)
                {
                    if (!owner.GetModPlayer<WireBugPlayer>().LockInput && !owner.CCed && !owner.ItemAnimationActive && !owner.shimmerWet && !UIManager.Visible)        //可以操作,没有在微光里,没有被定住,且没有同时使用物品，没有打开翔虫UI
                    {
                        if (owner.GetSkillKeyJPStatus("TwinVine").HasValue && owner.GetSkillKeyJPStatus("TwinVine").Value)
                        {
                            Phase = TwinVinePhase.MoveForward;
                            LockInput = true;
                            owner.RemoveAllGrapplingHooks();
                            owner.mount.Dismount(owner);
                            return;
                        }
                    }
                }

                if (Cooldown > 0) Cooldown--;

                Projectile.ai[1]++;
                if (Projectile.ai[1] == 10)
                {
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
                if (Projectile.ai[1] > 1200)
                {
                    Projectile.Kill();
                    return;
                }

            }
            else if (Phase == TwinVinePhase.MoveForward)    //拉扯前进
            {
                if (Main.rand.NextBool(12))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        SkillUtils.GenDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), Main.rand.Next(5), 1 + Main.rand.NextFloat() * 0.5f);
                    }
                }
                if (Target == -1 || !Main.npc[Target].active || !Main.npc[Target].CanBeChasedBy() || Main.npc[Target].Distance(owner.Center) > 1500)
                {
                    Projectile.Kill();
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

                for (int i = 0; i < 2; i++)
                {
                    SkillUtils.GenDust(owner.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), 0, 1 + Main.rand.NextFloat() * 0.5f);
                }

                if (owner.GetSkillKeyJPStatus("TwinVine").HasValue && owner.GetSkillKeyJPStatus("TwinVine").Value)  //提前停止
                {
                    Phase = TwinVinePhase.Stay;
                    Projectile.ai[2] = 0;
                    Cooldown = 120;
                    LockInput = false;
                    owner.velocity = Vector2.Normalize(owner.velocity) * 1;
                    return;
                }

                Projectile.ai[2]++;        //保险栓，每次只能前进300帧时间

                Vector2 TargetPos = Main.npc[Target].Center;
                float DistMin = (Main.npc[Target].width + Main.npc[Target].height) / 4f * 1.4f + 100f;
                owner.velocity = Vector2.Normalize(TargetPos - owner.Center) * (DragSpeed - 1);

                bool Colliding = Collision.SolidTiles(owner.position + Vector2.Normalize(owner.velocity), owner.width, owner.height);
                if (!Colliding)
                {
                    owner.position += Vector2.Normalize(owner.velocity);
                }
                owner.direction = Math.Sign(TargetPos.X - owner.Center.X);
                if (Colliding || owner.Distance(TargetPos) <= DistMin || Projectile.ai[2] > 300)
                {
                    owner.velocity = Vector2.Normalize(owner.velocity) * 1;
                    Phase = TwinVinePhase.Stay;
                    Projectile.ai[2] = 0;
                    Cooldown = 120;
                    LockInput = false;
                }

            }
            else if (Phase == TwinVinePhase.Default)
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

            Texture2D tex = DrawUtils.GetItemTexture(ItemType);
            if (Connected)
            {
                float OffSet = tex.Size().Length() / 2f;
                Vector2 DrawEnd = Projectile.Center - Projectile.rotation.ToRotationVector2() * OffSet;
                Color LineColor = Color.White;
                if (owner.Distance(Main.npc[Target].Center) > 1250)
                {
                    LineColor = Color.Red;
                }
                Terraria.Utils.DrawLine(Main.spriteBatch, DrawEnd, owner.Center, LineColor, Color.Transparent, 2);
            }

            if (Phase == TwinVinePhase.Pierce || Phase == TwinVinePhase.PierceButNoDamage)
            {
                Vector2 origin = tex.Size() / 2f;
                SpriteEffects spriteEffects = owner.direction >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                float rot = Projectile.rotation;
                rot += owner.direction >= 0 ? MathHelper.Pi / 4 : MathHelper.Pi / 4 * 3;


                Main.spriteBatch.Draw(tex,
                    Projectile.Center - Main.screenPosition,
                    null,
                    lightColor,
                    rot,
                    origin,
                    Projectile.scale,
                    spriteEffects,
                    0);
            }
            else
            {
                if (Target == -1) return false;
                bool RightDir = OriginalDir >= 0 && (SavedDir == Main.npc[Target].spriteDirection) || OriginalDir < 0 && (SavedDir != Main.npc[Target].spriteDirection);
                Vector2 origin = tex.Size() / 2f;

                SpriteEffects spriteEffects = RightDir ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                float rot = Projectile.rotation;
                rot += RightDir ? MathHelper.Pi / 4 : MathHelper.Pi / 4 * 3;


                Main.spriteBatch.Draw(tex,
                    Projectile.Center - Main.screenPosition,
                    null,
                    lightColor,
                    rot,
                    origin,
                    Projectile.scale,
                    spriteEffects,
                    0);
            }


            return false;
        }


        public override bool? CanHitNPC(NPC target)
        {
            if (Phase == TwinVinePhase.Pierce && Target == -1 && target.CanBeChasedBy())
            {
                return null;
            }
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)        //48,20
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = DrawUtils.GetItemTexture(ItemType);
            float dist = Math.Max(tex.Width, tex.Height) * owner.GetAdjustedItemScale(owner.HeldItem);
            float rot = Projectile.rotation;
            Vector2 UnitX = (rot + MathHelper.Pi / 4).ToRotationVector2();
            Vector2 UnitY = (rot - MathHelper.Pi / 4).ToRotationVector2();
            float point = 1;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center + UnitX * dist * 0.5f, Projectile.Center + UnitX * dist * 0.5f + UnitY * dist, dist, ref point);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Target = target.whoAmI;
            Phase = TwinVinePhase.Stay;
            Projectile.ai[1] = 0;

            SavedDir = target.spriteDirection;
            SavedRot = Projectile.rotation - target.rotation;
            SavedRelaPos = ((Projectile.Center + target.Center) / 2f - target.Center).RotatedBy(-target.rotation);
            DisableMeleeEffect = false;
            LockBug = false;
            Connected = true;
        }
    }
}