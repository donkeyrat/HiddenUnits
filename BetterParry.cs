using System.Collections;
using UnityEngine;

namespace HiddenUnits;

public class BetterParry : MonoBehaviour
{
    private ConditionalEvent ConditionalEvent;
    private ParticleSystem Part;
    private Rigidbody OwnWeapon;

    public string soundRef;
    public float parryPower = 1f;
    public float force;

    public void Start()
    {
        Part = GetComponentInChildren<ParticleSystem>();
        ConditionalEvent = GetComponent<ConditionalEvent>();
        if ((bool)ConditionalEvent.data.weaponHandler)
        {
            if ((bool)ConditionalEvent.data.weaponHandler.rightWeapon && (bool)ConditionalEvent.data.weaponHandler.rightWeapon.rigidbody)
            {
                OwnWeapon = ConditionalEvent.data.weaponHandler.rightWeapon.rigidbody;
            }
            else if ((bool)ConditionalEvent.data.weaponHandler.leftWeapon && (bool)ConditionalEvent.data.weaponHandler.leftWeapon.rigidbody)
            {
                OwnWeapon = ConditionalEvent.data.weaponHandler.leftWeapon.rigidbody;
            }
        }
    }

    public void DoParry()
    {
        StartCoroutine(ExecuteParry(ConditionalEvent.cachedEnemyWeapon));
    }

    private IEnumerator ExecuteParry(Rigidbody enemyWeapon)
    {
        if (!enemyWeapon)
        {
            yield break;
        }
        MeleeWeapon component = enemyWeapon.GetComponent<MeleeWeapon>();
        if ((bool)component && !(component.requiredPowerToParry > parryPower))
        {
            component.StopSwing();
            enemyWeapon.AddForce((enemyWeapon.transform.position - ConditionalEvent.data.mainRig.position).normalized * force, ForceMode.VelocityChange);
            Vector3 position = (enemyWeapon.position + OwnWeapon.transform.position) * 0.5f;
            if ((bool)Part && (bool)OwnWeapon)
            {
                Part.transform.position = position;
                Part.Play();
            }
            ServiceLocator.GetService<SoundPlayer>().PlaySoundEffect(soundRef, 1f, position, SoundEffectVariations.MaterialType.Metal);
        }
    }
}