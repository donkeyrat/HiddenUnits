using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class TriggerChickenStickRoam : MonoBehaviour
    {
        public void StartRoaming()
        {
            foreach (var chickenStick in transform.root.GetComponentsInChildren<ChickenSticks>())
            {
                chickenStick.Roam();
            }
            StartCoroutine(OnStartRoaming());
        }

        public IEnumerator OnStartRoaming()
        {
            yield return new WaitUntil(() =>
                transform.root.GetComponentsInChildren<ChickenSticks>().ToList()
                    .TrueForAll(x => x.currentState == ChickenSticks.ClubState.Roaming));
            roamBeginEvent.Invoke();
            yield return new WaitUntil(() =>
                transform.root.GetComponentsInChildren<ChickenSticks>().ToList()
                    .TrueForAll(x => x.currentState == ChickenSticks.ClubState.Idle));
            roamEndEvent.Invoke();
        }

        public UnityEvent roamBeginEvent = new UnityEvent();
        
        public UnityEvent roamEndEvent = new UnityEvent();
    }
}