using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits;

public class DamageResistPart : MonoBehaviour
{
    private float CurrentHealth;
    private bool DidDamageEvent;
    
    public DamageResistance parent;
    
    public int weight = 1;
    public float health = 500f;
    public float damageResistToRemoveOnFall = 0.1f;

    public float thresholdToDamage = 0.5f;
    public Gradient healthGradient;
    public int materialIndex;
    
    public UnityEvent fallOffEvent;
    public UnityEvent damageEvent;
    
    private Renderer VisualRenderer;
    
    private void Start()
    {
        CurrentHealth = health;
        VisualRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        VisualRenderer.materials[materialIndex].color = healthGradient.Evaluate(CurrentHealth / health);
        
        if (CurrentHealth <= 0) RemovePart();
        else if (CurrentHealth / health <= thresholdToDamage && !DidDamageEvent)
        {
            damageEvent.Invoke();
            DidDamageEvent = true;
        }
    }
    
    public void RemovePart()
    {
        parent.resistPercentage =
            Mathf.Clamp(parent.resistPercentage - damageResistToRemoveOnFall, 0f, 1f);
        parent.damageResistParts.Remove(this);
        
        fallOffEvent.Invoke();
    }

    public void SetRenderer(Renderer newRenderer)
    {
        VisualRenderer = newRenderer;
        VisualRenderer.materials[materialIndex].color = healthGradient.Evaluate(CurrentHealth / health);
    }
}