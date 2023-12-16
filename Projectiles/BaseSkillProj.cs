using Terraria;
using Terraria.ModLoader;
using WireBugMod.System;
using WireBugMod.UI;
using WireBugMod.Utils;

namespace WireBugMod.Projectiles
{
    public abstract class BaseSkillProj : ModProjectile
    {
        /// <summary>
        /// 使用的第一个翔虫
        /// </summary>
        public int UsedBugID1 = -1;
        /// <summary>
        /// 使用的第二个翔虫
        /// </summary>
        public int UsedBugID2 = -1;
        /// <summary>
        /// 锁定玩家操作
        /// </summary>
        public bool LockInput = true;

        /// <summary>
        /// 锁定拥有的翔虫
        /// </summary>
        public bool LockBug = true;

        /// <summary>
        /// 暂时无法恢复翔虫
        /// </summary>
        public bool LockAllBug = false;

        /// <summary>
        /// 玩家是否举盾
        /// </summary>
        public bool ShieldRaise = false;

        /// <summary>
        /// 是否激活GP
        /// </summary>
        public bool ActivatingGP = false;

        /// <summary>
        /// 盾的强度,0为无盾，3为无敌盾，仅在激活GP时生效
        /// </summary>
        public int ShieldLevel = 0;

        /// <summary>
        /// 禁用近战判定和效果
        /// </summary>
        public bool DisableMeleeEffect = false;

    }
}