using RoR2;
using UnityEngine;

namespace SupplyDrop.Utils
{

    public class BloodBookFollowerComm : MonoBehaviour
    {
        private ItemFollower follower;
        private bool initialized;

        private void Start()
        {
            follower = GetComponent<ItemFollower>();
        }

        private void Update()
        {
            if (initialized) return;
            if (!follower) return;
            if (!follower.followerInstance) return;

            var bleed = follower.followerInstance.GetComponentInChildren<BleedingScript>(true);
            if (!bleed) return;

            bleed.model = GetComponentInParent<CharacterModel>();
            initialized = true;
        }
    }
}
