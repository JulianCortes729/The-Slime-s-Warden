using UnityEngine;

/// <summary>
/// Abstracción del pool de balas. Permite que PlayerCombat y EnemyShooter
/// compartan la misma implementación sin duplicar código.
/// </summary>
public interface IBulletPool
{
    Bullet Get(Vector2 position, Vector2 direction);
    void Release(Bullet bullet);
}
