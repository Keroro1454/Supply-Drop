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
        public static ConfigOption<float> addGoodStatPercent;
        public static ConfigOption<float> addBadStatPercent;

        //Item Data
        public override string ItemName => "Two-Sided Die";

        public override string ItemLangTokenName => "TWOSIDEDDIE";

        public override string ItemPickupDesc => "Using a shrine rolls two stats, one to <style=cUtility>buff</style>, one to <style=cDeath>nerf</style>.";

        public override string ItemFullDescription => $"Whenever you use a shrine, {baseNumStatsRolled} (+{addNumStatsRolled} per stack) stats are picked at random. " +
            $"One is <style=cUtility>buffed by {FloatToPercentageString(goodStatPercent)}</style>, the other is <style=cDeath>nerfed by {FloatToPercentageString(badStatPercent)}</style>.";

        public override string ItemLore => "The little newt had loved to play with their siblings. But ever since they left them, all that time ago, they had no one to play with.\n\n" +
            "So they devised new games, games that could be played all alone. The newt was still lonely, but the games helped distract them from the loneliness of their quiet little tide pool.\n\n" +
            "Eventually, the newt grew, and their simple playthings were replaced, as the newt learned to make new toys. Complex experiments, wonderful creations, bizarre specimens. Now long forgotten, " +
            "the discarded playthings drift through time and space, waiting for a new master to play with them.";

        public override ItemTier Tier => ItemTier.Lunar;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("TwoSidedDie.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("TwoSidedDieIcon");
        public static GameObject ItemBodyModelPrefab;

        public static Range[] goodRanges;
        public static Range[] badRanges;
        List<int> weights = new List<int>();

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
            SetupAttributes();

            ItemDef.pickupModelPrefab.transform.localScale = new Vector3(6f, 6f, 6f);
        }

        private void CreateConfig(ConfigFile config)
        {
            goodStatPercent = config.ActiveBind<float>("Item: " + ItemName, "Base Good Stat is Buffed with 1 Salvaged Wires", .50f, "How much should the one stat be buffed by with a single two-sided die? (.5 = 50%)");
            badStatPercent = config.ActiveBind<float>("Item: " + ItemName, "Base Bad Stat is Buffed with 1 Two-Sided Die", .25f, "How much should the one stat be nerfed by with a single two-sided die? (.25 = 25%)");
            baseNumStatsRolled = config.ActiveBind<int>("Item: " + ItemName, "Base Attack Speed Gained with 1 Salvaged Wires", 2, "How many stats should be rolled with a single two-sided die?");
            addNumStatsRolled = config.ActiveBind<int>("Item: " + ItemName, "Additional Attack Speed Gained per Salvaged Wires", 2, "How many more stats should be rolled for each two-sided die after the first?");
            addGoodStatPercent = config.ActiveBind<float>("Item: " + ItemName, "Base Attack Speed Gained with 1 Salvaged Wires", .1f, "How much more should affected stats be buffed by after all stats are being affected? (.1 = 10%)");
            addBadStatPercent = config.ActiveBind<float>("Item: " + ItemName, "Additional Attack Speed Gained per Salvaged Wires", .05f, "How much more should affected stats be nerfed by after all stats are being affected? (.05 = 5%)");
        }
        public void SetupAttributes()
        {
            goodRanges = new Range[]
            {
                new Range(0, 9, "healthier"),
                new Range(10, 18, "stronger"),
                new Range(19, 27, "quicker"),
                new Range(28, 36, "richer"),
                new Range(37, 45, "wiser"),
                new Range(46, 54, "preciser"),
                new Range(55, 63, "livelier"),
                new Range(64, 72, "tougher"),
                new Range(73, 81, "lighter"),
                new Range(82, 90, "bigger"),
                new Range(91, 99, "luckier!"),
                new Range(100, int.MaxValue, "like a million bucks!!!")
            };
            badRanges = new Range[]
            {
                new Range(0, 9, "scrawnier"),
                new Range(10, 18, "weaker"),
                new Range(19, 27, "slower"),
                new Range(28, 36, "pooer"),
                new Range(37, 45, "dumber"),
                new Range(46, 54, "sloppier"),
                new Range(55, 63, "drowsier"),
                new Range(64, 72, "squishier"),
                new Range(73, 81, "heavier"),
                new Range(82, 90, "smaller"),
                new Range(91, 99, "unluckier..."),
                new Range(100, int.MaxValue, "totally worthless...")
            };

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
            //#9: Size
            weights.Add(6);
            //#10: Luck
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
            On.RoR2.PurchaseInteraction.OnInteractionBegin += ShrineInteraction;
            GetStatCoefficients += ApplyBuffDebuff;
        }
        public struct Range
        {
            public int Lower;
            public int Upper;
            public string EffectDesc;

            public Range(int lower, int upper, string effectdesc)
            {
                Lower = lower;
                Upper = upper;
                EffectDesc = effectdesc;
            }
            public bool Contains(int value)
            {
                return value >= Lower && value <= Upper;
            }
        }
        private void ShrineInteraction(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            CharacterBody body = activator.GetComponent<CharacterBody>();
            int inventoryCount = GetCount(body);
            if(inventoryCount > 0)
            {
                if (self.CanBeAffordedByInteractor(activator) && self.isShrine)
                {
                    var rollCheckComponent = activator.gameObject.GetComponent<StoredVariables>();
                    if (!rollCheckComponent)
                    {
                        rollCheckComponent = activator.gameObject.AddComponent<StoredVariables>();
                    }
                    rollCheckComponent.timeToRoll++;
                }
            }    
            orig(self, activator);
        }
        private void ApplyBuffDebuff(CharacterBody body, StatHookEventArgs args)
        {
            int inventoryCount = GetCount(body);
            if (inventoryCount > 0)
            {
                TwoSidedDie weightRandom = new TwoSidedDie();
                var chatSpamFilter = 0;

                var storedVariablesComponent = body.gameObject.GetComponent<StoredVariables>();
                if (!storedVariablesComponent)
                {
                    storedVariablesComponent = body.gameObject.AddComponent<StoredVariables>();
                }
                var rollsRemaining = storedVariablesComponent.timeToRoll;
                if (rollsRemaining > 0)
                {
                    for (rollsRemaining--; rollsRemaining >= 0; rollsRemaining--)
                    {
                        var goodRoll = weightRandom.WeightedRandom();
                        var badRoll = weightRandom.WeightedRandom();

                        //HP Roll (10%)
                        if (goodRoll == 0 || badRoll == 0)
                        {
                            if (goodRoll == 0)
                            {
                                args.healthMultAdd += goodStatPercent;
                                if (chatSpamFilter < 10)
                                {
                                    Chat.AddMessage("You feel healthier!");
                                    chatSpamFilter++;
                                }
                            }
                            else
                            {
                                args.healthMultAdd -= badStatPercent;
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
                                args.damageMultAdd += goodStatPercent;
                            }
                            else
                            {
                                args.damageMultAdd -= badStatPercent;
                            }
                        }
                        //Speed Roll (10%)
                        if (goodRoll == 2 || badRoll == 2)
                        {
                            if (goodRoll == 2)
                            {
                                args.moveSpeedMultAdd += goodStatPercent;
                            }
                            else
                            {
                                args.moveSpeedMultAdd -= badStatPercent;
                            }
                        }
                        //Gold Roll (10%)
                        if (goodRoll == 3 || badRoll == 3)
                        {
                            if (goodRoll == 3)
                            {
                                storedVariablesComponent.goldModifier++;
                            }
                            else
                            {
                                storedVariablesComponent.goldModifier--;
                            }
                        }
                        //XP Roll (10%)
                        if (goodRoll == 4 || badRoll == 4)
                        {
                            if (goodRoll == 4)
                            {
                                args.levelMultAdd += goodStatPercent;
                            }
                            else
                            {
                                args.levelMultAdd -= badStatPercent;
                            }
                        }
                        //Crit Roll (10%)
                        if (goodRoll == 5 || badRoll == 5)
                        {
                            if (goodRoll == 5)
                            {
                                args.critAdd += goodStatPercent;
                            }
                            else
                            {
                                args.critAdd -= badStatPercent;
                            }
                        }
                        //Cooldown Roll (10%)
                        if (goodRoll == 6 || badRoll == 6)
                        {
                            if (goodRoll == 6)
                            {
                                args.cooldownMultAdd += goodStatPercent;
                            }
                            else
                            {
                                args.cooldownMultAdd -= badStatPercent;
                            }
                        }
                        //Armor Roll (10%)
                        if (goodRoll == 7 || badRoll == 7)
                        {
                            if (goodRoll == 7)
                            {
                                args.armorAdd += body.baseArmor * goodStatPercent;
                            }
                            else
                            {
                                args.armorAdd -= body.baseArmor * badStatPercent;
                            }
                        }
                        //Jump Height Roll (10%)
                        if (goodRoll == 8 || badRoll == 8)
                        {
                            if (goodRoll == 8)
                            {
                                args.jumpPowerMultAdd += goodStatPercent;
                            }
                            else
                            {
                                args.jumpPowerMultAdd -= badStatPercent;
                            }
                        }
                        //Size Roll (6%)
                        if (goodRoll == 9 || badRoll == 9)
                        {
                            if (goodRoll == 9)
                            {
                                body.modelLocator.modelTransform.localScale *= (1 + goodStatPercent);
                            }
                            else
                            {
                                body.modelLocator.modelTransform.localScale *= Mathf.Min(1 - badStatPercent, 0.25f);
                            }
                        }
                        //Luck Roll (3%)
                        if (goodRoll == 10 || badRoll == 10)
                        {
                            if (goodRoll == 10)
                            {
                                storedVariablesComponent.luckModifier++;
                            }
                            else
                            {
                                storedVariablesComponent.luckModifier--;
                            }
                        }
                        //EVERYTHING Roll (1%)
                        if (goodRoll == 11 || badRoll == 11)
                        {
                            if (goodRoll == 11)
                            {
                                args.healthMultAdd += goodStatPercent;
                                args.damageMultAdd += goodStatPercent;
                                args.moveSpeedMultAdd += goodStatPercent;
                                storedVariablesComponent.goldModifier++;
                                args.levelMultAdd += goodStatPercent;
                                args.critAdd += goodStatPercent;
                                args.cooldownMultAdd += goodStatPercent;
                                args.armorAdd += body.baseArmor * goodStatPercent;
                                args.jumpPowerMultAdd += goodStatPercent;
                                body.modelLocator.modelTransform.localScale *= (1 + goodStatPercent);
                                storedVariablesComponent.luckModifier++;
                            }
                            else
                            {
                                args.healthMultAdd -= badStatPercent;
                                args.damageMultAdd -= badStatPercent;
                                args.moveSpeedMultAdd -= badStatPercent;
                                storedVariablesComponent.goldModifier--;
                                args.levelMultAdd -= badStatPercent;
                                args.critAdd -= badStatPercent;
                                args.cooldownMultAdd -= badStatPercent;
                                args.armorAdd -= body.baseArmor * badStatPercent;
                                args.jumpPowerMultAdd -= badStatPercent;
                                body.modelLocator.modelTransform.localScale *= Mathf.Min(1 - badStatPercent, 0.25f);
                                storedVariablesComponent.luckModifier--;
                            }
                        }
                    }
                }
            }
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
    public class StoredVariables : MonoBehaviour
    {
        public int timeToRoll = 0;
        public int goldModifier = 0;
        public int luckModifier = 0;
    }
}