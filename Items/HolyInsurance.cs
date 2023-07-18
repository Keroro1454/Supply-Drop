using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;

using SupplyDrop.Utils;
using static SupplyDrop.Utils.ItemHelpers;
using static SupplyDrop.Utils.MathHelpers;
using static K1454.SupplyDrop.SupplyDropPlugin;

using BepInEx.Configuration;
using UnityEngine.UI;
using System.Linq;

namespace SupplyDrop.Items
{
    public class HolyInsurance : ItemBase<HolyInsurance>
    {
        //Config Stuff

        public static ConfigOption<float> baseGoldDrain;
        public static ConfigOption<float> addGoldDrain;
        public static ConfigOption<uint> baseGoldToCoverage;
        public static ConfigOption<float> addGoldToCoverage;
        public static ConfigOption<float> costTierMultiplier;

        //Item Data
        public override string ItemName => "Afterlife Insurance";

        public override string ItemLangTokenName => "HOLY_INSURANCE";

        public override string ItemPickupDesc => "Some of the <style=cShrine>money</style> you earn is invested into <style=cIsUtility>divine insurance</style> (Coverage may vary).";

        public override string ItemFullDescription => $"Gain {FloatToPercentageString(baseGoldDrain)} <style=cStack>(+{FloatToPercentageString(addGoldDrain)} per stack)</style> less <style=cShrine>money</style> from killing monsters. " +
            $"{FloatToPercentageString(baseGoldToCoverage)} <style=cStack>(+{FloatToPercentageString(addGoldToCoverage)} per stack)</style> of money lost is <style=cIsUtility>invested into upgrading your insurance</style> to cover more threats, " +
            $"up to 10 times. <style=cDeath>Upon dying</style> to an source you are <style=cIsUtility>insured</style> for, you will be revived, " +
            $"and your <style=cIsUtility>insurance</style> level will be reset to zero.";

        public override string ItemLore => "Congratulations on becoming the newest member of the <style=cIsUtility>EternalLife<sup>TM</sup></style> family! Our motto at <style=cIsUtility>EternalLife<sup>TM</sup></style> is simple--provide the very best in insurance solutions, " +
            "so you don't have to worry about what life (or death) throws your way!" +

            "\n\nYou have opted into our<style=cIsUtility> Dynamic Afterlife Policy Plan<sup> TM</sup>!</style> " +
            "This policy is unique, as you pay into it over time, and as you pay in, your coverage expands! " +
            "Our angelic actuaries have calculated the following as appropriate coverage tiers for you, " +
            "based on your career choice as <style=cMono>[REDACTED]</style>:" +

            "\n\nT1: <style=cHumanObjective> Small Pests and Vermin:</style> For creepy crawlies, rodents, and other small vermin" +
            "\nT2: <style=cHumanObjective> Improperly-Placed Debris:</style> For danger due to intentional or unintentional <style:cSub>(Read: Natural Causes)</style> action" +
            "\nT3: <style=cHumanObjective>Large Pests and Vermin:</style> For infestations that require trained professional removal" +
            "\nT4: <style=cHumanObjective>Hazardous Exposure (Minor):</style> For spills of unusual, toxic, and acidic materials" +
            "\nT5: <style=cHumanObjective>Catastrophic Equipment Failure:</style> For malfunctioning heavy machinery or technologies" +
            "\nT6: <style=cHumanObjective>Hazardous Exposure (Major):</style> For direct exposure to 'corrupting' materials or conditions" +
            "\nT7: <style=cHumanObjective>Supernatural Forces:</style> For those concerned by the possibility of otherworldly entities" +
            "\nT8: <style=cHumanObjective>Interstellar Incidents:</style> For all damages sustained off-planet" +
            "\nT9: <style=cHumanObjective>Extreme and/or Unmitigated Disaster:</style> For those who engage in activities with high probability of causing exceedingly large damages" +
            "\nT10: <style=cHumanObjective>Acts of God(s) :</style> For those that have attracted the ire of the divine.We at<style=cIsUtility> EternalLife<sup>TM</sup></style> do not recommend engaging in activities that could necessitate this coverage" +

