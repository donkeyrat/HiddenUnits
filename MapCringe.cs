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
			if (MyClone)
			{
				Destroy(MyClone);
			}
			MyClone = Instantiate(gameObject, transform.position, transform.rotation, transform.parent);
			MyClone.SetActive(true);
			Destroy(MyClone.GetComponent<MapCringe>());
		}

		private GameObject MyClone;
	}
}
