using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Landfall.TABS;
using UnityEngine;

namespace HiddenUnits;

public class DamageResistance : MonoBehaviour
{
    private void Start()
    {
        Unit = transform.root.GetComponent<Unit>();
        Unit.WasDealtDamageAction += ResistDamage;
    }

    private void Update()
    {
        
    }

    public void ResistDamage(float damage)
    {
        damage = Mathf.Clamp(damage, 0f, 999999f);
        Unit.data.health += damage * resistPercentage;

        var total = damageResistParts.Sum(damageResistPart => damageResistPart.weight);

        var random = Random.Range(0, total);
        var indexToDamage = -1;
        for (var i = 0; i < damageResistParts.Count; i++)
        {
            if (random < damageResistParts[i].weight)
            {
                indexToDamage = i;
                break;
            }
            random -= damageResistParts[i].weight;
        }

        if (indexToDamage >= 0)
        {
            damageResistParts[indexToDamage].TakeDamage(damage);
        }
    }

    private Unit Unit;
    
    public List<DamageResistPart> damageResistParts = new List<DamageResistPart>();

    public float resistPercentage = 0.5f;
}
