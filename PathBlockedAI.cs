using Unity.Entities;
using UnityEngine;

namespace HiddenUnits;

public class PathBlockedAI : MonoBehaviour
{
    public LayerMask raycastMask;
    public float raycastDistance = 3f;
    
    private void Start()
    {
        var entity = GetComponent<GameObjectEntity>();
        if (!entity) gameObject.AddComponent<GameObjectEntity>();
        var entityManager = entity.EntityManager;
        entityManager?.AddComponentData(entity.Entity, new PathBlocked
        {
            raycastMask = raycastMask,
            distance = raycastDistance
        });
    }
}