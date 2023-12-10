using Terraria;

namespace WireBugMod.Utils
{
    public static class NPCUtils
    {
        public static bool IsTheSameOwner(NPC npc1, NPC npc2)
        {
            if (npc1.whoAmI == npc2.whoAmI)
            {
                return true;
            }
            if (npc1.realLife != -1 && npc2.realLife != -1)         //均为某个体节
            {
                return npc1.realLife == npc2.realLife;
            }
            else if (npc1.realLife != -1 && npc2.realLife == -1)       //1为某个体节，2为独立
            {
                return npc1.realLife == npc2.whoAmI;
            }
            else if (npc1.realLife == -1 && npc2.realLife != -1)        //2为某个体节，1为独立
            {
                return npc2.realLife == npc1.whoAmI;
            }
            return false;
        }
    }
}
