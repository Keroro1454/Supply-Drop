using RoR2;
using UnityEngine;

namespace SupplyDrop.Utils
{
    
    public class BloodBookFollowerComm : MonoBehaviour
    {
        void Start()
        {

            var itemFollower = GetComponent<ItemFollower>();
            if (itemFollower)
            {
                var followerBleed = itemFollower.followerPrefab.GetComponent<BleedingScript>();
                if (followerBleed)
                {
                    followerBleed.model = GetComponentInParent<CharacterModel>();
                }
            }
        }
    }
}
