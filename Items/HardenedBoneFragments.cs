using R2API;
using RoR2;
using UnityEngine;
using static R2API.RecalculateStatsAPI;

using SupplyDrop.Utils;
using static SupplyDrop.Utils.ItemHelpers;
using static K1454.SupplyDrop.SupplyDropPlugin;

using BepInEx.Configuration;

namespace SupplyDrop.Items
{
    public class HardenedBoneFragments : ItemBase<HardenedBoneFragments>
    {
        //Config Stuff

        public static ConfigOption<float> baseBonusArmor;
        public static ConfigOption<float> addBonusArmor;
        public static ConfigOption<float> healthPercentBuffLoss;

        //Item Data
        public override string ItemName => "Hardened Bone Fragments";

        public override string ItemLangTokenName => "BONERS";

        public override string ItemPickupDesc => "Gain <style=cIsUtility>temporary armor</style> on kill. Some <style=cIsUtility>armor</style> is lost when injured.";

        public override string ItemFullDescription => $"Gain <style=cIsUtility>{(baseBonusArmor)}</style> <style=cStack>(+{(addBonusArmor)} per stack)</style> " +
            $"<style=cIsUtility>armor</style> on kill. Some <style=cIsUtility>armor</style> is lost upon taking damage; higher damage loses more <style=cIsUtility>armor</style>.";

        public override string ItemLore => "The last attacker hissed its final breath before falling silent next to its brethren.\n\nThe man holstered his weapon, " +
            "wisps of smoke curling around the barrel, before falling to his knees with a sigh. Taking his knife from its hilt, he stabbed through the purple scales of his defeated foes, " +
            "taking meat to eat...but also the creatures' bones, which he began wittling to flat surfaces. \n\nThe woman came over, shouldering her bow as she stopped and looked upon the gory scene before her." +
            "\n\n\"What are you doing?!\" \n\n\"What does it look like? I'm making myself some protection. Want some?\"\n\n\"Don't be ridiculous. What you're doing...that's just grotesque. " +
            "There's no way bits of bone will do anything other than make you look like some kind of monster.\"\n\n" +
            "The man lifted his head and met her gaze." +
            "\n\n\"Good. I'll fit right in here then.\"" +
            "\n\nThe man stood up, now covered in scales. Not purple like those of the creatures he had just felled, but a brilliant white. He moved on from the scene, seeking to complete his suit of armor.";

        public override ItemTier Tier => ItemTier.Tier1;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };


        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Bone.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("BoneIcon");


        public static GameObject ItemBodyModelPrefab;

        //private static List<CharacterBody> Playername = new List<CharacterBody>();

