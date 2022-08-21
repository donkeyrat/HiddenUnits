using System;
using Landfall.TABS;
using UnityEngine;

namespace HiddenUnits {
	
	public class TransferPossessionWerewolf : MonoBehaviour {
		
		private void Start() {
			
			WerewolfSpawner component = GetComponent<WerewolfSpawner>();
			if ((bool)component) {
				
				component.spawnUnitAction = (Action<GameObject>)Delegate.Combine(component.spawnUnitAction, new Action<GameObject>(Transfer));
			}
		}

		public void Transfer(GameObject unitRoot) {
			
			CameraAbilityPossess componentInParent = MainCam.instance.GetComponentInParent<CameraAbilityPossess>();
			Unit component = unitRoot.GetComponent<Unit>();
			Unit component2 = base.transform.root.GetComponent<Unit>();
			if ((bool)componentInParent && componentInParent.currentUnit == component2) { componentInParent.EnterUnit(component); }
		}
	}
}