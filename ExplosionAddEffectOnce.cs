using UnityEngine;

public class ExplosionAddEffectOnce : ExplosionEffect
{
    public UnitEffectBase EffectPrefab;

    public bool OnlyOnce;

    public float chance = 1f;

    public override void DoEffect(GameObject target)
    {
        EffectPrefab.GetType();
        UnitEffectBase unitEffectBase = UnitEffectBase.AddEffectToTarget(target.gameObject, EffectPrefab);
        if (unitEffectBase == null && chance > Random.value)
        {
            GameObject obj = Object.Instantiate(EffectPrefab.gameObject, target.transform.root);
            obj.transform.position = target.transform.root.position;
            obj.transform.rotation = Quaternion.LookRotation(target.transform.root.position - base.transform.position);
            TeamHolder.AddTeamHolder(obj, base.transform.root.gameObject);
            obj.GetComponent<UnitEffectBase>().DoEffect();
            unitEffectBase = obj.GetComponent<UnitEffectBase>();
            TargetableEffect[] componentsInChildren = unitEffectBase.gameObject.GetComponentsInChildren<TargetableEffect>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].DoEffect(base.transform, target.transform);
            }
        }
        else if (!OnlyOnce)
        {
            unitEffectBase.Ping();
        }
    }
}