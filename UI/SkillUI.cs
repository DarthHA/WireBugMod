using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using WireBugMod.System;
using WireBugMod.System.Skill;
using WireBugMod.Utils;

namespace WireBugMod.UI
{
    internal class SkillUI : UIState
    {
        public UIPanel Panel;
        public const int BackWidth = 934;
        public const int BackHeight = 500;
        public const int block = 16;


        public int Pages = 0;
        public string[] SkillNameRight = { "", "", "", "", "", "", "", "" };
        public int SelectedSkillRight = -1;

        public int SelectedSkillLeft = -1;
        public string[] SkillNameLeft = { "", "", "", "" };

        public string SkillDesc = "";
        public string SkillTitle = "";

        UIImageButton[] SkillLeft = new UIImageButton[4];
        UIImageButton[] SkillRight = new UIImageButton[8];
        UIImageButton MinusButton;
        UIImageButton PlusButton;

        public override void OnInitialize()
        {
            Panel = new UIPanel();
            Panel.Left.Set(300, 0);
            Panel.Top.Set(300, 0);
            Panel.Width.Set(BackWidth, 0);
            Panel.Height.Set(BackHeight, 0);
            Panel.PaddingLeft = Panel.PaddingRight = Panel.PaddingTop = Panel.PaddingBottom = 0;
            Panel.BackgroundColor = new Color(0, 0, 0, 0);
            Panel.BorderColor = new Color(0, 0, 0, 0);

            for (int i = 0; i < 4; i++)
            {
                SkillLeft[i] = new UIImageButton(Terraria.GameContent.TextureAssets.MagicPixel);
                SkillLeft[i].Width.Set(block * 14, 0f);
                SkillLeft[i].Height.Set(block * 3, 0f);
                SkillLeft[i].OnLeftClick += SkillLeft_OnLeftClick;
                SkillLeft[i].OnRightClick += SkillLeft_OnRightClick;
                SkillLeft[i].SetVisibility(0, 0);
            }
            SkillLeft[0].Left.Set(16f, 0f);
            SkillLeft[0].Top.Set(64f, 0f);
            SkillLeft[1].Left.Set(16f, 0f);
            SkillLeft[1].Top.Set(144f, 0f);
            SkillLeft[2].Left.Set(16f, 0f);
            SkillLeft[2].Top.Set(256f, 0f);
            SkillLeft[3].Left.Set(16f, 0f);
            SkillLeft[3].Top.Set(336f, 0f);

            for (int i = 0; i < 8; i++)
            {
                SkillRight[i] = new UIImageButton(Terraria.GameContent.TextureAssets.MagicPixel);
                SkillRight[i].Left.Set(282, 0f);
                SkillRight[i].Top.Set(32 + i * block * 3, 0f);
                SkillRight[i].Width.Set(block * 14, 0f);
                SkillRight[i].Height.Set(block * 3, 0f);
                SkillRight[i].SetVisibility(0, 0);
                SkillRight[i].OnLeftClick += SkillRight_OnLeftClick;
                SkillRight[i].OnRightClick += SkillRight_OnRightClick;
                SkillRight[i].OnScrollWheel += SkillRight_OnScrollWheel;
            }

            MinusButton = new UIImageButton(Terraria.GameContent.TextureAssets.MagicPixel);
            MinusButton.Left.Set(314, 0f);
            MinusButton.Top.Set(432, 0f);
            MinusButton.Width.Set(4 * block, 0f);
            MinusButton.Height.Set(2 * block, 0f);
            MinusButton.SetVisibility(0, 0);
            MinusButton.OnLeftClick += MinusButton_OnLeftClick;

            PlusButton = new UIImageButton(Terraria.GameContent.TextureAssets.MagicPixel);
            PlusButton.Left.Set(426, 0f);
            PlusButton.Top.Set(432, 0f);
            PlusButton.Width.Set(4 * block, 0f);
            PlusButton.Height.Set(2 * block, 0f);
            PlusButton.SetVisibility(0, 0);
            PlusButton.OnLeftClick += PlusButton_OnLeftClick;


            Append(Panel);
            for (int i = 0; i < 4; i++)
            {
                Panel.Append(SkillLeft[i]);
            }
            for (int i = 0; i < 8; i++)
            {
                Panel.Append(SkillRight[i]);
            }
            Panel.Append(MinusButton);
            Panel.Append(PlusButton);

        }

