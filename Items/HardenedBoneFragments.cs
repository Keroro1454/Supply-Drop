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
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.35f, 0.15f),
                    localAngles = new Vector3(-85f, 0f, 0f),
                    localScale = generalScale

                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.30f, 0.15f),
                    localAngles = new Vector3(-100f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 2.3f, 2f),
                    localAngles = new Vector3(90f, 0f, 0f),
                    localScale = generalScale * 6
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.05f, 0.15f, 0.15f),
                    localAngles = new Vector3(-75f, 0f, 0f),
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
                    localPos = new Vector3(0f, 0.15f, -0.05f),
                    localAngles = new Vector3(-22.5f, 0f, 0f),
                    localScale = generalScale
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.25f, -0.05f),
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
                    localPos = new Vector3(0f, 1.4f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale * 5
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.2f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
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
                    localPos = new Vector3(0f, 0f, 0.75f),
                    localAngles = new Vector3(115f, 0f, 0f),
                    localScale = generalScale * 8
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0.20f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
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
