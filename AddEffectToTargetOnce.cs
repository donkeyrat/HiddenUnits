using Landfall.TABS;
using UnityEngine;

public class AddEffectToTargetOnce : MonoBehaviour {

	public delegate void AddedEffectEventHandlerr(Unit attacker, DataHandler targetData);

	public UnitEffectBase EffectPrefab;
	
	public event AddedEffectEventHandlerr AddedEffectE;

	public bool onlyOnce;
	public void Go() {

		Unit component = transform.root.GetComponent<Unit>();
		if ((bool)component) { AddEffectt(component, component.data.targetData); }
	}

	public void AddEffectt(Unit attacker, DataHandler targetData)
	{
		if (!(attacker == null) && (!targetData || !targetData.Dead) && targetData) {

			UnitEffectBase unitEffectBase = UnitEffectBase.AddEffectToTarget(targetData.unit.transform.gameObject, EffectPrefab);
			if (unitEffectBase == null) {

				GameObject obj = Instantiate(EffectPrefab.gameObject, targetData.unit.transform.root);
				obj.transform.position = targetData.unit.transform.position;
				obj.transform.rotation = Quaternion.LookRotation(targetData.mainRig.position - attacker.data.mainRig.position);
				unitEffectBase = obj.GetComponent<UnitEffectBase>();
				TeamHolder.AddTeamHolder(obj, transform.root.gameObject);
				unitEffectBase.DoEffect();
			}
			else if (!onlyOnce) {

				unitEffectBase.transform.rotation = Quaternion.LookRotation(targetData.mainRig.position - attacker.data.mainRig.position);
				unitEffectBase.Ping();
			}
			AddedEffectE?.Invoke(attacker, targetData);
		}
	}
}
