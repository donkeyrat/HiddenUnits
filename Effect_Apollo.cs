using UnityEngine;

namespace HiddenUnits
{
    public class Effect_Apollo : UnitEffectBase
    {
        public override void DoEffect()
        {
        }

        public override void Ping()
        {
        }

        public void Remove()
        {
            if (mat)
            {
                transform.root.GetComponentInChildren<UnitColorHandler>().SetMaterial(mat);
            }
            foreach (var mat in transform.root.GetComponentsInChildren<MaterialEvent>()) { mat.enabled = false; }
        }


        public Material mat;
    }
}
