using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.Lance
{

    public class LanceWeaponProj : ModProjectile
    {
        public int ProjType = -1;

        public int ProjOwner = -1;

        public int Behavior = 0;

        public float OffSet = 0;

        public bool Hit = false;
        public override string Texture => "WireBugMod/Images/PlaceHolder";
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
            Projectile.damage = 1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.ownerHitCheck = true;
        }
        public override void AI()
        {
            if (ProjType == -1)
            {
                Projectile.Kill();
                return;
            }

            if (ProjOwner == -1 || !Main.projectile[ProjOwner].active)
            {
                Projectile.Kill();
                return;
            }


            Player owner = Main.player[Projectile.owner];
            if (owner.IsDead())
            {
                Projectile.Kill();
                return;
            }
            owner.heldProj = Projectile.whoAmI;
            Projectile.Center = owner.Center;
            owner.itemTime = owner.itemAnimation = 2;

            owner.ChangeItemRotation(Projectile.rotation, false);

            if (Behavior == 1)
            {
                Projectile.ai[1]++;

                float scale = 1;
                if (ProjType == ProjectileID.JoustingLance || ProjType == ProjectileID.HallowJoustingLance || ProjType == ProjectileID.ShadowJoustingLance)
                {
                    scale = 0.5f;
                }
                if (Projectile.ai[1] < 15)          //后仰
                {
                    OffSet = MathHelper.Lerp(0, -45, Projectile.ai[1] / 15f);
                    Projectile.localAI[0] = MathHelper.Lerp(0, MathHelper.Pi / 3 * 2 * owner.direction, Projectile.ai[1] / 15f);
                }
                else if (Projectile.ai[1] < 20)          //转向
                {
                    OffSet = -45;
                    Projectile.localAI[0] = MathHelper.Lerp(MathHelper.Pi / 3 * 2 * owner.direction, MathHelper.Pi / 6 * owner.direction, (Projectile.ai[1] - 15) / 5f);
                }
                else if (Projectile.ai[1] < 25) //前刺
                {
                    OffSet = MathHelper.Lerp(-45, 60, (Projectile.ai[1] - 20) / 5f);
                    Projectile.localAI[0] = MathHelper.Lerp(MathHelper.Pi / 6 * owner.direction, 0, (Projectile.ai[1] - 20) / 5f);
                }
                else                //维持
                {
                    OffSet = 60;
                    Projectile.localAI[0] = 0;
                }
                Projectile.Center = owner.Center + Projectile.rotation.ToRotationVector2() * OffSet * scale;
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].SetIFrame(120);
            Hit = true;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = DrawUtils.GetProjTexture(ProjType);

            Vector2 unit = Projectile.rotation.ToRotationVector2();
            float point = 1;
            float dist = 0;
            //float ScaleY = (float)Math.Cos(Projectile.localAI[0]);
            float length = tex.Size().Distance(Vector2.Zero) * owner.GetAdjustedItemScale(owner.HeldItem);
            if (ProjType == ProjectileID.JoustingLance || ProjType == ProjectileID.HallowJoustingLance || ProjType == ProjectileID.ShadowJoustingLance)
            {
                dist = length / 3f;
            }
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - unit * (length / 2f - dist), Projectile.Center + unit * (length / 2f + dist), 20, ref point);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = DrawUtils.GetProjTexture(ProjType);
            Vector2 origin = tex.Size() / 2f;
            //float ScaleY = (float)Math.Cos(Projectile.localAI[0]);
            if (ProjType == ProjectileID.JoustingLance || ProjType == ProjectileID.HallowJoustingLance || ProjType == ProjectileID.ShadowJoustingLance)
            {
                origin = new Vector2(owner.direction < 0 ? tex.Size().X / 6f : tex.Size().X / 6f * 5f, tex.Size().Y * 5 / 6f);
            }

            SpriteEffects spriteEffects = owner.direction >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float rot = owner.direction <= 0 ? Projectile.rotation + MathHelper.Pi / 4 : Projectile.rotation + MathHelper.Pi / 4 * 3;
            if (ProjType == 699)    //恐怖关刀
            {
                spriteEffects = owner.direction >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                rot = owner.direction <= 0 ? Projectile.rotation + MathHelper.Pi / 4 * 3 : Projectile.rotation + MathHelper.Pi / 4;
            }
            Main.spriteBatch.Draw(tex,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor * (1 - owner.immuneAlpha / 255f),
                rot,
                origin,
                owner.GetAdjustedItemScale(owner.HeldItem),
                spriteEffects,
                0);

            return false;
        }

        public override bool? CanDamage()
        {
            if (Behavior == 1 && Projectile.ai[1] <= 25)
            {
                return false;
            }
            return null;
        }

        public Vector2 GetTipPos()
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = DrawUtils.GetProjTexture(ProjType);
            float length = tex.Size().Distance(Vector2.Zero) * owner.GetAdjustedItemScale(owner.HeldItem);
            float dist = 0;
            Vector2 unit = Projectile.rotation.ToRotationVector2();
            if (ProjType == ProjectileID.JoustingLance || ProjType == ProjectileID.HallowJoustingLance || ProjType == ProjectileID.ShadowJoustingLance)
            {
                dist = length / 3f;
            }

            return Projectile.Center + unit * (length / 2f + dist - 5f);
        }
    }
}