using Landfall.TABS;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Landfall.MonoBatch;
using HiddenUnits;
using System.Collections;

public class HoldingHandlerMulti : BatchedMonobehaviour
{
    protected override void Start()
    {
        base.Start();
        unit = transform.root.GetComponent<Unit>();
        foreach (var weapon in unit.GetComponentsInChildren<Weapon>(true))
        {
            existingWeapons.Add(weapon.gameObject);
        }
        foreach (var left in GetComponentsInChildren<HandLeft>(true))
        {
            otherHands.Add(left);
            if (unit.unitBlueprint.LeftWeapon != null)
            {
                SetWeapon(left.gameObject, Instantiate(unit.unitBlueprint.LeftWeapon, left.transform.position, left.transform.rotation, unit.transform));
            }
        }
        foreach (var right in GetComponentsInChildren<HandRight>(true))
        {
            mainHands.Add(right);
            if (unit.unitBlueprint.RightWeapon != null)
            {
                SetWeapon(right.gameObject, Instantiate(unit.unitBlueprint.RightWeapon, right.transform.position, right.transform.rotation, unit.transform));
            }
        }
    }

    public void SetWeapon(GameObject handSlot, GameObject weaponing)
    {
        var weapon = weaponing;
        spawnedWeapons.Add(weapon);
        if (handSlot.GetComponent<HandLeft>()) { leftWeapons.Add(weapon); }
        else { rightWeapons.Add(weapon); }
        Rigidbody rigidbody = handSlot.GetComponent<Rigidbody>();
        var holdable = weapon.GetComponent<Holdable>();

        holdable.rig.isKinematic = true;
        holdable.rig.interpolation = RigidbodyInterpolation.Interpolate;
        holdable.rig.position = rigidbody.position;
        if (holdable.holdableData.setRotation)
        {
            holdable.rig.rotation = Quaternion.LookRotation(unit.data.mainRig.transform.TransformDirection(holdable.holdableData.forwardRotation), unit.data.mainRig.transform.TransformDirection(holdable.holdableData.upRotation));
        }

        rigidbody.GetComponentInChildren<Collider>(true).enabled = false;
        Vector3 vector = holdable.transform.TransformPoint(handSlot.GetComponent<HandRight>() ? holdable.holdableData.rightHand.localPosition : holdable.holdableData.leftHand.localPosition);
        rigidbody.AddForceAtPosition((vector - rigidbody.position).normalized * 100f, rigidbody.position, ForceMode.Acceleration);
        Vector3 vector2 = holdable.transform.position - unit.data.mainRig.position;
        vector2.y *= 3f;
        unit.data.mainRig.AddForce(vector2.normalized * 25f, ForceMode.Acceleration);
        var joint = JointActions.AttachJoint(rigidbody, holdable, vector, handSlot.transform.rotation, false, 0f);
        joints.Add(joint);
        if (handSlot.GetComponent<HandRight>()) { rightJoints.Add(joint); } else { leftJoints.Add(joint); }
        holdable.WasGrabbed(unit.data, rigidbody.transform);
        if (float.IsNaN(rigidbody.velocity.x))
        {
            Debug.LogError("VELOCITY NaN", rigidbody.gameObject);
            rigidbody.velocity = Vector3.zero;
        }
        if (float.IsNaN(holdable.rig.velocity.x))
        {
            Debug.LogError("VELOCITY NaN", holdable.rig.gameObject);
            holdable.rig.velocity = Vector3.zero;
        }

        rigidbody.velocity *= 0f;
        rigidbody.angularVelocity *= 0f;
        holdable.rig.velocity *= 0f;
        holdable.rig.angularVelocity *= 0f;
        holdable.gameObject.AddComponent<StoreHand>().hand = rigidbody;
        if (rigidbody.GetComponent<StoreHand>())
        {
            rigidbody.GetComponent<StoreHand>().weapon = holdable;
        }
        else
        {
            rigidbody.gameObject.AddComponent<StoreHand>().weapon = holdable;
        }

        if (unit.unitBlueprint.holdinigWithTwoHands)
        {
            joints.Add(JointActions.AttachJoint(otherHands.First().GetComponent<Rigidbody>(), holdable, otherHands.First().transform.position, otherHands.First().transform.rotation, true, 0f));
            holdable.WasGrabbed(unit.data, otherHands.First().transform);
            otherHands.Remove(otherHands.First());
        }

        holdable.holdableData.relativePosition.x += unit.unitBlueprint.weaponSeparation;
        if (handSlot.GetComponent<HandLeft>())
        {

            var data = holdable.holdableData;
            data.relativePosition.x *= -1f;
            data.upRotation.x *= -1f;
            data.forwardRotation.x *= -1f;
        }

        holdable.rig.mass *= unit.unitBlueprint.massMultiplier;

        foreach (var team in weapon.GetComponentsInChildren<TeamColor>()) { team.SetTeamColor(unit.Team); }
    }

