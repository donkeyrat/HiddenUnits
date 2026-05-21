using System.Collections;
using Landfall.TABS;
using Landfall.TABS.AI;
using TGCore;
using Unity.Entities;
using UnityEngine;

namespace HiddenUnits;

public class SpawnedUnitEntityData : MonoBehaviour
{
    private UnitAPI UnitAPI;
    
    public UnitSpawner spawner;
    
    public void Start()
    {
        UnitAPI = (transform.root.GetComponent<Unit>()? transform.root.GetComponent<Unit>() : GetComponent<TeamHolder>().spawner.transform.root.GetComponent<Unit>()).api;
        spawner.spawnUnitAction += SetEntityData;
    }

    private void SetEntityData(GameObject unitObject)
    {
        StartCoroutine(WaitForEntity(unitObject));
    }

    private IEnumerator WaitForEntity(GameObject unitObject)
    {
        yield return new WaitUntil(() => (GameObjectEntity)UnitAPI.GetField("m_goEntity"));
        var entity = unitObject.GetComponent<GameObjectEntity>();
        
        if (!entity)
        {
            entity = unitObject.AddComponent<GameObjectEntity>();
        }
        entity.EntityManager?.AddComponentData(entity.Entity, new PredictedPositionSpawner()
        {
            Value = UnitAPI.GetPredictedPosition()
        });
        unitObject.AddComponent<GetSpawnerPredictedPosition>().spawnerAPI = UnitAPI;
    }

    public class GetSpawnerPredictedPosition : MonoBehaviour
    {
        public UnitAPI spawnerAPI;

        public Vector3 GetPredictedPosition()
        {
            return spawnerAPI.GetPredictedPosition();
        }
    }
}