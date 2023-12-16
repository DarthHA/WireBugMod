using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Data;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.Weapons
{

    public class LanceWeaponProj : BaseWeaponProj
    {
        public override bool IsProjTexture => true;


        public float OffSet = 0;

        public override void SafeAI(Player owner)
        {
            owner.heldProj = Projectile.whoAmI;
            Projectile.Center = owner.Center;
            owner.itemTime = owner.itemAnimation = 2;

            owner.ChangeItemRotation(Projectile.rotation, false);

            if (Behavior == "LanceGuard")
            {
                Projectile.ai[1]++;

                float scale = 1;
                if (TexType == ProjectileID.JoustingLance || TexType == ProjectileID.HallowJoustingLance || TexType == ProjectileID.ShadowJoustingLance)
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

        public override void SafeOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.SafeOnHit(target, hit, damageDone);
        }

        public override bool? SafeColliding(Player owner, Vector2 TexSize, Rectangle targetHitbox)
        {
            Vector2 unit = Projectile.rotation.ToRotationVector2();
            float point = 1;
            float dist = 0;
            //float ScaleY = (float)Math.Cos(Projectile.localAI[0]);
            float length = TexSize.Length() * owner.GetAdjustedItemScale(owner.HeldItem);
            if (TexType == ProjectileID.JoustingLance || TexType == ProjectileID.HallowJoustingLance || TexType == ProjectileID.ShadowJoustingLance)
            {
                dist = length / 3f;
            }
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - unit * (length / 2f - dist), Projectile.Center + unit * (length / 2f + dist), 20, ref point);
        }

        public override void SafeDraw(Player owner, Texture2D texture, ref Color lightColor)
        {
            Vector2 origin = texture.Size() / 2f;
            //float ScaleY = (float)Math.Cos(Projectile.localAI[0]);
            if (TexType == ProjectileID.JoustingLance || TexType == ProjectileID.HallowJoustingLance || TexType == ProjectileID.ShadowJoustingLance)
            {
                origin = new Vector2(owner.direction < 0 ? texture.Size().X / 6f : texture.Size().X / 6f * 5f, texture.Size().Y * 5 / 6f);
            }

            SpriteEffects spriteEffects = owner.direction >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float rot = owner.direction <= 0 ? Projectile.rotation + MathHelper.Pi / 4 : Projectile.rotation + MathHelper.Pi / 4 * 3;
            if (TexType == 699)    //恐怖关刀
            {
                spriteEffects = owner.direction >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                rot = owner.direction <= 0 ? Projectile.rotation + MathHelper.Pi / 4 * 3 : Projectile.rotation + MathHelper.Pi / 4;
            }
            Main.spriteBatch.Draw(texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor * (1 - owner.immuneAlpha / 255f),
                rot,
                origin,
                owner.GetAdjustedItemScale(owner.HeldItem),
                spriteEffects,
                0);

        }

        public override bool SafeCanHit(NPC target)
        {
            if (Behavior == "LanceGuard" && Projectile.ai[1] <= 25)
            {
                return false;
            }
            return true;
        }

        public Vector2 GetTipPos()
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = DrawUtils.GetProjTexture(TexType);
            float length = tex.Size().Distance(Vector2.Zero) * owner.GetAdjustedItemScale(owner.HeldItem);
            float dist = 0;
            Vector2 unit = Projectile.rotation.ToRotationVector2();
            if (TexType == ProjectileID.JoustingLance || TexType == ProjectileID.HallowJoustingLance || TexType == ProjectileID.ShadowJoustingLance)
            {
                dist = length / 3f;
            }

            return Projectile.Center + unit * (length / 2f + dist - 5f);
        }


        public static void SummonSpear(Projectile ProjOwner, ref int SpearProj, float rot, float DamageScale = 0, int hitCooldown = 10, string Behavior = "")
        {
            if (SpearProj != -1) Main.projectile[SpearProj].Kill();

            Player owner = Main.player[ProjOwner.owner];

            int protmp = Projectile.NewProjectile(owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, 0, "WireBug"), owner.Center, Vector2.Zero, ModContent.ProjectileType<LanceWeaponProj>(), owner.GetWeaponDamage(), owner.GetWeaponKnockback(), owner.whoAmI);
            if (protmp >= 0)
            {
                Main.projectile[protmp].rotation = rot;
                Main.projectile[protmp].localNPCHitCooldown = hitCooldown;
                LanceWeaponProj modproj = Main.projectile[protmp].ModProjectile as LanceWeaponProj;
                modproj.ProjOwner = ProjOwner.whoAmI;
                modproj.TexType = owner.HeldItem.shoot;
                modproj.DamageScale = DamageScale;
                modproj.Behavior = Behavior;
                SpearProj = protmp;
            }
        }
    }
}