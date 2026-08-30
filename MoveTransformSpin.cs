using TGCore.Library;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits;

public class MoveTransformSpin : MonoBehaviour
{
    private float Counter;
    private Transform Target;
    private Vector3 TrackedPosition;
    private MoveTransform MoveTransform;
    private Quaternion StartRotation;
    private bool Returning;
    
    public AnimationCurve rotationProgressCurve;
    
    [Header("Movement")]
    
    public float force = 100f;
    public float returningForce = 600f;
    public float drag = 15f;
    
    [Header("Return")]
    
    public UnityEvent startReturnEvent;
    public UnityEvent returnEvent;
    public float returnDelay = 2f;
    public float returnThreshold = 0.2f;
    
    private void Start()
    {
        var teamHolder = GetComponent<TeamHolder>();
        if (teamHolder && teamHolder.spawnerWeapon)
        {
            Target = teamHolder.spawnerWeapon.transform;
        }
        else
        {
            TrackedPosition = transform.position;
        }

        StartRotation = transform.rotation;

        MoveTransform = GetComponent<MoveTransform>();
    }

    private void Update()
    {
        if (Target) TrackedPosition = Target.transform.position;
        
        Counter += Time.deltaTime;
        
        MoveTransform.velocity += transform.forward * (force * Time.deltaTime);
        MoveTransform.velocity -= MoveTransform.velocity * (drag * Time.deltaTime);
        
        if (Counter > returnDelay && !Returning)
        {
            Return();
        }
        else if (Returning)
        {
            var direction = (TrackedPosition - transform.position).normalized;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, returningForce * Time.deltaTime);

            if (Vector3.Distance(transform.position, TrackedPosition) < returnThreshold)
            {
                if (Target)
                {
                    var returnableProjectileEvent = Target.GetComponentInParent<ReturnableProjectileEvent>();
                    if (returnableProjectileEvent) returnableProjectileEvent.Go();
                }
                returnEvent.Invoke();
                Destroy(gameObject);
            }
        }
        else
        {  
            transform.rotation = new Quaternion(StartRotation.x, StartRotation.y + rotationProgressCurve.Evaluate(Counter), StartRotation.z, StartRotation.w);
        }
    }

    public void Return()
    {
        startReturnEvent.Invoke();
        Returning = true;
    }
    
    public void SetForce(float value)
    {
        returningForce = value;
    }
    
}