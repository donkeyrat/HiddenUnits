using System.Collections.Generic;
using UnityEngine;

namespace HiddenUnits
{
    public class HUChangeLayerOfRigibodies : MonoBehaviour
    {
        public bool includeWeapons = true;

        private RigidbodyHolder RigHolder;

        private List<Rigidbody> Rigs;

        private List<int> Layers;

        private List<int> ColliderLayers;

        private Collider[] Colliders;

        private WeaponHandler WeaponHandler;

        private void Start()
        {
            RigHolder = transform.root.GetComponentInChildren<RigidbodyHolder>();
            Rigs = new List<Rigidbody>();
            Layers = new List<int>();
            ColliderLayers = new List<int>();
            Rigs.AddRange(RigHolder.AllRigs);
            Colliders = transform.root.GetComponentInChildren<DataHandler>().transform.GetComponentsInChildren<Collider>();
            WeaponHandler = transform.root.GetComponentInChildren<WeaponHandler>();
            if (WeaponHandler && includeWeapons)
            {
                if ((bool)WeaponHandler.rightWeapon)
                {
                    Rigs.Add(WeaponHandler.rightWeapon.rigidbody);
                }
                if ((bool)WeaponHandler.leftWeapon)
                {
                    Rigs.Add(WeaponHandler.leftWeapon.rigidbody);
                }
            }
        }

        public void ChangeLayer()
        {
            for (int i = 0; i < Rigs.Count; i++)
            {
                if ((bool)Rigs[i])
                {
                    Layers.Add(Rigs[i].gameObject.layer);
                    Rigs[i].gameObject.layer = 20;
                }
            }
            for (int j = 0; j < Colliders.Length; j++)
            {
                if ((bool)Colliders[j])
                {
                    ColliderLayers.Add(Colliders[j].gameObject.layer);
                    Colliders[j].gameObject.layer = 20;
                }
            }
        }

        public void ResetLayer()
        {
            for (int i = 0; i < Rigs.Count; i++)
            {
                if ((bool)Rigs[i])
                {
                    Rigs[i].gameObject.layer = Layers[i];
                }
            }
            for (int j = 0; j < Colliders.Length; j++)
            {
                if ((bool)Colliders[j])
                {
                    Colliders[j].gameObject.layer = ColliderLayers[j];
                }
            }
        }
    }

}