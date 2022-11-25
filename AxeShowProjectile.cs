using UnityEngine;
using Landfall.TABS;
using System.Collections;
using System.Collections.Generic;

namespace HiddenUnits {

    public class AxeShowProjectile : MonoBehaviour {

        private void Start() 
        {
            spawned = Instantiate(GetComponent<AxeThrow>().objectToSpawn, pivot.position, pivot.rotation);
            if (GetComponent<RangeWeapon>() && GetComponent<RangeWeapon>().connectedData != null)
            {
                var componentsInChildren = spawned.GetComponentsInChildren<Renderer>(); 
                GetComponent<RangeWeapon>().connectedData.unit.AddRenderersToShowHide(componentsInChildren, GetComponent<ShowProjectile>().IsInBlindGame);
            }

            foreach (var rig in spawned.GetComponentsInChildren<Rigidbody>())
            {
                rig.isKinematic = true;
                if (rig.GetComponent<Joint>()) Destroy(rig.GetComponent<Joint>());
                Destroy(rig);
            }
            
            foreach (var mono in spawned.GetComponentsInChildren<MonoBehaviour>()) 
            {
                if (!(mono is SetTeamColorOnStart) && !(mono is TeamColor)) Destroy(mono);
            }
            
            spawned.transform.SetParent(pivot, true);
            spawned.transform.localScale = Vector3.one;
            
            foreach (var particle in spawned.GetComponentsInChildren<ParticleSystem>()) 
            {
                var idleParticle = particle.GetComponent<IdleBowParticle>();
                if (idleParticle) particle.Play();
                else Destroy(particle);
            }
        }

        private GameObject spawned;

        public Transform pivot;
    }
}
