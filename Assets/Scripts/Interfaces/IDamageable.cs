using UnityEngine;


/// Contrato para cualquier entidad que pueda recibir daño.
/// Desacopla las balas de conocer la implementación concreta del enemigo/jugador.
public interface IDamageable
{
    void TakeDamage(int damage, Vector2 hitDirection);
}
