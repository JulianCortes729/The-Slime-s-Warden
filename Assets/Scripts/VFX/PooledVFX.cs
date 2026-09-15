using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Partícula reciclable. Sabe volver a su propio pool cuando termina de
/// reproducirse, sin conocer al VFXManager.
/// </summary>
// ⚠️ SOLID (DIP): dependemos de IObjectPool (abstracción), no del VFXManager
// (implementación concreta). Este script podría vivir en otro proyecto sin cambios.
[RequireComponent(typeof(ParticleSystem))]
public class PooledVFX : MonoBehaviour
{
    // 📖 El campo [SerializeField] impactVFX se ELIMINÓ. Antes hacía falta para
    // decirle al manager "devolveme al pool de los EnemyHit". Ahora el pool nos
    // lo inyectan al nacer, así que el dato es redundante — y un dato redundante
    // es un dato que algún día vas a configurar mal en un prefab.
    private ParticleSystem _particleSystem;
    private IObjectPool<ParticleSystem> _pool;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Inyección de dependencias: la llama el VFXManager al crear la instancia
    /// (una sola vez en toda su vida). Es el "boleto de regreso".
    /// </summary>
    public void Initialize(IObjectPool<ParticleSystem> pool)
    {
        _pool = pool;
    }

    // Unity llama a este callback cuando el ParticleSystem termina de reproducirse.
    // 📖 REQUISITO: el módulo Main del prefab debe tener "Stop Action" en "Callback".
    // Si está en None, este método NUNCA se ejecuta, la partícula no vuelve nunca
    // al pool y a los 10 disparos el pool se agota. Ya verifiqué que tus dos
    // prefabs lo tienen bien (stopAction: 3 en el YAML).
    private void OnParticleSystemStopped()
    {
        // 📖 Chequeo explícito != null. Acá _pool es una interfaz de C# puro
        // (no un UnityEngine.Object), así que ?. funcionaría igual — pero
        // mantenemos un solo criterio en todo el proyecto para no tener que
        // pensar caso por caso cuál corresponde.
        if (_pool != null)
            _pool.Release(_particleSystem);
    }
}
