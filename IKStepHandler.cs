using System.Collections;
using System.Collections.Generic;
using Landfall.TABS;
using RootMotion.FinalIK;
using UnityEngine;

namespace HiddenUnits
{
    public class IKStepHandler : MonoBehaviour
    {
        private void Start()
        {
            ownUnit = transform.root.GetComponent<Unit>();
            anim = ownUnit.data.GetComponent<AnimationHandler>();
            mainRig = ownUnit.data.mainRig;
            
            if (!forwardNormal) forwardNormal = ownUnit.data.characterForwardObject;
    
            foreach (var target in targetPositions)
            {
                target.position += forwardNormal.forward * stepDistance;
            }
            
            for (int i = 0; i < followerHolder.childCount; i++)
            {
                originalFollowerPositions.Add(followerHolder.GetChild(i).localPosition);
            }
        }
    
        private void Update()
        {
            if (ownUnit.data.Dead)
            {
                ResetLegs();
                return;
            }
            if (anim.currentState == 0) return;
    
            var raycast = Physics.Raycast(mainRig.position, -mainRig.transform.up, distanceFromGround, groundMask);
            
            if (!raycast && followerHolder.parent != mainRig.transform) ResetLegs();
            else if (raycast && followerHolder.parent != ownUnit.transform) followerHolder.SetParent(ownUnit.transform);
    
            counter += Time.deltaTime;
            
            if (distanceBetweenSteps <= Vector3.Distance(mainRig.position, mainRigPositionAtStep) && cooldown <= counter)
            {
                counter = 0f;
                mainRigPositionAtStep = mainRig.position;
                
                StartCoroutine(DoLegMovement(currentLegIndex));
                
                currentLegIndex++;
                if (currentLegIndex >= legs.Count) currentLegIndex = 0;
            }
        }
    
        private IEnumerator DoLegMovement(int legIndex)
        {
            var moveTarget = legs[legIndex].solver.target;
    
            var targetPos = targetPositions[legIndex].position;
            var origin = new Vector3(targetPos.x, mainRig.position.y, targetPos.z);
            if (!Physics.Raycast(origin, (targetPos - origin).normalized, out var hitInfo, distanceFromGround, groundMask)) yield break;
    
            var movePos = hitInfo.point;
            
            var t = 0f;
            var startPos = moveTarget.position;
            while (t < stepUpCurve.keys[stepUpCurve.keys.Length - 1].time)
            {
                t += Time.deltaTime * stepSpeed;
                moveTarget.position = Vector3.Lerp(startPos, movePos + Vector3.up * stepUpCurve.Evaluate(t), Mathf.Clamp(t, 0f, 1f));
                yield return null;
            }
        }
    
        public void ResetLegs()
        {
            var savedPositions = new List<Vector3>();
            for (int i = 0; i < followerHolder.childCount; i++)
            {
                savedPositions.Add(followerHolder.GetChild(i).position);
            }
                
            followerHolder.SetParent(mainRig.transform);
            followerHolder.localPosition = Vector3.zero;
            followerHolder.localRotation = Quaternion.identity;
                
            for (int i = 0; i < followerHolder.childCount; i++)
            {
                followerHolder.GetChild(i).position = savedPositions[i];
                StartCoroutine(LerpTransformLocally(followerHolder.GetChild(i), originalFollowerPositions[i], 2f));
            }
        }
        
        private IEnumerator LerpTransformLocally(Transform lerper, Vector3 endPos, float speed)
        {
            var t = 0f;
            var startPos = lerper.localPosition;
            while (t < 1f)
            {
                t += Time.deltaTime * speed;
                lerper.localPosition = Vector3.Lerp(startPos, endPos, Mathf.Clamp(t, 0f, 1f));
                yield return null;
            }
        }
    
        private Unit ownUnit;
        private AnimationHandler anim;
        private Rigidbody mainRig;
    
        private int currentLegIndex;
    
        private Vector3 mainRigPositionAtStep;
    
        private float counter;
    
        [Header("Walking Settings")]
        
        public List<LimbIK> legs = new List<LimbIK>();
        public List<Transform> targetPositions = new List<Transform>();
        
        public Transform followerHolder;
        private List<Vector3> originalFollowerPositions = new List<Vector3>();
        
        public Transform forwardNormal;
    
        public float distanceBetweenSteps = 2f;
        public float cooldown = 0.1f;
    
        [Header("Leg Settings")] 
        
        public float stepDistance;
        public AnimationCurve stepUpCurve = new AnimationCurve();
        public float stepSpeed;
        public float distanceFromGround = 1f;
    
        public LayerMask groundMask;
    }
}