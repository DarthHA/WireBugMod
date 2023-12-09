using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WireBugMod.Projectiles;

namespace WireBugMod.System
{
    public class MiscEffectPlayer : ModPlayer
    {
        /// <summary>
        /// 纯视觉效果
        /// </summary>
        public bool RaiseShield = false;
        /// <summary>
        /// 激活盾反
        /// </summary>
        public bool ActivatingGP = false;
        /// <summary>
        /// 防御等级
        /// </summary>
        public int ShieldLevel = 0;

        /// <summary>
        /// 刚刚被命中
        /// </summary>
        public int JustHit = 0;


        /// <summary>
        /// 禁用近战效果
        /// </summary>
        public bool DisableMeleeEffect = false;

        public Vector2 ShieldOffset = Vector2.Zero;
        public float ShieldRotation = 0;

        #region On_Hook
        public override void Load()
        {
            On_Player.TryTogglingShield += On_Player_TryTogglingShield;
            On_PlayerDrawLayers.DrawPlayer_27_HeldItem += On_PlayerDrawLayers_DrawPlayer_27_HeldItem;
            On_PlayerDrawLayers.DrawPlayer_30_BladedGlove += On_PlayerDrawLayers_DrawPlayer_30_BladedGlove;
            On_Player.ItemCheck_EmitUseVisuals += On_Player_ItemCheck_EmitUseVisuals;
            On_Player.ItemCheck_ApplyUseStyle_Inner += On_Player_ItemCheck_ApplyUseStyle_Inner;
            On_Player.PlayerFrame += On_Player_PlayerFrame;
            On_PlayerDrawLayers.DrawPlayer_25_Shield += On_PlayerDrawLayers_DrawPlayer_25_Shield;
        }



        public override void Unload()
        {
            On_Player.TryTogglingShield -= On_Player_TryTogglingShield;
            On_PlayerDrawLayers.DrawPlayer_27_HeldItem -= On_PlayerDrawLayers_DrawPlayer_27_HeldItem;
            On_PlayerDrawLayers.DrawPlayer_30_BladedGlove -= On_PlayerDrawLayers_DrawPlayer_30_BladedGlove;
            On_Player.ItemCheck_EmitUseVisuals -= On_Player_ItemCheck_EmitUseVisuals;
            On_Player.ItemCheck_ApplyUseStyle_Inner -= On_Player_ItemCheck_ApplyUseStyle_Inner;
            On_Player.PlayerFrame -= On_Player_PlayerFrame;
            On_PlayerDrawLayers.DrawPlayer_25_Shield -= On_PlayerDrawLayers_DrawPlayer_25_Shield;
        }

        private static void On_PlayerDrawLayers_DrawPlayer_25_Shield(On_PlayerDrawLayers.orig_DrawPlayer_25_Shield orig, ref PlayerDrawSet drawinfo)
        {
            if (drawinfo.drawPlayer.TryGetModPlayer(out MiscEffectPlayer result))
            {
                drawinfo.drawPlayer.bodyRotation += result.ShieldRotation * drawinfo.drawPlayer.direction;
                drawinfo.Position += new Vector2(result.ShieldOffset.X * drawinfo.drawPlayer.direction, result.ShieldOffset.Y);
                orig.Invoke(ref drawinfo);
                drawinfo.drawPlayer.bodyRotation -= result.ShieldRotation * drawinfo.drawPlayer.direction;
                drawinfo.Position -= new Vector2(result.ShieldOffset.X * drawinfo.drawPlayer.direction, result.ShieldOffset.Y);
                return;
            }
            orig.Invoke(ref drawinfo);
        }

        private static void On_Player_TryTogglingShield(On_Player.orig_TryTogglingShield orig, Player self, bool shouldGuard)
        {
            if (self.TryGetModPlayer(out MiscEffectPlayer result))
            {
                if (result.RaiseShield)
                {
                    shouldGuard = true;
                }
            }
            orig.Invoke(self, shouldGuard);
        }

        private static void On_PlayerDrawLayers_DrawPlayer_30_BladedGlove(On_PlayerDrawLayers.orig_DrawPlayer_30_BladedGlove orig, ref PlayerDrawSet drawinfo)
        {
            if (drawinfo.drawPlayer.TryGetModPlayer(out MiscEffectPlayer result))
            {
                if (result.DisableMeleeEffect && !drawinfo.heldItem.IsAir)
                {
                    bool noUseGraphic = drawinfo.heldItem.noUseGraphic;
                    drawinfo.heldItem.noUseGraphic = true;
                    orig.Invoke(ref drawinfo);
                    drawinfo.heldItem.noUseGraphic = noUseGraphic;
                    return;
                }
            }
            orig.Invoke(ref drawinfo);
        }

