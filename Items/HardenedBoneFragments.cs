using R2API;
using RoR2;
using UnityEngine;
using static R2API.RecalculateStatsAPI;

using SupplyDrop.Utils;
using static SupplyDrop.Utils.ItemHelpers;
using static K1454.SupplyDrop.SupplyDropPlugin;

using BepInEx.Configuration;

//TO-DO: Need to add proper display to MUL-T, Arti, Merc, REX, Acrid, Loader, Captain.
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
            "taking meat to eat but also the creatures' bones, which he began wittling to flat surfaces. \n\nA woman came over, shouldering her bow as she stopped and looked upon the gory scene before her." +
            "\n\n\"What are you doing?!\" \n\n\"What does it look like? I'm making myself some protection. Want some?\"\n\n\"Don't be ridiculous. What you're doing...that's just grotesque. " +
            "There's no way bits of bone will do anything other than make you look like some kind of monster.\"\n\n\"Good. I'll fit right in here then.\"" +
            "\n\nThe man stood back up, covered in scales. Not purple like those of the creatures he had just felled, but a brilliant white. He moved on from the scene, seeking to complete his suit of armor.";

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
                    childName = "ThighL",
                    localPos = new Vector3(-0.03735F, 0.54676F, 0.0454F),
                    localAngles = new Vector3(306.2225F, 318.3368F, 173.3996F),
                    localScale = new Vector3(0.6054F, 0.6054F, 0.6054F)

                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.01084F, 0.55936F, 0.06061F),
                    localAngles = new Vector3(291.7547F, 340.8314F, 168.5774F),
                    localScale = new Vector3(0.6F, 0.6F, 0.6F)
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
                    childName = "ThighR",
                    localPos = new Vector3(0.17532F, 2.54733F, -0.45084F),
                    localAngles = new Vector3(78.82626F, 126.9632F, 106.335F),
                    localScale = new Vector3(9F, 9F, 9F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.13279F, 0.28842F, 0.00125F),
                    localAngles = new Vector3(280.0143F, 30.39325F, 230.534F),
                    localScale = new Vector3(0.51086F, 0.51086F, 0.51086F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(-0.0143F, 0.61826F, 0.01113F),
                    localAngles = new Vector3(286.764F, 223.3905F, 221.3165F),
                    localScale = new Vector3(0.37667F, 0.37667F, 0.37667F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.13222F, 0.1635F, 0.03461F),
                    localAngles = new Vector3(312.0747F, 168.3848F, 76.5492F),
                    localScale = new Vector3(0.29737F, 0.29737F, 0.29737F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighBackL",
                    localPos = new Vector3(-0.43272F, 0.36977F, 0.32373F),
                    localAngles = new Vector3(35.88982F, 311.3215F, 257.164F),
                    localScale = new Vector3(0.84261F, 0.84261F, 0.84261F)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.23006F, 0.09333F, 0.21804F),
                    localAngles = new Vector3(85.5148F, 186.49F, 268.3089F),
                    localScale = new Vector3(0.14248F, 0.14248F, 0.14248F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "MouthMuzzle",
                    localPos = new Vector3(1.77634F, 1.31525F, 0.93121F),
                    localAngles = new Vector3(19.04379F, 139.5004F, 286.6347F),
                    localScale = new Vector3(4.59928F, 4.59928F, 4.59928F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(-0.01282F, 0.62907F, 0.02253F),
                    localAngles = new Vector3(293.617F, 339.6562F, 181.0212F),
                    localScale = new Vector3(0.84632F, 0.84632F, 0.84632F)
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
                    childName = "ThighL",
                    localPos = new Vector3(-0.00828F, 0.51371F, 0.08872F),
                    localAngles = new Vector3(299.1651F, 349.1716F, 77.77521F),
                    localScale = new Vector3(0.51642F, 0.51642F, 0.51642F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "UpperArmL",
                    localPos = new Vector3(-0.00519F, 0.3528F, 0.04089F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.51557F, 0.51557F, 0.51557F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            //pickupModelPrefab.transform.localScale = new Vector3(3f, 3f, 3f);

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
                args.armorAdd += (2f * sender.GetBuffCount(BFBuff)) + (1f * sender.GetBuffCount(BFBuff) * (InventoryCount - 1));
            }
        }
    }
}
