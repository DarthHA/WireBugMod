using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WireBugMod.Projectiles.LSword
{
    public class SakuraBombProj : ModProjectile
    {
        public override string Texture => "WireBugMod/Images/PlaceHolder";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.timeLeft = 9999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;
        }
        public override void AI()
        {
            NPC target = Main.npc[(int)Projectile.ai[0]];
            if (!target.active)
            {
                Projectile.Kill();
                return;
            }
            Projectile.ai[1]++;
            Projectile.Center = target.Center;
            if (Projectile.ai[1] > 60)
            {
                if (Projectile.ai[1] % 7 == 1)
                {
                    Vector2 SpawnPos = target.position + new Vector2(Main.rand.Next(target.width), Main.rand.Next(target.height));
                    SlashProj.Summon(Main.player[Projectile.owner], SpawnPos, Projectile.damage, Projectile.knockBack);
                }
            }
            if (Projectile.ai[1] >= 120)
            {
                Projectile.Kill();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
