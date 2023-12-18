using Terraria.ModLoader;
using WireBugMod.Items;

namespace WireBugMod.System
{
    public class GetWireBug : GlobalNPC
    {
        public override void SetupTravelShop(int[] shop, ref int nextSlot)
        {
            shop[nextSlot] = ModContent.ItemType<WireBugItem>();
            nextSlot++;
        }
    }
}
