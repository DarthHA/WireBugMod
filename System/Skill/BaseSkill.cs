using System.Collections.Generic;

namespace WireBugMod.System.Skill
{
    public abstract class BaseSkill
    {
        /// <summary>
        /// 内部识别名
        /// </summary>
        public virtual string SkillName => "Skill";

        /// <summary>
        /// 翔虫冷却
        /// </summary>
        public virtual int Cooldown => 60;

        /// <summary>
        ///是否占用两只翔虫 
        /// </summary>
        public virtual int UseBugCount => 1;

        /// <summary>
        /// 技能判定优先级
        /// </summary>
        public virtual int Priority => 1;

        /// <summary>
        /// 不是冲刺虫技
        /// </summary>
        public virtual bool NotWireDash => true;

        /// <summary>
        /// 使用的武器种类
        /// </summary>
        public virtual List<WeaponType> weaponType => new() { WeaponType.None };

        /// <summary>
        /// 技能触发条件
        /// </summary>
        /// <param name="modplayer"></param>
        /// <returns></returns>
        public virtual bool UseCondition(WireBugPlayer modplayer)
        {
            return true;
        }

        /// <summary>
        /// 技能触发动作
        /// </summary>
        /// <param name="modplayer"></param>
        /// <param name="UseBug1"></param>
        /// <param name="UseBug2"></param>
        public virtual bool OnUse(WireBugPlayer modplayer, int UseBug1, int UseBug2 = -1)
        {
            return true;
        }

    }
}
