using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class RandomChance : MonoBehaviour
    {
        public void Start()
        {
            if (onStart) { Randomize(); }
        }

        public void Randomize()
        {
            if (Random.value > randomChance)
            {
                randomEvent.Invoke();
            }
        }

        public UnityEvent randomEvent;

        public float randomChance;

        public bool onStart;
    }
}
