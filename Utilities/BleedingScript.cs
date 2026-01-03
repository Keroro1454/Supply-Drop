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
        private void Awake()
        {
            if (!particles)
                particles = GetComponent<ParticleSystem>();
        }
        public void FixedUpdate()
        {
            var particleSystem = particles;
            if (model)
            {
                if (model.body)
                {
                    if (particles)
                    { 
                        int currentBuffLevel = Array.FindIndex(BloodBook.ranges, r => model.body.HasBuff(r.Buff));
                        if (currentBuffLevel >= 0)                        
                        {
                            var emission = particleSystem.emission;

                            emission.rateOverTime = currentBuffLevel switch
                            {
                                0 => 1f,
                                1 => 2f,
                                2 => 5f,
                                3 => 10f,
                                4 => 15f,
                                5 => 20f,
                                _ => 0f
                            };

                            if (!particleSystem.isPlaying)
                                particleSystem.Play();
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
}
