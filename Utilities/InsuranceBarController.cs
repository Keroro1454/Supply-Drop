using RoR2;
using UnityEngine;
using UnityEngine.UI;
using System;
using SupplyDrop.Items;
using SupplyDrop.Utils;

namespace SupplyDrop.Utilities
{
    public class InsuranceBarController : MonoBehaviour
    {
        public CharacterBody body;

        public void FixedUpdate()
        {
            var characterBody = body;
            if (body)
            {
                var insuranceBar = GetComponent<Slider>();

                var cachedSavingsComponent = body.gameObject.GetComponent<InsuranceSavingsTracker>();
                if (!cachedSavingsComponent)
                {
                    cachedSavingsComponent = body.gameObject.AddComponent<InsuranceSavingsTracker>();
                }
                var itemComponent = body.gameObject.GetComponent<HolyInsurance>();

                //Checks each Range in the InsuranceDictionary, finds which one our current savings fits in, and then assigns that Range's upper value to maxValue
                foreach (HolyInsurance.Range range in itemComponent.InsuranceDictionary.Values)
                {
                    if (cachedSavingsComponent.insuranceSavings >= range.Lower && cachedSavingsComponent.insuranceSavings < range.Upper)
                    {
                        insuranceBar.maxValue = Convert.ToSingle(range.Upper);
                    }
                }

                insuranceBar.value = cachedSavingsComponent.insuranceSavings;
            }
        }
    }
}
