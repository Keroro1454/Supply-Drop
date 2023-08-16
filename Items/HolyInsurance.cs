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

        public override string ItemLore => "Congratulations on becoming the newest member of the <style=cIsUtility>EternalLife<sup>TM</sup></style> family! Our motto at <style=cIsUtility>EternalLife<sup>TM</sup></style> is simple-provide the very best in insurance solutions, " +
            "so you don't have to worry about what life (or death) throws your way!" +

            "\n\nYou have opted into our<style=cIsUtility> Dynamic Afterlife Policy Plan<sup> TM</sup></style>! " +
            "This policy is unique, as you pay into it over time, and as you pay in, your coverage expands! " +
            "Our angelic actuaries have calculated the following as appropriate coverage tiers for you, " +
            "based on your career choice as <style=cMono>[REDACTED]</style>:" +

            "\n\nT1: <style=cHumanObjective>Small Pests and Vermin:</style> For creepy crawlies, rodents, and other small vermin" +
            "\nT2: <style=cHumanObjective>Improperly-Placed Debris:</style> For danger due to intentional or unintentional <style=cSub>(Read: Natural Causes)</style> action" +
            "\nT3: <style=cHumanObjective>Large Pests and Vermin:</style> For infestations that require trained professional removal" +
            "\nT4: <style=cHumanObjective>Hazardous Exposure (Minor):</style> For spills of unusual, toxic, and acidic materials" +
            "\nT5: <style=cHumanObjective>Catastrophic Equipment Failure:</style> For malfunctioning heavy machinery or technologies" +
            "\nT6: <style=cHumanObjective>Hazardous Exposure (Major):</style> For direct exposure to 'corrupting' materials or conditions" +
            "\nT7: <style=cHumanObjective>Supernatural Forces:</style> For those concerned by the possibility of otherworldly entities" +
            "\nT8: <style=cHumanObjective>Interstellar Incidents:</style> For all damages sustained off-planet" +
            "\nT9: <style=cHumanObjective>Extreme and/or Unmitigated Disaster:</style> For those who engage in activities with high probability of causing exceedingly large damages" +
            "\nT10: <style=cHumanObjective>Acts of God(s) :</style> For those that have attracted the ire of the divine. We at<style=cIsUtility> EternalLife<sup>TM</sup></style> do not recommend engaging in activities that could necessitate this coverage" +

            "\n\n<size=40%>Note that any causes of death not listed can not, and will not, be covered with your current plan. " +
            "Subscription to plan cannot retroactively alleviate any deaths you may suffer from (previously or currently). "+
            "As a subscriber, you agree to the terms that EternalLife<sup> TM</sup> is entitled to your eternal soul in the event of failure to pay for a needed coverage. Some additional terms and conditions may apply. " +
            "For questions and concerns, please visit your local place of worship, and direct all prayers to<style=cMono>[REDACTED]</style>, or visit us in-person by <style=cDeath>obliterating yourself</style>." +
            "\n\nThank you for choosing<style=cIsUtility> EternalLife<sup>TM</sup></style>" +
            "\nHave a blessed life...or several!";

        public override ItemTier Tier => ItemTier.Lunar;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("HolyInsurance.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("HolyInsuranceIcon");

        public static GameObject ItemBodyModelPrefab;


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

                ItemDef.pickupModelPrefab.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
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
            InsuranceDictionary.Add("BeetleBody", new Range(1, 2, T1Coverage));
            InsuranceDictionary.Add("JellyfishBody", new Range(1, 2, T1Coverage));
            InsuranceDictionary.Add("FlyingVerminBody", new Range(1, 2, T1Coverage));
            InsuranceDictionary.Add("HermitCrabBody", new Range(1, 2, T1Coverage));
            InsuranceDictionary.Add("UrchinTurretBody", new Range(1, 2, T1Coverage));
            InsuranceDictionary.Add("LemurianBody", new Range(1, 2, T1Coverage));

            //T2 Coverage: Improperly-Stored Debris
            InsuranceDictionary.Add("GolemBody", new Range(2, 3, T2Coverage));
            InsuranceDictionary.Add("TitanBody", new Range(2, 3, T2Coverage));
            //InsuranceDictionary.Add("Coil Golem", new Range(2, 3, T2Coverage));
            InsuranceDictionary.Add("ExplosivePotDestructibleBody", new Range(2, 3, T2Coverage));

            //T3 Coverage: Large Pests and Vermin
            InsuranceDictionary.Add("LemurianBruiserBody", new Range(3, 4, T3Coverage));
            InsuranceDictionary.Add("BeetleGuardBody", new Range(3, 4, T3Coverage));
            InsuranceDictionary.Add("GupBody", new Range(3, 4, T3Coverage));
            InsuranceDictionary.Add("GipBody", new Range(3, 4, T3Coverage));
            InsuranceDictionary.Add("GeepBody", new Range(3, 4, T3Coverage));
            InsuranceDictionary.Add("BisonBody", new Range(3, 4, T3Coverage));
            InsuranceDictionary.Add("VultureBody", new Range(3, 4, T3Coverage));
            InsuranceDictionary.Add("Assassin2Body", new Range(3, 4, T3Coverage));

            //T4 Coverage: Hazardous Exposure (Minor)
            InsuranceDictionary.Add("MiniMushrumBody", new Range(4, 5, T4Coverage));
            //InsuranceDictionary.Add("Mother Mushrum", new Range(4, 5, T4Coverage));
            InsuranceDictionary.Add("AcidLarvaBody", new Range(4, 5, T4Coverage));

            //T5 Coverage: Catastrophic Equipment Failure
            InsuranceDictionary.Add("MinorConstructBody", new Range(5, 6, T5Coverage));
            InsuranceDictionary.Add("MinorConstructAttachableBody", new Range(5, 6, T5Coverage));
            InsuranceDictionary.Add("RoboBallMiniBody", new Range(5, 6, T5Coverage));
            InsuranceDictionary.Add("BellBody", new Range(5, 6, T5Coverage));
            //InsuranceDictionary.Add("Brass Monolith", new Range(5, 6, T5Coverage));
            InsuranceDictionary.Add("EngiTurretBody", new Range(5, 6, T5Coverage));
            InsuranceDictionary.Add("EngiWalkerTurretBody", new Range(5, 6, T5Coverage));
            InsuranceDictionary.Add("EngiBeamTurretBody", new Range(5, 6, T5Coverage));

            //T6 Coverage: Hazardous Exposure (Major)
            InsuranceDictionary.Add("ClayBody", new Range(6, 7, T6Coverage));
            InsuranceDictionary.Add("ClayBruiserBody", new Range(6, 7, T6Coverage));
            InsuranceDictionary.Add("ClayGrenadierBody", new Range(6, 7, T6Coverage));

            //T7 Coverage: Supernatural Forces
            InsuranceDictionary.Add("WispBody", new Range(7, 8, T7Coverage));
            InsuranceDictionary.Add("WispSoulBody", new Range(7, 8, T7Coverage));
            InsuranceDictionary.Add("GreaterWispBody", new Range(7, 8, T7Coverage));
            InsuranceDictionary.Add("ArchWispBody", new Range(7, 8, T7Coverage));
            //InsuranceDictionary.Add("Frost Wisp", new Range(7, 8, T7Coverage));
            InsuranceDictionary.Add("ParentBody", new Range(7, 8, T7Coverage));
            InsuranceDictionary.Add("ImpBody", new Range(7, 8, T7Coverage));
            //InsuranceDictionary.Add("Child", new Range(7, 8, T7Coverage));
            InsuranceDictionary.Add("ArtifactShellBody", new Range(7, 8, T7Coverage));

            //T8 Coverage: Interstellar Incidents
            InsuranceDictionary.Add("LunarGolemBody", new Range(8, 9, T8Coverage));
            InsuranceDictionary.Add("LunarExploderBody", new Range(8, 9, T8Coverage));
            InsuranceDictionary.Add("LunarWispBody", new Range(8, 9, T8Coverage));
            InsuranceDictionary.Add("VoidInfestorBody", new Range(8, 9, T8Coverage));
            InsuranceDictionary.Add("VoidBarnacleBody", new Range(8, 9, T8Coverage));
            InsuranceDictionary.Add("NullifierBody", new Range(8, 9, T8Coverage));
            InsuranceDictionary.Add("VoidJailerBody", new Range(8, 9, T8Coverage));

            //T9 Coverage: Extreme and Unmitigated Disaster
            InsuranceDictionary.Add("MagmaWormBody", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("ElectricWormBody", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("BeetleQueen2Body", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("VagrantBody", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("DireseekerBody", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("ClayBossBody", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("MegaConstructBody", new Range(9, 10, T9Coverage));
            //InsuranceDictionary.Add("Iota Construct", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("RoboBallBossBody", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("SuperRoboBallBossBody", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("ImpBossBody", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("GrandParentBody", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("AncientWispBody", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("GravekeeperBody", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("VoidMegaCrabBody", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("VoidRaidCrabBody", new Range(9, 10, T9Coverage));
            InsuranceDictionary.Add("ScavBody", new Range(9, 10, T9Coverage));

            //T10 Coverage
            InsuranceDictionary.Add("BrotherBody", new Range(10, uint.MaxValue, T10Coverage));
            //InsuranceDictionary.Add("Providence", new Range(10, uint.MaxValue, T10Coverage));
            //InsuranceDictionary.Add("The", new Range(10, uint.MaxValue, T10Coverage));
            //InsuranceDictionary.Add("Crowdfunder Woolie", new Range(10, uint.MaxValue, T10Coverage));
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

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
                    childName = "Chest",
                    localPos = new Vector3(0.18219F, -0.06529F, -0.04509F),
                    localAngles = new Vector3(299.8667F, 29.11232F, 349.8899F),
                    localScale = new Vector3(0.03096F, 0.03096F, 0.03096F)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.08749F, 0.13625F, -0.13467F),
                    localAngles = new Vector3(6.42806F, 73.73848F, 331.7406F),
                    localScale = new Vector3(0.03F, 0.03F, 0.03F)
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.02547F, 0.00491F, -0.172F),
                    localAngles = new Vector3(5.6305F, 78.02337F, 341.478F),
                    localScale = new Vector3(0.03F, 0.03F, 0.03F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.09335F, 2.05053F, -2.51568F),
                    localAngles = new Vector3(0.51F, 88F, 352F),
                    localScale = new Vector3(0.28F, 0.28F, 0.28F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.17349F, 0.12851F, -0.29832F),
                    localAngles = new Vector3(273.1938F, 160.5572F, 322.9695F),
                    localScale = new Vector3(0.03721F, 0.03721F, 0.03721F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.09615F, 0.04596F, 0.06549F),
                    localAngles = new Vector3(285.2049F, 338.2951F, 209.1863F),
                    localScale = new Vector3(0.03287F, 0.03287F, 0.03287F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00955F, -0.06483F, -0.19421F),
                    localAngles = new Vector3(0.32483F, 90.88982F, 336.0152F),
                    localScale = new Vector3(0.03527F, 0.03527F, 0.03527F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.27599F, 0.68972F, 0.19514F),
                    localAngles = new Vector3(338.8711F, 197.1099F, 65.71246F),
                    localScale = new Vector3(0.08358F, 0.08358F, 0.08358F)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00029F, -0.16873F, -0.35909F),
                    localAngles = new Vector3(359.9306F, 90.0685F, 11.00691F),
                    localScale = new Vector3(0.03F, 0.03F, 0.03F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(1.90948F, -1.01914F, 2.20179F),
                    localAngles = new Vector3(60.26649F, 246.9373F, 273.693F),
                    localScale = new Vector3(0.21065F, 0.21065F, 0.21065F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.03962F, -0.19738F, -0.24154F),
                    localAngles = new Vector3(359.0274F, 291.7857F, 175.0815F),
                    localScale = new Vector3(0.0608F, 0.0608F, 0.0608F)
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Backpack",
                    localPos = new Vector3(0.23929F, -0.04112F, -0.0789F),
                    localAngles = new Vector3(270F, 0F, 0F),
                    localScale = new Vector3(0.02626F, 0.02626F, 0.02626F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00607F, 0.08962F, -0.25091F),
                    localAngles = new Vector3(70.35573F, 4.86549F, 268.1413F),
                    localScale = new Vector3(0.05034F, 0.05034F, 0.05034F)
                }
            });

            //MODDED CHARACTER IDRs START HERE

            rules.Add("mdlNemCommando", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.01137F, 1.07911F, -1.837F),
                    localAngles = new Vector3(277.7829F, 357.239F, 86.6313F),
                    localScale = new Vector3(0.11946F, 0.11946F, 0.11946F)
                }
            });
            rules.Add("mdlHANDOverclocked", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.47282F, 0.59166F, -1.44263F),
                    localAngles = new Vector3(272.6917F, 1.4447F, 80.36976F),
                    localScale = new Vector3(0.12012F, 0.12012F, 0.12012F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.31955F, -0.20512F, 0.00935F),
                    localAngles = new Vector3(0.05311F, 177.8366F, 179.8088F),
                    localScale = new Vector3(0.05316F, 0.05316F, 0.05316F)
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
                    childName = "Chest",
                    localPos = new Vector3(0.01907F, -0.09971F, -0.2888F),
                    localAngles = new Vector3(75.99004F, 346.6362F, 254.4505F),
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
                    childName = "StomachBone",
                    localPos = new Vector3(-0.15118F, 0.04194F, -0.12035F),
                    localAngles = new Vector3(278.6536F, 38.9723F, 98.37595F),
                    localScale = new Vector3(0.02446F, 0.02446F, 0.02446F)
                }
            });
            rules.Add("mdlExecutioner2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00742F, 0.21471F, -0.21099F),
                    localAngles = new Vector3(281.7602F, 344.066F, 108.33F),
                    localScale = new Vector3(0.04342F, 0.04342F, 0.04342F)
                }
            });
            rules.Add("mdlHouse(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.06118F, 0.0457F, -0.27575F),
                    localAngles = new Vector3(70.0668F, 194.3457F, 88.49615F),
                    localScale = new Vector3(0.04655F, 0.04655F, 0.04655F)
                }
            });
            rules.Add("mdlTeslaTrooper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.0303F, -0.18648F, -0.11893F),
                    localAngles = new Vector3(358.6501F, 91.16373F, 353.034F),
                    localScale = new Vector3(0.0347F, 0.0347F, 0.0347F)
                }
            });
            rules.Add("mdlDesolator", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.15829F, -0.10226F, -0.09997F),
                    localAngles = new Vector3(312.4372F, 75.743F, 342.6674F),
                    localScale = new Vector3(0.03826F, 0.03826F, 0.03826F)
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
                    childName = "ArsonistCanister",
                    localPos = new Vector3(0.09547F, -0.02179F, 0.10913F),
                    localAngles = new Vector3(18.85776F, 349.315F, 70.66489F),
                    localScale = new Vector3(0.0446F, 0.0446F, 0.0446F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.30521F, -0.23735F, -0.01135F),
                    localAngles = new Vector3(72.06647F, 87.88802F, 272.2217F),
                    localScale = new Vector3(0.04158F, 0.04158F, 0.04158F)
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

            //Have to redefine all the dictionary entries here to set the Range values to their proper costs. Commented stuff doesn't have the right name

            InsuranceDictionary["BeetleBody"] = new Range(baseCost, baseCost * 2, T1Coverage);
            InsuranceDictionary["JellyfishBody"] = new Range(baseCost, baseCost * 2, T1Coverage);
            InsuranceDictionary["FlyingVerminBody"] = new Range(baseCost, baseCost * 2, T1Coverage);
            InsuranceDictionary["HermitCrabBody"] = new Range(baseCost, baseCost * 2, T1Coverage);
            InsuranceDictionary["UrchinTurretBody"] = new Range(baseCost, baseCost * 2, T1Coverage);
            InsuranceDictionary["LemurianBody"] = new Range(baseCost, baseCost * 2, T1Coverage);

            InsuranceDictionary["GolemBody"] = new Range(baseCost * 2, baseCost * 3, T2Coverage);
            InsuranceDictionary["TitanBody"] = new Range(baseCost * 2, baseCost * 3, T2Coverage);
            //InsuranceDictionary["Coil Golem"] = new Range(baseCost * 2, baseCost * 3, T2Coverage);
            InsuranceDictionary["ExplosivePotDestructibleBody"] = new Range(baseCost * 2, baseCost * 3, T2Coverage);

            InsuranceDictionary["LemurianBruiserBody"] = new Range(baseCost * 3, baseCost * 4, T3Coverage);
            InsuranceDictionary["BeetleGuardBody"] = new Range(baseCost * 3, baseCost * 4, T3Coverage);
            InsuranceDictionary["GupBody"] = new Range(baseCost * 3, baseCost * 4, T3Coverage);
            InsuranceDictionary["GipBody"] = new Range(baseCost * 3, baseCost * 4, T3Coverage);
            InsuranceDictionary["GeepBody"] = new Range(baseCost * 3, baseCost * 4, T3Coverage);
            InsuranceDictionary["BisonBody"] = new Range(baseCost * 3, baseCost * 4, T3Coverage);
            InsuranceDictionary["VultureBody"] = new Range(baseCost * 3, baseCost * 4, T3Coverage);
            InsuranceDictionary["Assassin2Body"] = new Range(baseCost * 3, baseCost * 4, T3Coverage);

            InsuranceDictionary["MiniMushrumBody"] = new Range(baseCost * 4, baseCost * 5, T4Coverage);
            //InsuranceDictionary["Mother Mushrum"] = new Range(baseCost * 4, baseCost * 5, T4Coverage);
            InsuranceDictionary["AcidLarvaBody"] = new Range(baseCost * 4, baseCost * 5, T4Coverage);

            InsuranceDictionary["MinorConstructBody"] = new Range(baseCost * 5, baseCost * 6, T5Coverage);
            InsuranceDictionary["MinorConstructAttachableBody"] = new Range(baseCost * 5, baseCost * 6, T5Coverage);
            InsuranceDictionary["RoboBallMiniBody"] = new Range(baseCost * 5, baseCost * 6, T5Coverage);
            InsuranceDictionary["BellBody"] = new Range(baseCost * 5, baseCost * 6, T5Coverage);
            //InsuranceDictionary["Brass Monolith"] = new Range(baseCost * 5, baseCost * 6, T5Coverage);
            InsuranceDictionary["EngiTurretBody"] = new Range(baseCost * 5, baseCost * 6, T5Coverage);
            InsuranceDictionary["EngiWalkerTurretBody"] = new Range(baseCost * 5, baseCost * 6, T5Coverage);
            InsuranceDictionary["EngiBeamTurretBody"] = new Range(baseCost * 5, baseCost * 6, T5Coverage);

            InsuranceDictionary["ClayBody"] = new Range(baseCost * 6, baseCost * 7, T6Coverage);
            InsuranceDictionary["ClayBruiserBody"] = new Range(baseCost * 6, baseCost * 7, T6Coverage);
            InsuranceDictionary["ClayGrenadierBody"] = new Range(baseCost * 6, baseCost * 7, T6Coverage);

            InsuranceDictionary["WispBody"] = new Range(baseCost * 7, baseCost * 8, T7Coverage);
            InsuranceDictionary["WispSoulBody"] = new Range(baseCost * 7, baseCost * 8, T7Coverage);
            InsuranceDictionary["GreaterWispBody"] = new Range(baseCost * 7, baseCost * 8, T7Coverage);
            InsuranceDictionary["ArchWispBody"] = new Range(baseCost * 7, baseCost * 8, T7Coverage);
            //InsuranceDictionary["Frost Wisp"] = new Range(baseCost * 7, baseCost * 8, T7Coverage);
            InsuranceDictionary["ParentBody"] = new Range(baseCost * 7, baseCost * 8, T7Coverage);
            InsuranceDictionary["ImpBody"] = new Range(baseCost * 7, baseCost * 8, T7Coverage);
            //InsuranceDictionary["Child"] = new Range(baseCost * 7, baseCost * 8, T7Coverage);
            InsuranceDictionary["ArtifactShellBody"] = new Range(baseCost * 7, baseCost * 8, T7Coverage);

            InsuranceDictionary["LunarGolemBody"] = new Range(baseCost * 8, baseCost * 9, T8Coverage);
            InsuranceDictionary["LunarExploderBody"] = new Range(baseCost * 8, baseCost * 9, T8Coverage);
            InsuranceDictionary["LunarWispBody"] = new Range(baseCost * 8, baseCost * 9, T8Coverage);
            InsuranceDictionary["VoidInfestorBody"] = new Range(baseCost * 8, baseCost * 9, T8Coverage);
            InsuranceDictionary["VoidBarnacleBody"] = new Range(baseCost * 8, baseCost * 9, T8Coverage);
            InsuranceDictionary["NullifierBody"] = new Range(baseCost * 8, baseCost * 9, T8Coverage);
            InsuranceDictionary["VoidJailerBody"] = new Range(baseCost * 8, baseCost * 9, T8Coverage);

            InsuranceDictionary["MagmaWormBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["ElectricWormBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["BeetleQueen2Body"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["VagrantBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["DireseekerBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["ClayBossBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["MegaConstructBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            //InsuranceDictionary["Iota Construct"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["RoboBallBossBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["SuperRoboBallBossBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["ImpBossBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["GrandParentBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["AncientWispBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["GravekeeperBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["VoidMegaCrabBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["VoidRaidCrabBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);
            InsuranceDictionary["ScavBody"] = new Range(baseCost * 9, baseCost * 10, T9Coverage);

            InsuranceDictionary["BrotherBody"] = new Range(baseCost * 10, uint.MaxValue, T10Coverage);
            //InsuranceDictionary["Providence"] = new Range(baseCost * 10, uint.MaxValue, T10Coverage);
            //InsuranceDictionary["The"] = new Range(baseCost * 10, uint.MaxValue, T10Coverage);
            //InsuranceDictionary["Crowdfunder Woolie"] = new Range(baseCost * 10, uint.MaxValue, T10Coverage);

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
