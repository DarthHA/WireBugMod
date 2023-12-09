
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace WireBugMod.Utils
{
    public static class DrawUtils
    {
        public static void DrawTrail(Texture2D tex, List<CustomVertexInfo> bars, SpriteBatch spriteBatch, Color color, BlendState blendState)
        {
            List<CustomVertexInfo> triangleList = new List<CustomVertexInfo>();
            if (bars.Count > 2)
            {
                for (int k = 0; k < bars.Count - 2; k += 2)
                {
                    triangleList.Add(bars[k]);
                    triangleList.Add(bars[k + 2]);
                    triangleList.Add(bars[k + 1]);
                    triangleList.Add(bars[k + 1]);
                    triangleList.Add(bars[k + 2]);
                    triangleList.Add(bars[k + 3]);
                }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, blendState, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
                Vector2 vector = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
                Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight) / Main.GameViewMatrix.Zoom;
                Matrix projection = Matrix.CreateOrthographicOffCenter(0f, screenSize.X, screenSize.Y, 0f, 0f, 1f);
                Vector2 screenPos = vector - screenSize / 2f;
                Matrix model = Matrix.CreateTranslation(new Vector3(-screenPos.X, -screenPos.Y, 0f));
                WireBugMod.NormalTrailEffect.Parameters["uTransform"].SetValue(model * projection);
                WireBugMod.NormalTrailEffect.Parameters["color"].SetValue(color.ToVector4());
                Main.graphics.GraphicsDevice.Textures[0] = tex;
                Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                WireBugMod.NormalTrailEffect.CurrentTechnique.Passes[0].Apply();
                Main.graphics.GraphicsDevice.DrawUserPrimitives(0, triangleList.ToArray(), 0, triangleList.Count / 3);
                Main.graphics.GraphicsDevice.RasterizerState = originalState;
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }


        public static void DrawWire(Vector2 begin, Vector2 end, float percentage, Color color, float width)
        {
            begin -= Main.screenPosition;
            end -= Main.screenPosition;
            Texture2D tex = ModContent.Request<Texture2D>("WireBugMod/Images/WireBG").Value;
            Vector2 origin = new(0, tex.Height / 2);
            Vector2 scale = new(begin.Distance(end) / tex.Width, 1);
            float rot = (end - begin).ToRotation();
            EasyDraw.AnotherDraw(SpriteSortMode.Immediate);
            WireBugMod.CoilEffect.Parameters["color"].SetValue(Color.Cyan.ToVector4());
            WireBugMod.CoilEffect.Parameters["n"].SetValue(4);
            WireBugMod.CoilEffect.Parameters["width"].SetValue(width);
            WireBugMod.CoilEffect.Parameters["k"].SetValue(percentage * 1.5f);
            WireBugMod.CoilEffect.CurrentTechnique.Passes["CoilEffect"].Apply();
            Main.spriteBatch.Draw(tex, begin, null, color, rot, origin, scale, SpriteEffects.None, 0);
            EasyDraw.AnotherDraw(SpriteSortMode.Deferred);
        }

        public static void DrawSword(Texture2D Tex, Vector2 SwingCenter, float scale, float RotationY, float rotationX = 0, float RotationZ = MathHelper.Pi / 2, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            SwingCenter += Main.screenPosition;
            //计算四点
            Vector2 Pos1 = new Vector2(0, -Tex.Height).RotatedBy(RotationY) * scale;
            Vector2 Pos2 = Vector2.Zero;
            Vector2 Pos3 = new Vector2(Tex.Width, -Tex.Height).RotatedBy(RotationY) * scale;
            Vector2 Pos4 = new Vector2(Tex.Width, 0).RotatedBy(RotationY) * scale;


            float k = (float)Math.Sin(RotationZ);
            Pos1.Y *= k;
            Pos2.Y *= k;
            Pos3.Y *= k;
            Pos4.Y *= k;
            Pos1 = Pos1.RotatedBy(rotationX);
            Pos2 = Pos2.RotatedBy(rotationX);
            Pos3 = Pos3.RotatedBy(rotationX);
            Pos4 = Pos4.RotatedBy(rotationX);

            if (spriteEffects == SpriteEffects.FlipHorizontally)
            {
                Pos1.X = -Pos1.X;
                Pos2.X = -Pos2.X;
                Pos3.X = -Pos3.X;
                Pos4.X = -Pos4.X;
            }

            List<CustomVertexInfo> vertexInfos = new()
            {
                new CustomVertexInfo(SwingCenter + Pos1, Color.White, new Vector3(0, 0f, 1)),
                new CustomVertexInfo(SwingCenter + Pos2, Color.White, new Vector3(0, 1f, 1)),
                new CustomVertexInfo(SwingCenter + Pos3, Color.White, new Vector3(1, 0f, 1)),
                new CustomVertexInfo(SwingCenter + Pos4, Color.White, new Vector3(1, 1f, 1))
            };
            DrawTrail(Tex, vertexInfos, Main.spriteBatch, Color.White, BlendState.AlphaBlend);
        }



        public static Texture2D GetItemTexture(int type)
        {
            if (TextureAssets.Item[type].State == AssetState.NotLoaded)
            {
                Main.instance.LoadItem(type);
            }
            return TextureAssets.Item[type].Value;
        }

        public static Texture2D GetProjTexture(int type)
        {
            if (TextureAssets.Projectile[type].State == AssetState.NotLoaded)
            {
                Main.instance.LoadProjectile(type);
            }
            return TextureAssets.Projectile[type].Value;
        }
    }
}
