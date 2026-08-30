using System.Linq;
using UnityEngine;

namespace HiddenUnits 
{ 
    public class AttachWebToMage : MonoBehaviour 
    {
       public void Start()
       {
           Attach();
       }

        public void Attach()
        {
            var teamHolders = transform.GetComponentsInParent<TeamHolder>();
            if (teamHolders.Length <= 0) return;
            
            var teamHolderWithWeapon = teamHolders.Where(x => x.spawnerWeapon != null).ToArray();
            if (teamHolderWithWeapon.Length <= 0) return;
            
            transform.SetParent(teamHolderWithWeapon[0].spawnerWeapon.transform);
        }
    }
}
