using System.Collections.Generic;
using UnityEngine;
using Landfall.TABS;
using System.Linq;

public class MoveTowardsTarget : MonoBehaviour
{
    void Update()
    {
        transform.up = Vector3.up;
        if (target)
        {
            transform.position += (new Vector3(target.data.mainRig.position.x, transform.position.y, target.data.mainRig.position.z) - transform.position).normalized * Time.deltaTime * moveSpeed;
            if (Vector3.Distance(transform.position, new Vector3(target.data.mainRig.position.x, transform.position.y, target.data.mainRig.position.z)) < 0.5f)
            {
                hitList.Add(target);
                SetTarget();
            }
        }
        else
        {
            SetTarget();
        }
    }

    public void SetTarget()
    {
        var hits = Physics.SphereCastAll(transform.position, maxRange, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
        List<Unit> foundUnits = new List<Unit>();
        foreach (var hit in hits)
        {
            if (hit.transform.root.GetComponent<Unit>() && !foundUnits.Contains(hit.transform.root.GetComponent<Unit>()))
            {
                foundUnits.Add(hit.rigidbody.transform.root.GetComponent<Unit>());
            }
        }
        Unit[] query
        = (
          from Unit unit
          in foundUnits
          where !unit.data.Dead && unit.Team != GetComponent<TeamHolder>().team && !hitList.Contains(unit)
          orderby (unit.data.mainRig.transform.position - transform.position).magnitude
          select unit
        ).ToArray();
        if (query.Length > 0)
        {
            target = query[0];
        }
    }

    private Unit target;

    public float maxRange = 100f;

    public float moveSpeed = 1f;

    private List<Unit> hitList = new List<Unit>();
}
