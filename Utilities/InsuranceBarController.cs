using RoR2;
using UnityEngine;
using UnityEngine.UI;
using System;
using SupplyDrop.Items;

namespace SupplyDrop.Utils
{
    public class InsuranceBarController : MonoBehaviour
    {
        public CharacterBody body;

        public void FixedUpdate()
        {
            if (body)
            {
                var itemComponent = body.gameObject.GetComponent<HolyInsurance>();
                
                if (itemComponent.GetCount(body) > 0)
                { 
                    var insuranceBar = HolyInsurance.InsuranceBar;

                    var cachedSavingsComponent = body.gameObject.GetComponent<InsuranceSavingsTracker>();
                    if (!cachedSavingsComponent)
                    {
                        cachedSavingsComponent = body.gameObject.AddComponent<InsuranceSavingsTracker>();
                    }

                    //Checks each Range in the InsuranceDictionary, finds which one our current savings fits in, and then assigns that Range's upper value to maxValue
                    foreach (HolyInsurance.Range range in itemComponent.InsuranceDictionary.Values)
                    {
                        if (cachedSavingsComponent.insuranceSavings >= range.Lower && cachedSavingsComponent.insuranceSavings < range.Upper)
                        {
                            insuranceBar.GetComponentInChildren<Slider>().maxValue = Convert.ToSingle(range.Upper);
                        }
                    }
                    Debug.LogError(cachedSavingsComponent.insuranceSavings + "is the current amount of money saved!");
                    insuranceBar.GetComponentInChildren<Slider>().value = cachedSavingsComponent.insuranceSavings;
                }
            }
        }
    }
}
