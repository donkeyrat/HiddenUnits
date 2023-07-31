using Landfall.TABS;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using TGCore.Library;

public class ThorBehavior : MonoBehaviour
{
    void Start()
    {
        Unit = transform.root.GetComponent<Unit>();
    }

	public void GrabHammerRight()
	{
		if (!HoldingRight)
		{
			HoldingRight = true;
			grabEvent.Invoke();
			foreach (var hand in Unit.GetComponentInChildren<HandRight>().GetComponentsInChildren<Collider>())
			{
				hand.enabled = false;
			}
			if (Unit.holdingHandler)
			{
				Unit.WeaponHandler.fistRefernce = null;
				if (Unit.holdingHandler.rightObject)
				{
					var oldWeapon = Unit.holdingHandler.rightObject.gameObject;
					Unit.holdingHandler.LetGoOfWeapon(oldWeapon);
					Destroy(oldWeapon);
				}
				SetWeapon(Unit, Unit.Team, weaponToGrab, new PropItemData(), HoldingHandler.HandType.Right, Unit.data.mainRig.rotation, new List<GameObject>());
			}
			else if (Unit.GetComponentInChildren<HoldingHandlerMulti>())
			{
				var multi = Unit.GetComponentInChildren<HoldingHandlerMulti>();
				foreach (var hand in multi.mainHands)
				{
					multi.SetWeapon(hand.gameObject, weaponToGrab);
				}
			}
		}

	}
	public void GrabHammerLeft()
	{
		if (!HoldingLeft)
		{
			HoldingLeft = true;
			foreach (var hand in Unit.GetComponentInChildren<HandLeft>().GetComponentsInChildren<Collider>())
			{
				hand.enabled = false;
			}
			if (Unit.holdingHandler)
			{
				weaponToGrab.GetComponent<Holdable>().holdableData.snapConnect = false;
				if (Unit.holdingHandler.leftObject)
				{
					var oldWeapon = Unit.holdingHandler.leftObject.gameObject;
					Unit.holdingHandler.LetGoOfWeapon(oldWeapon);
					Destroy(oldWeapon);
				}
				Unit.holdingHandler.leftHandActivity = HoldingHandler.HandActivity.HoldingRightObject;
			}
			else if (Unit.GetComponentInChildren<HoldingHandlerMulti>())
			{
				var multi = Unit.GetComponentInChildren<HoldingHandlerMulti>();
				foreach (var hand in multi.otherHands)
				{
					multi.SetWeapon(hand.gameObject, weaponToGrab);
				}
			}
		}

	}

	public void UnGrabHammer()
    {
        if (HoldingLeft || HoldingRight)
        {
			HoldingLeft = false;
			HoldingRight = false;
			ungrabEvent.Invoke();
            if (Unit.holdingHandler)
            {
                Unit.holdingHandler.LetGoOfWeapon(weaponToGrab);
            }
            else if (Unit.GetComponentInChildren<HoldingHandlerMulti>())
            {
                var multi = Unit.GetComponentInChildren<HoldingHandlerMulti>();
                multi.LetGoOfAll();
            }
        }
    }

	public Weapon SetWeapon(Unit unit, Team team, GameObject weaponObject, PropItemData weaponData, HoldingHandler.HandType handType, Quaternion rotation, List<GameObject> objects, bool isUnitEditor = false)
	{
		var componentInChildren = unit.GetComponentInChildren<WeaponHandler>();
		if (!componentInChildren)
		{
			var componentInChildren2 = unit.GetComponentInChildren<MultipleWeaponHandler>();
			if ((bool)componentInChildren2)
			{
				componentInChildren2.SetWeapon(weaponObject, handType, false);
			}
			return null;
		}
		//if (unit.unitBlueprint.fistRef)
		//{
		//	componentInChildren.fistRefernce = unit.unitBlueprint.fistRef;
		//}
		var componentInChildren3 = unit.GetComponentInChildren<Torso>();
		if ((bool)weaponObject)
		{
			var gameObject = weaponObject;
			gameObject.transform.position = componentInChildren3.transform.position + componentInChildren3.transform.forward * 0.5f;
			gameObject.transform.rotation = rotation;
			gameObject.gameObject.transform.SetParent(unit.transform);
			objects.Add(gameObject);
			var component = gameObject.GetComponent<Weapon>();
			var component2 = gameObject.GetComponent<WeaponItem>();
			var component3 = gameObject.GetComponent<Holdable>();
			gameObject.GetComponent<MeleeWeapon>();
			var component4 = gameObject.GetComponent<RangeWeapon>();
			if (unit.unitBlueprint.removeCloseRangeMiss && (bool)component4)
			{
				component4.extraCDInMelee = 0f;
				component4.extraSpreadInMelee = 0f;
			}
			if ((bool)component3)
			{
				if (component3.useAlternaticeForIceGiant && (bool)unit.GetComponent<IceGiant>())
				{
					component3.holdableData.relativePosition = component3.iceGiantRelativePosition;
				}
				if (handType == HoldingHandler.HandType.Right)
				{
					unit.holdingHandler.rightHandActivity = HoldingHandler.HandActivity.HoldingRightObject;
					if (unit.unitBlueprint.holdinigWithTwoHands)
					{
						unit.holdingHandler.leftHandActivity = HoldingHandler.HandActivity.HoldingRightObject;
					}
					unit.holdingHandler.rightObject = component3;
				}
				else
				{
					unit.holdingHandler.leftHandActivity = HoldingHandler.HandActivity.HoldingLeftObject;
					unit.holdingHandler.leftObject = component3;
					component3.holdableData.relativePosition.x += unit.unitBlueprint.weaponSeparation;
					component3.holdableData.relativePosition.x *= -1f;
					component3.holdableData.upRotation.x *= -1f;
					component3.holdableData.forwardRotation.x *= -1f;
				}
				component3.holdingHandler = unit.holdingHandler;
			}
			else
			{
				Transform transform;
				transform = (handType != 0) ? unit.GetComponentInChildren<HandLeft>().transform : unit.GetComponentInChildren<HandRight>().transform;
				gameObject.transform.position = transform.position;
				gameObject.transform.rotation = transform.rotation;
				gameObject.transform.parent = transform;
			}
			componentInChildren.SetWeapon(component, handType);
			component2.Initialize(team);
			if (weaponData != null)
			{
				component2.SetPropData(weaponData, team);
			}
			return component;
		}
		return null;
	}

	private Unit Unit;

    private bool HoldingRight;

	private bool HoldingLeft;

    public GameObject weaponToGrab;

    public UnityEvent grabEvent = new UnityEvent();

    public UnityEvent ungrabEvent = new UnityEvent();
}
