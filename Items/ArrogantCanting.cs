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

        public override string ItemFullDescription => $"Elite enemies have a {FloatToPercentageString(baseDropChance)} (+{FloatToPercentageString(addDropChance)} " +
            $"per stack) chance to <style=cIsUtility>drop a random item</style> on death, but they also gain {FloatToPercentageString(baseHPIncrease)} (+{FloatToPercentageString(addHPIncrease)} " +
            $"per stack) <style=cDeath>more HP</style> and {FloatToPercentageString(baseDamageIncrease)} (+{FloatToPercentageString(addDamageIncrease)} " +
            $"per stack) <style=cDeath>more damage</style>.";

        public override string ItemLore => "Do you remember, Brother, when I made a steed for myself?\n\n" +
            "Stone. Silver. Fire. All the ingredients to make a perfect creation, fast and able. But that was not enough this time. I wished to prove how right I had always been." +
            "How little value soul, that which you held so sacred and precious, truly had.\n\n" +
            "So I forged something new, superior to soul. Stronger, more vibrant. Obedient. Infused into my design, I was certain a mere glance at this perfect creature would be enough to convince you at last.\n\n" +
            "But when you looked upon the warhorse, you did not sing its praises. Do you remember what you did? YOU, who protested so greatly at the imprisonment of that abominable gold creature?\n\n" +
            "You did not hesitate to seal my steed away. You could not accept my success. My wisdom. MY RIGHTEOUSNESS.\n\n" +
            "I should have known then.";

        public override ItemTier Tier => ItemTier.Lunar;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("ArrogantCanting.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("ArrogantCantingIcon");
        public static GameObject ItemBodyModelPrefab;

        public static List<PickupIndex> redItems = null;
        public static List<PickupIndex> greenItems = null;
        public static List<PickupIndex> whiteItems = null;

        

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();

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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FootL",
                    localPos = new Vector3(0.03417F, 0.21444F, 0.00097F),
                    localAngles = new Vector3(30.92299F, 94.97478F, 358.8921F),
                    localScale = new Vector3(0.04332F, 0.04455F, 0.04455F)
                }
            });

            //MODDED CHARACTER IDRs START HERE

            rules.Add("mdlNemCommando", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    childName = "FootL",
                    localPos = new Vector3(0.03683F, 0.38324F, 0.00515F),
                    localAngles = new Vector3(337.9349F, 11.15429F, 356.3635F),
                    localScale = new Vector3(0.08552F, 0.08552F, 0.08552F)
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
                    followerPrefab = ItemBodyModelPrefab,
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
        }

        //This whole chunk handles the item dropping
        public void OnKilledOtherServer(DamageReport damageReport)
        {
            if (redItems is null)
            {
                redItems = Run.instance.availableTier3DropList;
            }
            if (greenItems is null)
            {
                greenItems = Run.instance.availableTier2DropList;
            }
            if (whiteItems is null)
            {
                whiteItems = Run.instance.availableTier1DropList;
            }    

            var victimBody = damageReport.victimBody;
            var dropLocation = damageReport.victimBody.transform.position;
            var cbKiller = damageReport.attackerBody.GetComponent<CharacterBody>();

            //Vector3 constant = (Vector3.up * 20f) + (5 * Vector3.right * Mathf.Cos(2f * Mathf.PI / Run.instance.participatingPlayerCount)) + (5 * Vector3.forward * Mathf.Sin(2f * Mathf.PI / Run.instance.participatingPlayerCount));

            if (cbKiller)
            {
                var inventoryCount = GetCount(cbKiller);
                if (inventoryCount > 0)
                {
                    if (victimBody.isElite && Util.CheckRoll(Mathf.Min((baseDropChance * 100) + ((addDropChance * 100) * (inventoryCount-1)),100),0))
                    {
                        var redItem = Util.CheckRoll(1 + ((baseDropChance * 100) * (inventoryCount - 1)), cbKiller.master.luck);
                        if (redItem)
                        {

                            SpawnItem(redItems, Run.instance.treasureRng.RangeInt(0, redItems.Count));
                            return;
                        }

                        var greenItem = Util.CheckRoll(19 + ((baseDropChance * 100) * (inventoryCount - 1)), cbKiller.master.luck);
                        if (greenItem)
                        {
                            SpawnItem(greenItems, Run.instance.treasureRng.RangeInt(0, greenItems.Count));
                            return;
                        }

                        else
                        {
                            SpawnItem(whiteItems, Run.instance.treasureRng.RangeInt(0, whiteItems.Count));
                            return;
                        }
                    }
                }
            }

            void SpawnItem(List<PickupIndex> items, int nextItem)
            {
                PickupDropletController.CreatePickupDroplet(items[nextItem], victimBody.transform.position, Vector3.zero);
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