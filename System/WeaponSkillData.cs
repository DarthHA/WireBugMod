using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using WireBugMod.System.Skill;

namespace WireBugMod.System
{
    public enum WeaponType
    {
        ShortBlade,
        GreatSword,
        Lance,
        Axe,
        Hammer,
        LightBowgun,
        HeavyBowgun,
        Bow,
        None
    }
    public class WeaponSkillData : ModSystem
    {
        public static Dictionary<int, WeaponType> WeaponDictionary = new();

        public static Dictionary<WeaponType, List<string>> SkillDictionary = new();

        public static List<string> SkillList = new();

        public override void PostSetupContent()
        {
            /*
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                Item itemtmp = new();
                itemtmp.SetDefaults(i);
                if (itemtmp.IsAir || itemtmp.damage == 0 || itemtmp.ammo > 0) continue;
                if (itemtmp.ModItem != null) break;

                if (itemtmp.DamageType == DamageClass.Ranged)
                {
                    if (itemtmp.useAmmo == AmmoID.Arrow)
                    {
                        WeaponDictionary.Add(i, WeaponType.Bow);
                    }
                    else if (itemtmp.useAmmo == AmmoID.Rocket)
                    {
                        WeaponDictionary.Add(i, WeaponType.HeavyBowgun);
                    }
                    else if (itemtmp.useAmmo == AmmoID.Bullet)
                    {
                        WeaponDictionary.Add(i, WeaponType.LightBowgun);
                    }
                }
                else if (itemtmp.DamageType == DamageClass.Melee || itemtmp.DamageType == DamageClass.MeleeNoSpeed)
                {
                    if (ItemID.Sets.Spears[i] ||
                        itemtmp.type == ItemID.HallowJoustingLance || itemtmp.type == ItemID.JoustingLance ||
                        itemtmp.type == ItemID.ShadowJoustingLance)
                    {
                        WeaponDictionary.Add(i, WeaponType.Lance);
                    }
                    else if (!itemtmp.channel)
                    {
                        if (itemtmp.useStyle == ItemUseStyleID.Rapier)
                        {
                            WeaponDictionary.Add(i, WeaponType.ShortBlade);
                        }
                        else if (itemtmp.useStyle == ItemUseStyleID.Swing)
                        {
                            if (itemtmp.axe > 0)
                            {
                                WeaponDictionary.Add(i, WeaponType.Axe);
                            }
                            else if (itemtmp.hammer > 0)
                            {
                                WeaponDictionary.Add(i, WeaponType.Hammer);
                            }
                            else
                            {
                                Main.instance.LoadItem(i);
                                float width = Math.Max(TextureAssets.Item[i].Value.Width, TextureAssets.Item[i].Value.Height) * itemtmp.scale;
                                if (width <= 40)
                                {
                                    WeaponDictionary.Add(i, WeaponType.LongSword);
                                }
                                else
                                {
                                    WeaponDictionary.Add(i, WeaponType.GreatSword);
                                }
                            }
                        }
                    }
                }

                WriteJson();
            }
            */
            ReadJson();

            foreach (BaseSkill skill in SkillLoader.skills)
            {
                if (skill.NotWireDash)
                {
                    SkillList.Add(skill.SkillName);
                }
            }
        }

        public override void Unload()
        {
            SkillList.Clear();
            WeaponDictionary.Clear();
        }

        public static void WriteJson()
        {
            Dictionary<string, string> temp = new();
            foreach (int i in WeaponDictionary.Keys)
            {
                temp.Add(Lang.GetItemNameValue(i), WeaponDictionary[i].ToString());
            }
            string output = JsonConvert.SerializeObject(temp);
            File.WriteAllText(@"C:\WireBug\Weapon.json", output);
        }

        public static void ReadJson()
        {
            byte[] stext = ModContent.GetFileBytes("WireBugMod/WeaponData.json");
            string otext = Encoding.UTF8.GetString(stext);
            Dictionary<string, string> temp = JsonConvert.DeserializeObject<Dictionary<string, string>>(otext);

            foreach (string key in temp.Keys)
            {
                int finditem = -1;
                for (int i = 0; i < ItemLoader.ItemCount; i++)
                {
                    if (Lang.GetItemNameValue(i) == key) { finditem = i; break; }
                }
                if (finditem != -1)
                {
                    WeaponType wt = (WeaponType)Enum.Parse(typeof(WeaponType), temp[key]);
                    WeaponDictionary.Add(finditem, wt);
                }
            }
        }
    }

    public class Show : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            WeaponType result;
            if (WeaponSkillData.WeaponDictionary.TryGetValue(item.type, out result))
            {
                tooltips.Add(new TooltipLine(Mod, "FuckYou", result.ToString()));
            }
        }
    }
}
