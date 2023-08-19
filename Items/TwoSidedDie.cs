using R2API;
using RoR2;
using UnityEngine;
using static R2API.RecalculateStatsAPI;

using SupplyDrop.Utils;
using static SupplyDrop.Utils.ItemHelpers;
using static SupplyDrop.Utils.MathHelpers;
using static K1454.SupplyDrop.SupplyDropPlugin;

using BepInEx.Configuration;
using System.Collections.Generic;

namespace SupplyDrop.Items
{
    public class TwoSidedDie : ItemBase<TwoSidedDie>
    {
        //Config Stuff

        public static ConfigOption<float> goodStatPercent;
        public static ConfigOption<float> badStatPercent;
        public static ConfigOption<int> baseNumStatsRolled;
        public static ConfigOption<int> addNumStatsRolled;

        //Item Data
        public override string ItemName => "Two-Sided Die";

        public override string ItemLangTokenName => "TWOSIDEDDIE";

        public override string ItemPickupDesc => $"Using a shrine rolls {baseNumStatsRolled} stats; {baseNumStatsRolled / 2} to <style=cIsUtility>buff</style>, {baseNumStatsRolled / 2} to <style=cDeath>nerf</style>.";

        public override string ItemFullDescription => $"Whenever you use a shrine, {baseNumStatsRolled} (+{addNumStatsRolled} per stack) stats are picked at random. " +
            $"One is <style=cIsUtility>buffed by {FloatToPercentageString(goodStatPercent)}</style>, the other is <style=cDeath>nerfed by {FloatToPercentageString(badStatPercent)}</style>.";

        public override string ItemLore => "The little newt had loved to play with their siblings. But ever since they left them, all that time ago, they had no one to play with.\n\n" +
            "So they devised new games, games that could be played all alone. The newt was still lonely, but the games helped distract them from the loneliness of their quiet little tide pool.\n\n" +
            "Eventually, the newt grew, and their simple playthings were replaced, as the newt learned to make new toys. Complex experiments, wonderful creations, bizarre specimens. Now long forgotten, " +
            "the discarded playthings drift through time and space, waiting for a new master to play with them.";

        public override ItemTier Tier => ItemTier.Lunar;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("TwoSidedDie.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("TwoSidedDieIcon");
        public static GameObject ItemBodyModelPrefab;

        List<int> weights = new List<int>();

        public int timeToRoll = 0;
        public int goldModifier = 0;
        public int goodGoldRoll = 0;
        public int badGoldRoll = 0;
        public int luckModifier = 0;
        public int goodLuckRoll = 0;
        public int badLuckRoll = 0;

        public int normalSizeCounter = 0;
        Vector3 bodySizeReset;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
            SetupAttributes();

            ItemDef.pickupModelPrefab.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);
        }

