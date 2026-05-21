using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Landfall.TABS;
using TGCore.Library;
using UnityEngine;

namespace HiddenUnits;

public class ChainUnits : ProjectileHitEffect
{
    private TeamHolder TeamHolder;
    private Unit OldTarget;
    private List<Effect_ShareDamage> Effects = new List<Effect_ShareDamage>();
    
    [Header("Chain")]
    public GameObject lineObject;
    public int chainCount = 20;
    public float chainRadius = 5f;
    public float removeChainDelay = 5f;
    public float delayPerChain = 0.1f;
    public float chainBreakForce = 10000f;
    
    [Header("Hit")]
    public float damage;
    public UnitEffectBase shareDamageEffect;
    public UnitEffectBase igniteEffect;

    private void Start()
    {
        TeamHolder = GetComponent<TeamHolder>();
    }
    
    public override bool DoEffect(HitData hit)
    {
        OldTarget = hit.transform.root.GetComponent<Unit>();
        if (!OldTarget || 
            OldTarget.Team == TeamHolder.team || 
            OldTarget.data.Dead)
        {
            return false;
        }

        var hasChain = OldTarget.data.mainRig.GetComponent<IsChained>();
        if (!hasChain)
        {
            OldTarget.data.healthHandler.TakeDamage(damage, Vector3.zero);
        
            var chained = OldTarget.data.mainRig.gameObject.AddComponent<IsChained>();
            chained.removeTime = removeChainDelay;
            chained.effect = CreateEffect(OldTarget.transform, shareDamageEffect);
        
            StartCoroutine(DoChains());
        }
        else
        {
            foreach (var effect in hasChain.effect.GetComponent<Effect_ShareDamage>().connectedUnits)
            {
                if (effect != null)
                {
                    CreateEffect(effect.unit.transform, igniteEffect, false);
                }
            }
        }
        return false;
    }

    private IEnumerator DoChains()
    {
        for (var i = 0; i < chainCount; i++)
        {
            var target = SetTarget(OldTarget.data.mainRig.position);
            if (target && target.data && target.data.healthHandler && OldTarget)
            {
                var line = Instantiate(lineObject, OldTarget.transform, true);
                var t1 = line.transform.FindChildRecursive("T1");
                var t2 = line.transform.FindChildRecursive("T2");
                t1.position = OldTarget.data.mainRig.position;
                t1.SetParent(OldTarget.data.torso);
                t2.position = target.data.mainRig.position;
                t2.SetParent(target.data.torso);
                    
                target.data.healthHandler.TakeDamage(damage, Vector3.zero);
                
                var joint = JointActions.AttachJoint(OldTarget.data.mainRig, target.data.mainRig, 360f, 5f, false,
                    false, 5f);
                joint.breakForce = chainBreakForce;
                joint.breakTorque = chainBreakForce;
                
                var chained = target.data.mainRig.gameObject.AddComponent<IsChained>();
                chained.joint = joint;
                chained.removeTime = removeChainDelay;
                chained.effect = CreateEffect(target.transform, shareDamageEffect);
                
                chained.lines.Add(line);
                OldTarget.data.mainRig.GetComponent<IsChained>().lines.Add(line);
                    
                OldTarget = target;
            }

            yield return new WaitForSeconds(delayPerChain);
        }

        foreach (var effect in Effects)
        {
            if (effect != null)
            {
                effect.connectedUnits = Effects.ToArray();
                effect.DoEffect();
            }
        }
    }

    private Unit SetTarget(Vector3 source)
    {
        var hits = Physics.SphereCastAll(transform.position, chainRadius, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
        var foundUnits = hits
            .Select(hit => hit.transform.root.GetComponent<Unit>())
            .Where(x => x && !x.data.Dead && x.Team != TeamHolder.team && !x.data.mainRig.GetComponent<IsChained>())
            .OrderBy(x => (x.data.mainRig.transform.position - source).magnitude)
            .Distinct()
            .ToArray();
        return foundUnits.Length > 0 ? foundUnits[0] : null;
    }

    private GameObject CreateEffect(Transform target, UnitEffectBase effectToCreate, bool isDamageShare = true)
    {
        var effectType = effectToCreate.GetType();
        var foundEffect = (UnitEffectBase)target.GetComponentInChildren(effectType);
        if (foundEffect == null)
        {
            var effect = Instantiate(effectToCreate.gameObject, target);
            effect.transform.position = target.position;
            if (isDamageShare) Effects.Add(effect.GetComponent<Effect_ShareDamage>());
            else effect.GetComponent<UnitEffectBase>().DoEffect();
            return effect.gameObject;
        }
        else
        {
            foundEffect.Ping();
            return foundEffect.gameObject;
        }
    }

    public class IsChained : MonoBehaviour
    {
        private float Counter;
        public float removeTime;
        public ConfigurableJoint joint;
        public GameObject effect;
        public List<GameObject> lines = new List<GameObject>();
        
        private void Update()
        {
            Counter += Time.deltaTime;
            if (Counter >= removeTime)
            {
                if (joint) Destroy(joint);
                BreakChain();
            }
        }

        private void OnJointBreak(float breakForce)
        {
            Debug.Log("Break the chain");
            BreakChain();
        }

        private void BreakChain()
        {
            if (effect) Destroy(effect);
            foreach (var line in lines)
            {
                if (line != null) Destroy(line);
            }
            Destroy(this);
        }
    }
}