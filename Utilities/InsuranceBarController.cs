using RoR2;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using SupplyDrop.Items;

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
                var healthBar = GetComponent<Slider>();
                healthBar.maxValue = body.maxHealth;
                healthBar.value = body.healthComponent.health;
            }
        }
    }
}
