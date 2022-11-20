using UnityEngine;
using System;
using Landfall.TABS;

namespace HiddenUnits
{
    public class Kill : MonoBehaviour
    {
        public void KillYourself()
        {
            if (transform.root.GetComponent<Unit>()) transform.root.GetComponent<Unit>().data.healthHandler.Die();
            if (destroy) transform.root.GetComponent<Unit>().DestroyUnit();
        }

        public bool destroy;
    }
}
