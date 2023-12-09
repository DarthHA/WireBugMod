using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace WireBugMod.Utils
{
    public static class PlayerUtils
    {
        public static bool IsDead(this Player player)
        {
            return !player.active || player.dead || player.ghost;
        }


        public static bool PressLeftInGame(this Player player)
        {
            if (Main.mouseLeft && !player.mouseInterface && !Main.blockMouse)
            {
                return true;
            }
            return false;
        }

        public static void SetIFrame(this Player owner, int time, bool blink = false)
        {
            bool success = false;
            for (int i = 0; i < owner.hurtCooldowns.Length; i++)
            {
                if (owner.hurtCooldowns[i] < time)
                {
                    success = true;
                    owner.hurtCooldowns[i] = time;
                }
            }
            if (success)
            {
                owner.immune = true;
                if (owner.immuneTime < time) owner.immuneTime = time;
                if (!blink)
                {
                    owner.immuneNoBlink = true;
                }
            }
        }

        public static void ClearIFrame(this Player owner)
        {
            for (int i = 0; i < owner.hurtCooldowns.Length; i++)
            {
                owner.hurtCooldowns[i] = 0;

            }
            owner.immune = false;
            owner.immuneTime = 0;
        }


        public static Vector2 SearchForNotBlockedPos(Vector2 Center, Vector2 End, float step = 16)
        {
            Vector2 result = Center + new Vector2(0, 1);
            Vector2 unit = Vector2.Normalize(End - Center);
            if (unit == Vector2.Zero) return result;
            float dist = End.Distance(Center);

            for (float i = 0; i < dist; i += step)
            {
                if (Collision.SolidCollision(result + unit * step, 1, 1) || !ValidPos(result + unit * step))
                {
                    return result;
                }
                result += unit * step;
            }
            return End;
        }

        private static bool ValidPos(Vector2 Pos)
        {
            int X = (int)(Pos.X / 16f);
            int Y = (int)(Pos.Y / 16f);
            return X > 0 && X < Main.maxTilesX && Y > 0 && Y < Main.maxTilesY;
        }
        public static float PointMulti(Vector2 vec1, Vector2 vec2)
        {
            return vec1.X * vec2.X + vec1.Y * vec2.Y;
        }

        public static int GetWeaponDamage(this Player player)
        {
            return player.GetWeaponDamage(player.HeldItem);
        }

        public static float GetWeaponKnockback(this Player player)
        {
            return player.GetWeaponKnockback(player.HeldItem);
        }

        public static void ChangeItemRotation(this Player player, float rotation, bool BodyRotate = true)
        {
            Vector2 vecRot = rotation.ToRotationVector2();
            player.itemRotation = (float)Math.Atan2(vecRot.Y * player.direction, vecRot.X * player.direction) + (BodyRotate ? player.fullRotation : 0);
        }

        public static void SetPlayerFallStart(this Player player, Vector2 Pos)
        {
            player.fallStart = (int)(Pos.Y / 16f);
            player.fallStart2 = (int)(Pos.Y / 16f);
        }
        public static float GetRotationByDirection(float rotation, int direction)
        {
            if (direction >= 0)
            {
                return rotation;
            }
            else
            {
                Vector2 temp = rotation.ToRotationVector2();
                temp.X = -temp.X;
                rotation = temp.ToRotation();
                return rotation;
            }
        }

        public static bool HasTwoWeapons(this Player player, int type)
        {
            int Count = 0;
            for (int i = 0; i < 10; i++)
            {
                if (!player.inventory[i].IsAir && player.inventory[i].type == type)
                {
                    Count++;
                }
            }
            return Count >= 2;
        }

    }
}