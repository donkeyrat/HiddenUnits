using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class AchillesArmorEvent : MonoBehaviour
    {
        public void OnArmorActivated()
        {
            armorActivationEvent.Invoke();
        }

        public void OnArmorDeactivated()
        {
            armorDeactivationEvent.Invoke();
        }

        public UnityEvent armorActivationEvent = new UnityEvent();
        
        public UnityEvent armorDeactivationEvent = new UnityEvent();
    }
}