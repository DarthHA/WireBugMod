using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.System.SpecialEffects;
using WireBugMod.Utils;

namespace WireBugMod.System.Skill
{
    public class SESystem : ModSystem
    {
        public override void Load()
        {
            SELoader.Load(Mod);
        }
        public override void Unload()
        {
            SELoader.Unload();
        }

        public static void ExecuteSpecialEffects(Player player,int SEType, Vector2 Pos, float Rotation, float DamageScale)
        {
            if (player.HeldItem.IsAir) return;
            foreach(BaseSE baseSE in SELoader.SEs)
            {
                if (baseSE.ItemType == player.HeldItem.type && baseSE.SEType == SEType)
                {
                    baseSE.Execute(player, Pos, Rotation, DamageScale);
                    break;
                }
            }
        }


    }
}
