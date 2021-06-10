using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using TILER2;
using SupplyDrop.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using static K1454.SupplyDrop.SupplyDropPlugin;

namespace SupplyDrop.Items
{
    public class PlagueMask : Item<PlagueMask>
    {
        public override string displayName => "Vintage Plague Mask";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Healing });
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langID = null) => "Heal extra the more damage items you have.";
        protected override string GetDescString(string langID = null) => "All <style=cIsHealing>healing</style> is increased by " +
            "<style=cIsHealing>2%</style> <style=cStack>(+2% per stack)</style> for every <style=cIsDamage>damage item</style> you possess.";           
        protected override string GetLoreString(string landID = null) => "A fire crackled from within the ornate fireplace that dominated one side " +
            "of the pristine office. Across, beautiful bookshelves that seemed to stretch into the heavens were packed with tomes, " +
            "ancient and modern, as well as various priceless curios.\n\n" +
            "One of the office's great mahogany doors, covered in carvings of men and women of science, opened silently. Three men walked into room. " +
            "Two of the men were massive; they were dressed in fine suits, and had their faces obscured with menacing hoods of black fabric and steel. " +
            "The last, a tall, gaunt man being led by the other two, wore a simple white lab uniform, and a terrifying leather hat and mask with " +
            "a beak. The two hooded figures sat him in a simple chair at the foot of a magnificent desk, facing away from the shut doors.\n\n" +
            "The man peered through the glass lenses of his mask at the Administrator. The leader of The Order stared impassively back from behind " +
            "a mask made of solid gold, intricately sculpted into the face the Order had proven to be god.\n\n" +
            "\"Doctor. You have broken the Oath of the Order.\"\n\n" + 
            "\"Fuck you.\"\n\n" + 
            "The Administrator did not flinch at the words, simply watching motionlessly the increasingly agitated man before them. " +
            "\"Your attempts to reveal the Order's involvement within recent UES voyages have, of course, been abject failures.\"\n\n" + 
            "\"Fu-\"\n\n" +
            "The Adminstrator raised their hand; \"But the Order does not tolerate such blasphemous acts. We are an Order of science and reason, " +
            "and you have acted against reason.\"\n\n" +
            "\"Reason? You idiots are going to unleash those two! Onto everyone! How's that for reasonable?!\"\n\n" +
            "\"You have thusly been deemed Unreasonable. Your membership to the Order has been severed. Goodbye Doctor.\"\n\n" +
            "The two men stepped forward, and ripped away the man's mask, revealing a face full of emotion. Rage. Grief. Terror. " +
            "The exposed man yelled at those in the room, cried and struggled, but he was easily hoisted out of the chair and dragged " +
            "out of the room by the two men.\n\n" +
            "The golden face stared stoicly as the man it just condemned was removed from the room. As the doors slammed shut, " +
            "it looked down at the mask that had been left behind.";

        private static List<CharacterBody> Playername = new List<CharacterBody>();
        public static GameObject ItemBodyModelPrefab;
        private ItemIndex[] indiciiToCheck;
        public static Dictionary<NetworkInstanceId, int> DamageItemCounts { get; private set; } = new Dictionary<NetworkInstanceId, int>();
        public PlagueMask()
        {
            modelResource = MainAssets.LoadAsset<GameObject>("PlagueMask.prefab");
            iconResource = MainAssets.LoadAsset<Sprite>("PlagueMaskIcon");
        }
        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = modelResource;
                displayRules = GenerateItemDisplayRules();
            }

            base.SetupAttributes();
        }
        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {

            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommando", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.2f, 0.3f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(.275f, .275f, .275f)

        }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.17f, 0.22f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(0.25f, 0.25f, 0.25f)
        }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 5f, -1.5f),
                    localAngles = new Vector3(60f, 0f, 0f),
                    localScale = new Vector3(2f, 2f, 2f)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0f, -0.03f, 0.3f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(.25f, .25f, .25f)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.03f, 0.23f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.1f, 0.295f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(0.22f, 0.22f, 0.22f)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0f, 0f, 1f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(0.5f, 0.5f, 0.5f)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.08f, 0.25f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(.25f, .25f, .25f)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 4.7f, 2.3f),
                    localAngles = new Vector3(160f, 0f, 0f),
                    localScale = new Vector3(2.25f, 2.25f, 2.25f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.05f, 0.25f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlBandit", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.2f, 0.25f),
                    localAngles = new Vector3(10f, 180f, 0f),
                    localScale = new Vector3(0.25f, 0.25f, 0.25f)
                }
            });
            return rules;
        }
        public override void Install()
        {
            base.Install();

            On.RoR2.Run.Start += DamageItemListCreator;
            On.RoR2.CharacterBody.OnInventoryChanged += GetTotalDamageItems;
            IL.RoR2.HealthComponent.Heal += IL_AddBonusHeal;
        }
        public override void Uninstall()
        {
            base.Uninstall();

            On.RoR2.Run.Start -= DamageItemListCreator;
            On.RoR2.CharacterBody.OnInventoryChanged -= GetTotalDamageItems;
            IL.RoR2.HealthComponent.Heal -= IL_AddBonusHeal;
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
        private void IL_AddBonusHeal(ILContext il)
        //This uses the calculated damageItemCount variable to determine how much bonus healing you get. IL is pain.
        {
            ILCursor c = new ILCursor(il);

            bool found = false;
            int amountArg = 1;

            found = c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld(typeof(HealthComponent.ItemCounts).GetField("increaseHealing")),
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out _)
            );
            if (!found)
            {
                Debug.LogError("Couldn't find where HealthComponent::Heal() reads number of Rejuvenation Racks, aborting PlagueMask hook...");
                this.enabled = false;
                return;
            }

            Instruction ifToChange = c.Previous;

            found = c.TryGotoNext(MoveType.After,
                x => x.MatchStarg(out amountArg)
            );
            if (!found)
            {
                Debug.LogError("HealthComponent::Heal() does not store the muliplied heal value after Rejuvenation Racks back into the arg, aborting PlagueMask hook...");
                this.enabled = false;
                return;
            }

            c.Emit(OpCodes.Ldarg_0);

            ILLabel newLabel = c.DefineLabel();
            newLabel.Target = c.Previous;
            ifToChange.Operand = newLabel;

            c.Emit(OpCodes.Ldc_I4, (int)this.catalogIndex);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Func<HealthComponent, ItemIndex, float, float>>((healthComponent, maskIndex, healAmount) =>
            {
                if (!healthComponent || !healthComponent.body || !healthComponent.body.inventory)
                {
                    return 0;
                }
                if (!PlagueMask.DamageItemCounts.TryGetValue(healthComponent.body.netId, out int damageItemCount))
                    damageItemCount = 0;
                if (GetCount(healthComponent.body) == 0)
                {
                    return 0;
                }
                return healAmount * (0.02f + (0.02f * ((float)healthComponent.body.inventory.GetItemCount(maskIndex) - 1)) * damageItemCount);
            });
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Add);
            c.Emit(OpCodes.Starg, amountArg);
        }
    }
}
