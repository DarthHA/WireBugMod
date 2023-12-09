using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace WireBugMod
{
    public class WireBugMod : Mod
    {
        public static WireBugMod Instance;

        public static Effect NormalTrailEffect;
        public static Effect PieEffect;
        public static Effect CoilEffect;

        public static ModKeybind FlyBugKey;
        public static ModKeybind WireSkillKey1;
        public static ModKeybind WireSkillKey2;
        public static ModKeybind SwitchSkillKey;

        public WireBugMod()
        {
            Instance = this;
        }

        public override void Load()
        {
            NormalTrailEffect = ModContent.Request<Effect>("WireBugMod/Effects/NormalTrailEffect", AssetRequestMode.ImmediateLoad).Value;
            PieEffect = ModContent.Request<Effect>("WireBugMod/Effects/PieEffect", AssetRequestMode.ImmediateLoad).Value;
            CoilEffect = ModContent.Request<Effect>("WireBugMod/Effects/CoilEffect", AssetRequestMode.ImmediateLoad).Value;

            FlyBugKey = KeybindLoader.RegisterKeybind(this, "WireDash", "F");
            WireSkillKey1 = KeybindLoader.RegisterKeybind(this, "WireSkill1", "G");
            WireSkillKey2 = KeybindLoader.RegisterKeybind(this, "WireSkill2", "H");
            SwitchSkillKey = KeybindLoader.RegisterKeybind(this, "SwitchSkill", "B");
        }

        public override void Unload()
        {
            FlyBugKey = null;
            WireSkillKey1 = null;
            WireSkillKey2 = null;
            SwitchSkillKey = null;

            NormalTrailEffect = null;
            PieEffect = null;
            CoilEffect = null;

            Instance = null;
        }

    }
}