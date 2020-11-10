using RoR2;
using UnityEngine;
using System;
using System.Linq;
using SupplyDrop.Items;

namespace SupplyDrop.Utils
{
    public class BleedingScript : MonoBehaviour
    {
        public ParticleSystem particles;    
        public CharacterModel model;

        public void Awake()
        {
            model = GetComponentInParent<CharacterModel>();
        }
        public void FixedUpdate()
        {
            var particleSystem = particles;
            if (model)
            {
                if (model.body)
                {
                    Chat.AddMessage("Blood should be pouring");
                        int currentBuffLevel = Array.FindIndex(BloodBook.ranges, r => model.body.HasBuff(r.Buff));
                        if (Enumerable.Range(0, 5).Contains(currentBuffLevel))                        
                        {
                            if (!particleSystem.isPlaying)
                            {
                                if (currentBuffLevel == 0)
                                {
                                    var newDripSpeed = particleSystem.emission;
                                    newDripSpeed.rateOverTime = 1f;
                                }
                                if (currentBuffLevel == 1)
                                {
                                    var newDripSpeed = particleSystem.emission;
                                    newDripSpeed.rateOverTime = 2f;
                                }
                                if (currentBuffLevel == 2)
                                {
                                    var newDripSpeed = particleSystem.emission;
                                    newDripSpeed.rateOverTime = 5f;
                                }
                                if (currentBuffLevel == 3)
                                {
                                    var newDripSpeed = particleSystem.emission;
                                    newDripSpeed.rateOverTime = 10f;
                                }
                                if (currentBuffLevel == 4)
                                {
                                    var newDripSpeed = particleSystem.emission;
                                    newDripSpeed.rateOverTime = 15f;
                                }
                                if (currentBuffLevel == 5)
                                {
                                    var newDripSpeed = particleSystem.emission;
                                    newDripSpeed.rateOverTime = 20f;
                                }
                                particleSystem.Play();
                            }
                        }
                        else
                        {
                            particleSystem.Stop();
                        }
                    
                }
            }
        }
    }
}
