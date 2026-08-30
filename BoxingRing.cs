using Landfall.TABS;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits;

public class BoxingRing : MonoBehaviour
{
    private TeamHolder TeamHolder;
    
    public UnityEvent foundUnitsEvent;
    public UnityEvent noUnitsEvent;
    
    public Transform checkPivot;
    public LayerMask checkLayer;
    public float xCheckSize;
    public float yCheckSize;
    public float zCheckSize;

    private void Start()
    {
        TeamHolder = GetComponent<TeamHolder>();
    }
    
    private void OnTriggerEnter(Collider col)
    {
        var unit = col.transform.root.GetComponent<Unit>();
        if (!unit) return;
        
        var entity = unit.GetComponent<GameObjectEntity>();
        if (!entity.EntityManager.HasComponent(entity.Entity, typeof(TaggedUnitTag)))
        {
            entity.EntityManager.AddComponentData(entity.Entity, new TaggedUnitTag());
        }
    }
    
    private void OnTriggerExit(Collider col)
    {
        var unit = col.transform.root.GetComponent<Unit>();
        if (!unit) return;
        
        var entity = unit.GetComponent<GameObjectEntity>();
        if (entity.EntityManager.HasComponent(entity.Entity, typeof(TaggedUnitTag)))
        {
            entity.EntityManager.RemoveComponent(entity.Entity, typeof(TaggedUnitTag));
        }
    }
    
    public void CheckAround()
    {
        var colliders = Physics.OverlapBox(checkPivot.position, new Vector3(xCheckSize, yCheckSize, zCheckSize), checkPivot.rotation, checkLayer);
        var foundUnit = false;
        foreach (var col in colliders)
        {
            var rootUnit = col.transform.root.GetComponent<Unit>();
            if (rootUnit && (!TeamHolder || TeamHolder.team != rootUnit.Team))
            {
                foundUnit = true;
            }
        }
        
        if (foundUnit) foundUnitsEvent.Invoke();
        else noUnitsEvent.Invoke();
    }
}