    public void FixedUpdate()
    {
        if (!unit.data.Dead)
        {
            if (spawnedWeapons.Count > 0)
            {
                foreach (var weapon in spawnedWeapons)
                {
                    if (weapon != null)
                    {
                        DoPID(weapon);
                    }
                }
            }
            foreach (var weapon in existingWeapons)
            {
                if (weapon != null && weapon.GetComponent<Holdable>()) { DoPID(weapon); }
            }
            if (existingWeapons.Count > 0)
            {
                foreach (var weapon in existingWeapons)
                {
                    if (weapon != null)
                    {

                        weapon.GetComponent<Weapon>().UpdateCounters(1f);
                        if (!weapon.GetComponent<Weapon>().IsOnCooldown() && unit.data.distanceToTarget <= weapon.GetComponent<Weapon>().maxRange && unit.data.angleToTarget <= weapon.GetComponent<Weapon>().maxAngle)
                        {
                            weapon.GetComponent<Weapon>().internalCounter = Mathf.Clamp(Random.Range(weapon.GetComponent<Weapon>().internalCooldown * -0.8f, weapon.GetComponent<Weapon>().internalCooldown * 0.2f), -1f, 1f);
                            weapon.GetComponent<Weapon>().Attack(unit.data.targetMainRig.position, unit.data.targetMainRig, Vector3.zero);
                        }
                    }
                }
            }
            if (spawnedWeapons.Count > 0)
            {
                foreach (var weapon in spawnedWeapons)
                {
                    if (weapon != null)
                    {

                        weapon.GetComponent<Weapon>().UpdateCounters(1f);
                        if (!weapon.GetComponent<Weapon>().IsOnCooldown() && unit.data.distanceToTarget <= weapon.GetComponent<Weapon>().maxRange && unit.data.angleToTarget <= weapon.GetComponent<Weapon>().maxAngle)
                        {
                            weapon.GetComponent<Weapon>().internalCounter = Mathf.Clamp(Random.Range(weapon.GetComponent<Weapon>().internalCooldown * -0.8f, weapon.GetComponent<Weapon>().internalCooldown * 0.2f), -1f, 1f);
                            weapon.GetComponent<Weapon>().Attack(unit.data.targetMainRig.position, unit.data.targetMainRig, Vector3.zero);
                        }
                    }
                }
            }
        }
    }

    public void LetGoOfAll(bool destroy = false)
    {
        foreach (var joint in joints)
        {
            Destroy(joint);
        }
        joints.Clear();
        rightJoints.Clear();
        leftJoints.Clear();
        if (spawnedWeapons.Count > 0)
        {
            foreach (var weapon in spawnedWeapons)
            {
                if (weapon != null)
                {
                    weapon.GetComponent<Holdable>().WasDropped();
                    if (destroy) { Destroy(weapon); }
                }
            }
        }
        spawnedWeapons.Clear();
        leftWeapons.Clear();
        rightWeapons.Clear();
    }

    public void LetGoOfSpecific(HoldingHandler.HandType handType, bool destroy = false)
    {
        if (handType == HoldingHandler.HandType.Left)
        {

            foreach (var joint in leftJoints) { Destroy(joint); }
            leftJoints.Clear();
            if (leftWeapons.Count > 0)
            {
                foreach (var weapon in leftWeapons)
                {
                    if (weapon != null)
                    {
                        spawnedWeapons.Remove(weapon);
                        weapon.GetComponent<Holdable>().WasDropped();
                        if (destroy) { Destroy(weapon); }
                    }
                }
                leftWeapons.Clear();
            }
        }
        else if (handType == HoldingHandler.HandType.Right)
        {

            foreach (var joint in rightJoints) { Destroy(joint); }
            rightJoints.Clear();
            if (rightWeapons.Count > 0)
            {
                foreach (var weapon in rightWeapons)
                {
                    if (weapon != null)
                    {
                        spawnedWeapons.Remove(weapon);
                        weapon.GetComponent<Holdable>().WasDropped();
                        if (destroy) { Destroy(weapon); }
                    }
                }
                rightWeapons.Clear();
            }
        }
    }

