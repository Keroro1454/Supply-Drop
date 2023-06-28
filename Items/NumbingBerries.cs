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
    public class NumbingBerries : ItemBase<NumbingBerries>
    {
        //Config Stuff

        public static ConfigOption<float> baseBonusArmor;
        public static ConfigOption<float> addBonusArmor;
        public static ConfigOption<float> baseBerryBuffDuration;
        public static ConfigOption<float> addBerryBuffDuration;

        //Item Data

        public override string ItemName => "Numbing Berries";

        public override string ItemLangTokenName => "BERRIES";

        public override string ItemPickupDesc => "Gain temporary <style=cIsUtility>armor</style> upon taking damage.";

        public override string ItemFullDescription => $"Gain <style=cIsUtility>{baseBonusArmor}</style> <style=cStack>(+{addBonusArmor} per stack)</style> " +
            $"<style=cIsUtility>armor</style> for {baseBerryBuffDuration} seconds <style=cStack>(+{addBerryBuffDuration} seconds per stack)</style> upon taking damage.";

        public override string ItemLore => "<style=cMono>> ACCESSING JEFFERSON'S HORTICULTURE CATALOG...\n> ACCESSING RESTRICTED ORGANISMS SUB-CATALOG, PLEASE WAIT FOR VERIFICATION..." +
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

        public override ItemTier Tier => ItemTier.Tier1;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };


        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Berry.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("BerryIcon");


        public static GameObject ItemBodyModelPrefab;

        public BuffDef NumbBerryBuff { get; private set; }

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
            baseBonusArmor = config.ActiveBind<float>("Item: " + ItemName, "Base Armor Gained on Taking Damage with 1 Numbing Berries", 5f, "How much armor should you gain upon taking damage with a single numbing berries?");
            addBonusArmor = config.ActiveBind<float>("Item: " + ItemName, "Additional Armor Gained on Taking Damage per Numbing Berries", 5f, "How much additional armor gained upon taking damage should each numbing berries after the first give?");
            baseBerryBuffDuration = config.ActiveBind<float>("Item: " + ItemName, "Base duration of the Armor Boost with 1 Numbing Berries", 2f, "How long should the armor boost last for with a single numbing berries, in seconds?");
            addBerryBuffDuration = config.ActiveBind<float>("Item: " + ItemName, "Additional duration of the Armor Boost per Numbing Berries", .5f, "How much additional armor boost duration should each numbing berries after the first give, in seconds?");
        }
        private void CreateBuff()
        {
            NumbBerryBuff = ScriptableObject.CreateInstance<BuffDef>();
            NumbBerryBuff.name = "SupplyDrop Berry Buff";
            NumbBerryBuff.canStack = false;
            NumbBerryBuff.isDebuff = false;
            NumbBerryBuff.iconSprite = MainAssets.LoadAsset<Sprite>("BerryBuffIcon");

            ContentAddition.AddBuffDef(NumbBerryBuff);
        }
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab);

            Vector3 generalScale = new Vector3(0f, 0f, 0f);

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
                    childName = "MouthMuzzle",
                    localPos = new Vector3(-1.80733F, 2.78646F, 2.18605F),
                    localAngles = new Vector3(8.76867F, 166.0483F, 119.149F),
                    localScale = new Vector3(0.32626F, 0.32626F, 0.32626F)
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
            rules.Add("mdlRailGunner", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0F, 0F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = generalScale
                }
            });
            rules.Add("mdlVoidSurvivor", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Stomach",
                    localPos = new Vector3(0F, 0F, 0F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = generalScale
                }
            });
            return rules;
        }
        public override void Hooks()
        {
            ItemDef.pickupModelPrefab.transform.localScale = new Vector3(2f, 2f, 2f);

            On.RoR2.HealthComponent.TakeDamage += CalculateBerryBuff;
            GetStatCoefficients += AddBerryBuff;
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