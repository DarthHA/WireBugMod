
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using WireBugMod.Utils;

namespace WireBugMod.UI
{
    public class UIManager : ModSystem
    {
        public static bool Visible = false;
        public static bool ShouldUpdate = false;
        public static UserInterface _SkillUIInterface;
        static SkillUI _SkillUI;

        public override void Load()
        {
            _SkillUI = new SkillUI();
            _SkillUI.Activate();
            _SkillUIInterface = new UserInterface();
            _SkillUIInterface.SetState(_SkillUI);
        }

        public override void Unload()
        {
            _SkillUI?.Deactivate();
            _SkillUI = null;
            _SkillUIInterface = null;
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int DrawingUIIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (DrawingUIIndex != -1)
            {
                layers.Insert(DrawingUIIndex, new LegacyGameInterfaceLayer(
                    "WireBugMod: SkillUI",
                    delegate
                    {
                        _SkillUIInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (!Main.gameMenu && !Main.LocalPlayer.IsDead() && Visible && !Main.playerInventory)
            {
                _SkillUIInterface?.Update(gameTime);
            }
            else
            {
                ShouldUpdate = true;
                Visible = false;
            }

        }
    }

}