using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;

namespace WireBugMod.System.SpecialEffects
{
    public abstract class BaseSE
    {
        /// <summary>
        /// 物品种类
        /// </summary>
        public virtual int ItemType => 1;

        /// <summary>
        /// 特效种类
        /// </summary>
        public virtual int SEType => 1;

        /// <summary>
        /// 发起的事件
        /// </summary>
        /// <param name="player"></param>
        /// <param name="Pos"></param>
        /// <param name="Rotation"></param>
        /// <param name="DamageScale"></param>
        public virtual void Execute(Player player, Vector2 Pos, float Rotation, float DamageScale)
        {

        }
    }
}
