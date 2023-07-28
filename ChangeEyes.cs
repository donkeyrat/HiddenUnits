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
            OriginalEye = EyeSet.eyeObject;
        }

        public void DoEyeChange()
        {
            if (EyeSet)
            {
                foreach (var eye in EyeSet.spawnedEyes)
                {
                    GooglyEyes.instance.eyes.Remove(eye);
                    Destroy(eye.gameObject);
                }
                EyeSet.spawnedEyes.Clear();
                    
                foreach (var eyes in EyeSet.eyeSets) eyes.useCurves = false;
                
                EyeSet.eyeObject = newEye;
                EyeSet.GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(EyeSet, new object[] { });
            }
        }

        public void DoEyeReset()
        {
            if (EyeSet)
            {
                foreach (var eye in EyeSet.spawnedEyes)
                {
                    GooglyEyes.instance.eyes.Remove(eye);
                    Destroy(eye.gameObject);
                }
                EyeSet.spawnedEyes.Clear();
                    
                EyeSet.eyeObject = OriginalEye;
                EyeSet.GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(EyeSet, new object[] { });
            }
        }
        
        private Unit OwnUnit;
        private EyeSpawner EyeSet;
        private GameObject OriginalEye;

        public GameObject newEye;
    }
}