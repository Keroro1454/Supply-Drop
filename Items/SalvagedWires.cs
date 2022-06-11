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
            "\n\n\"Rule 1: Batteries on, or you're gone.\"" +
            "\n\nThe derelict frigate drifted slowly, almost imperceptibly. If the Safekeepings had found itself abandoned in an atmosphere, " +
            "Alyk would have heard the sounds he was all too accustomed to. The aged guts of the ship creaking, the groaning of rusted hyperdrive intakes, the crackle of errant wisps of energy as they finally escaped the old wiring." +
            "\n\n\"Rule 2: O2 below 20, leave it be.\"" +
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
                    localPos = new Vector3(-0.07f, 0.1f, 0.1f),
                    localAngles = new Vector3(330f, 50f, 0f),
                    localScale = generalScale
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
            rules.Add("mdlBandit2", new ItemDisplayRule[]
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
            return rules;
        }
        public override void Hooks()
        {
            //pickupModelPrefab.transform.localScale = new Vector3(1f, 1f, 1f) * 6f;

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