    public void DoPID(GameObject weapon)
    {
        AddForce(weapon.GetComponent<Holdable>().pidData, unit.data.mainRig.transform.TransformPoint(weapon.GetComponent<Holdable>().holdableData.relativePosition), unit.data.muscleControl * weapon.GetComponent<Holdable>().pidData.holdingForceMultiplier);
        var forwardDirection = weapon.GetComponent<Holdable>().holdableData.forwardRotation;
        if (weapon.GetComponent<Holdable>().pidData.extraUpPerMeter != 0f)
        {
            forwardDirection += Vector3.up * unit.data.distanceToTarget;
        }
        AddTorque(weapon.GetComponent<Holdable>().pidData, unit.data.characterForwardObject.TransformDirection(forwardDirection), unit.data.muscleControl * weapon.GetComponent<Holdable>().pidData.holdingTorqueMultiplier);
        AddTorqueUp(weapon.GetComponent<Holdable>().pidData, unit.data.characterForwardObject.TransformDirection(weapon.GetComponent<Holdable>().holdableData.upRotation), unit.data.muscleControl * weapon.GetComponent<Holdable>().pidData.holdingTorqueMultiplier);
    }

    public void AddForce(PidDataInstance pidData, Vector3 targetPosition, float multiplier = 1f)
    {
        Vector3 a = targetPosition - pidData.rig.position;
        currentForce = a * pidData.proportionalForce;
        pidData.rig.AddForce(currentForce * Time.fixedDeltaTime * 60f * multiplier, ForceMode.Acceleration);
    }

    public void AddTorque(PidDataInstance pidData, Vector3 targetRotation, float multiplier = 1f)
    {
        Vector3 forward = pidData.rig.transform.forward;
        Vector3 vector = Vector3.Angle(targetRotation, forward) * Vector3.Cross(targetRotation, forward).normalized;
        if (pidData.capAngle != 0f)
        {
            vector = Vector3.ClampMagnitude(vector, pidData.capAngle);
        }
        currentTorque = vector * pidData.proportionalTorque;
        pidData.rig.AddTorque(multiplier * Time.fixedDeltaTime * 60f * -currentTorque, ForceMode.Acceleration);
    }

    public void AddTorqueUp(PidDataInstance pidData, Vector3 targetRotation, float multiplier = 1f)
    {
        Vector3 up = pidData.rig.transform.up;
        Vector3 vector = Vector3.Angle(targetRotation, up) * Vector3.Cross(targetRotation, up).normalized;
        if (pidData.capAngle != 0f)
        {
            vector = Vector3.ClampMagnitude(vector, pidData.capAngle);
        }
        currentTorqueUp = vector * pidData.proportionalTorque;
        pidData.rig.AddTorque(multiplier * Time.fixedDeltaTime * 60f * -currentTorqueUp, ForceMode.Acceleration);
    }

    private Unit unit;

    [HideInInspector]
    public List<GameObject> spawnedWeapons = new List<GameObject>();

    [HideInInspector]
    public List<GameObject> leftWeapons = new List<GameObject>();

    private List<GameObject> rightWeapons = new List<GameObject>();

    private List<GameObject> existingWeapons = new List<GameObject>();

    private List<ConfigurableJoint> joints = new List<ConfigurableJoint>();

    private List<ConfigurableJoint> rightJoints = new List<ConfigurableJoint>();

    private List<ConfigurableJoint> leftJoints = new List<ConfigurableJoint>();

    [HideInInspector]
    public List<HandRight> mainHands = new List<HandRight>();

    [HideInInspector]
    public List<HandLeft> otherHands = new List<HandLeft>();

    private Vector3 currentForce;

    private Vector3 currentTorque;

    private Vector3 currentTorqueUp;
    
    public class StoreHand : MonoBehaviour
    {
        public Rigidbody hand;

        public Holdable weapon;
    }
}