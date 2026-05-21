using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits;

public class DamageResistPart : MonoBehaviour
{
    private float CurrentHealth;
    
    public DamageResistance parent;
    
    public int weight = 1;
    public float health = 500f;
    public float damageResistToRemoveOnFall = 0.1f;
    
    public UnityEvent fallOffEvent;
    
    private Renderer VisualRenderer;
    
    private void Start()
    {
        CurrentHealth = health;
        VisualRenderer = GetComponentInChildren<MeshRenderer>();
        VisualRenderer.material.color = Color.Lerp(Color.red, Color.green, CurrentHealth / health);
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        VisualRenderer.material.color = Color.Lerp(Color.red, Color.green, CurrentHealth / health);
        if (CurrentHealth <= 0) RemovePart();
    }
    
    public void RemovePart()
    {
        parent.resistPercentage =
            Mathf.Clamp(parent.resistPercentage - damageResistToRemoveOnFall, 0f, 1f);
        parent.damageResistParts.Remove(this);
        
        fallOffEvent.Invoke();
    }
}