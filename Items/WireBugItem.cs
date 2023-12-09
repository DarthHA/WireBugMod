using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Projectiles;
using WireBugMod.System;

namespace WireBugMod.Items
{
    public class WireBugItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.value = 10000;
            Item.rare = ItemRarityID.Expert;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = Item.useAnimation = 20;
            Item.shoot = ModContent.ProjectileType<ReturningBug>();
            Item.shootSpeed = 10;
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }
        public override bool CanUseItem(Player player)
        {
            WireBugPlayer modplayer = player.GetModPlayer<WireBugPlayer>();
            for (int i = 0; i < modplayer.bugs.Count; i++)
            {
                if (modplayer.bugs[i].IsReady())
                {
                    return true;
                }
            }
            return false;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int bug = -1;
            WireBugPlayer modplayer = player.GetModPlayer<WireBugPlayer>();
            for (int i = 0; i < modplayer.bugs.Count; i++)
            {
                if (modplayer.bugs[i].IsReady())
                {
                    bug = i;
                    break;
                }
            }
            if (bug != -1)
            {
                int protmp = Projectile.NewProjectile(player.GetSource_FromThis("WireBug"), position, velocity, ModContent.ProjectileType<ReturningBug>(), 0, 0, player.whoAmI);
                if (protmp >= 0)
                {
                    (Main.projectile[protmp].ModProjectile as ReturningBug).WaitTime = 60;
                    (Main.projectile[protmp].ModProjectile as ReturningBug).LockInput = false;
                    (Main.projectile[protmp].ModProjectile as ReturningBug).UsedBugID1 = bug;
                    Main.projectile[protmp].spriteDirection = Math.Sign(velocity.X + 0.01f);
                }
                modplayer.bugs[bug].SetCD(60, 1);
            }
            return false;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.DirtBlock, 10);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }

        public override void UpdateInventory(Player player)
        {
            player.GetModPlayer<WireBugPlayer>().HasWireBug = true;
        }
    }
}