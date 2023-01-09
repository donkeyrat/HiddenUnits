using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits 
{ 
    public class MeleeWeaponSpawnParented : CollisionWeaponEffect
    {
        public enum Rot
        {
            TowardsHit,
            Normal,
            InverseNormal
        }
    
        public enum Pos
        {
            TransformPos,
            ContactPoint
        }
    
        public Rot rot;
    
        public Pos pos;
    
        public GameObject objectToSpawn;
    
        public float cd = 0.1f;
    
        private float counter;
    
        public UnityEvent SpawnEvent;
    
        private void Update()
        {
            counter += Time.deltaTime;
        }
    
        public override void DoEffect(Transform hitTransform, Collision collision)
        {
            if (!(counter < cd))
            {
                counter = 0f;
                Quaternion rotation = Quaternion.identity;
                if (rot == Rot.Normal)
                {
                    rotation = Quaternion.LookRotation(collision.contacts[0].normal);
                }
                if (rot == Rot.InverseNormal)
                {
                    rotation = Quaternion.LookRotation(-collision.contacts[0].normal);
                }
                if (rot == Rot.TowardsHit)
                {
                    rotation = Quaternion.LookRotation(hitTransform.position - base.transform.position);
                }
                Vector3 position = Vector3.zero;
                if (pos == Pos.TransformPos)
                {
                    position = base.transform.position;
                }
                if (pos == Pos.ContactPoint)
                {
                    position = collision.contacts[0].point;
                }
                TeamHolder.AddTeamHolder(Instantiate(objectToSpawn, position, rotation, transform), base.gameObject);
                SpawnEvent.Invoke();
            }
        }
    }
}