        public BuffDef BFBuff { get; private set; }

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
            baseBonusArmor = config.ActiveBind<float>("Item: " + ItemName, "Base Armor Gained on Kill with 1 Hardened Bone Fragments", 5f, "How much armor should you gain on kill with a single hardened bone fragments?");
            addBonusArmor = config.ActiveBind<float>("Item: " + ItemName, "Additional Armor Gained on Kill per Hardened Bone Fragments", 1f, "How much additional armor gained on kill should each hardened bone fragments after the first give?");
            healthPercentBuffLoss = config.ActiveBind<float>("Item: " + ItemName, "Percent of Max HP to Lose 1 Armor Buff Stack", 2f, "How much % max HP lost should it take to lose 1 armor buff stack? This is in addition to a base 1 stack lost to any damage.");
        }
        private void CreateBuff()
        {
            BFBuff = ScriptableObject.CreateInstance<BuffDef>();
            BFBuff.name = "SupplyDrop BF Buff";
            BFBuff.canStack = true;
            BFBuff.isDebuff = false;
            BFBuff.iconSprite = MainAssets.LoadAsset<Sprite>("BoneBuffIcon");

            ContentAddition.AddBuffDef(BFBuff);
        }
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(1.5f, 1.5f, 1.5f);

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
                    localPos = new Vector3(0.0243F, 0.5429F, 0.0674F),
                    localAngles = new Vector3(301.7016F, 34.46665F, 36.47068F),
                    localScale = new Vector3(0.72454F, 0.72454F, 0.72454F)
                }
                        });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.0174F, 0.50503F, 0.09967F),
                    localAngles = new Vector3(348.8768F, 10.78889F, 52.3673F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.07275F, 0.56277F, 0.10193F),
                    localAngles = new Vector3(334.9013F, 331.8105F, 60.32729F),
                    localScale = new Vector3(0.48403F, 0.48403F, 0.48403F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmL",
                    localPos = new Vector3(0.57971F, 0.51282F, 1.09512F),
                    localAngles = new Vector3(350F, 20F, 50F),
                    localScale = new Vector3(12F, 12F, 12F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.02831F, 0.45521F, -0.04161F),
                    localAngles = new Vector3(357.433F, 165.2902F, 173.0274F),
                    localScale = new Vector3(0.71467F, 0.71467F, 0.71467F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.00856F, 0.61298F, 0.0389F),
                    localAngles = new Vector3(300.3116F, 346.9906F, 34.60462F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.02315F, 0.6131F, 0.03351F),
                    localAngles = new Vector3(311.9789F, 355.788F, 16.59218F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
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
                    childName = "Pelvis",
                    localPos = new Vector3(-0.12794F, 0.1995F, 0.26054F),
                    localAngles = new Vector3(345.4112F, 351.1218F, 104.6227F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.0509F, 5.23473F, 0.16688F),
                    localAngles = new Vector3(294.3047F, 85.8226F, 337.6613F),
                    localScale = new Vector3(5F, 5F, 5F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.00769F, 0.63039F, 0.04345F),
                    localAngles = new Vector3(292.8366F, 10.82814F, 32.16016F),
                    localScale = new Vector3(0.7F, 0.7F, 0.7F)
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(-0.03258F, 0.44718F, -0.0641F),
                    localAngles = new Vector3(29.17717F, 213.7592F, 342.5879F),
                    localScale = new Vector3(0.7F, 0.7F, 0.7F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.07659F, 0.47772F, -0.01183F),
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
                    localPos = new Vector3(0.24418F, -0.19231F, -0.14632F),
                    localAngles = new Vector3(335.493F, 232.5327F, 67.74959F),
                    localScale = new Vector3(3F, 3F, 3F)
                }
            });
            rules.Add("mdlHANDOverclocked", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.08707F, 0.15233F, 0.36341F),
                    localAngles = new Vector3(52.07069F, 165.9054F, 22.52827F),
                    localScale = new Vector3(3F, 3F, 3F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.0466F, 0.43139F, 0.02906F),
                    localAngles = new Vector3(37.87798F, 281.7024F, 297.9764F),
                    localScale = new Vector3(1F, 1F, 1F)
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
                    childName = "ThighR",
                    localPos = new Vector3(0.0102F, 0.83397F, -0.17573F),
                    localAngles = new Vector3(307.5371F, 180.9858F, 198.5795F),
                    localScale = new Vector3(1F, 1F, 1F)
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
                    childName = "ThighR",
                    localPos = new Vector3(0.00876F, 0.52629F, 0.05774F),
                    localAngles = new Vector3(304.9744F, 33.53241F, 249.3741F),
                    localScale = new Vector3(0.7F, 0.7F, 0.7F)
                }
            });
            rules.Add("mdlExecutioner2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.00492F, 0.44458F, 0.09764F),
                    localAngles = new Vector3(12.61741F, 3.16437F, 155.0273F),
                    localScale = new Vector3(0.39985F, 0.39985F, 0.39985F)
                }
            });
            rules.Add("mdlHouse(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "RightThigh",
                    localPos = new Vector3(0.04247F, 0.62657F, -0.1503F),
                    localAngles = new Vector3(353.0175F, 160.7006F, 77.43121F),
                    localScale = new Vector3(0.7F, 0.7F, 0.7F)
                }
            });
            rules.Add("mdlTeslaTrooper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.05833F, 0.55218F, -0.15502F),
                    localAngles = new Vector3(346.1962F, 195.8331F, 251.4224F),
                    localScale = new Vector3(0.6F, 0.6F, 0.6F)
                }
            });
            rules.Add("mdlDesolator", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(0.01952F, 0.56944F, 0.08699F),
                    localAngles = new Vector3(317.0453F, 154.5583F, 60.96878F),
                    localScale = new Vector3(0.6F, 0.6F, 0.6F)
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
                    childName = "UpperLegR",
                    localPos = new Vector3(0.01846F, 0.45619F, -0.10884F),
                    localAngles = new Vector3(348.8529F, 156.5286F, 179.2693F),
                    localScale = new Vector3(0.499F, 0.499F, 0.499F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "thigh.L",
                    localPos = new Vector3(-0.13063F, 0.55376F, 0.0071F),
                    localAngles = new Vector3(334.1169F, 277.0669F, 99.66897F),
                    localScale = new Vector3(0.5F, 0.5F, 0.5F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += CalculateBFBuffGain;
            On.RoR2.HealthComponent.TakeDamage += BFBuffLoss;

            GetStatCoefficients += AddBFBuff;
        }

        private void CalculateBFBuffGain(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            if(damageReport.attackerBody)
            {
                var inventoryCount = GetCount(damageReport.attackerBody);
                if(inventoryCount > 0)
                {
                    damageReport.attackerBody.AddBuff(BFBuff);
                }
            }
            orig(self, damageReport);
        }
        private void BFBuffLoss(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            float buffsToLose = Mathf.Min((Mathf.RoundToInt((damageInfo.damage / self.body.maxHealth) / healthPercentBuffLoss) * 100) + 1, self.body.GetBuffCount(BFBuff));
            while (buffsToLose > 0)
            {
                self.body.RemoveBuff(BFBuff);
                buffsToLose -= 1;
            }
            orig(self, damageInfo);
        }
        private void AddBFBuff(CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if (sender.HasBuff(BFBuff))
            {
                args.armorAdd += (baseBonusArmor * sender.GetBuffCount(BFBuff)) + (addBonusArmor * sender.GetBuffCount(BFBuff) * (InventoryCount - 1));
            }
        }
    }
}