        private static void On_PlayerDrawLayers_DrawPlayer_27_HeldItem(On_PlayerDrawLayers.orig_DrawPlayer_27_HeldItem orig, ref PlayerDrawSet drawinfo)
        {
            if (drawinfo.drawPlayer.TryGetModPlayer(out MiscEffectPlayer result))
            {
                if (result.DisableMeleeEffect && !drawinfo.heldItem.IsAir)
                {
                    bool noUseGraphic = drawinfo.heldItem.noUseGraphic;
                    drawinfo.heldItem.noUseGraphic = true;
                    orig.Invoke(ref drawinfo);
                    drawinfo.heldItem.noUseGraphic = noUseGraphic;
                    return;
                }
            }
            orig.Invoke(ref drawinfo);
        }
        private static void On_Player_ItemCheck_ApplyUseStyle_Inner(On_Player.orig_ItemCheck_ApplyUseStyle_Inner orig, Player self, float mountOffset, Item sItem, Rectangle heldItemFrame)
        {
            if (self.TryGetModPlayer(out MiscEffectPlayer result))
            {
                if (result.DisableMeleeEffect && !sItem.IsAir)
                {
                    int useStyle = sItem.useStyle;
                    sItem.useStyle = ItemUseStyleID.Shoot;
                    orig.Invoke(self, mountOffset, sItem, heldItemFrame);
                    sItem.useStyle = useStyle;
                    return;
                }
            }
            orig.Invoke(self, mountOffset, sItem, heldItemFrame);
        }

        private static void On_Player_PlayerFrame(On_Player.orig_PlayerFrame orig, Player self)
        {
            if (self.TryGetModPlayer(out MiscEffectPlayer result))
            {
                if (result.DisableMeleeEffect && !self.HeldItem.IsAir)
                {
                    int useStyle = self.HeldItem.useStyle;
                    self.HeldItem.useStyle = ItemUseStyleID.Shoot;
                    orig.Invoke(self);
                    self.HeldItem.useStyle = useStyle;
                    return;
                }
            }
            orig.Invoke(self);
        }
        private static Rectangle On_Player_ItemCheck_EmitUseVisuals(On_Player.orig_ItemCheck_EmitUseVisuals orig, Player self, Item sItem, Rectangle itemRectangle)
        {
            if (self.TryGetModPlayer(out MiscEffectPlayer result))
            {
                if (result.DisableMeleeEffect)
                {
                    return itemRectangle;
                }
            }
            return orig.Invoke(self, sItem, itemRectangle);
        }
        #endregion


        public override void PostUpdateMiscEffects()
        {
            CheckRaiseShield();
            CheckDisableMeleeEffect();
            if (ActivatingGP)
            {
                Player.onHitDodge = false;
                Player.shadowDodge = false;
                Player.blackBelt = false;
                Player.brainOfConfusionItem = null;
            }
            if (RaiseShield)
            {
                Player.shieldParryTimeLeft = 0;//取消原版格挡
                Player.statDefense -= 20;
            }
            if (JustHit > 0) JustHit--;
        }

        public override void ResetEffects()
        {
            ShieldRotation = 0;
            ShieldOffset = Vector2.Zero;
        }
        public override void UpdateDead()
        {
            RaiseShield = false;
            ActivatingGP = false;
            ShieldRotation = 0;
            ShieldOffset = Vector2.Zero;
            JustHit = 0;
        }


        public override void OnHurt(Player.HurtInfo info)
        {
            JustHit = 2;
        }
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (ActivatingGP)
            {
                modifiers.ModifyHurtInfo += (ref Player.HurtInfo info) => { info.SoundDisabled = true; info.DustDisabled = true; info.Dodgeable = false; };
                if (ShieldLevel == 2)
                {
                    modifiers.FinalDamage *= 0.025f;
                }
            }

        }


        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (ShieldLevel == 3) return true;
            return false;
        }

        public override bool? CanHitNPCWithItem(Item item, NPC target)
        {
            if (DisableMeleeEffect) return false;
            return null;
        }


        private void CheckDisableMeleeEffect()
        {
            DisableMeleeEffect = false;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.ModProjectile != null && proj.ModProjectile is BaseSkillProj)
                {
                    BaseSkillProj modproj = proj.ModProjectile as BaseSkillProj;
                    if (modproj.DisableMeleeEffect)
                    {
                        DisableMeleeEffect = true;
                        return;
                    }
                }
            }
        }

        private void CheckRaiseShield()
        {
            RaiseShield = false;
            ActivatingGP = false;
            ShieldLevel = 0;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.ModProjectile != null && proj.ModProjectile is BaseSkillProj)
                {
                    BaseSkillProj modproj = proj.ModProjectile as BaseSkillProj;
                    if (modproj.ShieldRaise)
                    {
                        RaiseShield = true;
                    }
                    if (modproj.ActivatingGP)
                    {
                        ActivatingGP = true;
                    }
                    if (modproj.ShieldLevel > ShieldLevel) ShieldLevel = modproj.ShieldLevel;
                }
            }

        }



    }

}
