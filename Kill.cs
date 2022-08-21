using UnityEngine;
using System;
using Landfall.TABS;

namespace HiddenUnits
{
    public class Kill : MonoBehaviour
    {
        public void KillYourself()
        {
            transform.root.GetComponent<Unit>().data.healthHandler.Die();
        }
    }
}
