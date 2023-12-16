using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles.Weapons
{
    public abstract class BaseWeaponProj : ModProjectile
    {
        /// <summary>
        /// True为弹幕贴图，False为物品贴图
        /// </summary>
        public virtual bool IsProjTexture => false;

        /// <summary>
        /// 贴图种类
        /// </summary>
        public int TexType = -1;
            
        /// <summary>
        /// 弹幕归属
        /// </summary>
        public int ProjOwner = -1;

        /// <summary>
        /// 行为
        /// </summary>
        public string Behavior = "";

        /// <summary>
        /// 命中数
        /// </summary>
        public int HitCount = 0;

        /// <summary>
        /// 动作值
        /// </summary>
        public float DamageScale = 1;

        /// <summary>
        /// 必暴
        /// </summary>
        public bool MustCrit = false;
        public override string Texture => "WireBugMod/Images/PlaceHolder";
        public override void SetStaticDefaults()
        {

        }
        public sealed override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.timeLeft = 99999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 9999;
            Projectile.ownerHitCheck = true;

            SafeSetDefaults();
        }

        public virtual void SafeSetDefaults()
        {

        }

        public sealed override void AI()
        {
            if (TexType == -1)
            {
                Projectile.Kill();
                return;
            }

            if (ProjOwner == -1 || !Main.projectile[ProjOwner].active)
            {
                Projectile.Kill();
                return;
            }

            Player owner = Main.player[Projectile.owner];
            if (owner.IsDead())
            {
                Projectile.Kill();
                return;
            }

            SafeAI(owner);
        }


        public virtual void SafeAI(Player owner)
        {

        }


        public sealed override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            HitCount++;
            SafeOnHit(target, hit, damageDone);
        }

        public virtual void SafeOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }

        public sealed override bool? CanHitNPC(NPC target)
        {
            if (DamageScale == 0) return false;
            return SafeCanHit(target) ? null : false;
        }

        public virtual bool SafeCanHit(NPC target)
        {
            return true;
        }

        public sealed override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= DamageScale;
            SafeModifyHit(target, ref modifiers);
        }

        public virtual void SafeModifyHit(NPC target, ref NPC.HitModifiers modifiers)
        {

        }

        public sealed override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = IsProjTexture ? DrawUtils.GetProjTexture(TexType) : DrawUtils.GetItemTexture(TexType);
            return SafeColliding(owner, tex.Size(), targetHitbox);
        }

        public virtual bool? SafeColliding(Player owner,Vector2 TexSize, Rectangle targetHitbox)
        {
            return null;
        }
        public sealed override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = IsProjTexture ? DrawUtils.GetProjTexture(TexType) : DrawUtils.GetItemTexture(TexType);
            SafeDraw(owner,tex,ref lightColor);
            return false;
        }

        public virtual void SafeDraw(Player owner, Texture2D texture, ref Color lightColor)
        {

        }

        
    }
}