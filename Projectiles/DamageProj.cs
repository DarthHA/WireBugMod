using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace WireBugMod.Projectiles
{
    public class DamageProj : ModProjectile
    {
        public override string Texture => "WireBugMod/Images/PlaceHolder";
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public static void Summon(Player player, Vector2 Pos, int width, int height, int dmg, float kb)
        {
            int protmp = Projectile.NewProjectile(player.GetSource_ItemUse_WithPotentialAmmo(player.HeldItem, 0), Pos, Vector2.Zero, ModContent.ProjectileType<DamageProj>(), dmg, kb, player.whoAmI);
            if (protmp >= 0)
            {
                Main.projectile[protmp].width = width;
                Main.projectile[protmp].height = height;
                Main.projectile[protmp].Center = Pos;
            }
        }
    }
}
