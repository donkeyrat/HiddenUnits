using System.Reflection;
using Landfall.TABS;
using UnityEngine;

namespace HiddenUnits
{
    public class ChangeEyes : MonoBehaviour
    {
        private void Start()
        {
            OwnUnit = transform.root.GetComponent<Unit>();
            EyeSet = OwnUnit.GetComponentInChildren<EyeSpawner>();
        }

        public void DisableEyes()
        {
            EyeSet?.SetEyesActive(false);
        }

        public void EnableEyes()
        {
            EyeSet?.SetEyesActive(true);
        }
        
        private Unit OwnUnit;
        private EyeSpawner EyeSet;
    }
}