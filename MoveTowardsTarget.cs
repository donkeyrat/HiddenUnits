using System.Collections.Generic;
using UnityEngine;
using Landfall.TABS;
using System.Linq;

public class MoveTowardsTarget : MonoBehaviour
{
    void Start()
    {
        teamHolder = GetComponent<TeamHolder>();
    }
    
    void Update()
    {
        transform.up = Vector3.up;
        if (target)
        {
            var amountToMove = (new Vector3(target.data.mainRig.position.x, transform.position.y, target.data.mainRig.position.z) - transform.position).normalized;
            transform.position += amountToMove * (Time.deltaTime * moveSpeed);
            if (Vector3.Distance(transform.position, new Vector3(target.data.mainRig.position.x, transform.position.y, target.data.mainRig.position.z)) < 0.5f)
            {
                hitList.Add(target);
            }
        }
        SetTarget();
    }

    public void SetTarget()
    {
        var hits = Physics.SphereCastAll(transform.position, maxRange, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
        var foundUnits = hits
            .Select(hit => hit.transform.root.GetComponent<Unit>())
            .Where(x => teamHolder && x && !x.data.Dead && x.Team != teamHolder.team && !hitList.Contains(x))
            .OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
            .Distinct()
            .ToArray();
        
        if (foundUnits.Length > 0) target = foundUnits[0];
    }

    private Unit target;

    private TeamHolder teamHolder;

    public float maxRange = 100f;

    public float moveSpeed = 1f;

    private List<Unit> hitList = new List<Unit>();
}
