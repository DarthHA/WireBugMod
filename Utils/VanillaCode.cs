using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.GameContent;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace WireBugMod.Utils
{
    public static class VanillaCode
    {
        public static void DrawFlail(int vanillaProjType, Vector2 SourcePos, Vector2 TargetPos)
        {
            if (vanillaProjType == ProjectileID.SolarWhipSword)
            {
                DrawSolarEruption(SourcePos, TargetPos);
                return;
            }
            Rectangle? sourceRectangle = null;
            float num = 0f;
            Asset<Texture2D> asset;
            switch (vanillaProjType)
            {
                default:     //通常是蓝月
                    asset = TextureAssets.Chain3;
                    break;
                case 25:          //腐化链球
                    asset = TextureAssets.Chain2;
                    break;
                case 35:         //阳炎之怒
                    asset = TextureAssets.Chain6;
                    break;
                case 63:          //太极连枷
                    asset = TextureAssets.Chain7;
                    break;
                case 154:       //血肉之球
                    asset = TextureAssets.Chain13;
                    break;
                case 247:     //花花链球
                    asset = TextureAssets.Chain19;
                    break;
                case 273:     //链刃
                    asset = TextureAssets.Chain23;
                    break;
                case 383:     //锚
                    asset = TextureAssets.Chain34;
                    break;
                case 404:       //朱砂链球
                    asset = TextureAssets.Chain37;
                    break;
                case 481:       //铁链血滴子
                    asset = TextureAssets.Chain40;
                    break;
                case 757:       //滴滴怪链球
                    asset = TextureAssets.Extra[99];
                    sourceRectangle = asset.Frame(1, 6);
                    num = -2f;
                    break;
                case 947:        //铁链锤
                    asset = TextureAssets.Chain41;
                    break;
                case 948:      //烈焰链锤
                    asset = TextureAssets.Chain43;
                    break;
            }

            Vector2 origin = sourceRectangle.HasValue ? (sourceRectangle.Value.Size() / 2f) : (asset.Size() / 2f);
            Vector2 center = TargetPos;
            Vector2 v = SourcePos.MoveTowards(center, 4f) - center;
            Vector2 vector = v.SafeNormalize(Vector2.Zero);
            float num2 = (sourceRectangle.HasValue ? sourceRectangle.Value.Height : asset.Height()) + num;
            float rotation = vector.ToRotation() + (float)Math.PI / 2f;
            int num3 = 0;
            float num4 = v.Length() + num2 / 2f;
            int num5 = 0;
            while (num4 > 0f)
            {
                Color color = Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16f));
                switch (vanillaProjType)
                {
                    case 757:       //滴滴怪链球
                        sourceRectangle = asset.Frame(1, 6, 0, num3 % 6);
                        break;
                    case 948:      //烈焰链锤
                        if (num5 >= 6)
                        {
                            asset = TextureAssets.Chain41;
                        }
                        else if (num5 >= 4)
                        {
                            asset = TextureAssets.Chain42;
                            byte b = 140;
                            if (color.R < b)
                                color.R = b;

                            if (color.G < b)
                                color.G = b;

                            if (color.B < b)
                                color.B = b;
                        }
                        else
                        {
                            color = Color.White;
                        }
                        num5++;
                        break;
                }

                Main.spriteBatch.Draw(asset.Value, center - Main.screenPosition, sourceRectangle, color, rotation, origin, 1f, SpriteEffects.None, 0f);
                center += vector * num2;
                num3++;
                num4 -= num2;
            }

            Color color2 = Lighting.GetColor((int)TargetPos.X / 16, (int)(TargetPos.Y / 16f));
            Texture2D ballTexture = DrawUtils.GetProjTexture(vanillaProjType);
            float rot2 = (TargetPos - SourcePos).ToRotation();
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (vanillaProjType == 273 || vanillaProjType == 481 || vanillaProjType==383) rot2 += MathHelper.Pi / 2f;
            if (vanillaProjType == 404)
            {
                rot2 += MathHelper.Pi / 2f;
                if ((TargetPos - SourcePos).X < 0)
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;
                }
            }
            Main.spriteBatch.Draw(ballTexture, TargetPos - Main.screenPosition, null, color2, rot2, ballTexture.Size() / 2f, 1f, spriteEffects, 0f);
        }


        internal static void DrawSolarEruption(Vector2 SourcePos, Vector2 TargetPos, float scale = 1)
        {
            Texture2D value69 = DrawUtils.GetProjTexture(ProjectileID.SolarWhipSword);
            Color alpha8 = Lighting.GetColor((int)SourcePos.X / 16, (int)(SourcePos.Y / 16f));
            if (SourcePos == TargetPos)
                return;

            float rot = (TargetPos - SourcePos).ToRotation();
            float DrawLength = TargetPos.Distance(SourcePos) + 16f;
            bool flag33 = DrawLength < 100f;
            Vector2 RotVect = rot.ToRotationVector2();
            Rectangle rectangle15 = new Rectangle(0, 2, value69.Width, 40);
            float rotation26 = rot - MathHelper.Pi / 2f;
            Main.spriteBatch.Draw(value69, SourcePos.Floor() - Main.screenPosition, rectangle15, alpha8, rotation26, rectangle15.Size() / 2f - Vector2.UnitY * 4f, scale, SpriteEffects.None, 0);
            DrawLength -= 40f * scale;
            Vector2 vector62 = SourcePos.Floor();
            vector62 += RotVect * scale * 24f;
            rectangle15 = new Rectangle(0, 68, value69.Width, 18);
            if (DrawLength > 0f)
            {
                float num273 = 0f;
                while (num273 + 1f < DrawLength)
                {
                    if (DrawLength - num273 < rectangle15.Height)
                        rectangle15.Height = (int)(DrawLength - num273);

                    Main.spriteBatch.Draw(value69, vector62 - Main.screenPosition, rectangle15, alpha8, rotation26, new Vector2(rectangle15.Width / 2, 0f), scale, SpriteEffects.None, 0);
                    num273 += rectangle15.Height * scale;
                    vector62 += RotVect * rectangle15.Height * scale;
                }
            }

            Vector2 vector63 = vector62;
            vector62 = SourcePos.Floor();
            vector62 += RotVect * scale * 24f;
            rectangle15 = new Rectangle(0, 46, value69.Width, 18);
            int num274 = 18;
            if (flag33)
                num274 = 9;

            float num275 = DrawLength;
            if (DrawLength > 0f)
            {
                float num276 = 0f;
                float num277 = num275 / num274;
                num276 += num277 * 0.25f;
                vector62 += RotVect * num277 * 0.25f;
                for (int num278 = 0; num278 < num274; num278++)
                {
                    float num279 = num277;
                    if (num278 == 0)
                        num279 *= 0.75f;

                    Main.spriteBatch.Draw(value69, vector62 - Main.screenPosition, rectangle15, alpha8, rotation26, new Vector2(rectangle15.Width / 2, 0f), scale, SpriteEffects.None, 0);
                    num276 += num279;
                    vector62 += RotVect * num279;
                }
            }

            Main.spriteBatch.Draw(value69, vector63 - Main.screenPosition, new Rectangle(0, 90, value69.Width, 48), alpha8, rotation26, value69.Frame().Top(), scale, SpriteEffects.None, 0);

        }
    }
}
