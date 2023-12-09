using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using WireBugMod.System;

namespace WireBugMod.Prefixes
{
    // This class serves as an example for declaring item 'prefixes', or 'modifiers' in other words.
    public abstract class WirebugWhisperer : ModPrefix
    {
        public virtual float Power => 0.03f;


        // Change your category this way, defaults to PrefixCategory.Custom. Affects which items can get this prefix.
        public override PrefixCategory Category => PrefixCategory.Accessory;

        // See documentation for vanilla weights and more information.
        // In case of multiple prefixes with similar functions this can be used with a switch/case to provide different chances for different prefixes
        // Note: a weight of 0f might still be rolled. See CanRoll to exclude prefixes.
        // Note: if you use PrefixCategory.Custom, actually use ModItem.ChoosePrefix instead.
        public override float RollChance(Item item)
        {
            return 5f;
        }

        // Determines if it can roll at all.
        // Use this to control if a prefix can be rolled or not.
        public override bool CanRoll(Item item)
        {
            return true;
        }


        // Modify the cost of items with this modifier with this function.
        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 1f + (Power - 0.03f) * 5f;
        }

        public override void ApplyAccessoryEffects(Player player)
        {
            if (player.GetModPlayer<WireBugPlayer>().BugRecoveryStat < 0.9f)
            {
                player.GetModPlayer<WireBugPlayer>().BugRecoveryStat += Power;
            }
        }

        public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
        {
            TooltipLine result = new(Mod, "WireBugMod:WireWhisper", string.Format(Language.GetTextValue("Mods.WireBugMod.PrefixDescription"), Power * 100));

            result.OverrideColor = Color.Cyan;

            yield return result;
        }
    }

    public class WirebugWhisperer1 : WirebugWhisperer
    {
        public override float Power => 0.03f;
    }

    public class WirebugWhisperer2 : WirebugWhisperer
    {
        public override float Power => 0.06f;
    }

    public class WirebugWhisperer3 : WirebugWhisperer
    {
        public override float Power => 0.09f;
    }
}
