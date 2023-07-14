using R2API;
using RoR2;
using UnityEngine;
using static R2API.RecalculateStatsAPI;

using SupplyDrop.Utils;
using static SupplyDrop.Utils.ItemHelpers;
using static SupplyDrop.Utils.MathHelpers;
using static K1454.SupplyDrop.SupplyDropPlugin;

using BepInEx.Configuration;

namespace SupplyDrop.Items
{
    public class SilverWatch : ItemBase<SilverWatch>
    {
        //Config Stuff

        public static ConfigOption<float> baseDurationIncrease;
        public static ConfigOption<float> addDurationIncrease;

        //Item Data

        public override string ItemName => "Silver Pocketwatch";

        public override string ItemLangTokenName => "SILVERWATCH";

        public override string ItemPickupDesc => "<style=cUtility>Buffs</style> last longer...but so do <style=cDeath>debuffs</style>";

        public override string ItemFullDescription => $"All <style=cUtility>buff</style> durations are increased by {FloatToPercentageString(baseDurationIncrease)} (+{FloatToPercentageString(addDurationIncrease)}% " +
            "per stack). All <style=cDeath>debuff</style> durations are also increased by {FloatToPercentageString(baseDurationIncrease)} (+{FloatToPercentageString(addDurationIncrease)}% " +
            "per stack).";

        public override string ItemLore => "\"Relative to true time, everything is moving at blazing fast speeds. True time is in no hurry, after all.\n\n" +
            "If you were to wind a watch in the primordial stream of time, you would create a watch that, taken anywhere, would always seem to be off. Somehow, it would always be just a bit slow.\n\n" +
            "One can only imagine what a creature acclimated to such a life would look like. I like to think something so relaxed would be quite pleasant company.\"";

        public override ItemTier Tier => ItemTier.Lunar;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("SilverWatch.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("WatchIcon");
        public static GameObject ItemBodyModelPrefab;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
           baseDurationIncrease = config.ActiveBind<float>("Item: " + ItemName, "Base Increase to Buff/Debuff Durations with 1 Silver Pocketwatch", .5f, "How much should buff/debuff durations be increased by with 1 Silver Pocketwatch? (.5 = 50%)");
           addDurationIncrease = config.ActiveBind<float>("Item: " + ItemName, "Additional Increase to Buff/Debuff Durations per Silver Pocketwatch", .25f, "How much should buff/debuff durations be increased for each additional Silver Pocketwatch? (.25 = 25%)");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(.125f, .125f, .125f);

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
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.25f, -0.36f),
                    localAngles = new Vector3(180f, 180f, 180f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, -0.025f, -0.12f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = new Vector3(.09f, .09f, .09f)
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.03487F, 0.26702F, -0.23743F),
                    localAngles = new Vector3(16.22462F, 354.7491F, 355.1796F),
                    localScale = new Vector3(0.08257F, 0.08257F, 0.08257F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.8f, -2.5f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = new Vector3(0.75f, 0.75f, 0.75f)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.15f, -0.4f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0F, 0.1F, -0.38F),
                    localAngles = new Vector3(9.86298F, 0.05314F, 357.6569F),
                    localScale = new Vector3(0.1F, 0.125F, 0.1F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.2f, -0.41f),
                    localAngles = new Vector3(80f, 180f, 0f),
                    localScale = new Vector3(0.08f, 0.08f, 0.08f)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "WeaponPlatform",
                    localPos = new Vector3(0f, -0.5f, 0.4f),
                    localAngles = new Vector3(0f, 0f, 180f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.25f, -0.37f),
                    localAngles = new Vector3(0f, 0f, 90f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 3f, 4f),
                    localAngles = new Vector3(-30f, 0f, 0f),
                    localScale = new Vector3(0.75f, 0.75f, 0.75f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.2f, -0.35f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Backpack",
                    localPos = new Vector3(0.43838F, 0.22354F, 0.00284F),
                    localAngles = new Vector3(10.28261F, 185.1785F, 91.81888F),
                    localScale = new Vector3(0.05926F, 0.05892F, 0.05926F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ShoulderL",
                    localPos = new Vector3(-0.14555F, 0.12824F, 0.149F),
                    localAngles = new Vector3(35.93249F, 73.72224F, 84.21194F),
                    localScale = new Vector3(0.03568F, 0.03669F, 0.03669F)
                }
            });

            //MODDED CHARACTER IDRs START HERE

            rules.Add("mdlNemCommando", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "JetR",
                    localPos = new Vector3(-0.02321F, 0.00866F, -0.0044F),
                    localAngles = new Vector3(61.14747F, 125.2273F, 129.7773F),
                    localScale = new Vector3(0.16949F, 0.16949F, 0.16949F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "JetL",
                    localPos = new Vector3(0.0198F, 0.00524F, 0.00559F),
                    localAngles = new Vector3(302.7839F, 59.69757F, 124.8021F),
                    localScale = new Vector3(0.16949F, 0.16949F, 0.16949F)
                }
            });
            rules.Add("mdlHANDOverclocked", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.01434F, 0.60066F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.88058F, 0.88058F, 0.88058F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.39827F, 0.31022F, -0.01165F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.09306F, 0.09306F, 0.09306F)
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
                    childName = "Sword",
                    localPos = new Vector3(0.0086F, -0.06774F, 0.01411F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.04909F, 0.04909F, 0.04909F)
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
                    childName = "SpearHitbox",
                    localPos = new Vector3(-0.00414F, -0.78807F, -0.00407F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(0.02899F, 0.02899F, 0.02899F)
                }
            });
            rules.Add("mdlExecutioner2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.02982F, -0.00594F, 0.27733F),
                    localAngles = new Vector3(76.88757F, 342.5656F, 83.25074F),
                    localScale = new Vector3(0.05059F, 0.05059F, 0.05059F)
                }
            });
            rules.Add("mdlHouse(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.2383F, -0.04141F, 0.00528F),
                    localAngles = new Vector3(0.43422F, 182.53F, 160.776F),
                    localScale = new Vector3(0.04135F, 0.04135F, 0.04135F)
                }
            });
            rules.Add("mdlTeslaTrooper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00826F, 0.44339F, -0.23484F),
                    localAngles = new Vector3(9.38397F, 359.6089F, 356.5085F),
                    localScale = new Vector3(0.06377F, 0.06377F, 0.06377F)
                }
            });
            rules.Add("mdlDesolator", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "RadCannonItems",
                    localPos = new Vector3(-0.00377F, -1.48042F, -0.00011F),
                    localAngles = new Vector3(-0.00001F, 180F, 180F),
                    localScale = new Vector3(0.0883F, 0.0883F, 0.0883F)
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
                    childName = "Chest",
                    localPos = new Vector3(-0.29114F, 0.52624F, -0.30575F),
                    localAngles = new Vector3(9.63699F, 6.94206F, 56.15643F),
                    localScale = new Vector3(0.03767F, 0.03767F, 0.03767F)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.29543F, 0.52537F, -0.31812F),
                    localAngles = new Vector3(31.4669F, 153.7513F, 52.74549F),
                    localScale = new Vector3(0.03767F, 0.03767F, 0.03767F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "thigh.L",
                    localPos = new Vector3(-0.19673F, 0.03266F, 0.04399F),
                    localAngles = new Vector3(346.6592F, 166.2192F, 187.89F),
                    localScale = new Vector3(0.04678F, 0.04678F, 0.04678F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += BuffManager;
        }
    }
}