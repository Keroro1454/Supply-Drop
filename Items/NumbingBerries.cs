using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using K1454.SupplyDrop;

namespace SupplyDrop.Items
{
    class NumbingBerries : Item<NumbingBerries>
    {
        public override string displayName => "Numbing Berries";

        public override ItemTier itemTier => RoR2.ItemTier.Tier1;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string NewLangName(string langid = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "Gain temporary armor upon taking damage.";

        protected override string NewLangDesc(string langID = null) => "Gain <style=cIsUtility>5</style> <style=cStack>(+5 per stack)</style> <style=cIsUtility>armor</style> for 2 seconds <style=cStack>(+0.5 second per stack)</style> upon taking damage.";

        protected override string NewLangLore(string landID = null) => "> ACCESSING JEFFERSON'S HORTICULTURE CATALOG...\n\n> ACCESSING RESTRICTED ORGANISMS SUB-CATALOG, PLEASE WAIT FOR VERIFICATION...\n\n> VERIFICATION SUCCESS. ACCESSING YOUR QUERY...\n\n> OUTPUT:\n\nSpecies Genus: Vaccinum\n\nSpecies Section: Achilococcus\n\nSpecies is native to the Andromeda system, though the exact planetary origin is currently unknown. Species is a flowering bush. Both the branches and leaves display dark green coloration, and substantial hydrophobic tendencies.\n\nThe fruit of the flowering bush is officially called numbberries. It forms as small, round berries with a cool mint skin coloration, and a striking bright purple flesh. Colloquially, the fruit is often referred to as 'The Fruit of the Damned', 'Unstoppapples' (despite the lack of similarities between apples and the fruit), or 'Madmen Mints' (owing to the fruit's skin coloration). All of these names are in reference to the chief effect caused by ingesting the fruit.\n\nNumbberries possess an inordinate amount of unique nerve suppressants.The effect of ingesting these is a complete absence of fatigue in the victim, as well as an effectively-unlimited increase to pain tolerance.Examples of this increase in tolerance to pain include victims maintaining complete functionality and reporting no symptoms of pain in spite of:\n\n- Over 30 puncture wounds in the abdominal and chest region\n\n- Severe burns over 75% of their body\n\n- Loss of 1 arm--victim reported not noticing it was missing until they were told\n\nOther symptoms caused by ingestion include severe recklessness and loss of fear and a powerful desire to travel.\n\nAdditionally, numbberries have proven to be fatally addictive.If forced to go without ingesting more numbberries within a small window of time (usually between 2-3 days), addicts quickly begin developing extreme growths on their posterior.Soon after(usually between 1-2 days), regardless of if they acquire more numbberries at this point, they will succumb to a violent series of spasms before suddenly dying of unknown causes; this is the chief reason behind their classification as a restricted organism.\n\n> END OUTPUT";

        private static List<RoR2.CharacterBody> Playername = new List<RoR2.CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public BuffIndex NumbBerryBuff { get; private set; }

        public NumbingBerries()
        {
            modelPathName = "@SupplyDrop:Assets/Main/Models/Prefabs/Berry.prefab";
            iconPathName = "@SupplyDrop:Assets/Main/Textures/Icons/BerryIcon.png";

            onAttrib += (tokenIdent, namePrefix) =>
            {
                var numbBerryBuff = new R2API.CustomBuff(
                    new BuffDef
                    {
                        buffColor = Color.white,
                        canStack = false,
                        isDebuff = false,
                        name = namePrefix + "NumbBerryBuff",
                        iconPath = "@SupplyDrop:Assets/Main/Textures/Icons/BerryBuffIcon.png"
                    });
                NumbBerryBuff = R2API.BuffAPI.Add(numbBerryBuff);
            };
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = SupplyDropPlugin.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0f, 0f, 0f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0f, 0.1f, 0f),
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
                    childName = "Stomach",
                    localPos = new Vector3(0f, 0.1f, 0f),
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
                    childName = "Head",
                    localPos = new Vector3(0.75f, 3.7f, -2.3f),
                    localAngles = new Vector3(90f, 0f, 0f),
                    localScale = new Vector3(0.5f, 0.5f, 0.5f)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0f, 0.15f, 0f),
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
                    childName = "Stomach",
                    localPos = new Vector3(0f, 0.1f, 0f),
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
                    childName = "Stomach",
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
                    localPos = new Vector3(1.5f, -0.1f, -0.3f),
                    localAngles = new Vector3(0f, 0f, 40f),
                    localScale = new Vector3(0.3f, 0.3f, 0.3f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0f, 0f, 0f),
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
                    childName = "Head",
                    localPos = new Vector3(0f, 0f, 0f),
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
                    childName = "Stomach",
                    localPos = new Vector3(0f, 0.2f, 0f),
                    localAngles = new Vector3(-25f, 0f, 0f),
                    localScale = generalScale
                }
            });
            return rules;
        }

        protected override void LoadBehavior()
        {
            
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = regDef.pickupModelPrefab;
                regItem.ItemDisplayRules = GenerateItemDisplayRules();
                regDef.pickupModelPrefab.transform.localScale = new Vector3(2f, 2f, 2f);
            }
            On.RoR2.HealthComponent.TakeDamage += CalculateBerryBuff;

            GetStatCoefficients += AddBerryBuff;
        }

        protected override void UnloadBehavior()
        {
            On.RoR2.HealthComponent.TakeDamage -= CalculateBerryBuff;

            GetStatCoefficients -= AddBerryBuff;
        }

        private void CalculateBerryBuff(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
                var InventoryCount = GetCount(self.body);
            if (InventoryCount > 0)
                {
                self.body.AddTimedBuffAuthority(NumbBerryBuff, 2f + (.5f * (InventoryCount - 1)));
                }

            orig(self, damageInfo);
        }

        private void AddBerryBuff(CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if (sender.HasBuff(NumbBerryBuff))
            {
                args.armorAdd += 5f * (5 * (InventoryCount - 1));
            }
        }
    }
}