using Steamworks;
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
        /// ʹ�õĵ�һ�����
        /// </summary>
        public int UsedBugID1 = -1;
        /// <summary>
        /// ʹ�õĵڶ������
        /// </summary>
        public int UsedBugID2 = -1;
        /// <summary>
        /// ������Ҳ���
        /// </summary>
        public bool LockInput = true;

        /// <summary>
        /// ����ӵ�е����
        /// </summary>
        public bool LockBug = true;

        /// <summary>
        /// ��ʱ�޷��ָ����
        /// </summary>
        public bool LockAllBug = false;

        /// <summary>
        /// ����Ƿ�ٶ�
        /// </summary>
        public bool ShieldRaise = false;

        /// <summary>
        /// �Ƿ񼤻�GP
        /// </summary>
        public bool ActivatingGP = false;

        /// <summary>
        /// �ܵ�ǿ��,0Ϊ�޶ܣ�3Ϊ�޵жܣ����ڼ���GPʱ��Ч
        /// </summary>
        public int ShieldLevel = 0;

        /// <summary>
        /// ���ý�ս�ж���Ч��
        /// </summary>
        public bool DisableMeleeEffect = false;

        /// <summary>
        /// ����ʱ�䣬��ʵ������
        /// </summary>
        public int SleepTimer = 0;

        public override void PostAI()
        {
            if (SleepTimer > 0) SleepTimer--;
        }

    }
}