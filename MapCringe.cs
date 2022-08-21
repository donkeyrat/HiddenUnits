using UnityEngine;
using Landfall.TABS.GameState;

namespace HiddenUnits
{
    public class MapCringe : GameStateListener
    {

        public override void OnEnterBattleState()
        {
        }

        public override void OnEnterPlacementState()
		{
			gameObject.SetActive(false);
			if (myClone)
			{
				Destroy(myClone);
			}
			myClone = Instantiate(gameObject, transform.position, transform.rotation, transform.parent);
			myClone.SetActive(true);
			Destroy(myClone.GetComponent<MapCringe>());
		}

		private GameObject myClone;
	}
}
