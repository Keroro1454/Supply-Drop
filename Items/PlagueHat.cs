using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;

using SupplyDrop.Utils;
using static SupplyDrop.Utils.ItemHelpers;
using static SupplyDrop.Utils.MathHelpers;
using static K1454.SupplyDrop.SupplyDropPlugin;

using BepInEx.Configuration;


namespace SupplyDrop.Items
{
    public class PlagueHat : ItemBase<PlagueHat>
    {
        //Config Stuff

        public static ConfigOption<float> baseStackHPPercent;
        public static ConfigOption<float> addStackHPPercent;

        //Item Data

        public override string ItemName => "Vintage Plague Hat";

        public override string ItemLangTokenName => "PLAGUE_HAT";

        public override string ItemPickupDesc => "Gain <style=cIsHealing>HP</style> the more <style=cIsUtility>utility items</style> you have.";

        public override string ItemFullDescription => $"Increase your <style=cIsHealing>health permanently</style> by <style=cIsHealing>{FloatToPercentageString(baseStackHPPercent)}</style> " +
            $"<style=cStack>(+{FloatToPercentageString(addStackHPPercent)} per stack)</style> for every <style=cIsUtility>utility item</style> you possess.";

        public override string ItemLore => "Order: \"NR-G Sports Soda (49)\"\nTracking Number: 49******\n" +
            "Estimated Delivery: 3/01/2056\nShipping Method: Priority\nShipping Address: P.O. Box 749, Sector A, Moon\nShipping Details:" +
            "You are cordially invited to the Order's 49th Annual Induction Gala.\n\n" +
            "Enclosed with this invitation is your unique sigil, encoded coordinates for 'Party' cipher decryption, " +
            "and your mask and headwear, selected for your pre-destined role within the Order.\n\n" +
            "Please note that attendance is mandatory. Missing the Sigil Ceremony will be noted, " +
            "and absenteeism will result in you being deemed <b>Unreasonable</b>.\n\n" +
            "Please also note the mask and hat are mandatory attire. Deviation of any kind will result in you being deemed <b>Unreasonable</b>.\n\n" +
            "Cordially,\n" +
            "The Administrator's Right Hand\n\n" +
            "<i>We seek truth and knowledge in the darkness\n" +
            "For on the day we achieve True Knowledge\n" +
            "We shall reveal our faces and rule in the light</i>";

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Healing, ItemTag.AIBlacklist };


        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("PlagueHat.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("PlagueHatIcon");


        public static GameObject ItemBodyModelPrefab;

        private ItemIndex[] indiciiToCheck;
        Dictionary<NetworkInstanceId, int> UtilityItemCounts = new Dictionary<NetworkInstanceId, int>();

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            baseStackHPPercent = config.ActiveBind<float>("Item: " + ItemName, "Base HP Gained for Each Utility Item with 1 Vintage Plague Hat", .01f, "How much HP, as a % of max HP, should you gain with a single vintage plague hat? (.01 = 1%)");
            addStackHPPercent = config.ActiveBind<float>("Item: " + ItemName, "Additional HP Gained for Each Utility Item per Vintage Plague Hat", .01f, "How much additional HP, as a % of max HP, should each vintage plague hat after the first give?");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlCommandoDualies", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.385f, 0f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(.2f, .2f, .2f)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.31f, -0.05f),
                    localAngles = new Vector3(12f, 180f, 0f),
                    localScale = new Vector3(0.175f, 0.175f, 0.175f)
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00841F, 0.20958F, -0.0078F),
                    localAngles = new Vector3(358.3102F, 185.285F, 4.45487F),
                    localScale = new Vector3(0.17303F, 0.17303F, 0.17303F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.5f, 2.45f, 1.7f),
                    localAngles = new Vector3(70f, 0f, 20f),
                    localScale = new Vector3(1.25f, 1.25f, 1.25f)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0f, 0.167f, 0f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(.2f, .2f, .2f)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.15f, -0.1f),
                    localAngles = new Vector3(30f, 180f, 0f),
                    localScale = new Vector3(0.14f, 0.14f, 0.14f)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.25f, 0f),
                    localAngles = new Vector3(15f, 180f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.17f)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.6f, 0.05f, 0.2f),
                    localAngles = new Vector3(0f, 180f, -40f),
                    localScale = new Vector3(0.25f, 0.25f, 0.25f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.23f, 0f),
                    localAngles = new Vector3(15f, 180f, 0f),
                    localScale = new Vector3(.18f, .18f, .18f)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 1f, 1.6f),
                    localAngles = new Vector3(90f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.275f, 0f),
                    localAngles = new Vector3(25f, 180f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00752F, 0.19722F, -0.00225F),
                    localAngles = new Vector3(357.2906F, 357.6486F, 350.4565F),
                    localScale = new Vector3(0.14173F, 0.14173F, 0.1406F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.03852F, 0.15149F, -0.07248F),
                    localAngles = new Vector3(328.2166F, 343.6819F, 1.48388F),
                    localScale = new Vector3(0.18024F, 0.18024F, 0.18024F)
                }
            });

            //MODDED CHARACTER IDRs START HERE

            rules.Add("mdlNemCommando", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.02589F, 1.09349F, -0.07805F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.71156F, 0.71156F, 0.71156F)
                }
            });
            rules.Add("mdlHANDOverclocked", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.80177F, 1.19489F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.5558F, 0.5558F, 0.5558F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.01307F, 0.23432F, 0.00049F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.22167F, 0.22167F, 0.22167F)
                }
            });
            //            rules.Add("mdlNemforcer(Clone)", new RoR2.ItemDisplayRule[]
            //            {
            //                new RoR2.ItemDisplayRule
            //                {
            //                    ruleType = ItemDisplayRuleType.ParentedPrefab,
            //                    followerPrefab = ItemBodyModelPrefab,
            //                    childName = "Chest",
            //                    localPos = new Vector3(-0.25983F, 0.30917F, -0.02484F),
            //                    localAngles = new Vector3(343.1456F, 273.5997F, 0.5956F),
            //                    localScale = new Vector3(0.20149F, 0.20149F, 0.20149F)
            //                }
            //            });
            rules.Add("mdlPaladin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00152F, 0.42701F, 0.05823F),
                    localAngles = new Vector3(15.85449F, 359.8131F, 359.6208F),
                    localScale = new Vector3(0.26527F, 0.26527F, 0.26527F)
                }
            });
            //            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            //            {
            //                new RoR2.ItemDisplayRule
            //                {
            //                    ruleType = ItemDisplayRuleType.ParentedPrefab,
            //                    followerPrefab = ItemBodyModelPrefab,
            //                    childName = "Chest",
            //                    localPos = new Vector3(-0.04f, 0.26f, 0.22f),
            //                    localAngles = new Vector3(0f, 0f, 0f),
            //                    localScale = generalScale * 0.9f
            //                }
            //            });
            rules.Add("mdlPathfinder", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadBone",
                    localPos = new Vector3(-0.00025F, 0.22567F, -0.01275F),
                    localAngles = new Vector3(344.0845F, 0.07477F, 0.63681F),
                    localScale = new Vector3(0.15002F, 0.15002F, 0.15002F)
                }
            });
            rules.Add("mdlExecutioner2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.0004F, 0.21983F, -0.07135F),
                    localAngles = new Vector3(335.8835F, 0.03992F, 0.7408F),
                    localScale = new Vector3(0.15094F, 0.15094F, 0.15094F)
                }
            });
            rules.Add("mdlHouse(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.012F, 0.22362F, -0.00639F),
                    localAngles = new Vector3(7.489F, 359.3903F, 10.57586F),
                    localScale = new Vector3(0.14004F, 0.14004F, 0.14004F)
                }
            });
            rules.Add("mdlTeslaTrooper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.0071F, 0.18979F, 0.01551F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.17661F, 0.17661F, 0.17661F)
                }
            });
            rules.Add("mdlDesolator", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00099F, 0.3003F, -0.00207F),
                    localAngles = new Vector3(345.0984F, 0.02197F, 0.22089F),
                    localScale = new Vector3(0.1655F, 0.1655F, 0.1655F)
                }
            });
            //            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            //            {
            //                new RoR2.ItemDisplayRule
            //                {
            //                    ruleType = ItemDisplayRuleType.ParentedPrefab,
            //                    followerPrefab = ItemBodyModelPrefab,
            //                    childName = "Head",
            //                    localPos = new Vector3(0F, 0.01245F, -0.00126F),
            //                    localAngles = new Vector3(0F, 0F, 0F),
            //                    localScale = new Vector3(0.00339F, 0.00339F, 0.00339F)
            //                }
            //            });
            rules.Add("mdlArsonist", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.0061F, 0.19101F, -0.00515F),
                    localAngles = new Vector3(348.9476F, 359.3085F, 0.50094F),
                    localScale = new Vector3(0.12607F, 0.12607F, 0.12607F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "head",
                    localPos = new Vector3(-0.01383F, 0.20557F, -0.00007F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.15018F, 0.15018F, 0.15018F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            On.RoR2.Run.Start += UtilityItemListCreator;
            On.RoR2.CharacterBody.OnInventoryChanged += GetTotalUtilityItems;
            GetStatCoefficients += GainBonusHP;
        }

        private void UtilityItemListCreator(On.RoR2.Run.orig_Start orig, Run self)
        //May need to be moved to a separate class if multiple items need to access this list
        {
            orig(self);
            indiciiToCheck = ItemCatalog.allItems.Where(x => ItemCatalog.GetItemDef(x).ContainsTag(ItemTag.Utility)).ToArray();
            UtilityItemCounts = new Dictionary<NetworkInstanceId, int>();
            Debug.Log("Item List Method has been run and a Utility Item List has been created");
            Debug.Log(indiciiToCheck.Length);
        }
        private void GetTotalUtilityItems(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        //This compares your inventory to the utility item list each time your inventory changes, and generates the appropriate value for damageItemCount
        {
            orig(self);
            var utilityItemCount = 0;
            foreach (ItemIndex x in indiciiToCheck)
            {
                utilityItemCount += self.inventory.GetItemCount(x);
            }
            UtilityItemCounts[self.netId] = utilityItemCount;
        }
        private void GainBonusHP(CharacterBody sender, StatHookEventArgs args)
        {
            var inventoryCount = GetCount(sender);
            if (GetCount(sender) > 0 && UtilityItemCounts.ContainsKey(sender.netId))
            {
                args.healthMultAdd += (UtilityItemCounts[sender.netId] * (baseStackHPPercent + ((inventoryCount - 1) * addStackHPPercent)));
            }
        }
    }
}