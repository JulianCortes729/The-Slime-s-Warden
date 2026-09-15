using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerBlackboard blackboard;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float knockbackForce = 5f;

    [SerializeField] private PlayerVisuals playerVisuals;

    [SerializeField] private IntEventChannel _playerHealthChannel;
    [SerializeField] private VoidEventChannel _playerDeathChannel;

    private void Awake()
    {
        if (blackboard == null) blackboard = GetComponent<PlayerBlackboard>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (playerVisuals == null) playerVisuals = GetComponent<PlayerVisuals>();
        if (_playerHealthChannel == null) Debug.LogError("PlayerHealth: No se ha asignado el PlayerHealthChannel en el inspector.");
        if (_playerDeathChannel == null) Debug.LogError("PlayerHealth: No se ha asignado el PlayerDeathChannel en el inspector.");
    }

    void Start(){
        _playerHealthChannel.RaiseEvent(blackboard.currentLives);
    }

    public void TakeDamage(int damage, Vector2 knockbackDir)
    {
        // 1. Guard Clause: Si ya soy invulnerable, cancelo la ejecución inmediatamente.
        if (blackboard.isInvulnerable) return;
        if (blackboard.isDead) return;
        // 2. Restar vida
        blackboard.currentLives -= damage;

        // 3. Llamar al evento de salud del jugador
        _playerHealthChannel.RaiseEvent(blackboard.currentLives);

        // 4. Comprobar si murió
        if (blackboard.currentLives <= 0)
        {
            Die();
            return;
        }

        Vector2 pureDirection = new Vector2(knockbackDir.x, 1f).normalized;
        // 4. Si sobrevivió, aplicar efectos e I-Frames
        // TODO: Aplicar fuerza de Knockback usando knockbackDir
        rb.AddForce(pureDirection * knockbackForce, ForceMode2D.Impulse);

        //Llenamos el temporizador de aturdimiento físico al máximo.
        //Esto le avisará a PlayerMovement que no lea inputs por 0.2s, 
        //dándole tiempo a la fuerza física del AddForce a hacer efecto.
        blackboard.knockbackTimer = blackboard.knockbackDuration;

        // TODO: Llamar al sistema visual para que parpadee
        playerVisuals.StartInvulnerability();
        StartCoroutine(InvulnerabilityRoutine());
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        // Encendemos el "escudo"
        blackboard.isInvulnerable = true;

        // Esperamos el tiempo de invulnerabilidad
        yield return new WaitForSeconds(blackboard.invulnerabilityTime);

        // Apagamos el "escudo"
        blackboard.isInvulnerable = false;
        playerVisuals.StopInvulnerability();
    }

    private void Die()
    {
        blackboard.isDead = true;
        _playerDeathChannel.RaiseEvent();
    }


}