        private void CreateConfig(ConfigFile config)
        {
            goodStatPercent = config.ActiveBind<float>("Item: " + ItemName, "Base Good Stat is Buffed By with 1 1 Two-Sided Die", .50f, "How much should the one stat be buffed by with a single two-sided die? (.5 = 50%)");
            badStatPercent = config.ActiveBind<float>("Item: " + ItemName, "Base Bad Stat is Nerfed By with 1 Two-Sided Die", .25f, "How much should the one stat be nerfed by with a single two-sided die? (.25 = 25%)");
            baseNumStatsRolled = config.ActiveBind<int>("Item: " + ItemName, "Base Number of Stats Rolled with 1 Two-Sided Die", 2, "How many stats should be rolled with a single two-sided die?");
            addNumStatsRolled = config.ActiveBind<int>("Item: " + ItemName, "Additional Number of Stats Rolled per Two-Sided Die", 2, "How many more stats should be rolled for each two-sided die after the first?");
        }
        public void SetupAttributes()
        {
            //#0: HP
            weights.Add(10);
            //#1: Damage
            weights.Add(10);
            //#2: Speed
            weights.Add(10);
            //#3: Gold
            weights.Add(10);
            //#4: XP
            weights.Add(10);
            //#5: Crit
            weights.Add(10);
            //#6: Cooldowns
            weights.Add(10);
            //#7: Armor
            weights.Add(10);
            //#8: Jump
            weights.Add(10);
            //#9: Luck
            weights.Add(6);
            //#10: Size
            weights.Add(3);
            //#11: EVERYTHING!
            weights.Add(1);
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
                    childName = "Pelvis",
                    localPos = new Vector3(-0.08154F, -0.08107F, 0.13188F),
                    localAngles = new Vector3(49.34736F, 154.0507F, 140.8065F),
                    localScale = new Vector3(0.00593F, 0.00593F, 0.00593F)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.00208F, -0.1081F, 0.11237F),
                    localAngles = new Vector3(350F, 20F, 50F),
                    localScale = new Vector3(0.00669F, 0.00669F, 0.00669F)
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.08973F, -0.07531F, 0.14956F),
                    localAngles = new Vector3(333.7748F, 85.98743F, 25.71445F),
                    localScale = new Vector3(0.00806F, 0.00806F, 0.00806F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hip",
                    localPos = new Vector3(-0.80082F, -0.52007F, -0.04797F),
                    localAngles = new Vector3(85.90327F, 81.63601F, 85.18798F),
                    localScale = new Vector3(0.0776F, 0.0776F, 0.0776F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.13648F, 0.0702F, 0.19105F),
                    localAngles = new Vector3(279.1171F, 264.1212F, 166.8357F),
                    localScale = new Vector3(0.01234F, 0.01234F, 0.01234F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.11043F, 0.13608F, 0.00734F),
                    localAngles = new Vector3(350F, 20F, 50F),
                    localScale = new Vector3(0.01916F, 0.01916F, 0.01916F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.07934F, 0.03535F, 0.09635F),
                    localAngles = new Vector3(281.6085F, 180.0001F, 179.9999F),
                    localScale = new Vector3(0.00779F, 0.00779F, 0.00779F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0.4554F, -0.42787F, -0.1F),
                    localAngles = new Vector3(0F, 30F, 350F),
                    localScale = new Vector3(0.02442F, 0.02442F, 0.02442F)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.119F, -0.01527F, 0.13382F),
                    localAngles = new Vector3(350F, 20F, 50F),
                    localScale = new Vector3(0.00988F, 0.00988F, 0.00988F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-1.50453F, -2.54864F, 6.0931F),
                    localAngles = new Vector3(2.02295F, 10.79687F, 51.8512F),
                    localScale = new Vector3(0.08457F, 0.08457F, 0.08457F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HandL",
                    localPos = new Vector3(0.11601F, 0.3246F, -0.00091F),
                    localAngles = new Vector3(0.54158F, 1.49096F, 10.94836F),
                    localScale = new Vector3(0.00703F, 0.00703F, 0.00703F)
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "GunBarrel",
                    localPos = new Vector3(0.11666F, 0.32543F, -0.00506F),
                    localAngles = new Vector3(5.64815F, 213.8979F, 332.4933F),
                    localScale = new Vector3(0.00709F, 0.00709F, 0.00709F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.17704F, -0.12655F, 0.08999F),
                    localAngles = new Vector3(322.6534F, 103.1517F, 57.29504F),
                    localScale = new Vector3(0.00826F, 0.00826F, 0.00826F)
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
                    localPos = new Vector3(0.20891F, -0.04733F, 0.72026F),
                    localAngles = new Vector3(335.493F, 232.5327F, 67.74959F),
                    localScale = new Vector3(0.0348F, 0.0348F, 0.0348F)
                }
            });
            rules.Add("mdlHANDOverclocked", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.84508F, 0.61262F, -1.57745F),
                    localAngles = new Vector3(30.05503F, 5.90662F, 330.852F),
                    localScale = new Vector3(0.04223F, 0.04223F, 0.04223F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.20553F, 0.06428F, -0.15422F),
                    localAngles = new Vector3(319.5918F, 236.7235F, 59.01325F),
                    localScale = new Vector3(0.01397F, 0.01397F, 0.01397F)
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
                    childName = "Pelvis",
                    localPos = new Vector3(0.21501F, 0.09457F, -0.09146F),
                    localAngles = new Vector3(296.5848F, 92.26048F, 296.3256F),
                    localScale = new Vector3(0.00958F, 0.00958F, 0.00958F)
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
                    localPos = new Vector3(0.20314F, -0.04124F, -0.01563F),
                    localAngles = new Vector3(71.50156F, 34.82064F, 267.4682F),
                    localScale = new Vector3(0.00772F, 0.00772F, 0.00772F)
                }
            });
            rules.Add("mdlExecutioner2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.20641F, -0.03294F, 0.07756F),
                    localAngles = new Vector3(298.2086F, 14.25678F, 50.07993F),
                    localScale = new Vector3(0.00831F, 0.00831F, 0.00831F)
                }
            });
            rules.Add("mdlHouse(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.14874F, -0.11301F, 0.12039F),
                    localAngles = new Vector3(6.3079F, 22.66821F, 245.9716F),
                    localScale = new Vector3(0.00549F, 0.00549F, 0.00549F)
                }
            });
            rules.Add("mdlTeslaTrooper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.27567F, -0.24226F, 0.06472F),
                    localAngles = new Vector3(34.52929F, 225.0661F, 234.0565F),
                    localScale = new Vector3(0.00968F, 0.00968F, 0.00968F)
                }
            });
            rules.Add("mdlDesolator", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.12704F, 0.08498F, -0.27089F),
                    localAngles = new Vector3(349.4721F, 32.35767F, 228.9647F),
                    localScale = new Vector3(0.01286F, 0.01286F, 0.01286F)
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
                    localPos = new Vector3(-0.12722F, -0.15195F, -0.12722F),
                    localAngles = new Vector3(352.1798F, 3.71516F, 11.09392F),
                    localScale = new Vector3(0.00956F, 0.00956F, 0.00956F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.13969F, 0.30041F, 0.21507F),
                    localAngles = new Vector3(340.7634F, 353.4143F, 58.50154F),
                    localScale = new Vector3(0.01134F, 0.01134F, 0.01134F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            On.RoR2.PurchaseInteraction.OnInteractionBegin += ShrineInteraction;
            GetStatCoefficients += ApplyBuffDebuff;

            On.RoR2.DeathRewards.OnKilledServer += MoneyBonus;
            On.RoR2.Util.CheckRoll_float_float_CharacterMaster += LuckBonus;
        }
        private void ShrineInteraction(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            CharacterBody body = activator.GetComponent<CharacterBody>();
            int inventoryCount = GetCount(body);
            
            if(inventoryCount > 0)
            {
                if (self.CanBeAffordedByInteractor(activator) && self.isShrine)
                {
                    //This just stores the model's "correct" size so it can be reset later. Will only be accessed once.
                    if (normalSizeCounter == 0)
                    {
                        bodySizeReset = body.modelLocator.modelTransform.localScale;
                        normalSizeCounter++;
                    }

                    timeToRoll++;

                    //Resets modifiers that can't be reset within the ApplyBuffDebuff method
                    goldModifier = 0;
                    luckModifier = 0;
                    body.modelLocator.modelTransform.localScale = bodySizeReset;

                    //This forces the ApplyBuffDebuff method to trigger after shrine usage. Unsure if there's a more efficient way to do that.
                    body.RecalculateStats();
                }
            }    
            orig(self, activator);
        }
        private void ApplyBuffDebuff(CharacterBody body, StatHookEventArgs args)
        {
            int inventoryCount = GetCount(body);
            if (inventoryCount > 0)
            {
                var chatSpamFilter = 0;

                if (timeToRoll > 0)
                {
                    var goodHPRoll = 0;
                    var badHPRoll = 0;
                    var goodDamageRoll = 0;
                    var badDamageRoll = 0;
                    var goodSpeedRoll = 0;
                    var badSpeedRoll = 0;
                    var goodXPRoll = 0;
                    var badXPRoll = 0;
                    var goodCritRoll = 0;
                    var badCritRoll = 0;
                    var goodCDRoll = 0;
                    var badCDRoll = 0;
                    var goodArmorRoll = 0;
                    var badArmorRoll = 0;
                    var goodSizeRoll = 0;
                    var badSizeRoll = 0;
                    var goodJumpRoll = 0;
                    var badJumpRoll = 0;
                    var goodEveryRoll = 0;
                    var badEveryRoll = 0;
                    
                    while (timeToRoll > 0)
                    {
                        var goodRoll = WeightedRandom();
                        var badRoll = WeightedRandom();

                        //HP Roll (10%)
                        if (goodRoll == 0 || badRoll == 0)
                        {
                            if (goodRoll == 0)
                            {
                                goodHPRoll++;
                                args.healthMultAdd += (goodStatPercent * goodHPRoll);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel healthier!");
                                    chatSpamFilter++;
                                }
                            }
                            else
                            {
                                badHPRoll++;
                                args.healthMultAdd -= (badStatPercent * badHPRoll);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel scrawnier.");
                                    chatSpamFilter++;
                                }
                            }
                        }
                        //Damage Roll (10%)
                        if (goodRoll == 1 || badRoll == 1)
                        {
                            if (goodRoll == 1)
                            {
                                goodDamageRoll++;
                                args.damageMultAdd += (goodStatPercent * goodDamageRoll);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel stronger!");
                                    chatSpamFilter++;
                                }
                            }
                            else
                            {
                                badDamageRoll++;
                                args.damageMultAdd -= (badStatPercent * badDamageRoll);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel weaker.");
                                    chatSpamFilter++;
                                }
                            }
                        }
                        //Speed Roll (10%)
                        if (goodRoll == 2 || badRoll == 2)
                        {
                            if (goodRoll == 2)
                            {
                                goodSpeedRoll++;
                                args.moveSpeedMultAdd += (goodStatPercent * goodSpeedRoll);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel quicker!");
                                    chatSpamFilter++;
                                }
                            }
                            else
                            {
                                badSpeedRoll++;
                                args.moveSpeedMultAdd -= (badStatPercent * badSpeedRoll);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel slower.");
                                    chatSpamFilter++;
                                }
                            }
                        }
                        //Gold Roll (10%)
                        if (goodRoll == 3 || badRoll == 3)
                        {
                            if (goodRoll == 3)
                            {
                                goodGoldRoll++;
                                goldModifier++;
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel richer!");
                                    chatSpamFilter++;
                                }
                            }
                            else
                            {
                                badGoldRoll++;
                                goldModifier--;
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel poorer.");
                                    chatSpamFilter++;
                                }
                            }
                        }
                        //XP Roll (10%)
                        if (goodRoll == 4 || badRoll == 4)
                        {
                            if (goodRoll == 4)
                            {
                                goodXPRoll++;
                                args.levelMultAdd += (goodStatPercent * goodXPRoll);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel wiser!");
                                    chatSpamFilter++;
                                }
                            }
                            else
                            {
                                badXPRoll++;
                                args.levelMultAdd -= (badStatPercent * badXPRoll);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel dumber.");
                                    chatSpamFilter++;
                                }
                            }
                        }
                        //Crit Roll (10%)
                        if (goodRoll == 5 || badRoll == 5)
                        {
                            if (goodRoll == 5)
                            {
                                goodCritRoll++;
                                args.critAdd += (goodStatPercent * goodCritRoll);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel preciser!");
                                    chatSpamFilter++;
                                }
                            }
                            else
                            {
                                badCritRoll++;
                                args.critAdd -= (badStatPercent * badCritRoll);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel sloppier.");
                                    chatSpamFilter++;
                                }
                            }
                        }
                        //Cooldown Roll (10%)
                        if (goodRoll == 6 || badRoll == 6)
                        {
                            if (goodRoll == 6)
                            {
                                goodCDRoll++;
                                args.cooldownMultAdd += (goodStatPercent * goodCDRoll);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel livelier!");
                                    chatSpamFilter++;
                                }
                            }
                            else
                            {
                                badCDRoll++;
                                args.cooldownMultAdd -= (badStatPercent * badCDRoll);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel drowsier.");
                                    chatSpamFilter++;
                                }
                            }
                        }
                        //Armor Roll (10%)
                        if (goodRoll == 7 || badRoll == 7)
                        {
                            if (goodRoll == 7)
                            {
                                goodArmorRoll++;
                                args.armorAdd += Mathf.Max(body.baseArmor * (goodStatPercent * goodArmorRoll), 5);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel tougher!");
                                    chatSpamFilter++;
                                }
                            }
                            else
                            {
                                badArmorRoll++;
                                args.armorAdd -= Mathf.Max(body.baseArmor * (badStatPercent * badArmorRoll), 5);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel squishier.");
                                    chatSpamFilter++;
                                }
                            }
                        }
                        //Jump Height Roll (10%)
                        if (goodRoll == 8 || badRoll == 8)
                        {
                            if (goodRoll == 8)
                            {
                                goodJumpRoll++;
                                args.jumpPowerMultAdd += (goodStatPercent * goodJumpRoll * 2);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel lighter!");
                                    chatSpamFilter++;
                                }
                            }
                            else
                            {
                                badJumpRoll++;
                                args.jumpPowerMultAdd -= (badStatPercent * badJumpRoll * 2);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel heavier.");
                                    chatSpamFilter++;
                                }
                            }
                        }
                        //Luck Roll (6%)
                        if (goodRoll == 9 || badRoll == 9)
                        {
                            if (goodRoll == 9)
                            {
                                goodLuckRoll++;
                                luckModifier++;
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel LUCKIER!!");
                                    chatSpamFilter++;
                                }
                            }
                            else
                            {
                                badLuckRoll++;
                                luckModifier--;
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel unluckier...");
                                    chatSpamFilter++;
                                }
                            }
                        }
                        //Size Roll (3%)
                        if (goodRoll == 10 || badRoll == 10)
                        {
                            if (goodRoll == 10)
                            {
                                goodSizeRoll++;
                                body.modelLocator.modelTransform.localScale *= (1 + (goodStatPercent * goodSizeRoll));
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel BIGGER!");
                                    chatSpamFilter++;
                                }
                            }
                            else
                            {
                                badSizeRoll++;
                                body.modelLocator.modelTransform.localScale *= Mathf.Max(1 - (badStatPercent * badSizeRoll), 0.10f);
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel tiny...");
                                    chatSpamFilter++;
                                }
                            }
                        }
                        //EVERYTHING Roll (1%)
                        if (goodRoll == 11 || badRoll == 11)
                        {
                            if (goodRoll == 11)
                            {
                                goodEveryRoll++;
                                args.healthMultAdd += (goodStatPercent * goodEveryRoll);
                                args.damageMultAdd += (goodStatPercent * goodEveryRoll);
                                args.moveSpeedMultAdd += (goodStatPercent * goodEveryRoll);
                                goodGoldRoll++;
                                goldModifier++;
                                args.levelMultAdd += (goodStatPercent * goodEveryRoll);
                                args.critAdd += (goodStatPercent * goodEveryRoll);
                                args.cooldownMultAdd += (goodStatPercent * goodEveryRoll);
                                args.armorAdd += Mathf.Max(body.baseArmor * (goodStatPercent * goodEveryRoll), 5);
                                args.jumpPowerMultAdd += (goodStatPercent * goodEveryRoll * 2);
                                body.modelLocator.modelTransform.localScale *= (1 + (goodStatPercent * goodEveryRoll));
                                goodLuckRoll++;
                                luckModifier++;
                                Chat.AddMessage("YOU FEEL LIKE A MILLION BUCKS!");
                                chatSpamFilter++;
                            }
                            else
                            {
                                badEveryRoll++;
                                args.healthMultAdd -= (badStatPercent * badEveryRoll);
                                args.damageMultAdd -= (badStatPercent * badEveryRoll);
                                args.moveSpeedMultAdd -= (badStatPercent * badEveryRoll);
                                badGoldRoll++;
                                goldModifier--;
                                args.levelMultAdd -= (badStatPercent * badEveryRoll);
                                args.critAdd -= (badStatPercent * badEveryRoll);
                                args.cooldownMultAdd -= (badStatPercent * badEveryRoll);
                                args.armorAdd -= Mathf.Max(body.baseArmor * (badStatPercent * badEveryRoll), 5);
                                args.jumpPowerMultAdd -= (badStatPercent * badEveryRoll * 2);
                                body.modelLocator.modelTransform.localScale *= Mathf.Max(1 - (badStatPercent * badEveryRoll), 0.25f);
                                badLuckRoll++;
                                luckModifier--;
                                Chat.AddMessage("You feel utterly worthless...");
                                chatSpamFilter++;
                            }
                        }

                        //This won't let more than 10 chat messages be sent as a result of rolling. Maybe set this to be a configurable value? Don't know if that's worth doing...
                        if(chatSpamFilter > 10)
                        {
                            Chat.AddMessage("In summary: You feel like a confused mess right now.");
                        }

                        timeToRoll--;
                    }
                }
            }
        }
        private void MoneyBonus(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport rep)
        {
            orig(self, rep);

            var cbKiller = self.GetComponent<CharacterBody>();
            if (cbKiller)
            {
                var inventoryCount = GetCount(cbKiller);
                if (inventoryCount > 0)
                {
                    if (goldModifier != 0)
                    {
                        uint origGold = self.goldReward;

                        if (goldModifier > 0)
                        {
                            uint increasedGold = (uint)Mathf.FloorToInt(origGold * (goodStatPercent * goodGoldRoll));
                            self.goldReward = increasedGold;
                        }
                        else
                        {
                            uint reducedGold = (uint)Mathf.FloorToInt(origGold * (badStatPercent * badGoldRoll));
                            self.goldReward = reducedGold;
                        }
                    }
                }
            }
        }
        private bool LuckBonus(On.RoR2.Util.orig_CheckRoll_float_float_CharacterMaster orig, float percentChance, float luck, CharacterMaster effectOriginMaster)
        {
            if (percentChance >= 1f)
            {
                if (effectOriginMaster)
                {
                    var inventoryCount = GetCount(effectOriginMaster);
                    if (inventoryCount > 0)
                    {
                        if (luckModifier != 0)
                        {
                            var currentPercent = percentChance;
                            if (luckModifier > 0)
                            {
                                percentChance = currentPercent + (currentPercent * (goodStatPercent * goodLuckRoll));
                            }
                            else
                            {
                                percentChance = currentPercent - (currentPercent * (badStatPercent * badLuckRoll));
                            }
                        }
                    }
                }
            }
            return orig(percentChance, luck, effectOriginMaster);
        }
        int WeightedRandom()
        {
            float total = 0f;
            foreach (float weight in weights)
            {
                total += weight;
            }

            float max = weights[0],
            random = Random.Range(0f, total);

            for (int index = 0; index < weights.Count; index++)
            {
                if (random < max)
                {
                    return index;
                }
                else if (index == weights.Count - 1)
                {
                    return weights.Count - 1;
                }
                max += weights[index + 1];
            }
            return -1;
        }
    }
}