        private void SkillRight_OnScrollWheel(UIScrollWheelEvent evt, UIElement listeningElement)
        {
            if (PlayerInput.ScrollWheelDeltaForUI < 0)
            {
                PlusButton_OnLeftClick(evt, listeningElement);
            }
            else if (PlayerInput.ScrollWheelDeltaForUI > 0)
            {
                MinusButton_OnLeftClick(evt, listeningElement);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.gameMenu || Main.LocalPlayer.IsDead() || !UIManager.Visible)
                return;
            base.Draw(spriteBatch);
        }

        // Here we draw our UI
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            Rectangle body = Panel.GetInnerDimensions().ToRectangle();
            Texture2D UIBodyTex = ModContent.Request<Texture2D>("WireBugMod/UI/UISheet").Value;
            Texture2D MinusButtonTex = ModContent.Request<Texture2D>("WireBugMod/UI/UIButtonMinus").Value;
            Texture2D PlusButtonTex = ModContent.Request<Texture2D>("WireBugMod/UI/UIButtonPlus").Value;
            Texture2D SkillSlotTex = ModContent.Request<Texture2D>("WireBugMod/UI/UISkillSlot").Value;
            Texture2D SkillSlotSTex = ModContent.Request<Texture2D>("WireBugMod/UI/UISkillSlot_S").Value;
            Texture2D SkillETex = ModContent.Request<Texture2D>("WireBugMod/UI/UIE").Value;
            //Texture2D BlueBook = ModContent.Request<Texture2D>("WireBugMod/Images/SwitchSkill1").Value;
            //Texture2D RedBook = ModContent.Request<Texture2D>("WireBugMod/Images/SwitchSkill2").Value;
            Texture2D RedBookSlot = ModContent.Request<Texture2D>("WireBugMod/UI/UISwitchRed").Value;
            Texture2D RedBookSlotS = ModContent.Request<Texture2D>("WireBugMod/UI/UISwitchRed_S").Value;
            Texture2D BlueBookSlot = ModContent.Request<Texture2D>("WireBugMod/UI/UISwitchBlue").Value;
            Texture2D BlueBookSlotS = ModContent.Request<Texture2D>("WireBugMod/UI/UISwitchBlue_S").Value;

            //绘制主界面
            spriteBatch.Draw(UIBodyTex, body, Color.White);       //主菜单

            //绘制替换技显示
            for (int i = 0; i < 4; i++)
            {
                Rectangle hitbox1 = SkillLeft[i].GetInnerDimensions().ToRectangle();
                bool selected = SelectedSkillLeft == i;
                if (selected)
                {
                    spriteBatch.Draw(i < 2 ? RedBookSlotS : BlueBookSlotS, hitbox1, Color.White);
                }
                else
                {
                    spriteBatch.Draw(i < 2 ? RedBookSlot : BlueBookSlot, hitbox1, Color.White);
                }
                Terraria.Utils.DrawBorderString(spriteBatch, SkillNameLeft[i], hitbox1.TopLeft() + hitbox1.Size() / 2f + new Vector2(0, 5), selected ? Color.White : Color.DarkGray, 1, 0.5f, 0.5f);
                Terraria.Utils.DrawBorderString(spriteBatch, i % 2 == 0 ? "1" : "2", hitbox1.TopLeft() + new Vector2(36, 36), selected ? Color.White : Color.DarkGray, 1, 0.5f, 0.5f);
            }

