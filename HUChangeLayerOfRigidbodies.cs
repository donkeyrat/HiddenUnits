using System.Collections.Generic;
using UnityEngine;

namespace HiddenUnits
{
    public class HUChangeLayerOfRigibodies : MonoBehaviour
    {
        public bool includeWeapons = true;

        private RigidbodyHolder rigHolder;

        private List<Rigidbody> rigs;

        private List<int> layers;

        private List<int> colliderLayers;

        private Collider[] colliders;

        private WeaponHandler weaponHandler;

        private void Start()
        {
            rigHolder = base.transform.root.GetComponentInChildren<RigidbodyHolder>();
            rigs = new List<Rigidbody>();
            layers = new List<int>();
            colliderLayers = new List<int>();
            rigs.AddRange(rigHolder.AllRigs);
            colliders = base.transform.root.GetComponentInChildren<DataHandler>().transform.GetComponentsInChildren<Collider>();
            weaponHandler = base.transform.root.GetComponentInChildren<WeaponHandler>();
            if (weaponHandler && includeWeapons)
            {
                if ((bool)weaponHandler.rightWeapon)
                {
                    rigs.Add(weaponHandler.rightWeapon.rigidbody);
                }
                if ((bool)weaponHandler.leftWeapon)
                {
                    rigs.Add(weaponHandler.leftWeapon.rigidbody);
                }
            }
        }

        public void ChangeLayer()
        {
            for (int i = 0; i < rigs.Count; i++)
            {
                if ((bool)rigs[i])
                {
                    layers.Add(rigs[i].gameObject.layer);
                    rigs[i].gameObject.layer = 20;
                }
            }
            for (int j = 0; j < colliders.Length; j++)
            {
                if ((bool)colliders[j])
                {
                    colliderLayers.Add(colliders[j].gameObject.layer);
                    colliders[j].gameObject.layer = 20;
                }
            }
        }

        public void ResetLayer()
        {
            for (int i = 0; i < rigs.Count; i++)
            {
                if ((bool)rigs[i])
                {
                    rigs[i].gameObject.layer = layers[i];
                }
            }
            for (int j = 0; j < colliders.Length; j++)
            {
                if ((bool)colliders[j])
                {
                    colliders[j].gameObject.layer = colliderLayers[j];
                }
            }
        }
    }

}