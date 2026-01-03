using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.RecalculateStatsAPI;

using SupplyDrop.Utils;
using static SupplyDrop.Utils.ItemHelpers;
using static SupplyDrop.Utils.MathHelpers;
using static K1454.SupplyDrop.SupplyDropPlugin;

using BepInEx.Configuration;
using UnityEngine.Networking;

namespace SupplyDrop.Items
{
    public class ArrogantCanting : ItemBase<ArrogantCanting>
    {
        //Config Stuff

        public static ConfigOption<float> baseHPIncrease;
        public static ConfigOption<float> addHPIncrease;
        public static ConfigOption<float> baseDamageIncrease;
        public static ConfigOption<float> addDamageIncrease;
        public static ConfigOption<float> baseDropChance;
        public static ConfigOption<float> addDropChance;

        //Item Data

        public override string ItemName => "Arrogant Canting";

        public override string ItemLangTokenName => "ARROGANTCANTING";

        public override string ItemPickupDesc => "Elite enemies have a <style=cIsUtility>chance to drop items</style>, but are <style=cDeath>more powerful</style>.";

        public override string ItemFullDescription => $"Elite enemies have a {FloatToPercentageString(baseDropChance)} <style=cStack>(+{FloatToPercentageString(addDropChance)} " +
            $"per stack)</style> chance to <style=cIsUtility>drop a random item</style> on death, but they also gain {FloatToPercentageString(baseHPIncrease)} <style=cStack>(+{FloatToPercentageString(addHPIncrease)} " +
            $"per stack)</style> <style=cDeath>more HP</style> and {FloatToPercentageString(baseDamageIncrease)} <style=cStack>(+{FloatToPercentageString(addDamageIncrease)} " +
            $"per stack)</style> <style=cDeath>more damage</style>.";

        public override string ItemLore => "Do you remember, Brother, when I made a steed for myself?\n\n" +
            "Stone. Silver. Fire. All the ingredients to make a perfect creation, fast and able. But that was not enough this time. I wished to prove how right I had always been. " +
            "To prove how little value soul, that which you held so sacred and precious, truly had.\n\n" +
            "So I forged something new, superior to soul. Stronger, more vibrant. Obedient. Infused into my design, I was certain a mere glance at this perfect creature would be enough to convince you at last.\n\n" +
            "But when you looked upon the warhorse, you did not sing its praises. Do you remember what you did? YOU, who protested so greatly at the imprisonment of that abominable gold creature?\n\n" +
            "You did not hesitate to seal my steed away. You could not accept my success. My wisdom. MY RIGHTEOUSNESS.\n\n" +
            "I should have known then.";

        public override ItemTier Tier => ItemTier.Lunar;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.OnKillEffect};

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("ArrogantCanting.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("ArrogantCantingIcon");
        public static GameObject ItemBodyModelPrefab;

        public static PickupDropTable SacrificePickupDropTable => RoR2.Artifacts.SacrificeArtifactManager.dropTable;
        public static Xoroshiro128Plus dropRoll;


        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
            dropRoll = new Xoroshiro128Plus(0UL);

            ItemDef.pickupModelPrefab.transform.localScale = new Vector3(2f, 2f, 2f);
        }

