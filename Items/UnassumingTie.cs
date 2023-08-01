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
    public class UnassumingTie : ItemBase<UnassumingTie>
    {
        //Config Stuff

        public static ConfigOption<float> baseStackHPPercent;
        public static ConfigOption<float> addStackHPPercent;
        public static ConfigOption<float> windedDebuffDuration;
        public static ConfigOption<float> secondWindBaseDuration;
        public static ConfigOption<float> secondWindBonusMultiplier;
        public static ConfigOption<float> secondWindBaseSpeedPercent;
        public static ConfigOption<float> secondWindAddSpeedPercent;

        //Item Data

        public override string ItemName => "Unassuming Tie";

        public override string ItemLangTokenName => "UNASSUMING_TIE";

        public override string ItemPickupDesc => "Gain some <style=cIsUtility>shield</style>, and receive a <style=cIsUtility>speed boost</style> when your <style=cIsUtility>shield</style> is broken.";

        public override string ItemFullDescription => $"Gain a <style=cIsUtility>shield</style> equal to <style=cIsUtility>{FloatToPercentageString(baseStackHPPercent)}</style>" +
            $" <style=cStack>(+{FloatToPercentageString(addStackHPPercent)} per stack)</style> of your maximum health. Breaking your <style=cIsUtility>shield</style> gives you a" +
            $" <style=cIsUtility>Second Wind</style> for {secondWindBaseDuration}s, plus a bonus amount based on your <style=cIsUtility>maximum shield</style>. " +
            $"Second Wind increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>{FloatToPercentageString(secondWindBaseSpeedPercent)}</style> <style=cStack>(+{FloatToPercentageString(secondWindAddSpeedPercent)} per stack)</style>.";

        public override string ItemLore => "\"This necktie was a staple accessory of a member of a notorious group of well-dressed heisters " +
            "that operated during the early 21st century. The gang was wildly successful, breaking into, looting, " +
            "and escaping from some of the most secure sites on Earth at the time. Even when authorities attempted to apprehend the criminals, " +
            "reports state that shooting at them 'only seem to make [the heisters] move faster, however the hell that works.'\n" +
            "While the identities of these criminals were never discovered, the gang suddenly ceased operations for unknown reasons after over a decade of activity. " +
            "\nThis piece serves as a testament to their dedication to style, no matter the situation.\"\n\n" +
            "- <i>Placard description for \"Striped Tie\" at the Galactic Museum of Law Enforcement and Criminality</i>";

        public override ItemTier Tier => ItemTier.Tier1;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };


        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Tie.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("TieIcon");


        public static GameObject ItemBodyModelPrefab;

        public BuffDef SecondWindBuff { get; private set; }
        public BuffDef WindedDebuff { get; private set; }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();
            CreateItem();
            Hooks();

            ItemDef.pickupModelPrefab.transform.localScale = new Vector3(3f, 3f, 3f);
        }
        private void CreateConfig(ConfigFile config)
        {
            baseStackHPPercent = config.ActiveBind<float>("Item: " + ItemName, "Base Shield Gained with 1 Unassuming Tie", .04f, "How much shield as a % of max HP should you gain with a single unassuming tie? (.04 = 4%)");
            addStackHPPercent = config.ActiveBind<float>("Item: " + ItemName, "Additional Shield Gained per Unassuming Tie", .02f, "How much additional shield as a % of max HP should each unassuming tie after the first give?");
            windedDebuffDuration = config.ActiveBind<float>("Item: " + ItemName, "Duration of the Winded Debuff", 10f, "How long should the Winded debuff last for, in seconds?");
            secondWindBaseDuration = config.ActiveBind<float>("Item: " + ItemName, "Base duration of the Second Wind Buff", 5f, "How long should the Second Wind Buff last for at base, in seconds?");
            secondWindBonusMultiplier = config.ActiveBind<float>("Item: " + ItemName, "Value for bonus duration of the Second Wind Buff", 5f, "What should the value that is multiplied by your shield/HP ratio to find the bonus duration of the Second Wind buff be, in seconds?");
            secondWindBaseSpeedPercent = config.ActiveBind<float>("Item: " + ItemName, "Base Movement Speed Gained with 1 Unassuming Tie", .15f, "How much movement speed should you gain with a single unassuming tie? (.1 = 15%)");
            secondWindAddSpeedPercent = config.ActiveBind<float>("Item: " + ItemName, "Additional Movement Speed Gained per 1 Unassuming Tie", .1f, "How much additional movement speed should each unassuming tie after the first give?");
        }
        private void CreateBuff()
        {
            SecondWindBuff = ScriptableObject.CreateInstance<BuffDef>();
            SecondWindBuff.name = "SupplyDrop Tie Speed Buff";
            SecondWindBuff.canStack = false;
            SecondWindBuff.isDebuff = false;
            SecondWindBuff.iconSprite = MainAssets.LoadAsset<Sprite>("SecondWindBuffIcon");

            ContentAddition.AddBuffDef(SecondWindBuff);

            WindedDebuff = ScriptableObject.CreateInstance<BuffDef>();
            WindedDebuff.name = "SupplyDrop Tie Cooldown Debuff";
            WindedDebuff.canStack = false;
            WindedDebuff.isDebuff = true;
            WindedDebuff.iconSprite = MainAssets.LoadAsset<Sprite>("WindedDebuffIcon.png");

            ContentAddition.AddBuffDef(WindedDebuff);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(.2f, .2f, .2f);

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
                    localPos = new Vector3(-0.04f, 0.26f, 0.22f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale * 0.9f
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.1f, 0.25f, 0.17f),
                    localAngles = new Vector3(0f, -20f, 0f),
                    localScale = generalScale * 0.9f
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.04262F, 0.33085F, 0.14558F),
                    localAngles = new Vector3(349.3234F, 4.55339F, 356.476F),
                    localScale = new Vector3(0.16394F, 0.16394F, 0.16394F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.9f, 3.3f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1.5f, 1.5f, 1.5f)
        }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.05f, 0.25f, 0.27f),
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
                    localPos = new Vector3(-0.05f, 0.12f, 0.16f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.05f, 0.25f, 0.20f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale * 0.9f
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(0.2f, 0.9f, 0f),
                    localAngles = new Vector3(270f, 0f, 0f),
                    localScale = generalScale * 1.5f
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.05f, 0.25f, 0.29f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.3f, 2f, -2.5f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(1.5f, 1.5f, 1.5f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.05f, 0.25f, 0.2f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.10836F, 0.1882F, 0.10487F),
                    localAngles = new Vector3(332.8644F, 307.084F, 8.23414F),
                    localScale = new Vector3(0.0951F, 0.0951F, 0.0951F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.031F, 0.13398F, 0.18136F),
                    localAngles = new Vector3(338.976F, 356.0499F, 0.01298F),
                    localScale = new Vector3(0.1478F, 0.1478F, 0.1478F)
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
                    localPos = new Vector3(-0.19576F, 1.04528F, 1.06145F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.75722F, 0.75722F, 0.75722F)
                }
            });
            rules.Add("mdlHANDOverclocked", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.19347F, 0.61577F, 1.68818F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.25983F, 0.30917F, -0.02484F),
                    localAngles = new Vector3(343.1456F, 273.5997F, 0.5956F),
                    localScale = new Vector3(0.20149F, 0.20149F, 0.20149F)
                }
            });
            rules.Add("mdlNemforcer(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.25983F, 0.30917F, -0.02484F),
                    localAngles = new Vector3(343.1456F, 273.5997F, 0.5956F),
                    localScale = new Vector3(0.20149F, 0.20149F, 0.20149F)
                }
            });
            rules.Add("mdlPaladin", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.00279F, 0.0983F, 0.46931F),
                    localAngles = new Vector3(2.38497F, 20.40325F, 357.4077F),
                    localScale = new Vector3(0.21353F, 0.21353F, 0.21353F)
                }
            });
            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.04102F, 0.22901F, 0.13424F),
                    localAngles = new Vector3(351.8255F, 341.0915F, 353.5952F),
                    localScale = new Vector3(0.16063F, 0.16063F, 0.16063F)
                }
            });
            rules.Add("mdlPathfinder", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.02549F, 0.28877F, 0.15474F),
                    localAngles = new Vector3(346.1422F, 359.6971F, 11.65047F),
                    localScale = new Vector3(0.14055F, 0.14055F, 0.14055F)
                }
            });
            rules.Add("mdlExecutioner2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.04102F, 0.22901F, 0.13424F),
                    localAngles = new Vector3(351.8255F, 341.0915F, 353.5952F),
                    localScale = new Vector3(0.16063F, 0.16063F, 0.16063F)
                }
            });
            rules.Add("mdlHouse(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.0079F, 0.03641F, 0.08847F),
                    localAngles = new Vector3(347.5305F, 7.17441F, 8.35055F),
                    localScale = new Vector3(0.15692F, 0.15692F, 0.15692F)
                }
            });
            rules.Add("mdlTeslaTrooper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.02868F, 0.16961F, 0.39955F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.15301F, 0.15301F, 0.15301F)
                }
            });
            rules.Add("mdlDesolator", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.03567F, 0.26552F, 0.44425F),
                    localAngles = new Vector3(354.8311F, 0.02335F, 359.9561F),
                    localScale = new Vector3(0.16493F, 0.16493F, 0.16493F)
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
                    localPos = new Vector3(0.00847F, 0.29498F, 0.12152F),
                    localAngles = new Vector3(344.4007F, 1.99147F, 4.91642F),
                    localScale = new Vector3(0.15405F, 0.15405F, 0.15405F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.20855F, 0.26122F, 0.10612F),
                    localAngles = new Vector3(341.0337F, 64.80407F, 357.7422F),
                    localScale = new Vector3(0.16303F, 0.16303F, 0.16303F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += CalculateBuff;
            GetStatCoefficients += AddMaxShield;
            GetStatCoefficients += AddSecondWindBuff;
            On.RoR2.CharacterBody.RemoveBuff_BuffIndex += AddWindedDebuff;
        }

        private void AddMaxShield(CharacterBody sender, StatHookEventArgs args)
        {
            var inventoryCount = GetCount(sender);
            if (inventoryCount > 0)
            {
                ItemHelpers.AddMaxShieldHelper(sender, args, inventoryCount, baseStackHPPercent, addStackHPPercent);
            }
        }
        private void AddWindedDebuff(On.RoR2.CharacterBody.orig_RemoveBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            orig(self, buffType);
            if (buffType == SecondWindBuff.buffIndex)
            {
                self.AddTimedBuffAuthority(WindedDebuff.buffIndex, windedDebuffDuration);
            }             
        }
        private void CalculateBuff(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var preDamageShield = self.body.healthComponent.shield; 
            orig(self, damageInfo);
            var inventoryCount = GetCount(self.body);
            if (inventoryCount > 0)
            {
                float dmgTaken = damageInfo.damage;
                if (dmgTaken >= preDamageShield && preDamageShield > 0)
                {
                    if (self.body.GetBuffCount(SecondWindBuff) == 0 && self.body.GetBuffCount(WindedDebuff) == 0)
                    {
                        self.body.AddTimedBuffAuthority(SecondWindBuff.buffIndex, secondWindBaseDuration + (secondWindBonusMultiplier * (self.body.maxShield / self.body.maxHealth)));
                    }
                }
            }
        }
        private void AddSecondWindBuff(CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if (sender.HasBuff(SecondWindBuff))
            {
                args.moveSpeedMultAdd += (secondWindBaseSpeedPercent + ((InventoryCount-1) * secondWindAddSpeedPercent));

            }
        }
    }
}