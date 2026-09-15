using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sistema de salud reutilizable. Implementa <c>IDamageable</c>.
/// </summary>
public class HealthSystem : MonoBehaviour, IDamageable
{
    [Header("Configuración de Vida")]
    /// <summary>Vida máxima.</summary>
    [SerializeField] private int maxHealth = 1;
    
    [Header("Eventos de Inspector (Visual/Audio)")]
    /// <summary>Evento del Inspector invocado al morir.</summary>
    [SerializeField] private UnityEvent onDied;

    /// <summary>Vida actual interna.</summary>
    private int _currentHealth;
    /// <summary>Indica si la entidad está muerta.</summary>
    private bool _isDead;

    /// <summary>Se dispara cuando cambia la vida: (vidaActual, vidaMaxima).</summary>
    public event Action<int, int> OnHealthChanged;
    /// <summary>Se dispara al recibir daño con la dirección del impacto.</summary>
    public event Action<Vector2> OnDamaged;
    /// <summary>Se dispara cuando la entidad muere.</summary>
    public event Action OnDiedAction;

    /// <summary>Inicializa la vida al máximo.</summary>
    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    /// <summary>
    /// Aplica daño y notifica a los suscriptores.
    /// </summary>
    /// <param name="damage">Cantidad de daño a aplicar.</param>
    /// <param name="hitDirection">Dirección del impacto.</param>
    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (_isDead || damage <= 0) return;

        _currentHealth -= damage;

        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        OnDamaged?.Invoke(hitDirection);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>Marca la entidad como muerta e invoca eventos de muerte.</summary>
    private void Die()
    {
        _isDead = true;
        _currentHealth = 0;

        OnDiedAction?.Invoke();
        onDied?.Invoke();
    }

    /// <summary>Restaura vida hasta el máximo.</summary>
    /// <param name="amount">Cantidad a curar.</param>
    public void Heal(int amount)
    {
        if (_isDead || amount <= 0) return;

        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }
    
    /// <summary>Reinicia vida y estado de muerte (uso con pooling).</summary>
    public void ResetHealth()
    {
        _isDead = false;
        _currentHealth = maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }
}
