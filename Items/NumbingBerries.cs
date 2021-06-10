using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using SupplyDrop.Utils;
using static K1454.SupplyDrop.SupplyDropPlugin;

namespace SupplyDrop.Items
{
    public class NumbingBerries : Item<NumbingBerries>
    {
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("The amount of temporary armor gained after taking damage for the first stack of the item.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseBonusArmor { get; private set; } = 5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("The amount of temporary armor gained after taking damage for additional stacks of the item.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float addBonusArmor { get; private set; } = 5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In seconds, the duration of the armor boost for the first stack of the item.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseBerryBuffDuration { get; private set; } = 2f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("In seconds, the duration of the armor boost for additional stacks of the item.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float addBerryBuffDuration { get; private set; } = .5f;

        public override string displayName => "Numbing Berries";
        public override ItemTier itemTier => ItemTier.Tier1;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langID = null) => "Gain temporary armor upon taking damage.";
        protected override string GetDescString(string langID = null) => $"Gain <style=cIsUtility>{baseBonusArmor}</style> <style=cStack>(+{addBonusArmor} per stack)</style> " +
            $"<style=cIsUtility>armor</style> for {baseBerryBuffDuration} seconds <style=cStack>(+{addBerryBuffDuration} seconds per stack)</style> upon taking damage.";
        protected override string GetLoreString(string landID = null) => "<style=cMono>> ACCESSING JEFFERSON'S HORTICULTURE CATALOG...\n> ACCESSING RESTRICTED ORGANISMS SUB-CATALOG, PLEASE WAIT FOR VERIFICATION..." +
            "\n> VERIFICATION SUCCESS. ACCESSING YOUR QUERY...\n> OUTPUT:</style>\n\nSpecies Genus: Vaccinum\nSpecies Section: Achilococcus" +
            "\n\nSpecies is native to the Andromeda system, though the exact planetary origin is currently unknown. Species is a flowering bush. Both the branches and leaves display dark green coloration, " +
            "and substantial hydrophobic tendencies.\n\nThe fruit of the flowering bush is officially called numbberries. It forms as small, round berries with a mint skin coloration, and a bright purple flesh. " +
            "Colloquially, the fruit is referred to as 'The Fruit of the Damned', 'Unstoppapples', or 'Madmen Mints'. " +
            "All of these names are in reference to the chief effect caused by ingesting the fruit.\n\nNumbberries possess an inordinate amount of unique nerve suppressants. " +
            "The effect of ingesting these is a complete absence of fatigue in the victim, as well as an effectively-unlimited increase to pain tolerance. " +
            "Examples of this increase in tolerance to pain include victims maintaining complete functionality and reporting no symptoms of pain in spite of:" +
            "\n\n- Over 30 puncture wounds in the abdominal and chest region\n\n- Severe burns over 75% of their body\n\n- Loss of 1 arm; victim reported not noticing it was missing until they were told" +
            "\n\nOther symptoms caused by ingestion include severe recklessness, loss of fear, and a powerful desire to travel.\n\nAdditionally, numbberries have proven to be fatally addictive. " +
            "If forced to go without ingesting more numbberries within a small window of time (usually between 2-3 days), addicts quickly begin developing extreme growths on their posterior. " +
            "Soon after (usually between 1-2 days), regardless of if they acquire more numbberries at this point, they will succumb to a violent series of spasms before suddenly dying of unknown causes." +
            "\n\n<style=cMono>END OUTPUT</style>";

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        public BuffDef NumbBerryBuff { get; private set; }
        public NumbingBerries()
        {
            modelResource = MainAssets.LoadAsset<GameObject>("Berry.prefab");
            iconResource = MainAssets.LoadAsset<Sprite>("BerryIcon");
        }
        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = modelResource;
                displayRules = GenerateItemDisplayRules();
            }

            base.SetupAttributes();

            NumbBerryBuff = ScriptableObject.CreateInstance<BuffDef>();
            NumbBerryBuff.name = "SupplyDrop Berry Buff";
            NumbBerryBuff.canStack = false;
            NumbBerryBuff.isDebuff = false;
            NumbBerryBuff.iconSprite = MainAssets.LoadAsset<Sprite>("BerryBuffIcon");
            BuffAPI.Add(new CustomBuff(NumbBerryBuff));
        }
        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {
            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0f, 0f, 0f);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new ItemDisplayRule[]
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
                    localPos = new Vector3(-0.7f, 5f, -1.3f),
                    localAngles = new Vector3(115f, 0f, 0f),
                    localScale = new Vector3(.5f, .5f, .5f)
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
            rules.Add("mdlBandit2", new ItemDisplayRule[]
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
            return rules;
        }
        public override void Install()
        {
            base.Install();

            itemDef.pickupModelPrefab.transform.localScale = new Vector3(2f, 2f, 2f);

            On.RoR2.HealthComponent.TakeDamage += CalculateBerryBuff;
            GetStatCoefficients += AddBerryBuff;
        }
        public override void Uninstall()
        {
            base.Uninstall();
            On.RoR2.HealthComponent.TakeDamage -= CalculateBerryBuff;

            GetStatCoefficients -= AddBerryBuff;
        }
        private void CalculateBerryBuff(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {

            var InventoryCount = GetCount(self.body);
            if (InventoryCount > 0)
                {
                self.body.AddTimedBuffAuthority(NumbBerryBuff.buffIndex, baseBerryBuffDuration + (addBerryBuffDuration * (InventoryCount - 1)));
                }
            orig(self, damageInfo);
        }
        private void AddBerryBuff(CharacterBody sender, StatHookEventArgs args)
        {
            var InventoryCount = GetCount(sender);
            if (sender.HasBuff(NumbBerryBuff))
            {
                args.armorAdd += baseBonusArmor + (addBonusArmor * (InventoryCount - 1));
            }
        }
    }
}