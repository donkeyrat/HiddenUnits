using UnityEngine;

namespace HiddenUnits
{
    public class ChangeOutline : MonoBehaviour
    {
        public void Start()
        {
            if (transform.root.GetComponent<Outline>())
            {
                var outline = transform.root.GetComponent<Outline>();
                outline.OutlineMode = outlineMode;
                outline.SetHighlightColor(outlineColor);
                outline.OutlineWidth = outlineWidth;
            }
        }

        public Outline.Mode outlineMode;

        public Color outlineColor = Color.white;

        public float outlineWidth = 1f;
    }
}