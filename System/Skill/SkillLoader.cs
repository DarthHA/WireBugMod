using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace WireBugMod.System.Skill
{
    public static class SkillLoader
    {
        public static List<BaseSkill> skills;
        public static void Load(Mod mod)
        {
            skills = new();
            foreach (Type type in AssemblyManager.GetLoadableTypes(mod.Code))
            {
                if (type.IsSubclassOf(typeof(BaseSkill)) && !type.IsAbstract && type != typeof(BaseSkill))
                {
                    BaseSkill instance = (BaseSkill)FormatterServices.GetUninitializedObject(type);
                    skills.Add(instance);
                }
            }
            skills.Sort((a, b) => { return a.Priority.CompareTo(b.Priority); });
        }
        public static void Unload()
        {
            skills.Clear();
            skills = null;
        }

    }


}
