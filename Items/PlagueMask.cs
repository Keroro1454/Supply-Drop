using R2API;
using RoR2;
using UnityEngine;
using MonoMod.Cil;

using SupplyDrop.Utils;
using static SupplyDrop.Utils.ItemHelpers;
using static SupplyDrop.Utils.MathHelpers;
using static K1454.SupplyDrop.SupplyDropPlugin;

using BepInEx.Configuration;
using System;
using System.Linq;

using System.Collections.Generic;
using UnityEngine.Networking;

namespace SupplyDrop.Items
{
    public class PlagueMask : ItemBase<PlagueMask>
    {

        //Config Stuff

        public static ConfigOption<float> baseStackHealPercent;
        public static ConfigOption<float> addStackHealPercent;

        //Item Data

        public override string ItemName => "Vintage Plague Mask";

        public override string ItemLangTokenName => "PLAGUE_MASK";

        public override string ItemPickupDesc => "Heal more the more damage items you have.";

        public override string ItemFullDescription => $"All <style=cIsHealing>healing</style> is increased by " +
            $"<style=cIsHealing>{FloatToPercentageString(baseStackHealPercent)}</style> <style=cStack>(+{FloatToPercentageString(addStackHealPercent)} per stack)</style> for every <style=cIsDamage>damage item</style> you possess.";

        public override string ItemLore => "A fire crackled from within the ornate fireplace that dominated one side " +
            "of the pristine office. Across, beautiful bookshelves that seemed to stretch into the heavens were packed with tomes, " +
            "ancient and modern, as well as various priceless curios.\n\n" +
            "One of the office's great mahogany doors, covered in elaborate and disturbing carvings, opened silently. Three men walked into room. " +
            "Two of the men were massive; dressed in fine suits, their faces were obscured with menacing hoods of black fabric and steel. " +
            "The last man, a tall, gaunt creature being coralled by the other two, wore a simple white lab uniform, a leather hat, and a mask with " +
            "a beak. The two hooded figures led him into the room and sat him in a simple chair, at the foot of a magnificent desk.\n\n" +
            "The man peered through the glass lenses of his mask at the Administrator. The leader of The Order stared impassively back from behind " +
            "a mask made of solid gold, intricately sculpted into the face the Order had proven to be god.\n\n" +
            "\"Doctor. You have broken the Oath of the Order.\"\n\n" +
            "\"Fuck you.\"\n\n" +
            "The Administrator did not flinch at the words. They stared motionlessly at the increasingly agitated man before them. " +
            "\"You have been found attempting...unsuccessfully...to reveal the Order's involvement in recent UES voyages.\"\n\n" +
            "\"Fu-\"\n\n" +
            "The Adminstrator raised their hand. \"The Order does not tolerate such blasphemous acts. We are an Order of science and reason, " +
            "and you have acted against reason.\"\n\n" +
            "\"Reason? You idiots are going to unleash those them! Onto everyone! How is that reasonable?!\"\n\n" +
            "\"You have thusly been deemed <b>Unreasonable</b>. Your membership to the Order has been severed.\"\n\n" +
            "The Administrator reached out, and placed a single, gloved finger upon the quivering man's mask.\n\n" +
            "\"Goodbye Doctor.\"\n\n" +
            "The two men stepped forward, and ripped away the man's mask, revealing a face full of emotion. Rage. Grief. Terror. " +
            "The exposed man yelled at those in the room, cried and struggled, but he was easily hoisted out of the chair and dragged " +
            "out of the room by the two men.\n\n" +
            "The golden face stared stoicly as the man it just condemned was removed from the room. As the doors slammed shut, " +
            "it looked down at the mask that had been left behind.";

