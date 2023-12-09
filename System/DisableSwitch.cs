using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace WireBugMod.System
{
    public class DisableSwitch : ModSystem
    {
        public override void Load()
        {
            On_Player.dropItemCheck += dropItemCheck;
            On_Player.ScrollHotbar += ScrollHotbar;
        }

        public override void Unload()
        {
            On_Player.dropItemCheck -= dropItemCheck;
            On_Player.ScrollHotbar -= ScrollHotbar;
        }

        private void ScrollHotbar(On_Player.orig_ScrollHotbar orig, Player self, int Offset)
        {
            if (self.TryGetModPlayer<WireBugPlayer>(out var player))
            {
                if (player.LockInput)
                {
                    return;
                }
            }
            orig.Invoke(self, Offset);
        }


        private void dropItemCheck(On_Player.orig_dropItemCheck orig, Player self)
        {
            if (self.TryGetModPlayer<WireBugPlayer>(out var player))
            {
                if (player.LockInput)
                {
                    PlayerInput.Triggers.Current.Hotbar1 = false;
                    PlayerInput.Triggers.Current.Hotbar2 = false;
                    PlayerInput.Triggers.Current.Hotbar3 = false;
                    PlayerInput.Triggers.Current.Hotbar4 = false;
                    PlayerInput.Triggers.Current.Hotbar5 = false;
                    PlayerInput.Triggers.Current.Hotbar6 = false;
                    PlayerInput.Triggers.Current.Hotbar7 = false;
                    PlayerInput.Triggers.Current.Hotbar8 = false;
                    PlayerInput.Triggers.Current.Hotbar9 = false;
                    PlayerInput.Triggers.Current.Hotbar10 = false;
                }
            }
            orig.Invoke(self);
        }
    }
}
