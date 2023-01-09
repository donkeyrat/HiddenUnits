using System.Collections;
using UnityEngine;

namespace HiddenUnits {

    public class HUBinder : MonoBehaviour 
    {

        public static void UnitGlad() 
        {
            if (!instance) 
            {
                instance = new GameObject 
                {
                    hideFlags = HideFlags.HideAndDontSave
                }.AddComponent<HUBinder>();
            }
            instance.StartCoroutine(StartUnitgradLate());
        }

        private static IEnumerator StartUnitgradLate() 
        {
            yield return new WaitUntil(() => FindObjectOfType<ServiceLocator>() != null);
            yield return new WaitUntil(() => ServiceLocator.GetService<ISaveLoaderService>() != null);
            new HUMain();
        }

        private static HUBinder instance;
    }
}