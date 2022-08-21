using UnityEngine;
using Landfall.TABS;

namespace HiddenUnits
{
    public class FollowBodyPart : MonoBehaviour
    {
        public void Start()
        {
            if (targetPart == TargetPart.Head)
            {
                chosenPart = transform.root.GetComponent<Unit>().data.head;
            }
            else if (targetPart == TargetPart.Torso)
            {
                chosenPart = transform.root.GetComponent<Unit>().data.torso;
            }
            else if (targetPart == TargetPart.Hip)
            {
                chosenPart = transform.root.GetComponent<Unit>().data.hip.transform;
            }
            else if (targetPart == TargetPart.LegLeft)
            {
                chosenPart = transform.root.GetComponent<Unit>().data.legLeft;
            }
            else if (targetPart == TargetPart.LegRight)
            {
                chosenPart = transform.root.GetComponent<Unit>().data.legRight;
            }
            else if (targetPart == TargetPart.ArmLeft)
            {
                chosenPart = transform.root.GetComponent<Unit>().data.leftArm;
            }
            else if (targetPart == TargetPart.ArmRight)
            {
                chosenPart = transform.root.GetComponent<Unit>().data.rightArm;
            }
            else if (targetPart == TargetPart.KneeLeft)
            {
                chosenPart = transform.root.GetComponent<Unit>().data.footLeft;
            }
            else if (targetPart == TargetPart.KneeRight)
            {
                chosenPart = transform.root.GetComponent<Unit>().data.footRight;
            }
            else if (targetPart == TargetPart.ElbowLeft)
            {
                chosenPart = transform.root.GetComponent<Unit>().data.leftHand;
            }
            else if (targetPart == TargetPart.ElbowRight)
            {
                chosenPart = transform.root.GetComponent<Unit>().data.rightHand;
            }
        }

        public void Update()
        {
            if (chosenPart) {
                transform.position = chosenPart.position;
                transform.rotation = chosenPart.rotation;
            }
        }

        public enum TargetPart
        {
            Head,
            Torso,
            Hip,
            LegLeft,
            LegRight,
            ArmLeft,
            ArmRight,
            KneeLeft,
            KneeRight,
            ElbowLeft,
            ElbowRight
        };

        public TargetPart targetPart;

        private Transform chosenPart;
    }
}
