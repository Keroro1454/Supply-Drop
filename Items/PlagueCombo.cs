using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

using SupplyDrop.Utils;
using static SupplyDrop.Utils.ItemHelpers;
using static SupplyDrop.Utils.MathHelpers;
using static K1454.SupplyDrop.SupplyDropPlugin;

using BepInEx.Configuration;

using System.Collections.Generic;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;

namespace SupplyDrop.Items
{
    public class PlagueCombo : ItemBase<PlagueCombo>
    {

        //Config Stuff

        public static ConfigOption<float> baseStackHealPercent;
        public static ConfigOption<float> addStackHealPercent;
        public static ConfigOption<float> baseStackHPPercent;
        public static ConfigOption<float> addStackHPPercent;
        public static ConfigOption<float> baseStackBlightPercent;
        public static ConfigOption<float> addStackBlightPercent;

        //Item Data

        public override string ItemName => "The Pestilence";

        public override string ItemLangTokenName => "PLAGUE_COMBO";

        public override string ItemPickupDesc => "The effects of the Plague Doctor...plus something more.";

        public override string ItemFullDescription => $"All <style=cIsHealing>healing</style> is increased by " + $"<style=cIsHealing>{FloatToPercentageString(baseStackHealPercent)}</style> <style=cStack>(+{FloatToPercentageString(addStackHealPercent)} per stack)</style> " + 
            $"for every <style=cIsDamage>damage item</style> you possess. " + 
            $"Increase your <style=cIsHealing>health permanently</style> by <style=cIsHealing>{FloatToPercentageString(baseStackHPPercent)}</style> " + 
            $"<style=cStack>(+{FloatToPercentageString(addStackHPPercent)} per stack)</style> for every <style=cIsUtility>utility item</style> you possess. " + 
            $"On hit, <style=cIsDamage>{FloatToPercentageString(baseStackBlightPercent)}</style> <style=cStack>(+{FloatToPercentageString(addStackBlightPercent)} per stack)</style> " +
            $"to apply a stack of Blight for every <style=cIsHealing>healing item</style> you possess.";

        public override string ItemLore => "Ignorance has spread throughout the flock.\n\n" +
            "<i>Yes</i>\n\n" +
            "It consumes humanity like the foul parasite it is, hollowing out its hosts of their blessing.\n\n" +
            "<i>Yes</i>\n\n" +
            "STEALING OUR BIRTHRIGHTS!\n\n" +
            "<i>Yes!</i>\n\n" +
            "We see it for what it is. We perceive its true, disgusting form. And it is scared.\n\n" +
            "<i>YES!</i>\n\n" +
            "Yet even as We fight to save the flock, Ignorance feasts.\n\n" +
            "<i>Yes</i>\n\n" +
            "And, most pathetically of all, the flock is not entirely innocent in this vile ritual. The infected are not unwilling hosts.\n\n" +
            "<i>...Yes?</i>\n\n" +
            "The infected are not unwilling hosts.\n\n" +
            "<i>Yes</i>\n\n" +
            "The TRUTH is that the flock is giving itself over to Ignorance! Offering up their birthrights willingly!\n\n" +
            "<i>Yes!</i>\n\n" +
            "They sabotage Our efforts! Destroy Our works! They smile and submit with glee as Ignorance consumes them, and they work to spread Its filth!\n\n" +
            "<i><b>Yes!!</i></b>\n\n" +
            "NO LONGER! WE SHALL ROOT IGNORANCE OUT OF THE FLOCK! AND THE TRAITORS TO HUMANITY ALONG WITH IT!\n\n" +
            "<i><b>Yes!!!</i></b>\n\n" +
            "THE CANCER WILL BE CULLED!\n\n" +
            "<i><b>YES!!!</i></b>\n\n" +
            "THROUGH OUR MERCY THE FLOCK SHALL BE CULLED AND CLEANED!\n\n" +
            "YES!!!\n\n";

