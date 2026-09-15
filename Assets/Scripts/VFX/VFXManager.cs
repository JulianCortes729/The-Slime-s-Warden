using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// FIX #16: eliminado "using JetBrains.Annotations" — no se usaba ningún símbolo de ese namespace

public enum VFXType
{
    PlayerHit,
    EnemyHit
}

[Serializable]
public struct VFXSetup
{
    public ParticleSystem prefab;
    public VFXType VFXType;
}

/// <summary>
/// Servicio de efectos visuales. Escucha un VFXCueChannel y mantiene un pool
/// de ParticleSystems por cada VFXType configurado.
/// </summary>
// 🧩 Patrón: Observer vía ScriptableObject para la IDA (pedir un efecto),
// inyección del pool para la VUELTA (devolverlo). Ver Paso 3.
[DisallowMultipleComponent]
public class VFXManager : MonoBehaviour
{

    // 📖 static PRIVADO, no público. Su único trabajo es que el segundo
    // VFXManager que aparezca se dé cuenta y se destruya. Sin esto, dos
    // managers suscritos al mismo canal spawnearían DOS explosiones por bala.
    private static VFXManager _instance;

    [Header("Channels")]
    [Tooltip("Arrastrá VFXCueChannel.asset.")]
    [SerializeField] private VFXCueChannel vfxCueChannel;

