using UnityEngine;

namespace HiddenUnits
{
    public class EnableRandomChild : MonoBehaviour
    {
        void Start()
        {
            if (randomizeOnStart) { RandomizeChildren(); }
        }

        public void RandomizeChildren()
        {
            transform.GetChild(Random.Range(0, transform.childCount - 1)).gameObject.SetActive(true);
        }

        public void DisableChildren()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public bool randomizeOnStart;
    }
}