        public override ItemTier Tier => ItemTier.Tier2;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Healing, ItemTag.AIBlacklist };

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("PlagueMask.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("PlagueMaskIcon");
        public static GameObject ItemBodyModelPrefab;

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        private ItemIndex[] indiciiToCheck;

        public static Dictionary<NetworkInstanceId, int> DamageItemCounts { get; private set; } = new Dictionary<NetworkInstanceId, int>();

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }
        private void CreateConfig(ConfigFile config)
        {
            baseStackHealPercent = config.ActiveBind<float>("Item: " + ItemName, "Base Bonus Healing Gained with 1 Vintage Plague Mask", .02f, "How much bonus healing per Damage item should you gain with a single Vintage Plague Mask? (.02 = 2%)");
            addStackHealPercent = config.ActiveBind<float>("Item: " + ItemName, "Additional Bonus Healing Gained per Vintage Plague Mask", .02f, "How much additional bonus healing per Damage item should each Vintage Plague Mask after the first give?");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {

            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(1f, 1f, 1f);

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
                    childName = "Head",
                    localPos = new Vector3(-0.01304F, 0.29438F, 0.31595F),
                    localAngles = new Vector3(15.33581F, 177.646F, 357.5494F),
                    localScale = new Vector3(0.24452F, 0.24452F, 0.24452F)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.02223F, 0.19926F, 0.18849F),
                    localAngles = new Vector3(4.57668F, 186.1113F, 4.3393F),
                    localScale = new Vector3(0.2343F, 0.2343F, 0.2343F)
                }
            });
            rules.Add("mdlBandit2", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00594F, -0.01029F, 0.23759F),
                    localAngles = new Vector3(344.0777F, 182.9241F, 351.1419F),
                    localScale = new Vector3(0.15818F, 0.15818F, 0.15818F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(2.03425F, 1.3625F, -1.41168F),
                    localAngles = new Vector3(357.9371F, 319.3255F, 321.0483F),
                    localScale = new Vector3(1.63629F, 1.63629F, 1.63629F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0.00209F, -0.00341F, 0.30679F),
                    localAngles = new Vector3(356.4465F, 180.2676F, 1.7038F),
                    localScale = new Vector3(0.25637F, 0.25637F, 0.25637F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(-0.01919F, -0.03624F, 0.17724F),
                    localAngles = new Vector3(342.567F, 176.9706F, 2.87537F),
                    localScale = new Vector3(0.20949F, 0.20949F, 0.20949F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00322F, 0.13827F, 0.26245F),
                    localAngles = new Vector3(8.77602F, 179.5124F, 359.7854F),
                    localScale = new Vector3(0.19539F, 0.19539F, 0.19539F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Eye",
                    localPos = new Vector3(-0.00502F, 1.1248F, -0.0259F),
                    localAngles = new Vector3(78.27003F, 359.6313F, 181.9635F),
                    localScale = new Vector3(0.3581F, 0.3581F, 0.3581F)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00906F, 0.11803F, 0.2835F),
                    localAngles = new Vector3(5.42284F, 178.7943F, 358.8734F),
                    localScale = new Vector3(0.21719F, 0.21719F, 0.21719F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.01772F, 4.86077F, 2.57096F),
                    localAngles = new Vector3(16.90637F, 178.4799F, 180.8923F),
                    localScale = new Vector3(1.69669F, 1.69669F, 1.69669F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.01697F, 0.00157F, 0.22409F),
                    localAngles = new Vector3(344.4851F, 174.7115F, 0.58271F),
                    localScale = new Vector3(0.20969F, 0.20969F, 0.20969F)
                }
            });
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00441F, 0.04618F, 0.18083F),
                    localAngles = new Vector3(1.85043F, 178.0576F, 0.72698F),
                    localScale = new Vector3(0.1313F, 0.1313F, 0.1313F)
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.0111F, 0.19833F, 0.30672F),
                    localAngles = new Vector3(39.26635F, 177.2378F, 2.83626F),
                    localScale = new Vector3(0.23369F, 0.23369F, 0.23369F)
                }
            });

            //MODDED CHARACTER IDRs START HERE

            rules.Add("mdlNemCommando", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.12014F, 0.42764F, 1.14433F),
                    localAngles = new Vector3(358.8447F, 186.2512F, 3.73659F),
                    localScale = new Vector3(0.85208F, 0.85208F, 0.85208F)
                }
            });
            rules.Add("mdlHANDOverclocked", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-1.7068F, 0.06969F, 0.9616F),
                    localAngles = new Vector3(322.2795F, 110.3694F, 349.6482F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlEnforcer", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.34598F, 0.01239F, -0.021F),
                    localAngles = new Vector3(348.8326F, 85.61517F, 2.77577F),
                    localScale = new Vector3(0.26003F, 0.26003F, 0.26003F)
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
                    childName = "Head",
                    localPos = new Vector3(-0.0036F, 0.09954F, 0.38303F),
                    localAngles = new Vector3(345.7723F, 178.9577F, 358.8304F),
                    localScale = new Vector3(0.30983F, 0.30983F, 0.30983F)
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
                    childName = "HeadBone",
                    localPos = new Vector3(-0.00479F, 0.10457F, 0.27908F),
                    localAngles = new Vector3(5.02868F, 177.7624F, 1.93477F),
                    localScale = new Vector3(0.20332F, 0.20332F, 0.20332F)
                }
            });
            rules.Add("mdlExecutioner2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.018F, 0.11955F, 0.23105F),
                    localAngles = new Vector3(0.32762F, 184.1391F, 0.63831F),
                    localScale = new Vector3(0.21677F, 0.21677F, 0.21677F)
                }
            });
            rules.Add("mdlHouse(Clone)", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.01272F, 0.03106F, 0.19385F),
                    localAngles = new Vector3(351.4142F, 182.2931F, 359.4361F),
                    localScale = new Vector3(0.15892F, 0.15892F, 0.15892F)
                }
            });
            rules.Add("mdlTeslaTrooper", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.01529F, 0.09458F, 0.25002F),
                    localAngles = new Vector3(5.16575F, 172.9076F, 1.32432F),
                    localScale = new Vector3(0.12107F, 0.12107F, 0.12107F)
                }
            });
            rules.Add("mdlDesolator", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00596F, 0.22527F, 0.29083F),
                    localAngles = new Vector3(14.13129F, 179.0553F, 358.2143F),
                    localScale = new Vector3(0.19017F, 0.19017F, 0.19017F)
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
                    childName = "Head",
                    localPos = new Vector3(-0.00532F, 0.06512F, 0.25115F),
                    localAngles = new Vector3(352.9211F, 178.085F, 2.8308F),
                    localScale = new Vector3(0.18454F, 0.18454F, 0.18454F)
                }
            });
            rules.Add("mdlRocket", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "head",
                    localPos = new Vector3(0.22466F, 0.01385F, 0.00549F),
                    localAngles = new Vector3(341.8051F, 266.3583F, 0.00615F),
                    localScale = new Vector3(0.20955F, 0.20955F, 0.20955F)
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            On.RoR2.Run.Start += DamageItemListCreator;
            On.RoR2.CharacterBody.OnInventoryChanged += GetTotalDamageItems;
            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
        }
        private void DamageItemListCreator(On.RoR2.Run.orig_Start orig, Run self)
        //May need to be moved to a separate class if multiple items need to access this list
        {
            orig(self);
            indiciiToCheck = ItemCatalog.allItems.Where(x => ItemCatalog.GetItemDef(x).ContainsTag(ItemTag.Damage)).ToArray();
            DamageItemCounts = new Dictionary<NetworkInstanceId, int>();
            Debug.Log("Item List Method has been run and a Damage Item List has been created");
            Debug.Log(indiciiToCheck.Length);
        }
        private void GetTotalDamageItems(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        //This compares your inventory to the damage item list each time your inventory changes, and generates the appropriate value for damageItemCount
        {
            orig(self);
            var damageItemCount = 0;
            foreach (ItemIndex x in indiciiToCheck)
            {
                damageItemCount += self.inventory.GetItemCount(x);
            }
            DamageItemCounts[self.netId] = damageItemCount;
        }
        private float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        //This should handle the healing shenanigans. Thank God I don't need to use IL anymore
        {
            if (self && self.body && self.body.inventory && GetCount(self.body) > 0)
            {
                int maskCount = GetCount(self.body);
                amount = amount + (amount * baseStackHealPercent * DamageItemCounts[self.netId]) + (amount * addStackHealPercent * DamageItemCounts[self.netId] * (maskCount - 1));
            }
            return orig(self, amount, procChainMask, nonRegen);
        }
    }
}