            //绘制选择显示
            for (int i = 0; i < 8; i++)
            {
                Rectangle hitbox1 = SkillRight[i].GetInnerDimensions().ToRectangle();
                bool selected = SelectedSkillRight == i;
                if (selected)
                {
                    spriteBatch.Draw(SkillSlotSTex, hitbox1, Color.White);
                }
                else
                {
                    spriteBatch.Draw(SkillSlotTex, hitbox1, Color.White);
                }
                Terraria.Utils.DrawBorderString(spriteBatch, SkillNameRight[i], hitbox1.TopLeft() + hitbox1.Size() / 2f + new Vector2(0, 5), Color.White, 1, 0.5f, 0.5f);
                if (SkillNameRight[i] != "")
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (SkillNameLeft[j] == SkillNameRight[i])
                        {
                            spriteBatch.Draw(SkillETex, hitbox1, selected ? Color.White : Color.DarkGray);
                            break;
                        }
                    }
                }

            }

            //绘制翻页按钮
            spriteBatch.Draw(MinusButtonTex, MinusButton.GetInnerDimensions().ToRectangle(), Color.White);
            spriteBatch.Draw(PlusButtonTex, PlusButton.GetInnerDimensions().ToRectangle(), Color.White);

            //绘制描述
            Terraria.Utils.DrawBorderString(spriteBatch, SkillTitle, body.TopLeft() + new Vector2(726, 47), Color.Yellow, 1, 0.5f, 0.5f);
            Terraria.Utils.DrawBorderString(spriteBatch, SkillDesc, body.TopLeft() + new Vector2(554, 76), Color.White, 1);

            //绘制标题
            Terraria.Utils.DrawBorderString(spriteBatch, Language.GetTextValue("Mods.WireBugMod.SkillInfos.ChangeSB"), body.TopLeft() + new Vector2(130, 16), Color.White, 0.75f, 0.5f, 0.5f);
            Terraria.Utils.DrawBorderString(spriteBatch, Language.GetTextValue("Mods.WireBugMod.SkillInfos.SelectSB"), body.TopLeft() + new Vector2(400, 16), Color.White, 0.75f, 0.5f, 0.5f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Main.LocalPlayer.TryGetModPlayer(out WireBugPlayer result))
            {
                if (!result.HasWireBug)
                {
                    UIManager.ShouldUpdate = true;
                    UIManager.Visible = false;
                    return;
                }
            }

            if (UIManager.ShouldUpdate)
            {
                UIManager.ShouldUpdate = false;
                SelectedSkillLeft = -1;
                SelectedSkillRight = -1;
                RefreshInfo();
            }
            Rectangle hitbox = Panel.GetInnerDimensions().ToRectangle();
            if (ContainsMouse(hitbox))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            for (int i = 0; i < 4; i++)
            {
                if (SkillLeft[i].IsMouseHovering)
                {
                    SelectedSkillLeft = i;
                    UpdateDescOnly();
                    break;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                if (SkillRight[i].IsMouseHovering && !SkillRight[i].IgnoresMouseInteraction)
                {
                    SelectedSkillRight = i;
                    UpdateDescOnly();
                    break;
                }
            }

        }

        private void SkillLeft_OnRightClick(UIMouseEvent evt, UIElement listeningElement)
        {
            for (int i = 0; i < 4; i++)
            {
                if (SkillLeft[i].IsMouseHovering)
                {
                    SelectedSkillLeft = -1;
                    SelectedSkillRight = -1;
                    break;
                }
            }
        }

        private void SkillRight_OnRightClick(UIMouseEvent evt, UIElement listeningElement)
        {
            for (int i = 0; i < 8; i++)
            {
                if (SkillRight[i].IsMouseHovering)
                {
                    SelectedSkillRight = -1;
                    break;
                }
            }
        }

        private void MinusButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement)
        {
            if (Pages > 0)
            {
                Pages--;
                SelectedSkillRight = -1;
                RefreshInfo();
            }

        }

        private void PlusButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement)
        {
            int start = (Pages + 1) * 8;
            if (WeaponSkillData.SkillList.Count > start)
            {
                Pages++;
                SelectedSkillRight = -1;
                RefreshInfo();
            }

        }

        private void SkillRight_OnLeftClick(UIMouseEvent evt, UIElement listeningElement)
        {
            for (int i = 0; i < 8; i++)
            {
                if (SkillRight[i].IsMouseHovering)
                {
                    SelectedSkillRight = i;
                    if (SelectedSkillLeft != -1)
                    {
                        int index = Pages * 8 + SelectedSkillRight;
                        Main.LocalPlayer.GetModPlayer<UIPlayer>().SetSkillName(WeaponSkillData.SkillList[index], SelectedSkillLeft);
                    }
                }
            }
            RefreshInfo();
        }

        private void SkillLeft_OnLeftClick(UIMouseEvent evt, UIElement listeningElement)
        {
            for (int i = 0; i < 4; i++)
            {
                if (SkillLeft[i].IsMouseHovering)
                {
                    SelectedSkillLeft = i;
                }
            }
            SelectedSkillRight = -1;
            RefreshInfo();
        }

        private void RefreshInfo()
        {
            int start = Pages * 8;
            for (int i = 0; i < 8; i++)
            {
                int index = i + start;
                if (WeaponSkillData.SkillList.Count > index)
                {
                    SkillNameRight[i] = Language.GetTextValue("Mods.WireBugMod.Skills." + WeaponSkillData.SkillList[index]);
                    SkillRight[i].IgnoresMouseInteraction = false;
                }
                else
                {
                    SkillNameRight[i] = "";
                    SkillRight[i].IgnoresMouseInteraction = true;
                }
            }
            UIPlayer modplayer = Main.LocalPlayer.GetModPlayer<UIPlayer>();

            SkillNameLeft[0] = Language.GetTextValue("Mods.WireBugMod.Skills." + modplayer.SkillName1);
            SkillNameLeft[1] = Language.GetTextValue("Mods.WireBugMod.Skills." + modplayer.SkillName2);
            SkillNameLeft[2] = Language.GetTextValue("Mods.WireBugMod.Skills." + modplayer.SwitchSkillName1);
            SkillNameLeft[3] = Language.GetTextValue("Mods.WireBugMod.Skills." + modplayer.SwitchSkillName2);

            SkillTitle = "";
            SkillDesc = "";
            if (SelectedSkillRight != -1)
            {
                int index = Pages * 8 + SelectedSkillRight;
                SkillTitle = WeaponSkillData.SkillList[index];
            }
            else if (SelectedSkillLeft != -1)
            {
                SkillTitle = Main.LocalPlayer.GetModPlayer<UIPlayer>().GetSkillName(SelectedSkillLeft);
            }
            if (SkillTitle != "")
            {
                int Cooldown = 60;
                int UseBugCount = 1;
                foreach (BaseSkill skill in SkillLoader.skills)
                {
                    if (skill.SkillName == SkillTitle)
                    {
                        Cooldown = skill.Cooldown;
                        UseBugCount = skill.UseBugCount;
                        break;
                    }
                }
                SkillDesc = Language.GetTextValue("Mods.WireBugMod.SkillInfos." + SkillTitle) + "\n";
                SkillDesc = Language.ActiveCulture.LegacyId == (int)GameCulture.CultureName.Chinese ? BreakLongStringForCN(SkillDesc, 35) : BreakLongString(SkillDesc, 35);
                SkillDesc += string.Format(Language.GetTextValue("Mods.WireBugMod.SkillInfos.BugRecoverySpeed"), (Cooldown / 60f).ToString()) + "\n";
                SkillDesc += string.Format(Language.GetTextValue("Mods.WireBugMod.SkillInfos.BugCost"), UseBugCount.ToString());
                SkillTitle = Language.GetTextValue("Mods.WireBugMod.Skills." + SkillTitle);
            }
            else
            {
                SkillDesc = "";
            }
        }

        private void UpdateDescOnly()
        {
            SkillTitle = "";
            SkillDesc = "";
            if (SelectedSkillRight != -1)
            {
                int index = Pages * 8 + SelectedSkillRight;
                SkillTitle = WeaponSkillData.SkillList[index];
            }
            else if (SelectedSkillLeft != -1)
            {
                SkillTitle = Main.LocalPlayer.GetModPlayer<UIPlayer>().GetSkillName(SelectedSkillLeft);
            }
            if (SkillTitle != "")
            {
                int Cooldown = 60;
                int UseBugCount = 1;
                foreach (BaseSkill skill in SkillLoader.skills)
                {
                    if (skill.SkillName == SkillTitle)
                    {
                        Cooldown = skill.Cooldown;
                        UseBugCount = skill.UseBugCount;
                        break;
                    }
                }
                SkillDesc = Language.GetTextValue("Mods.WireBugMod.SkillInfos." + SkillTitle) + "\n";
                SkillDesc = Language.ActiveCulture.LegacyId == (int)GameCulture.CultureName.Chinese ? BreakLongStringForCN(SkillDesc, 35) : BreakLongString(SkillDesc, 35);
                SkillDesc += string.Format(Language.GetTextValue("Mods.WireBugMod.SkillInfos.BugRecoverySpeed"), (Cooldown / 60f).ToString()) + "\n";
                SkillDesc += string.Format(Language.GetTextValue("Mods.WireBugMod.SkillInfos.BugCost"), UseBugCount.ToString());
                SkillTitle = Language.GetTextValue("Mods.WireBugMod.Skills." + SkillTitle);
            }
            else
            {
                SkillDesc = "";
            }
        }


        private bool ContainsMouse(Rectangle hitbox)
        {
            Vector2 point = Main.MouseWorld - Main.screenPosition;
            if (point.X > hitbox.X && point.Y > hitbox.Y && point.X < hitbox.X + hitbox.Width)
            {
                return point.Y < hitbox.Y + hitbox.Height;
            }

            return false;
        }


        private string BreakLongString(string inputStr, int textWidth)
        {
            List<string> tempList = new List<string>();//临时存放拼接字符串的列表
            List<string> lastList = new List<string>();//最终的数据
            int strLength = inputStr.Length;//获取要换行字符串的长度
            if (strLength > textWidth)
            {
                string[] listArray = inputStr.Split(' ');//先把字符串分割成一个个单词，后面再重新连接
                string joinStr = "";
                string theDeleteStr = "";//用来存放因为增加了它才超过固定长度的那个单词。
                for (int j = 0; j < listArray.Length; j++)
                {
                    tempList.Add(listArray[j]);//把分割好的单词 一个个的往list里面添加
                    joinStr = String.Join(" ", tempList.ToArray());//然后转化成字符串
                                                                   //每添加一个都跟固定长度比较一下，当小的时候，继续添加；如果大于的时候进入判断
                    if (joinStr.Length > textWidth)
                    {
                        //因为大于了固定长度，所以把最后一个单词删掉，删掉后的字符串为一条完整的记录，
                        int lastSpaceIndex = joinStr.LastIndexOf(" ");
                        lastList.Add((theDeleteStr + " " + joinStr.Substring(0, lastSpaceIndex)).Trim());
                        theDeleteStr = listArray[j];
                        //刚好是最后一个的时候
                        if (j == listArray.Length - 1)
                            lastList.Add(theDeleteStr);

                        tempList.Clear();//清空临时list
                    }
                    else if (j == listArray.Length - 1)//当遍历到结尾，剩下的当做最后一行
                    {
                        lastList.Add((theDeleteStr + " " + joinStr).Trim());
                        tempList.Clear();
                    }
                }
            }

            string result = "";
            foreach (string str in lastList)
            {
                result += str + "\n";
            }
            return result;



        }


        private string BreakLongStringForCN(string inputStr, int textWidth)
        {
            return Terraria.GameContent.FontAssets.MouseText.Value.CreateWrappedText(inputStr, textWidth * 10);
        }
    }
}
