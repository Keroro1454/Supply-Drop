using RoR2;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
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

                //Acquire components for calculations
                var cachedSavingsComponent = body.gameObject.GetComponent<InsuranceSavingsTracker>();
                if (!cachedSavingsComponent)
                {
                    cachedSavingsComponent = body.gameObject.AddComponent<InsuranceSavingsTracker>();
                }
                var itemComponent = body.gameObject.GetComponent<HolyInsurance>();

                //Finds index where current savings amounts fits in
                int topTierAffordable = Array.FindIndex(itemComponent.ranges, r => r.Contains(cachedSavingsComponent.insuranceSavings));
                //Takes the index we just found and determines what the Upper value in that index is
                var insuranceBarMax = itemComponent.ranges.FirstOrDefault(r => r.Upper == topTierAffordable).Upper;
                //Convert that Upper value from a double to a float/single
                insuranceBar.maxValue = Convert.ToSingle(insuranceBarMax);
                //Tells the bar what value we actually have
                insuranceBar.value = cachedSavingsComponent.insuranceSavings;
            }
        }
    }
}
