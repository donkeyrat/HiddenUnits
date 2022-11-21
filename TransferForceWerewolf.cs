using System;
using Landfall.TABS;
using UnityEngine;

namespace HiddenUnits 
{
	public class TransferForceWerewolf : MonoBehaviour 
	{
		private void Start() 
		{
			
			WerewolfSpawner component = GetComponent<WerewolfSpawner>();
			component.spawnUnitAction = (Action<GameObject>)Delegate.Combine(component.spawnUnitAction, new Action<GameObject>(Transfer));
		}

		private void Transfer(GameObject unitRoot) {
			
			Unit component = unitRoot.GetComponent<Unit>();
			Unit component2 = base.transform.root.GetComponent<Unit>();
			
			for (int i = 0; i < component.data.allRigs.AllRigs.Length; i++) { component.data.allRigs.AllRigs[i].velocity = component2.data.mainRig.velocity; }
		}
	}
}