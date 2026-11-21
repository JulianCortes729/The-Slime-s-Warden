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

    /// <summary>Inicializa la vida al máximo.</summary>
    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    /// <summary>
    /// Aplica daño; al llegar a 0, muere.
    /// </summary>
    /// <param name="damage">Cantidad de daño a aplicar.</param>
    /// <param name="hitDirection">Dirección del impacto.</param>
    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (_isDead || damage <= 0) return;

        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>Marca la entidad como muerta e invoca onDied (Inspector).</summary>
    private void Die()
    {
        _isDead = true;
        _currentHealth = 0;

        onDied?.Invoke();
    }
}