        private void CreateConfig(ConfigFile config)
        {
            baseHPIncrease = config.ActiveBind<float>("Item: " + ItemName, "Base Increase to HP Buff Elites Get with 1 Arrogant Canting", .25f, "How much bonus HP should elites get with 1 Arrogant Canting? (.25 = 25%)");
            addHPIncrease = config.ActiveBind<float>("Item: " + ItemName, "Additional Increase to HP Buff Elites Get per Arrogant Canting", .25f, "How much bonus HP should elites get for each additional Arrogant Canting? (.25 = 25%)");
            baseDamageIncrease = config.ActiveBind<float>("Item: " + ItemName, "Base Increase to Damage Buff Elites Get with 1 Arrogant Canting", .15f, "How much bonus damage should elites get with 1 Arrogant Canting? (.15 = 15%)");
            addDamageIncrease = config.ActiveBind<float>("Item: " + ItemName, "Additional Increase to Damage Buff Elites Get per Arrogant Canting", .15f, "How much bonus damage should elites get for each additional Arrogant Canting? (.15 = 15%)");
            baseDropChance = config.ActiveBind<float>("Item: " + ItemName, "Base Chance for Elites to Drop Items with 1 Arrogant Canting", .06f, "What should the chance an elite drop an item be with 1 Arrogant Canting? (.06 = 6%)");
            addDropChance = config.ActiveBind<float>("Item: " + ItemName, "Additional Chance for Elites to Drop Items per Arrogant Canting", .06f, "What should the chance an elite drop an item be for each additional Arrogant Canting? (.06 = 6%)");
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
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Chest",
                    localPos = new Vector3(0, 0, 0),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1, 1, 1)
                }
            });
            rules.Add("mdlCommandoDualies", new ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(0.00277F, 0.17479F, 0.0266F),
                    localAngles = new Vector3(0.00617F, 359.8606F, 7.22759F),
                    localScale = new Vector3(0.03707F, 0.03707F, 0.03707F)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(0.01297F, 0.11871F, 0.00568F),
                    localAngles = new Vector3(359.3404F, 356.1982F, 0.70586F),
                    localScale = new Vector3(0.02652F, 0.02652F, 0.02652F)
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(0.00291F, 0.21169F, -0.04033F),
                    localAngles = new Vector3(317.8236F, 358.0385F, 358.1294F),
                    localScale = new Vector3(0.04213F, 0.04213F, 0.04213F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "MuzzleNailgun",
                    localPos = new Vector3(1.18679F, 0.1173F, -0.03888F),
                    localAngles = new Vector3(350.6134F, 71.73103F, 82.40997F),
                    localScale = new Vector3(0.34089F, 0.34089F, 0.34089F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(-0.00118F, 0.17366F, 0.03878F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.07042F, 0.07042F, 0.07042F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(0.0078F, 0.14205F, 0.01929F),
                    localAngles = new Vector3(324.8272F, 359.9283F, 357.544F),
                    localScale = new Vector3(0.02846F, 0.03558F, 0.02846F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(-0.00806F, 0.16336F, -0.01393F),
                    localAngles = new Vector3(39.97188F, 179.9329F, 359.8967F),
                    localScale = new Vector3(0.04412F, 0.04412F, 0.04412F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "WeaponPlatform",
                    localPos = new Vector3(0.00331F, -0.09F, -0.0336F),
                    localAngles = new Vector3(283.2124F, 177.6522F, 1.84774F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(0.00705F, 0.16607F, -0.0321F),
                    localAngles = new Vector3(308.2388F, 358.9312F, 356.0719F),
                    localScale = new Vector3(0.04377F, 0.04377F, 0.04377F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(-0.12926F, 0.80654F, -0.53378F),
                    localAngles = new Vector3(279.2639F, 5.08149F, 356.154F),
                    localScale = new Vector3(0.50728F, 0.7074F, 0.58804F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(0.02313F, 0.21775F, -0.07184F),
                    localAngles = new Vector3(307.5771F, 340.4614F, 3.61965F),
                    localScale = new Vector3(0.05273F, 0.05273F, 0.05273F)
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(-0.00375F, 0.24964F, -0.09597F),
                    localAngles = new Vector3(50.55249F, 180.3299F, 1.73038F),
                    localScale = new Vector3(0.07527F, 0.07463F, 0.07041F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(0.03417F, 0.21444F, 0.00097F),
                    localAngles = new Vector3(30.92299F, 94.97478F, 358.8921F),
                    localScale = new Vector3(0.04332F, 0.04455F, 0.04455F)
                }
            });
            rules.Add("mdlSeeker", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(0.03417F, 0.21444F, 0.00097F),
                    localAngles = new Vector3(30.92299F, 94.97478F, 358.8921F),
                    localScale = new Vector3(0.04332F, 0.04455F, 0.04455F)
                }
            });
            rules.Add("mdlFalseSon", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(0.03656F, -0.06252F, -0.01044F),
                    localAngles = new Vector3(47.32598F, 89.34612F, 359.2574F),
                    localScale = new Vector3(0.0554F, 0.0554F, 0.0554F)
                }
            });
            rules.Add("mdlChef", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "Wheel",
                    localPos = new Vector3(0.51771F, 0.01124F, -0.01005F),
                    localAngles = new Vector3(0.32579F, 83.71847F, 312.9304F),
                    localScale = new Vector3(0.10235F, 0.10235F, 0.10235F)
                }
            });
            rules.Add("mdlDroneTech", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(-0.11657F, -0.09575F, 0.00385F),
                    localAngles = new Vector3(313.9219F, 97.58095F, 174.3286F),
                    localScale = new Vector3(0.02756F, 0.02756F, 0.02756F)
                }
            });
            rules.Add("mdlDrifter", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootR",
                    localPos = new Vector3(-0.13141F, 0.1724F, -0.01229F),
                    localAngles = new Vector3(359.4962F, 271.9681F, 18.23417F),
                    localScale = new Vector3(0.03821F, 0.03622F, 0.03821F)
                }
            });
            //MODDED CHARACTER IDRs START HERE

            rules.Add("mdlNemCommando", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(-0.01742F, 0.87634F, 0.0401F),
                    localAngles = new Vector3(38.36021F, 4.24105F, 9.9883F),
                    localScale = new Vector3(0.17393F, 0.17393F, 0.17393F)
                }
            });
            rules.Add("mdlHANDOverclocked", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "UpperArmL",
                    localPos = new Vector3(-0.04755F, -0.02314F, -0.06916F),
                    localAngles = new Vector3(358.8415F, 88.95627F, 261.6975F),
                    localScale = new Vector3(0.65672F, 0.65672F, 2.04141F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "UpperArmL",
                    localPos = new Vector3(-0.04755F, -0.02314F, -0.06916F),
                    localAngles = new Vector3(358.8415F, 88.95627F, 261.6975F),
                    localScale = new Vector3(0.65672F, 0.65672F, 2.04141F)
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
                    childName = "FootL",
                    localPos = new Vector3(0.03683F, 0.38324F, 0.00515F),
                    localAngles = new Vector3(337.9349F, 11.15429F, 356.3635F),
                    localScale = new Vector3(0.08552F, 0.08552F, 0.08552F)
                }
            });
            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootR",
                    localPos = new Vector3(0.00003F, 0.00151F, -0.0003F),
                    localAngles = new Vector3(16.11882F, 355.5665F, 359.4258F),
                    localScale = new Vector3(0.00034F, 0.00041F, 0.00041F)
                }
            });
            rules.Add("mdlPathfinder", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(-0.01601F, 0.19647F, -0.03739F),
                    localAngles = new Vector3(49.17606F, 179.0234F, 355.9081F),
                    localScale = new Vector3(0.03928F, 0.03928F, 0.03928F)
                }
            });
            rules.Add("mdlExecutioner2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "FootL",
                    localPos = new Vector3(-0.01211F, 0.17431F, -0.04927F),
                    localAngles = new Vector3(40.66501F, 183.761F, 354.1893F),
                    localScale = new Vector3(0.04697F, 0.04697F, 0.04697F)
                }
            });
            rules.Add("mdlHouse(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "LeftFoot",
                    localPos = new Vector3(-0.00221F, 0.2621F, -0.02835F),
                    localAngles = new Vector3(44.60031F, 187.0047F, 3.19586F),
                    localScale = new Vector3(0.05667F, 0.05667F, 0.05667F)
                }
            });
            rules.Add("mdlTeslaTrooper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "ToeL",
                    localPos = new Vector3(-0.00812F, 0.10074F, 0.0469F),
                    localAngles = new Vector3(357.8404F, 359.4761F, 357.9219F),
                    localScale = new Vector3(0.05955F, 0.05955F, 0.05955F)
                }
            });
            rules.Add("mdlDesolator", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "ToeL",
                    localPos = new Vector3(-0.00114F, 0.13838F, 0.03387F),
                    localAngles = new Vector3(4.98274F, 180.27F, 3.00657F),
                    localScale = new Vector3(0.07641F, 0.07641F, 0.07641F)
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
                    childName = "FootL",
                    localPos = new Vector3(-0.0218F, 0.25019F, -0.04457F),
                    localAngles = new Vector3(318.5143F, 356.2689F, 2.86264F),
                    localScale = new Vector3(0.03767F, 0.04566F, 0.03767F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab, followerPrefabAddress = new AssetReferenceGameObject(""),
                    childName = "shin.L",
                    localPos = new Vector3(-0.05437F, 0.07299F, -0.01956F),
                    localAngles = new Vector3(63.23962F, 49.92986F, 160.8041F),
                    localScale = new Vector3(0.05644F, 0.05644F, 0.05644F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            GetStatCoefficients += ElitesEatVeggies;
            GlobalEventManager.onCharacterDeathGlobal += EliteKillCheck;
        }

        //This chunk checks if the enemy killed was an elite
        private void EliteKillCheck(DamageReport damageReport)
        {
            //Confirm networking for item drops is active
            if (!NetworkServer.active)
            {
                return;
            }

            if (damageReport.victimBody && damageReport.victimBody.isElite && damageReport.attacker)
            {
                ActivateCanting(damageReport);
            }
        }
        //This chunk handles the roll
        private bool RollCheck(CharacterMaster master)
        {
            if (master)
            {
                var inventoryCount = GetCount(master);
                if (inventoryCount > 0)
                {
                    //Item scales hyperbolically, to slightly nerf stacking (10 stacks gives you 48% drop chance as opposed to 60%)
                    return Util.CheckRoll((1 - 1/(1 + baseDropChance + (addDropChance * (inventoryCount - 1)))) * 100, 0);
                }
            }
            return false;
        }
        //This chunk triggers the item drop
        private void ActivateCanting(DamageReport damageReport)
        {
            if (damageReport.attackerMaster && RollCheck(damageReport.attackerMaster))
            {
                var itemReward = SacrificePickupDropTable.GeneratePickupPreReplacement(dropRoll);

                PickupDropletController.CreatePickupDroplet(itemReward, damageReport.victimBody.corePosition, Vector3.up * 20f, false, false);
            }
        }

        //This chunk handles the elite buffing
        private void ElitesEatVeggies(CharacterBody body, StatHookEventArgs args)
        {
            int cantingsActive = Util.GetItemCountGlobal(ItemDef.itemIndex, true);
            if (cantingsActive > 0 && body.inventory && body.teamComponent && (body.teamComponent.teamIndex == TeamIndex.Monster | body.teamComponent.teamIndex == TeamIndex.Lunar | body.teamComponent.teamIndex == TeamIndex.Void))
            {
                args.damageMultAdd += (baseDamageIncrease + (cantingsActive * addDamageIncrease));
                args.healthMultAdd += (baseHPIncrease + (cantingsActive * addHPIncrease));
            }
        }
    }
}