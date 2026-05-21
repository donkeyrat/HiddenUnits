using Landfall.TABS;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits;

public class ProjectileHitHades : ProjectileHitEffect
{
    private HadesEgg Egg;
    private TeamHolder TeamHolder;

    public float healthToAdd;
    public GameObject particleToSpawn;
    
    private void Start()
    {
        TeamHolder = GetComponent<TeamHolder>();
        if (TeamHolder && TeamHolder.spawner)
        {
            Egg = TeamHolder.spawner.transform.root.GetComponentInChildren<HadesEgg>();
        }
    }
    
    public override bool DoEffect(HitData hit)
    {
        var unit = hit.transform.root.GetComponent<Unit>();
        if (!unit) return false;
        
        var spawnedParticle =  Instantiate(particleToSpawn, hit.point, Quaternion.LookRotation(hit.normal));

        if (Egg)
        {
            spawnedParticle.GetComponentInChildren<ParticleSystem>().externalForces.AddInfluence(Egg.soulField);
            if (TeamHolder)
            {
                TeamHolder.AddTeamHolder(spawnedParticle, TeamHolder.spawner);
            }
            Egg.AddHealth(healthToAdd);
        }
        
        return false;
    }
}