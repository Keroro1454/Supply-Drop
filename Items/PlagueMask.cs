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

namespace SupplyDrop.Items
{
    class PlagueMask : Item_V2<PlagueMask>
    {
        public override string displayName => "Vintage Plague Mask";

        public override ItemTier itemTier => ItemTier.Tier2;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Healing });
        protected override string GetNameString(string langid = null) => displayName;

        protected override string GetPickupString(string langID = null) => "Heal extra the more damage items you have.";

        protected override string GetDescString(string langID = null) => "All <style=cIsHealing>healing</style> is increased by <style=cIsHealing>2%</style> <style=cStack>(+2% per stack)</style> for every <style=cIsDamage>damage item" +
            "</style> you possess.";
            
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
            "\"Your attempts to reveal the Order's involvement within recent UES voyages have, of course, been abject failures.\"" + 
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
        Dictionary<NetworkInstanceId, int> DamageItemCounts = new Dictionary<NetworkInstanceId, int>();

        public PlagueMask()
        {
            modelResourcePath = "@SupplyDrop:Assets/Main/Models/Prefabs/PlagueMask.prefab";
            iconResourcePath = "@SupplyDrop:Assets/Main/Textures/Icons/TestIcon.png";
        }

        public override void SetupAttributes()
        {
            if (ItemBodyModelPrefab == null)
            {
                ItemBodyModelPrefab = Resources.Load<GameObject>(modelResourcePath);
                displayRules = GenerateItemDisplayRules();
            }

            base.SetupAttributes();
        }

        private static ItemDisplayRuleDict GenerateItemDisplayRules()
        {

            ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            ItemBodyModelPrefab.GetComponent<ItemDisplay>().rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.25f, 0.3f),
                    localAngles = new Vector3(0f, 180f, 0f),
                    localScale = new Vector3(.3f, .3f, .3f)

        }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.05f, -0.2f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(0.125f, 0.125f, 0.125f)
        }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(2.37f, 2.3f, -0.4f),
                    localAngles = new Vector3(-30f, 90f, 180f),
                    localScale = new Vector3(2f, 2f, 2f)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0f, -0.3f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(.2f, .2f, .2f)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.02f, -0.05f, -0.23f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(0.14f, 0.14f, 0.14f)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.025f, 0.15f, -0.28f),
                    localAngles = new Vector3(-149f, 0f, 0f),
                    localScale = new Vector3(0.17f, 0.17f, 0.17f)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0f, -1f, -0.9f),
                    localAngles = new Vector3(-138f, 0f, 0f),
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
                    localPos = new Vector3(-0.02f, 0.19f, -0.285f),
                    localAngles = new Vector3(-149f, 0f, 0f),
                    localScale = new Vector3(.2f, .2f, .2f)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, 0.7f, -3f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(2f, 2f, 2f)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0f, -0.1f, -0.28f),
                    localAngles = new Vector3(-138f, 0f, 0f),
                    localScale = new Vector3(0.15f, 0.15f, 0.15f)
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
        //This creates a list of all damage items. May need to be moved to a separate class if multiple items need to access this list
        {
            orig(self);
            indiciiToCheck = ItemCatalog.allItems.Where(x => ItemCatalog.GetItemDef(x).ContainsTag(ItemTag.Damage)).ToArray();
            Debug.Log("Item List Method has been run and a Damage Item List has been created");
            Debug.Log(indiciiToCheck.Length);
        }

        private void GetTotalDamageItems(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        //This compares your inventory to the damage item list each time your inventory changes, and generates the appropriate value for damageItemCount
        {
            if (GetCount(self) > 0)
            {
                var damageItemCount = 0;
                foreach (ItemIndex x in indiciiToCheck)
                {
                    damageItemCount += self.inventory.GetItemCount(x);
                }
                DamageItemCounts[self.netId] = damageItemCount;
                orig(self);
            }
        }

        private void IL_AddBonusHeal(ILContext il)
        //This uses the calculated damageItemCount variable to determine how much bonus healing you get. IL is pain.
        {
            ILCursor c = new ILCursor(il);

            bool found;
            int local = 2; //As of 1.0.5.1 this was 2

            found = c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(1),
                x => x.MatchStloc(out local)
                );
            
            if (found)
            {
                c.Emit(OpCodes.Ldloc, local);
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<float, HealthComponent, float>>((amount, component) =>
                {
                    float newHeal;
                    if (component.body is CharacterBody body)
                    {
                        if (GetCount(body) > 0)
                        {
                            newHeal = amount + (amount * (.02f * DamageItemCounts[body.netId] * GetCount(body)));
                        }
                        else
                        {
                            newHeal = amount;
                        }
                        return newHeal;
                    }
                    else
                    {
                        newHeal = amount;
                    }
                    return newHeal;
                }
                );
                c.Emit(OpCodes.Stloc, local);
            }            
        }   
    }
}
