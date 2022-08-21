using UnityEngine;
using Landfall.TABS;

namespace HiddenUnits
{
    public class FindCodeAnimation : MonoBehaviour
    {
        public void Find()
        {
            transform.root.GetComponent<Unit>().data.head.GetComponentInChildren<CodeAnimation>().PlayIn();
        }
    }
}
