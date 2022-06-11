using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static R2API.RecalculateStatsAPI;
using SupplyDrop.Utils;
using static K1454.SupplyDrop.SupplyDropPlugin;

//TO-DO: Need to add proper display to MUL-T, Arti, Merc, REX, Acrid, Loader, Captain.
namespace SupplyDrop.Items
{
    public class HardenedBoneFragments : Item<HardenedBoneFragments>
    {
        //Config Stuff
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("The amount of temporary armor gained from each kill for the first stack of the item.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseBonusArmor { get; private set; } = 2f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("The amount of additional temporary armor gained from each kill from additional stacks of the item.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float addBonusArmor { get; private set; } = 1f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("The percentage of maximum HP needed to be lost to lose 1 additional bone fragment buff stack when taking damage. Default: 2 = 2%", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float healthPercentBuffLoss { get; private set; } = 2f;

        //Item Data
        public override string displayName => "Hardened Bone Fragments";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langID = null) => "Gain temporary armor on kill. Armor is lost when injured.";
        protected override string GetDescString(string langID = null) => $"Gain <style=cIsUtility>{baseBonusArmor}</style> <style=cStack>(+{addBonusArmor} per stack)</style> " +
            $"<style=cIsUtility>armor</style> on kill. Some <style=cIsUtility>armor</style> is lost upon taking damage; higher damage loses more armor.";
        protected override string GetLoreString(string landID = null) => "The last attacker hissed its final breath before falling silent next to its brethren.\n\nThe man holstered his weapon, " +
            "wisps of smoke curling around the barrel, before falling to his knees with a sigh. Taking his knife from its hilt, he stabbed through the purple scales of his defeated foes, " +
            "taking meat to eat but also the creatures' bones, which he began wittling to flat surfaces. \n\nA woman came over, shouldering her bow as she stopped and looked upon the gory scene before her." +
            "\n\n\"What are you doing?!\" \n\n\"What does it look like? I'm making myself some protection. Want some?\"\n\n\"Don't be ridiculous. What you're doing...that's just grotesque. " +
            "There's no way bits of bone will do anything other than make you look like some kind of monster.\"\n\n\"Good. I'll fit right in here then.\"" +
            "\n\nThe man stood back up, covered in scales. Not purple like those of the creatures he had just felled, but a brilliant white. He moved on from the scene, seeking to complete his suit of armor.";

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public BuffDef BFBuff { get; private set; }
        public HardenedBoneFragments()
        {
            modelResource = MainAssets.LoadAsset<GameObject>("Bone.prefab");
            iconResource = MainAssets.LoadAsset<Sprite>("BoneIcon");
        }
        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = modelResource;
                displayRules = GenerateItemDisplayRules();
            }

            base.SetupAttributes();

            BFBuff = ScriptableObject.CreateInstance<BuffDef>();
            BFBuff.name = "SupplyDrop BF Buff";
            BFBuff.canStack = true;
            BFBuff.isDebuff = false;
            BFBuff.iconSprite = MainAssets.LoadAsset<Sprite>("BoneBuffIcon");
            ContentAddition.AddBuffDef(BFBuff);
        }      
        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(1.5f, 1.5f, 1.5f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "null",
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
                    childName = "null",
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
                    childName = "null",
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
                    childName = "null",
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
                    childName = "null",
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
                    childName = "null",
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
                    childName = "null",
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
                    childName = "null",
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
                    childName = "null",
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
                    childName = "null",
                    localPos = new Vector3(0f, 0.20f, -0.05f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            return rules;
        }
        public override void Install()
        {
            base.Install();

            itemDef.pickupModelPrefab.transform.localScale = new Vector3(3f, 3f, 3f);

            On.RoR2.GlobalEventManager.OnCharacterDeath += CalculateBFBuffGain;
            On.RoR2.HealthComponent.TakeDamage += BFBuffLoss;

            GetStatCoefficients += AddBFBuff;
        }
        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.GlobalEventManager.OnCharacterDeath -= CalculateBFBuffGain;
            On.RoR2.HealthComponent.TakeDamage -= BFBuffLoss;

            GetStatCoefficients -= AddBFBuff;
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