    [Header("Configuración del Pool")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 20;

    [Tooltip("Un prefab por cada VFXType. Sin duplicados y sin campos vacíos.")]
    [SerializeField] private List<VFXSetup> vfxList;

    private readonly Dictionary<VFXType, IObjectPool<ParticleSystem>> _vfxPools = new();

    // 📖 Guardamos la escena de origen para que los mensajes de error digan
    // DÓNDE está el objeto mal configurado.
    private string _originScene;

    private void Awake()
    {
        _originScene = gameObject.scene.name;

        // 📖 FAIL-FAST: validamos ANTES de reclamar el puesto de instancia.
        // Si esta copia está rota, NO se queda con el puesto — así una copia
        // sana de otra escena todavía puede tomar el mando.
        if (!ValidateSetup())
        {
            enabled = false;   // 📖 impide que Unity llame a OnEnable → no se suscribe
            return;
        }

        if (_instance != null && _instance != this)   
        {
            enabled = false;
            Destroy(gameObject);
            return;
        }
        _instance = this;

        foreach (var vfx in vfxList)
            CreatePool(vfx);
    }

    /// <summary>
    /// Verifica canal, prefabs vacíos y tipos duplicados. Explica qué falta.
    /// </summary>
    // 📖 BUG #2 CORREGIDO: antes, un prefab vacío en la lista llegaba hasta
    // Instantiate(null) y explotaba con "ArgumentException: The Object you want
    // to instantiate is null" en medio del gameplay, con un stack ilegible.
    private bool ValidateSetup()
    {
        bool isValid = true;

        if (vfxCueChannel == null)
        {
            Debug.LogError($"[VFXManager] Falta 'vfxCueChannel' en el VFXManager de la escena " +
                           $"'{_originScene}'. Asignale VFXCueChannel.asset. Servicio de VFX APAGADO.", this);
            isValid = false;
        }

        if (vfxList == null || vfxList.Count == 0)
        {
            Debug.LogError($"[VFXManager] 'vfxList' está vacía en la escena '{_originScene}'. " +
                           $"Servicio de VFX APAGADO.", this);
            return false;   // 📖 sin lista no hay nada más que validar
        }

        // 📖 HashSet y no List: Contains() en un HashSet es O(1) (hash directo),
        // en una List es O(n) (recorre todo). Con 2 elementos da igual, pero es
        // la herramienta correcta y no cuesta nada usarla bien.
        // 🔴 GC: esto aloca — pero corre UNA vez en Awake, nunca en un hot path.
        HashSet<VFXType> seenTypes = new HashSet<VFXType>();

        for (int i = 0; i < vfxList.Count; i++)
        {
            VFXSetup setup = vfxList[i];

            if (setup.prefab == null)
            {
                Debug.LogError($"[VFXManager] El elemento {i} de 'vfxList' (tipo {setup.VFXType}) " +
                               $"no tiene prefab asignado, en la escena '{_originScene}'. " +
                               $"Servicio de VFX APAGADO.", this);
                isValid = false;
                continue;
            }

            // 📖 BUG #4 CORREGIDO: antes se usaba TryAdd, que ignora el duplicado
            // EN SILENCIO. Configurabas dos EnemyHit, el segundo desaparecía y
            // no había forma de darse cuenta.
            if (!seenTypes.Add(setup.VFXType))
            {
                Debug.LogError($"[VFXManager] El VFXType '{setup.VFXType}' está repetido en " +
                               $"'vfxList' (elemento {i}), en la escena '{_originScene}'. " +
                               $"Dejá uno solo. Servicio de VFX APAGADO.", this);
                isValid = false;
            }

            if (setup.prefab.GetComponent<PooledVFX>() == null)
            {
                Debug.LogError($"[VFXManager] El prefab '{setup.prefab.name}' no tiene el " +
                               $"componente PooledVFX. Sin él nunca vuelve al pool y el pool " +
                               $"se agota. Servicio de VFX APAGADO.", this);
                isValid = false;
            }
        }

        return isValid;
    }


    private void OnEnable()
    {
        if (_instance != this) return;

        vfxCueChannel.OnRaiseEvent += OnVFXRequested;
    }

    // 📖 todo += tiene su -= gemelo. El canal es un asset
    // (no se destruye nunca), así que sin esto mantendría viva la referencia a
    // este objeto para siempre → memory leak.
    private void OnDisable()
    {
        if (_instance != this) return;

        if (vfxCueChannel != null)
            vfxCueChannel.OnRaiseEvent -= OnVFXRequested;
    }


    // 📖 BUG #3 CORREGIDO: este método no existía. Sin él, el static seguía
    // apuntando a un VFXManager destruido. Se auto-curaba de casualidad porque
    // el == de Unity detecta objetos destruidos, pero depender de un accidente
    // no es una estrategia.
    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    private void OnVFXRequested(VFXType type, Vector3 position)
    {
        SpawnExplosion(type, position);
    }
    private void CreatePool(VFXSetup setup)
    {
        // 📖
        // Declaramos la variable en null ANTES de construir el pool, porque el
        // createFunc necesita referenciar al pool... que todavía no existe.
        // Funciona porque una lambda captura la VARIABLE, no su valor actual.
        // Cuando el createFunc finalmente se ejecute (en el primer Get()), la
        // variable ya va a tener el pool adentro.
        ObjectPool<ParticleSystem> pool = null;

        pool = new ObjectPool<ParticleSystem>(
            createFunc: () =>
            {
                // 📖antes era Instantiate(setup.prefab) sin
                // padre, y las partículas nacían sueltas en la raíz de la
                // Hierarchy. Con el transform como padre quedan prolijas y
                // mueren junto al manager, sin dejar referencias colgando.
                ParticleSystem instance = Instantiate(setup.prefab, transform);

                // 🧩 Inyección del "boleto de regreso" — idéntico a lo que hace
                // BulletPool.CreateBullet() con bullet.Initialize(_pool).
                // Le damos al PooledVFX la referencia a SU pool, una sola vez
                // en toda su vida. Nunca más necesita un singleton.
                if (instance.TryGetComponent(out PooledVFX pooled))
                    pooled.Initialize(pool);

                return instance;
            },
            actionOnGet: ps => ps.gameObject.SetActive(true),
            actionOnRelease: ps => ps.gameObject.SetActive(false),
            actionOnDestroy: ps =>
            {
                if (ps != null) Destroy(ps.gameObject);
            },
            collectionCheck: Application.isEditor,  // 🟡 PERF: doble-release check solo en editor
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        // 📖 Asignación directa y no TryAdd: los duplicados ya los rechazó
        // ValidateSetup, así que acá es imposible pisar una clave existente.
        _vfxPools[setup.VFXType] = pool;
    }

    // ── Ejecución ─────────────────────────────────────────────────────────────

    // 📖 PRIVADO a propósito. El canal es la única puerta de entrada.
    // Volver a hacerlo público sería volver al Service Locator que acabamos de matar.
    private void SpawnExplosion(VFXType type, Vector3 position)
    {
        if (!_vfxPools.TryGetValue(type, out IObjectPool<ParticleSystem> pool)) return;

        ParticleSystem ps = pool.Get();
        ps.transform.position = position;

        // 📖 Clear ANTES de Play, y con withChildren = true. Una instancia
        // reciclada puede traer partículas viejas congeladas en memoria; sin el
        // Clear se ven aparecer de golpe en la posición nueva por un frame.
        // El "true" alcanza también a los 3 sub-emisores de tus prefabs.
        ps.Clear(true);
        ps.Play(true);
    }

}