using System.Collections.Generic;
using System.Linq;
using Landfall.TABS;
using TGCore.Library;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits;

public class SendUnitsElsewhere : MonoBehaviour
{
    private Unit OwnUnit;
    private List<Unit> SuckedUpUnits = new  List<Unit>();
    private Dictionary<UnitEffectBase, float> AddedEffects = new Dictionary<UnitEffectBase, float>();
    
    public float radius;
    public LayerMask layer;
    public UnitEffectBase effectToGive;
    public Transform placeToSendTo;
    public UnityEvent sendUnitsEvent;
    
    public float maximumHealthToSuck = 500f;

    private void Start()
    {
        OwnUnit = transform.root.GetComponent<Unit>();
    }
    
    public void EatUnits()
    {
        var unitsToEat = Physics.SphereCastAll(transform.position, radius, Vector3.up, 0.1f, layer)
            .Select(hit => hit.transform.root.GetComponent<Unit>())
            .Where(x => !x.data.Dead)
            .Where(x => x && !x.data.Dead && x.Team != OwnUnit.Team && !x.GetComponentInChildren<FreezeRigs>() && x.data.maxHealth < maximumHealthToSuck)
            .OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
            .Distinct()
            .ToArray();
        foreach (var unit in unitsToEat)
        {
            AddedEffects.Add(CreateEffect(unit.transform), unit.targetingPriorityMultiplier);
            SuckedUpUnits.Add(unit);
        }
    }

    public void SpitOutUnits()
    {
        var spatOutUnits = false;
        foreach (var unit in SuckedUpUnits.Where(unit => unit))
        {
            spatOutUnits = true;
            unit.data.sinceGrounded = 0f;
            unit.data.fallTime = 0f;
            
            var vector = placeToSendTo.position - unit.data.mainRig.transform.position;
            
            for (var j = 0; j < unit.data.transform.childCount; j++)
            {
                var child = unit.data.transform.GetChild(j);
                child.position += vector;
            }
        }
        SuckedUpUnits.Clear();
        ReleaseSuckedUpUnits();
        
        if (spatOutUnits) sendUnitsEvent.Invoke();
    }
    

    public void ReleaseSuckedUpUnits()
    {
        foreach (var effect in AddedEffects.Where(effect => effect.Key))
        {
            effect.Key.GetComponent<FreezeRigs>().UnFreeze();
            effect.Key.GetComponent<HideRenderers>().UnHideAll();
            effect.Key.GetComponent<SendRootUnitToHell>().EnablePossession();
            var entity = effect.Key.transform.root.GetComponent<GameObjectEntity>();
            if (World.Active != null && entity && entity.EntityManager != null) effect.Key.GetComponent<SetTargetingPriority>().SetPriority(effect.Value);
            effect.Key.GetComponent<ChangeLayerOfChildren>().ResetLayer();
        }
        AddedEffects.Clear();
    }
    
    
    private UnitEffectBase CreateEffect(Transform target)
    {
        var effectType = effectToGive.GetType();
        var foundEffect = (UnitEffectBase)target.GetComponentInChildren(effectType);
        if (foundEffect == null)
        {
            var effect = Instantiate(effectToGive.gameObject, target);
            effect.transform.position = target.position;
            var effectBase = effect.GetComponent<UnitEffectBase>();
            effectBase.DoEffect();
            return effectBase;
        }
        else
        {
            foundEffect.Ping();
            return foundEffect;
        }
    }

    private void OnDestroy()
    {
        ReleaseSuckedUpUnits();
    }
}