            "\n\n<size=40%> Note that any causes of death not listed can not, and will not, be covered with your current plan. " +
            "Subscription to plan cannot retroactively alleviate any deaths you may suffer from (previously or currently). "+
            "As a subscriber, you agree to the term that EternalLife<sup> TM</sup> is entitled to your eternal soul in the event of failure to pay for a needed coverage. Some additional terms and conditions may apply. " +
            "For questions and concerns, please visit your local place of worship, and direct all prayers to<style=cMono>[REDACTED]</style>, or visit us in-person by <style= cDeath > obliterating yourself</style>." +
            "\n\n Thank you for choosing<style=cIsUtility> EternalLife<sup>TM</sup></style>! Have a blessed life (or several)!";

        public override ItemTier Tier => ItemTier.Lunar;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("TestModel.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("TestIcon");

        public static GameObject ItemBodyModelPrefab;
        public static GameObject ItemFollowerPrefab;

        private static List<CharacterBody> Playername = new List<CharacterBody>();

        public static BuffDef T1Coverage { get; private set; }
        public static BuffDef T2Coverage { get; private set; }
        public static BuffDef T3Coverage { get; private set; }
        public static BuffDef T4Coverage { get; private set; }
        public static BuffDef T5Coverage { get; private set; }
        public static BuffDef T6Coverage { get; private set; }
        public static BuffDef T7Coverage { get; private set; }
        public static BuffDef T8Coverage { get; private set; }
        public static BuffDef T9Coverage { get; private set; }
        public static BuffDef T10Coverage { get; private set; }

        public static GameObject InsuranceBar;
        public static GameObject InsuranceBarImage;
        public static GameObject InsuranceBarOutline;

        public static Range[] ranges;

        public Dictionary<string, Range> InsuranceDictionary = new Dictionary<string, Range>();
        //Navigation customNav = new Navigation();

        public override void Init(ConfigFile config)
        {
                CreateConfig(config);
                CreateLang();
                CreateItem();
                SetupAttributes();
                Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            baseGoldDrain = config.ActiveBind<float>("Item: " + ItemName, "Base Percentage of Gold Taxed for Insurance with 1 Afterlife Insurance", .25f, "How much of a percentage of gold earned should be taxed with a single Afterlife Insurance? (.25 = 25%)");
            addGoldDrain = config.ActiveBind<float>("Item: " + ItemName, "Additional Percentage of Gold Taxed for Insurance per Afterlife Insurance", .10f, "How much additional percentage of gold earned should be taxed for each Afterlife Insurance after the first?");
            baseGoldToCoverage = config.ActiveBind<uint>("Item: " + ItemName, "Base Conversion Ratio of Gold Taken to Gold Stored with 1 Afterlife Insurance", 1, "How much gold is stored for each gold taxed with a single Afterlife Insurance? (1 = 100%)");
            addGoldToCoverage = config.ActiveBind<float>("Item: " + ItemName, "Additional Conversion Ratio of Gold Taken to Gold Stored per Afterlife Insurance", .25f, "How much additional gold is stored for each gold taxed should each Afterlife Insurance after the first give?");
            costTierMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Multiplier Applied to All Insurance Tier Costs", 1f, "Apply a multiplier to all insurance tier costs to make them more or less expensive. (1 = 1x)");
        }
        private void CreateBuff()
        {
            T1Coverage = ScriptableObject.CreateInstance<BuffDef>();
            T1Coverage.name = "SupplyDrop Afterlife Insurance T1";
            T1Coverage.canStack = false;
            T1Coverage.isDebuff = false;
            T1Coverage.iconSprite = MainAssets.LoadAsset<Sprite>("HolyInsuranceT1Icon");

            ContentAddition.AddBuffDef(T1Coverage);

            T2Coverage = ScriptableObject.CreateInstance<BuffDef>();
            T2Coverage.name = "SupplyDrop Afterlife Insurance T2";
            T2Coverage.canStack = false;
            T2Coverage.isDebuff = false;
            T2Coverage.iconSprite = MainAssets.LoadAsset<Sprite>("HolyInsuranceT2Icon");

            ContentAddition.AddBuffDef(T2Coverage);

            T3Coverage = ScriptableObject.CreateInstance<BuffDef>();
            T3Coverage.name = "SupplyDrop Afterlife Insurance T3";
            T3Coverage.canStack = false;
            T3Coverage.isDebuff = false;
            T3Coverage.iconSprite = MainAssets.LoadAsset<Sprite>("HolyInsuranceT3Icon");

            ContentAddition.AddBuffDef(T3Coverage);

            T4Coverage = ScriptableObject.CreateInstance<BuffDef>();
            T4Coverage.name = "SupplyDrop Afterlife Insurance T4";
            T4Coverage.canStack = false;
            T4Coverage.isDebuff = false;
            T4Coverage.iconSprite = MainAssets.LoadAsset<Sprite>("HolyInsuranceT4Icon");

            ContentAddition.AddBuffDef(T4Coverage);

            T5Coverage = ScriptableObject.CreateInstance<BuffDef>();
            T5Coverage.name = "SupplyDrop Afterlife Insurance T5";
            T5Coverage.canStack = false;
            T5Coverage.isDebuff = false;
            T5Coverage.iconSprite = MainAssets.LoadAsset<Sprite>("HolyInsuranceT5Icon");

            ContentAddition.AddBuffDef(T5Coverage);

            T6Coverage = ScriptableObject.CreateInstance<BuffDef>();
            T6Coverage.name = "SupplyDrop Afterlife Insurance T6";
            T6Coverage.canStack = false;
            T6Coverage.isDebuff = false;
            T6Coverage.iconSprite = MainAssets.LoadAsset<Sprite>("HolyInsuranceT6Icon");

            ContentAddition.AddBuffDef(T6Coverage);

            T7Coverage = ScriptableObject.CreateInstance<BuffDef>();
            T7Coverage.name = "SupplyDrop Afterlife Insurance T7";
            T7Coverage.canStack = false;
            T7Coverage.isDebuff = false;
            T7Coverage.iconSprite = MainAssets.LoadAsset<Sprite>("HolyInsuranceT7Icon");

            ContentAddition.AddBuffDef(T7Coverage);

            T8Coverage = ScriptableObject.CreateInstance<BuffDef>();
            T8Coverage.name = "SupplyDrop Afterlife Insurance T8";
            T8Coverage.canStack = false;
            T8Coverage.isDebuff = false;
            T8Coverage.iconSprite = MainAssets.LoadAsset<Sprite>("HolyInsuranceT8Icon");

            ContentAddition.AddBuffDef(T8Coverage);

            T9Coverage = ScriptableObject.CreateInstance<BuffDef>();
            T9Coverage.name = "SupplyDrop Afterlife Insurance T9";
            T9Coverage.canStack = false;
            T9Coverage.isDebuff = false;
            T9Coverage.iconSprite = MainAssets.LoadAsset<Sprite>("HolyInsuranceT9Icon");

            ContentAddition.AddBuffDef(T9Coverage);

            T10Coverage = ScriptableObject.CreateInstance<BuffDef>();
            T10Coverage.name = "SupplyDrop Afterlife Insurance T10";
            T10Coverage.canStack = false;
            T10Coverage.isDebuff = false;
            T10Coverage.iconSprite = MainAssets.LoadAsset<Sprite>("HolyInsuranceT10Icon");

            ContentAddition.AddBuffDef(T10Coverage);
        }
        public void SetupAttributes()
        {
            //Here we set up all the coverages. Commented out entries still need to have their names verified

            //T1 Coverage: Small Pests and Vermin
            InsuranceDictionary.Add("BeetleMonster", new Range(1, 2, T1Coverage));
/*            InsuranceDictionary.Add("Jellyfish", new Range(1, 2));
            InsuranceDictionary.Add("Blind Pest", new Range(1, 2));
            InsuranceDictionary.Add("Blind Vermin", new Range(1, 2));
            InsuranceDictionary.Add("Hermit Crab", new Range(1, 2));
            InsuranceDictionary.Add("Malachite Urchin", new Range(1, 2));*/
            InsuranceDictionary.Add("LemurianMonster", new Range(1, 2, T1Coverage));

            //T2 Coverage: Improperly-Stored Debris
/*            InsuranceDictionary.Add("Stone Golem", new Range(2, 3));
            InsuranceDictionary.Add("Stone Titan", new Range(2, 3));
            InsuranceDictionary.Add("Coil Golem", new Range(2, 3));*/

            //T3 Coverage: Large Pests and Vermin
            InsuranceDictionary.Add("LemurianBruiserMonster", new Range(3, 4, T3Coverage));
            InsuranceDictionary.Add("BeetleGuardMonster", new Range(3, 4, T3Coverage));
/*            InsuranceDictionary.Add("Gup", new Range(3, 4));
            InsuranceDictionary.Add("Bighorn Bison", new Range(3, 4));
            InsuranceDictionary.Add("Alloy Vulture", new Range(3, 4));
            InsuranceDictionary.Add("Assassin", new Range(3, 4));*/

            //T4 Coverage: Hazardous Exposure (Minor)
/*            InsuranceDictionary.Add("Mini Mushrum", new Range(4, 5));
            InsuranceDictionary.Add("Mother Mushrum", new Range(4, 5));
            InsuranceDictionary.Add("Larva", new Range(4, 5));*/

            //T5 Coverage: Catastrophic Equipment Failure
/*            InsuranceDictionary.Add("Alpha Construct", new Range(5, 6));
            InsuranceDictionary.Add("Solus Probe", new Range(5, 6));
            InsuranceDictionary.Add("Overloading Bomb", new Range(5, 6));
            InsuranceDictionary.Add("Brass Contraption", new Range(5, 6));
            InsuranceDictionary.Add("Brass Monolith", new Range(5, 6));
            InsuranceDictionary.Add("Exploding Barrels", new Range(5, 6));
            InsuranceDictionary.Add("Self Damage", new Range(5, 6));*/

            //T6 Coverage: Hazardous Exposure (Major)
/*            InsuranceDictionary.Add("Clay Man", new Range(6, 7));
            InsuranceDictionary.Add("Clay Templar", new Range(6, 7));
            InsuranceDictionary.Add("Clay Apothecary", new Range(6, 7));
            InsuranceDictionary.Add("Void Fog", new Range(6, 7));*/

            //T7 Coverage: Supernatural Forces
            InsuranceDictionary.Add("Wisp1Monster", new Range(7, 8, T7Coverage));
            InsuranceDictionary.Add("GreaterWispMonster", new Range(7, 8, T7Coverage));
/*            InsuranceDictionary.Add("Archaic Wisp", new Range(7, 8));
            InsuranceDictionary.Add("Frost Wisp", new Range(7, 8));
            InsuranceDictionary.Add("Parent", new Range(7, 8));
            InsuranceDictionary.Add("Imp", new Range(7, 8));
            InsuranceDictionary.Add("Child", new Range(7, 8));
            InsuranceDictionary.Add("Artifact Reliquary", new Range(7, 8));*/

            //T8 Coverage: Interstellar Incidents
/*            InsuranceDictionary.Add("Chimera Tank", new Range(8, 9));
            InsuranceDictionary.Add("Chimera Exploder", new Range(8, 9));
            InsuranceDictionary.Add("Chimera Wisp", new Range(8, 9));
            InsuranceDictionary.Add("Void Infestor", new Range(8, 9));
            InsuranceDictionary.Add("Void Barnacle", new Range(8, 9));
            InsuranceDictionary.Add("Void Reaver", new Range(8, 9));
            InsuranceDictionary.Add("Void Jailer", new Range(8, 9));
            InsuranceDictionary.Add("Void Explosion", new Range(8, 9));*/

            //T9 Coverage: Extreme and Unmitigated Disaster
            InsuranceDictionary.Add("MagmaWorm", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("OverloadingWorm", new Range(9, 10, T9Coverage));
/*            InsuranceDictionary.Add("Beetle Queen", new Range(9, 10));
            InsuranceDictionary.Add("Wandering Vagrant", new Range(9, 10));
            InsuranceDictionary.Add("Direseeker", new Range(9, 10));
            InsuranceDictionary.Add("Clay Dunestrider", new Range(9, 10));
            InsuranceDictionary.Add("Xi Construct", new Range(9, 10));
            InsuranceDictionary.Add("Iota Construct", new Range(9, 10));
            InsuranceDictionary.Add("Solus Control Unit", new Range(9, 10));
            InsuranceDictionary.Add("Alloy Worship Unit", new Range(9, 10));
            InsuranceDictionary.Add("Imp Overlord", new Range(9, 10));
            InsuranceDictionary.Add("Grandparent", new Range(9, 10));
            InsuranceDictionary.Add("Ancient Wisp", new Range(9, 10));
            InsuranceDictionary.Add("Grovetender", new Range(9, 10));
            InsuranceDictionary.Add("Void Devestator", new Range(9, 10));
            InsuranceDictionary.Add("Voidling", new Range(9, 10));
            InsuranceDictionary.Add("Scavenger", new Range(9, 10));*/

            //T10 Coverage
            InsuranceDictionary.Add("BrotherMonster", new Range(10, uint.MaxValue, T10Coverage));
/*            InsuranceDictionary.Add("Providence", new Range(10, uint.MaxValue));
            InsuranceDictionary.Add("The", new Range(10, uint.MaxValue));
            InsuranceDictionary.Add("Crowdfunder Woolie", new Range(10, uint.MaxValue));*/
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("BloodBookTracker.prefab");
            ItemFollowerPrefab = ItemModel;

            var ItemFollower = ItemBodyModelPrefab.AddComponent<Utils.ItemFollower>();
            ItemFollower.itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemFollower.itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);
            ItemFollower.followerPrefab = ItemFollowerPrefab;
            ItemFollower.targetObject = ItemBodyModelPrefab;
            ItemFollower.distanceDampTime = 0.15f;
            ItemFollower.distanceMaxSpeed = 100;
            ItemFollower.SmoothingNumber = 0.25f;

            Vector3 generalScale = new Vector3(0.08f, 0.08f, 0.08f);

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
                    childName = "Base",
                    localPos = new Vector3(0.4f, -0.45f, -0.2f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.3f, -0.7f, 0f),
                    localAngles = new Vector3(-100f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.71034F, -0.63998F, 0.00227F),
                    localAngles = new Vector3(344.1754F, 269.9999F, 90.00006F),
                    localScale = generalScale
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-4F, -5.6233F, 2.49766F),
                    localAngles = new Vector3(270F, 180F, 0F),
                    localScale = new Vector3(0.12F, 0.12F, 0.12F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.8f, -0.6f, -0.5f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale * 1.25f
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-0.6f, -0.6f, -0.2f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-0.5f, -0.7f, -0.3f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1.5f, -1f, -2.5f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale * 1.5f
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-0.5f, -0.8f, -0.6f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale * 1.25f
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-7f, 7f, 5f),
                    localAngles = new Vector3(90f, 0f, 0f),
                    localScale = generalScale * 2
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-0.7f, -0.8f, -0.5f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.4f, -0.45f, -0.2f),
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.4F, 1.25452F, -0.2507F),
                    localAngles = new Vector3(73.45808F, 0F, 0F),
                    localScale = new Vector3(0.08F, 0.08F, 0.08F)
                }
            });

            //MODDED CHARACTER IDRs START HERE

            rules.Add("mdlNemCommando", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.87817F, -3.2917F, -0.1163F),
                    localAngles = new Vector3(285.2254F, 254.9387F, 108.4152F),
                    localScale = new Vector3(0.08F, 0.08F, 0.08F)
                }
            });
            rules.Add("mdlHANDOverclocked", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(1.71372F, 6.76202F, -3.49337F),
                    localAngles = new Vector3(0F, 349.7435F, 0F),
                    localScale = new Vector3(0.08F, 0.08F, 0.08F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.60871F, 0.60074F, 1.48497F),
                    localAngles = new Vector3(62.63927F, 257.8052F, 254.7548F),
                    localScale = new Vector3(0.08F, 0.08F, 0.08F)
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
                    childName = "Base",
                    localPos = new Vector3(0.62109F, 0.82824F, -1.07025F),
                    localAngles = new Vector3(358.4169F, 352.8858F, 355.2294F),
                    localScale = new Vector3(0.08F, 0.08F, 0.08F)
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
                    childName = "Base",
                    localPos = new Vector3(0.01657F, -0.80647F, -0.23414F),
                    localAngles = new Vector3(286.6669F, 119.4015F, 232.9694F),
                    localScale = new Vector3(0.08F, 0.08F, 0.08F)
                }
            });
            rules.Add("mdlExecutioner2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.2794F, -0.82243F, -0.27335F),
                    localAngles = new Vector3(283.4209F, 287.5865F, 71.18181F),
                    localScale = new Vector3(0.08F, 0.08F, 0.08F)
                }
            });
            rules.Add("mdlHouse(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HouseMesh",
                    localPos = new Vector3(0.28022F, 0.91114F, 1.72036F),
                    localAngles = new Vector3(76.06631F, 212.0132F, 206.0062F),
                    localScale = new Vector3(0.08F, 0.08F, 0.08F)
                }
            });
            rules.Add("mdlTeslaTrooper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Root",
                    localPos = new Vector3(0.25569F, 1.62016F, -0.88987F),
                    localAngles = new Vector3(9.38397F, 348.1637F, 356.5085F),
                    localScale = new Vector3(0.08F, 0.08F, 0.08F)
                }
            });
            rules.Add("mdlDesolator", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Root",
                    localPos = new Vector3(-0.55023F, 1.86072F, -1.09078F),
                    localAngles = new Vector3(4.60453F, 351.0109F, 355.5093F),
                    localScale = new Vector3(0.08F, 0.08F, 0.08F)
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
                    childName = "MainHurtbox",
                    localPos = new Vector3(0.2661F, 0.13232F, -0.89047F),
                    localAngles = new Vector3(10.73697F, 341.5828F, 359.0023F),
                    localScale = new Vector3(0.08F, 0.08F, 0.08F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.56086F, 0.87333F, 0.53255F),
                    localAngles = new Vector3(75.50265F, 252.8568F, 156.8284F),
                    localScale = new Vector3(0.08F, 0.08F, 0.08F)
                }
            });
            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.Run.RecalculateDifficultyCoefficentInternal += PolicyUpgradePriceCalculator;
            On.RoR2.DeathRewards.OnKilledServer += MoneyReduction;
            On.RoR2.CharacterMaster.OnBodyDeath += CoverageCheck;
            //On.RoR2.UI.HUD.Awake += InsuranceBarAwake;
            //On.RoR2.UI.HUD.Update += InsuranceBarUpdate;
        }

        public struct Range
        {
            public double Lower;
            public double Upper;
            public BuffDef Buff;

            public Range(double lower, double upper, BuffDef buff)
            {
                Lower = lower;
                Upper = upper;
                Buff = buff;
            }
            public bool Contains(double value)
            {
                return value >= Lower && value < Upper;
            }
        }
        private void PolicyUpgradePriceCalculator(On.RoR2.Run.orig_RecalculateDifficultyCoefficentInternal orig, Run self)
        {
            orig(self);

            var diffCoeff = self.difficultyCoefficient;
            var baseCost = 25 * Mathf.Pow(diffCoeff, 1.25f) * costTierMultiplier;

            //Have to redefine all the dictionary entries here to set the Range values to their proper costs
            InsuranceDictionary["BeetleMonster"] = new Range(baseCost, baseCost * 2, T1Coverage);   
            InsuranceDictionary["LemurianMonster"] = new Range(baseCost, baseCost * 2, T1Coverage);

            InsuranceDictionary["LemurianBruiserMonster"] = new Range(baseCost * 3, baseCost * 4, T3Coverage);
            InsuranceDictionary["BeetleGuardMonster"] = new Range(baseCost * 3, baseCost * 4, T3Coverage);

            InsuranceDictionary["Wisp1Monster"] = new Range(baseCost * 7, baseCost * 8, T7Coverage);
            InsuranceDictionary["GreaterWispMonster"] = new Range(baseCost * 7, baseCost * 8, T7Coverage);

            InsuranceDictionary["MagmaWorm"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);

            InsuranceDictionary["BrotherMonster"] = new Range(baseCost * 10, baseCost * uint.MaxValue, T10Coverage);

            //Sets up ranges for the buff application to draw from, since it can't use the dictionary (It is illiterate :p )
            ranges = new Range[]
            {
                new Range(baseCost, baseCost * 2, T1Coverage),
                new Range(baseCost * 2, baseCost * 3, T2Coverage),
                new Range(baseCost * 3, baseCost * 4, T3Coverage),
                new Range(baseCost * 4, baseCost * 5, T4Coverage),
                new Range(baseCost * 5, baseCost * 6, T5Coverage),
                new Range(baseCost * 6, baseCost * 7, T6Coverage),
                new Range(baseCost * 7, baseCost * 8, T7Coverage),
                new Range(baseCost * 8, baseCost * 9, T8Coverage),
                new Range(baseCost * 9, baseCost * 10, T9Coverage),
                new Range(baseCost * 10, uint.MaxValue, T10Coverage),
            };
        }
        private void MoneyReduction(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport rep)
        {
            orig(self, rep);

            var cbKiller = self.GetComponent<CharacterBody>();
            if(cbKiller)
            {
                var inventoryCount = GetCount(cbKiller);
                if (inventoryCount > 0)
                {
                    var insuranceSavingsTrackerComponent = rep.attackerMaster.gameObject.GetComponent<InsuranceSavingsTracker>();
                    if (!insuranceSavingsTrackerComponent)
                    {
                        rep.attackerMaster.gameObject.AddComponent<InsuranceSavingsTracker>();
                    }

                    uint origGold = self.goldReward;
                    uint reducedGold = (uint)Mathf.FloorToInt(self.goldReward * (1 - ((baseGoldDrain + (addGoldDrain * (inventoryCount - 1))) / ((inventoryCount * baseGoldDrain) + 1))));
                    uint investedGold = (uint)Mathf.FloorToInt((origGold - reducedGold) * (baseGoldToCoverage + addGoldToCoverage * (inventoryCount - 1)));
                    self.goldReward = reducedGold;

                    //Could you theoretically go over uint.MaxValue here? idk
                    insuranceSavingsTrackerComponent.insuranceSavings += investedGold;

                    Debug.LogError("The money is actually being tracked. Rn you have " + insuranceSavingsTrackerComponent.insuranceSavings);

                    //This chunk checks if you have a buff/tier, and if your savings have reached the point where you should be bumped into the next tier
                    int currentTier = Array.FindIndex(ranges, r => rep.attackerBody.HasBuff(r.Buff));
                    int nextTier = Array.FindIndex(ranges, r => r.Contains(insuranceSavingsTrackerComponent.insuranceSavings));
                    if (nextTier > currentTier)
                    {
                        if (currentTier != -1)
                        {
                            rep.attackerBody.RemoveBuff(ranges[currentTier].Buff);
                        }
                        rep.attackerBody.AddBuff(ranges[nextTier].Buff);
                        Debug.LogError("Buffs that are active:" + rep.attackerBody.activeBuffsList);
                    }
                }
            }     
        }
        private void CoverageCheck(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {
            if (GetCount(body) > 0)
            {
                var attackerComponent = self.gameObject.GetComponent<DamageReport>();

                var insuranceSavingsTrackerComponent = self.gameObject.GetComponent<InsuranceSavingsTracker>();
                if (!insuranceSavingsTrackerComponent)
                {
                    self.gameObject.AddComponent<InsuranceSavingsTracker>();
                }

                //Tries to get a key with the killer's name. If one exists, throws out the corresponding Range to that key.
                //Then check to ensure that Range's Upper value is less than the amount of gold you saved (confirms you're above that tier)
                if (InsuranceDictionary.TryGetValue(attackerComponent.attacker.name, out Range insuranceRange) && insuranceRange.Upper < insuranceSavingsTrackerComponent.insuranceSavings)
                {
                    self.Invoke("RespawnExtraLife", 2f);
                    self.Invoke("PlayExtraLifeSFX", 1f);
                    
                    //This chunk ensures the extra money you are forced to save once you unlock the final coverage tier isn't wasted,
                    //by only reducing your savings = to the cost of unlocking the final coverage tier (or just to zero, if you haven't unlocked that tier)
                    var savingsComponent = body.gameObject.GetComponent<Run>();
                    if (!savingsComponent)
                    {
                        body.gameObject.AddComponent<Run>();
                    }
                    var diffCoeff = savingsComponent.difficultyCoefficient;
                    var baseCost = Convert.ToUInt32(25 * Math.Pow(diffCoeff, 1.25f) * costTierMultiplier);
                    insuranceSavingsTrackerComponent.insuranceSavings = Math.Max(insuranceSavingsTrackerComponent.insuranceSavings - baseCost * 16, 0);
                }
            }
            orig(self, body);
        }

        //Commenting this out for now, will use a simple buff counter system until I figure out UI stuff

        /*public void InsuranceBarAwake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);

            var prefab = MainAssets.LoadAsset<GameObject>("InsuranceBar");
            InsuranceBar = UnityEngine.Object.Instantiate(prefab, self.mainContainer.transform);

            if (InsuranceBar)
            {
                var cachedSavingsComponent = self.targetMaster.gameObject.GetComponent<InsuranceSavingsTracker>();
                if (!cachedSavingsComponent)
                {
                    self.targetMaster.gameObject.AddComponent<InsuranceSavingsTracker>();
                }

                foreach (Range range in InsuranceDictionary.Values)
                {
                    if (cachedSavingsComponent.insuranceSavings >= range.Lower && cachedSavingsComponent.insuranceSavings < range.Upper)
                    {
                        InsuranceBar.GetComponentInChildren<Slider>().maxValue = Convert.ToSingle(range.Upper);
                    }
                }
                InsuranceBar.GetComponentInChildren<Slider>().value = cachedSavingsComponent.insuranceSavings;
            }
        }
        public void InsuranceBarUpdate(On.RoR2.UI.HUD.orig_Update orig, RoR2.UI.HUD self)
        {
            orig(self);

            if (InsuranceBar)
            {
                var cachedSavingsComponent = self.targetMaster.gameObject.GetComponent<InsuranceSavingsTracker>();
                if (!cachedSavingsComponent)
                {
                    self.targetMaster.gameObject.AddComponent<InsuranceSavingsTracker>();
                }

                foreach (Range range in InsuranceDictionary.Values)
                {
                    if (cachedSavingsComponent.insuranceSavings >= range.Lower && cachedSavingsComponent.insuranceSavings < range.Upper)
                    {
                        InsuranceBar.GetComponentInChildren<Slider>().maxValue = Convert.ToSingle(range.Upper);
                    }
                }

                InsuranceBar.AddComponent<InsuranceBarController>();
            }
        }*/
    }
}