        public override ItemTier Tier => ItemTier.FoodTier;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.IgnoreForDropList};

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("PlagueMask.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("PlagueMaskIcon");
        public static GameObject ItemBodyModelPrefab;

        private static List<CharacterBody> Playername = new List<CharacterBody>();

        public static Dictionary<NetworkInstanceId, int> DamageItemCounts { get; private set; } = new Dictionary<NetworkInstanceId, int>();
        public static Dictionary<NetworkInstanceId, int> UtilityItemCounts { get; private set; } = new Dictionary<NetworkInstanceId, int>();


        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }
        protected override void CreateCraftableDef()
        {
            //Plague Hat + Plague Mask = Pestilence
            var plagueCombo = ScriptableObject.CreateInstance<CraftableDef>();
            (plagueCombo as ScriptableObject).name = "cdPlagueCombo";
            plagueCombo.pickup = ItemDef;
            plagueCombo.recipes = new Recipe[]
            {
                new Recipe()
                {
                    ingredients = new RecipeIngredient[]
                    {
                        new RecipeIngredient()
                        {
                            pickup = PlagueHat.instance.ItemDef,
                            type = IngredientTypeIndex.AssetReference
                        },
                        new RecipeIngredient()
                        {
                            pickup = PlagueMask.instance.ItemDef,
                            type = IngredientTypeIndex.AssetReference
                        }
                    }
                }
            };
        }
        private void CreateConfig(ConfigFile config)
        {
            baseStackHealPercent = config.ActiveBind<float>("Item: " + ItemName, "Base Bonus Healing Gained for Each Damage Item With 1 Pestilence", .04f, "How much bonus healing per Damage item should you gain with a single Pestilence? (.05 = 4%)");
            addStackHealPercent = config.ActiveBind<float>("Item: " + ItemName, "Additional Bonus Healing Gained for Each Damage Item Per Pestilence", .02f, "How much additional bonus healing per Damage item should each Pestilence after the first give?");

            baseStackHPPercent = config.ActiveBind<float>("Item: " + ItemName, "Base HP Gained for Each Utility Item With 1 Pestilence", .01f, "How much HP, as a % of max HP, per Utility item should you gain with a single Pestilence? (.01 = 1%)");
            addStackHPPercent = config.ActiveBind<float>("Item: " + ItemName, "Additional HP Gained for Each Utility Item Per Pestilence", .01f, "How much additional HP, as a % of max HP, per Utility item should each Pestilence after the first give?");

            baseStackBlightPercent = config.ActiveBind<float>("Item: " + ItemName, "Base Chance For Blight On-Hit Per Healing Item With 1 Pestilence", .01f, "How much chance to apply Blight on hit per Healing item should you gain with a single Pestilence? (.01 = 1%)");
            addStackBlightPercent = config.ActiveBind<float>("Item: " + ItemName, "Additional Chance For Blight On-Hit Per Healing Item Per Pestilence", .01f, "How much additional chance to apply Blight on hit per Healing item should each Pestilence after the first give? (.01 = 1%)");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {

            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(1f, 1f, 1f);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Chest",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0, 0, 0)
                }
            });
            rules.Add("mdlCommandoDualies", new ItemDisplayRule[]
                        {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.01306F, 0.28604F, 0.28859F),
                    localAngles = new Vector3(22.38841F, 177.5128F, 357.2195F),
                    localScale = new Vector3(0.24452F, 0.24452F, 0.24452F)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(0.02223F, 0.19926F, 0.18849F),
                    localAngles = new Vector3(4.57668F, 186.1113F, 4.3393F),
                    localScale = new Vector3(0.2343F, 0.2343F, 0.2343F)
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(0.00594F, -0.01029F, 0.23759F),
                    localAngles = new Vector3(344.0777F, 182.9241F, 351.1419F),
                    localScale = new Vector3(0.15818F, 0.15818F, 0.15818F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "HeadCenter",
                    localPos = new Vector3(2.03425F, 1.3625F, -1.41168F),
                    localAngles = new Vector3(357.9371F, 319.3255F, 321.0483F),
                    localScale = new Vector3(1.63629F, 1.63629F, 1.63629F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "HeadCenter",
                    localPos = new Vector3(0.00209F, -0.00341F, 0.30679F),
                    localAngles = new Vector3(356.4465F, 180.2676F, 1.7038F),
                    localScale = new Vector3(0.25637F, 0.25637F, 0.25637F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "HeadCenter",
                    localPos = new Vector3(-0.01919F, -0.03624F, 0.17724F),
                    localAngles = new Vector3(342.567F, 176.9706F, 2.87537F),
                    localScale = new Vector3(0.20949F, 0.20949F, 0.20949F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.00322F, 0.13827F, 0.26245F),
                    localAngles = new Vector3(8.77602F, 179.5124F, 359.7854F),
                    localScale = new Vector3(0.19539F, 0.19539F, 0.19539F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Eye",
                    localPos = new Vector3(-0.00502F, 1.1248F, -0.0259F),
                    localAngles = new Vector3(78.27003F, 359.6313F, 181.9635F),
                    localScale = new Vector3(0.3581F, 0.3581F, 0.3581F)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.00906F, 0.11803F, 0.2835F),
                    localAngles = new Vector3(5.42284F, 178.7943F, 358.8734F),
                    localScale = new Vector3(0.21719F, 0.21719F, 0.21719F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.01772F, 4.86077F, 2.57096F),
                    localAngles = new Vector3(16.90637F, 178.4799F, 180.8923F),
                    localScale = new Vector3(1.69669F, 1.69669F, 1.69669F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.01697F, 0.00157F, 0.22409F),
                    localAngles = new Vector3(344.4851F, 174.7115F, 0.58271F),
                    localScale = new Vector3(0.20969F, 0.20969F, 0.20969F)
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.00441F, 0.04618F, 0.18083F),
                    localAngles = new Vector3(1.85043F, 178.0576F, 0.72698F),
                    localScale = new Vector3(0.1313F, 0.1313F, 0.1313F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.0111F, 0.19833F, 0.30672F),
                    localAngles = new Vector3(39.26635F, 177.2378F, 2.83626F),
                    localScale = new Vector3(0.23369F, 0.23369F, 0.23369F)
                }
            });
            rules.Add("mdlSeeker", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.00892F, 0.10328F, 0.22248F),
                    localAngles = new Vector3(8.21204F, 177.0956F, 359.6561F),
                    localScale = new Vector3(0.19182F, 0.19182F, 0.19182F)
                }
            });
            rules.Add("mdlFalseSon", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.03652F, 0.24593F, 0.38088F),
                    localAngles = new Vector3(7.91319F, 175.4739F, 1.73596F),
                    localScale = new Vector3(0.30766F, 0.30766F, 0.30766F)
                }
            });
            rules.Add("mdlChef", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.19985F, 0.37988F, -0.03342F),
                    localAngles = new Vector3(74.00185F, 283.6679F, 15.17659F),
                    localScale = new Vector3(0.33891F, 0.33891F, 0.33891F)
                }
            });
            rules.Add("mdlDroneTech", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.15319F, -0.3127F, -0.01606F),
                    localAngles = new Vector3(275.913F, 51.28439F, 37.01788F),
                    localScale = new Vector3(0.17419F, 0.17419F, 0.17419F)
                }
            });
            rules.Add("mdlDrifter", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.03981F, 0.32216F, -0.01027F),
                    localAngles = new Vector3(78.58268F, 288.3839F, 20.00283F),
                    localScale = new Vector3(0.25595F, 0.25595F, 0.25595F)
                }
            });
            //MODDED CHARACTER IDRs START HERE

            rules.Add("mdlNemCommando", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(0.12014F, 0.42764F, 1.14433F),
                    localAngles = new Vector3(358.8447F, 186.2512F, 3.73659F),
                    localScale = new Vector3(0.85208F, 0.85208F, 0.85208F)
                }
            });
            rules.Add("mdlHANDOverclocked", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-1.7068F, 0.06969F, 0.9616F),
                    localAngles = new Vector3(322.2795F, 110.3694F, 349.6482F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.34598F, 0.01239F, -0.021F),
                    localAngles = new Vector3(348.8326F, 85.61517F, 2.77577F),
                    localScale = new Vector3(0.26003F, 0.26003F, 0.26003F)
                }
            });
            //            rules.Add("mdlNemforcer(Clone)", new RoR2.ItemDisplayRule[]
            //            {
            //                new RoR2.ItemDisplayRule
            //                {
            //                    ruleType = ItemDisplayRuleType.ParentedPrefab,
            //                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
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
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.0036F, 0.09954F, 0.38303F),
                    localAngles = new Vector3(345.7723F, 178.9577F, 358.8304F),
                    localScale = new Vector3(0.30983F, 0.30983F, 0.30983F)
                }
            });
            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(0.00001F, 0.0012F, -0.00274F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.00208F, 0.00158F, 0.00208F)
                }
            });
            rules.Add("mdlPathfinder", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "HeadBone",
                    localPos = new Vector3(-0.00479F, 0.10457F, 0.27908F),
                    localAngles = new Vector3(5.02868F, 177.7624F, 1.93477F),
                    localScale = new Vector3(0.20332F, 0.20332F, 0.20332F)
                }
            });
            rules.Add("mdlExecutioner2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(0.018F, 0.11955F, 0.23105F),
                    localAngles = new Vector3(0.32762F, 184.1391F, 0.63831F),
                    localScale = new Vector3(0.21677F, 0.21677F, 0.21677F)
                }
            });
            rules.Add("mdlHouse(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(0.01272F, 0.03106F, 0.19385F),
                    localAngles = new Vector3(351.4142F, 182.2931F, 359.4361F),
                    localScale = new Vector3(0.15892F, 0.15892F, 0.15892F)
                }
            });
            rules.Add("mdlTeslaTrooper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.01529F, 0.09458F, 0.25002F),
                    localAngles = new Vector3(5.16575F, 172.9076F, 1.32432F),
                    localScale = new Vector3(0.12107F, 0.12107F, 0.12107F)
                }
            });
            rules.Add("mdlDesolator", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.00596F, 0.22527F, 0.29083F),
                    localAngles = new Vector3(14.13129F, 179.0553F, 358.2143F),
                    localScale = new Vector3(0.19017F, 0.19017F, 0.19017F)
                }
            });
            //            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            //            {
            //                new RoR2.ItemDisplayRule
            //                {
            //                    ruleType = ItemDisplayRuleType.ParentedPrefab,
            //                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
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
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Head",
                    localPos = new Vector3(-0.00532F, 0.06512F, 0.25115F),
                    localAngles = new Vector3(352.9211F, 178.085F, 2.8308F),
                    localScale = new Vector3(0.18454F, 0.18454F, 0.18454F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "head",
                    localPos = new Vector3(0.22466F, 0.01385F, 0.00549F),
                    localAngles = new Vector3(341.8051F, 266.3583F, 0.00615F),
                    localScale = new Vector3(0.20955F, 0.20955F, 0.20955F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += GetTotalDamageItems;
            On.RoR2.CharacterBody.OnInventoryChanged += GetTotalUtilityItems;
            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
            GetStatCoefficients += GainBonusHP;
            On.RoR2.GlobalEventManager.OnHitEnemy += BlightOnHit;
        }

        private void GetTotalDamageItems(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        //This compares your inventory to the damage item list each time your inventory changes, and generates the appropriate value for damageItemCount
        {
            orig(self);
            var inventoryCount = GetCount(self);
            if (inventoryCount > 0)
            {
                var damageItemCount = 0;
                foreach (ItemIndex x in indiciiToCheckDamageSD)
                {
                    damageItemCount += self.inventory.GetItemCount(x);
                }
                DamageItemCounts[self.netId] = damageItemCount;
            }
        }
        private void GetTotalUtilityItems(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        //This compares your inventory to the utility item list each time your inventory changes, and generates the appropriate value for utilityItemCount
        {
            orig(self);
            var inventoryCount = GetCount(self);
            if (inventoryCount > 0)
            {
                var utilityItemCount = 0;
                foreach (ItemIndex x in indiciiToCheckUtilitySD)
                {
                    utilityItemCount += self.inventory.GetItemCount(x);
                }
                UtilityItemCounts[self.netId] = utilityItemCount;
            }

        }
        private float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        //This should handle the healing shenanigans. Thank God I don't need to use IL anymore
        {
            if (self && self.body && self.body.inventory && GetCount(self.body) > 0)
            {
                int maskCount = GetCount(self.body);
                amount = amount + (amount * baseStackHealPercent * DamageItemCounts[self.netId]) + (amount * addStackHealPercent * DamageItemCounts[self.netId] * (maskCount - 1));
            }
            return orig(self, amount, procChainMask, nonRegen);
        }
        private void GainBonusHP(CharacterBody sender, StatHookEventArgs args)
        {
            var inventoryCount = GetCount(sender);
            if (GetCount(sender) > 0 && UtilityItemCounts.ContainsKey(sender.netId))
            {
                args.healthMultAdd += (UtilityItemCounts[sender.netId] * (baseStackHPPercent + ((inventoryCount - 1) * addStackHPPercent)));
            }
        }

        private void BlightOnHit(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (damageInfo.dotIndex != DotController.DotIndex.None) return;
            if (damageInfo.procCoefficient <= 0.0f) return;

            var masterAttacker = damageInfo.attacker.GetComponent<CharacterMaster>();
            if (masterAttacker)
            {
                var cbAttacker = damageInfo.attacker.GetComponent<CharacterBody>();
                if (cbAttacker)
                {
                    var victimBody = victim.GetComponent<CharacterBody>();
                    if (victimBody)
                    {
                        var inventoryCount = GetCount(cbAttacker);
                        if (inventoryCount > 0)
                        {
                            if (Util.CheckRoll((baseStackBlightPercent + (addStackBlightPercent * (inventoryCount - 1)) *damageInfo.procCoefficient), masterAttacker))
                            {
                                DotController.InflictDot(victim, cbAttacker.gameObject, victimBody.mainHurtBox, DotController.DotIndex.Blight, 5f);
                            }
                        }
                    }
                }
            }

        }
    }
}
