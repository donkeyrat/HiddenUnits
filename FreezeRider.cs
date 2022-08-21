using UnityEngine;
using Landfall.TABS.GameState;

namespace HiddenUnits
{
	public class FreezeRider : GameStateListener
	{
		public bool freezeY;

		public bool freezeRotation;

		private Rigidbody freezeBody;

		public Rigidbody FreezeBody => freezeBody;

        private bool done;

        public override void OnEnterBattleState()
        {
            FreezeBody.isKinematic = false;
        }

        public void Update()
        {
            if (!done && FreezeBody && GameStateManager.GameState == GameState.BattleState)
            {
                FreezeBody.isKinematic = false;
                done = true;
            }
        }


        public override void OnEnterPlacementState()
        {
            freezeBody = GetComponent<Rigidbody>();
            FreezeBody.isKinematic = true;
        }
    }
}
