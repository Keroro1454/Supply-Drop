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
    public class SalvagedWires : ItemBase<SalvagedWires>
    {
        //Config Stuff

        public static ConfigOption<float> baseStackHPPercent;
        public static ConfigOption<float> addStackHPPercent;
        public static ConfigOption<float> baseAttackSpeedPercent;
        public static ConfigOption<float> addAttackSpeedPercent;

        //Item Data
        public override string ItemName => "Salvaged Wires";

        public override string ItemLangTokenName => "WIRES";

        public override string ItemPickupDesc => "Gain some <style=cIsUtility>shield</style>, and gain increased <style=cIsDamage>attack speed</style> while your <style=cIsUtility>shield</style> is active.";

        public override string ItemFullDescription => $"Gain a <style=cIsUtility>shield</style> equal to <style=cIsUtility>{FloatToPercentageString(baseStackHPPercent)}</style>" +
            $" <style=cStack>(+{FloatToPercentageString(addStackHPPercent)} per stack)</style> of your maximum health." +
            $" While <style=cIsUtility>shield</style> is active, increases <style=cIsDamage>attack speed</style> by <style=cIsUtility>{FloatToPercentageString(baseAttackSpeedPercent)}</style>" +
            $" <style=cStack>(+{FloatToPercentageString(addAttackSpeedPercent)} per stack)</style>.";

        public override string ItemLore => "\"Now remember y'all. There are three rules of Space Scrappin'. You squirts may be dumber than rocks, but I 'spect y'all to remember them.\"" +
            "\n\n." +
            "\n." +
            "\n." +
            "\n\n\"Rule One: Bat-ries on, or you're gone.\"" +
            "\n\nThe derelict frigate drifted slowly, almost imperceptibly. If the Safekeepings had found itself abandoned in an atmosphere, " +
            "Alyk would have heard the sounds he was all too accustomed to: The aged guts of the ship creaking, the groaning of rusted hyperdrive intakes, the crackle of errant wisps of energy as they finally escaped the old wiring." +
            "\n\n\"Rule Two: Oh-two below twen-tee, leave it be.\"" +
            "\n\nBut in the void of space, Alyk could only hear his labored breaths, and his whispered Salvage Mantras. He paused to pull wires off the tracks, " +
            "his headlamp barely illuminating the area of the ducts in front of him. At 11, this was his first off-world salvage. His job was simple; due to his small frame, Alyk made for the perfect wire retriever. " +
            "And the Safekeepings was large enough, and full of enough electrum-alloy wiring, that a successful job here could mean food for his family for weeks." +
            "\n\n\"Rule 3: Pay only comes after a full day.\"" +
            "\n\nThe thought of smiles on his family's faces pushed the young boy forward. He stuffed the wires he had already claimed from the metal husk, and turned to face the dark tunnel before him. " +
            "Alyk took a deep breath, clenched his small hands into tight fists, then clamored down further into the silent wreck.";

        public override ItemTier Tier => ItemTier.Tier1;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("WireBundle.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("SalvagedWiresIcon");
        public static GameObject ItemBodyModelPrefab;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();

            ItemDef.pickupModelPrefab.transform.localScale = new Vector3(6f, 6f, 6f);
        }

        private void CreateConfig(ConfigFile config)
        {
            baseStackHPPercent = config.ActiveBind<float>("Item: " + ItemName, "Base Shield Gained with 1 Salvaged Wires", .04f, "How much shield as a % of max HP should you gain with a single salvaged wires? (.04 = 4%)");
            addStackHPPercent = config.ActiveBind<float>("Item: " + ItemName, "Additional Shield Gained per Salvaged Wires", .02f, "How much additional shield as a % of max HP should each salvaged wires after the first give?");
            baseAttackSpeedPercent = config.ActiveBind<float>("Item: " + ItemName, "Base Attack Speed Gained with 1 Salvaged Wires", .1f, "How much attack speed should you gain with a single salvaged wires? (.1 = 10%)");
            addAttackSpeedPercent = config.ActiveBind<float>("Item: " + ItemName, "Additional Attack Speed Gained per Salvaged Wires", .1f, "How much additional attack speed should each salvaged wires after the first give?");
        }
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0.5f, 0.5f, 0.5f);

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
                    childName = "ThighR",
                    localPos = new Vector3(0.025f, 0f, 0f),
                    localAngles = new Vector3(350f, 20f, 50f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0f, 0.15f, 0.05f),
                    localAngles = new Vector3(350f, 20f, 50f),
                    localScale = generalScale * .8f
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.00265F, -0.03657F, 0.0143F),
                    localAngles = new Vector3(359.48F, 43.11174F, 50.39743F),
                    localScale = new Vector3(0.31502F, 0.31502F, 0.31502F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmL",
                    localPos = new Vector3(0.5f, 1f, 0f),
                    localAngles = new Vector3(350f, 20f, 50f),
                    localScale = generalScale * 8f
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.10506F, 0.11393F, 0.11157F),
                    localAngles = new Vector3(279.1171F, 264.1212F, 166.8357F),
                    localScale = new Vector3(0.35019F, 0.35019F, 0.35019F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.02f, 0.1f, 0.1f),
                    localAngles = new Vector3(-10f, 20f, 50f),
                    localScale = generalScale * .8f
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(.7f, -0.3f, -0.1f),
                    localAngles = new Vector3(0f, 30f, 350f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.005f, 0.1f, 0.05f),
                    localAngles = new Vector3(-10f, 20f, 50f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-.5f, -.5f, 0f),
                    localAngles = new Vector3(350f, 10f, 50f),
                    localScale = new Vector3(3f, 3f, 3f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0f, 0.2f, 0f),
                    localAngles = new Vector3(-10f, 20f, 50f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Backpack",
                    localPos = new Vector3(0.34321F, -0.4299F, 0.01623F),
                    localAngles = new Vector3(5.64815F, 213.8979F, 332.4933F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.00666F, 0.11757F, -0.24366F),
                    localAngles = new Vector3(322.6534F, 103.1517F, 57.29504F),
                    localScale = new Vector3(0.34589F, 0.34589F, 0.34589F)
                }
            });

            //MODDED CHARACTER IDRs START HERE

            rules.Add("mdlNemCommando", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.31494F, -0.18761F, -0.17258F),
                    localAngles = new Vector3(335.493F, 232.5327F, 67.74959F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlHANDOverclocked", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.06073F, 0.32593F, 1.61903F),
                    localAngles = new Vector3(40.32495F, 358.8392F, 61.77407F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Shield",
                    localPos = new Vector3(-0.2206F, 0.42782F, -0.2573F),
                    localAngles = new Vector3(319.5918F, 236.7235F, 59.01326F),
                    localScale = new Vector3(0.34147F, 0.34147F, 0.34147F)
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
                    childName = "Stomach",
                    localPos = new Vector3(0.30973F, 0.17743F, 0.27637F),
                    localAngles = new Vector3(296.5848F, 92.26048F, 296.3256F),
                    localScale = new Vector3(0.25259F, 0.25259F, 0.25259F)
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
                    localPos = new Vector3(0.14386F, 0.02673F, 0.0927F),
                    localAngles = new Vector3(71.50155F, 34.82064F, 267.4682F),
                    localScale = new Vector3(0.23988F, 0.23988F, 0.23988F)
                }
            });
            rules.Add("mdlExecutioner2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00412F, 0.4909F, 0.06661F),
                    localAngles = new Vector3(11.91084F, 297.4003F, 161.7602F),
                    localScale = new Vector3(0.39985F, 0.39985F, 0.39985F)
                }
            });
            rules.Add("mdlHouse(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.07903F, 0.1611F, -0.02105F),
                    localAngles = new Vector3(356.6518F, 10.61094F, 276.0065F),
                    localScale = new Vector3(0.19345F, 0.32592F, 0.32592F)
                }
            });
            rules.Add("mdlTeslaTrooper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Gauntlet",
                    localPos = new Vector3(-0.01513F, 0.12734F, -0.07705F),
                    localAngles = new Vector3(34.52929F, 225.0661F, 234.0565F),
                    localScale = new Vector3(0.36985F, 0.36985F, 0.36985F)
                }
            });
            rules.Add("mdlDesolator", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "RadCannonItems",
                    localPos = new Vector3(-0.01233F, -0.94931F, 0.03734F),
                    localAngles = new Vector3(349.4721F, 32.35767F, 228.9647F),
                    localScale = new Vector3(0.33094F, 0.33094F, 0.33094F)
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
                    localPos = new Vector3(-0.07296F, -0.1664F, -0.10151F),
                    localAngles = new Vector3(352.1798F, 3.71516F, 11.09392F),
                    localScale = new Vector3(0.499F, 0.499F, 0.499F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.08808F, 0.27131F, -0.0954F),
                    localAngles = new Vector3(340.7634F, 353.4143F, 58.50154F),
                    localScale = new Vector3(0.29697F, 0.29697F, 0.29697F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            GetStatCoefficients += AttackSpeedBonus;
            GetStatCoefficients += AddMaxShield;
        }

        private void AddMaxShield(CharacterBody sender, StatHookEventArgs args)
        {
            var inventoryCount = GetCount(sender);
            if (inventoryCount > 0)
            {
                ItemHelpers.AddMaxShieldHelper(sender, args, inventoryCount, baseStackHPPercent, addStackHPPercent);
            }
        }
        private void AttackSpeedBonus(CharacterBody sender, StatHookEventArgs args)
        {
            var inventoryCount = GetCount(sender);
            if (sender.healthComponent.shield != 0 && inventoryCount > 0)
            {
                    args.attackSpeedMultAdd += (baseAttackSpeedPercent + (addAttackSpeedPercent * (inventoryCount - 1)));
            }
        }
    }
}