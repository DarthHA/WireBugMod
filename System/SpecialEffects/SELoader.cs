using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace WireBugMod.System.SpecialEffects
{
    public static class SELoader
    {
        public static List<BaseSE> SEs;
        public static void Load(Mod mod)
        {
            SEs = new();
            foreach (Type type in AssemblyManager.GetLoadableTypes(mod.Code))
            {
                if (type.IsSubclassOf(typeof(BaseSE)) && !type.IsAbstract && type != typeof(BaseSE))
                {
                    BaseSE instance = (BaseSE)FormatterServices.GetUninitializedObject(type);
                    SEs.Add(instance);
                }
            }
        }
        public static void Unload()
        {
            SEs.Clear();
            SEs = null;
        }

    }


}
