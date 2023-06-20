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

        public override string ItemLore => "\"This necktie was a staple accessory of one of a notorious group of well-dressed heisters " +
            "which were active during the early 21st century. The gang was wildly successful while active, breaking into, looting, " +
            "and escaping from some of the most secure sites on Earth at the time. Even when authorities attempted to apprehend the criminals, " +
            "reports state that shooting at them 'only seem to make [the heisters] move faster, however the hell that works.'\n" +
            "While the identities of these criminals were never discovered, the gang ceased operations for unknown reasons after over a decade of activity. " +
            "This piece serves as a testament to their dedication to style, no matter the situation.\"\n\n" +
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
                    childName = "ThighL",
                    localPos = new Vector3(0.01712F, 0.45999F, 0.01018F),
                    localAngles = new Vector3(69.3357F, 140.0085F, 160.7046F),
                    localScale = new Vector3(0.61816F, 0.61816F, 0.61816F)
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
            rules.Add("mdlHeretic", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.84235F, -0.09626F, -0.07357F),
                    localAngles = new Vector3(351.9045F, 93.81793F, 95.74849F),
                    localScale = new Vector3(1F, 1F, 1F)
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
            return rules;
        }
        public override void Hooks()
        {

            //For some reason if this isn't here the game absolutely freaks out and throws a ton of errors stating the object is null.
            //I seriously have no idea why the hell this is the case. DO NOT TOUCH!
            //itemDef.pickupModelPrefab.transform.localScale = new Vector3(3f, 3f, 3f);

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