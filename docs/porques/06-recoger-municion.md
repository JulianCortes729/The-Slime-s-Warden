# Revisión del ciclo 06: Recoger munición

## Para revisar primero
- Rendimiento (sección 10): el promedio subió 17 % (0,53 a 0,62 ms) sin que nada del ciclo corra por frame; atribuirlo al equipo es (inferido). Conviene repetir la medición.
- D4: `vfx_AmmoPickup.prefab` duplica ~29 000 líneas de YAML del pack y su `Circle01_v1.png` (5,3 MB, sin comprimir) explica casi todo el aumento de la exportación.
- D10: C3 se reescribió después de acordarlo («CC0» pasó a «licencia libre con procedencia anotada»).

## 1. Juego: ¿qué cambia para el jugador y qué valores se ajustan?
- Al tocar un coleccionable, el Brujo suma munición, ve un destello cian en cruz en su sitio y oye un chispazo corto. Antes desaparecía en silencio. Sigue sin haber contador de balas.
- Ajuste: `Collectible.prefab · ammoAmount` = 2: más balas por coleccionable (18 en el nivel; el Brujo empieza con 0).
- Ajuste: `Cue_Pickup_Ammo.asset · volumeRange` = 0,45-0,5 (~−6,5 dB): el disparo del Brujo está a 0,75-0,88; si sube, tapa el disparo; si baja, se pierde bajo la música. Se fija jugando, en pasos de 3 dB.
- Ajuste: `Cue_Pickup_Ammo.asset · pitchRange` = 0,95-1,05: más ancho, más variación entre recogidas.
- Ajuste: `vfx_AmmoPickup.prefab` raíz: escala 0,3 (la vista mide 7,3 de alto), duración 1,5 s (cuánto tarda en volver al pool), color cian (0, 0,9, 1).

## 2. Alcance: ¿se hizo más o menos de lo pedido?
- Pedido: que recoger munición se note (destello y sonido por canal, regla con prueba), sin contador en pantalla.
- De más: nada. De menos: nada (C1-C4 cumplen).

## 3. Archivos: ¿qué se creó o modificó y qué responsabilidad tiene cada uno?
- `Assets/Scripts/Bullet/CollectibleAmmo.cs` · modificado · léelo entero (patrón nuevo: guardia de una sola vez y regla probable): recogida y petición de efectos por canal.
- `Assets/Scripts/VFX/VFXManager.cs` · modificado · basta el resumen: suma `VFXType.AmmoPickup` al final del enum.
- `Assets/Prefabs/Collectible.prefab` · modificado · basta el resumen: añade `sfxChannel`, `pickupCue`, `vfxChannel`, `pickupVFX = 2`; las 18 copias lo heredan.
- `Assets/Scenes/SampleScene.unity` · modificado · basta el resumen: el `vfxList` suma `vfx_AmmoPickup` como tipo 2.
- `Assets/Prefabs/vfx_AmmoPickup.prefab`, `Assets/Audio/Cue_Pickup_Ammo.asset` (+ .meta) · creados · basta el resumen: destello copiado de vfx_Heal_02 (D4) y AudioCue con 3 clips, grupo SFX.
- `Assets/Electrified SFX Library/` (ReadMe y 3 WAV con .meta) · creado · basta el resumen: sonidos y licencia (D2, D9).
- `Assets/Tests/EditMode/CollectibleAmmoTests.cs` (+ .meta) · creado · léelo entero (muestra cómo probar sin escena): 5 pruebas.
- Docs del ciclo (`decisiones.md`, `rendimiento.md`, ciclos/06, porques/06). Ruido: `SceneMenu.unity` solo lo reordenó Unity al guardar.

## 4. Arquitectura: ¿cómo se integra con lo que ya existía?
- `CollectibleAmmo` no conoce al `VFXManager` ni al `AudioManager`: lanza `VFXCueChannel.RaiseEvent` y `AudioCueChannel.RaiseEvent` (SFXCueChannel), igual que `Bullet`.
- El `VFXManager` crea un pool por tipo de su `vfxList`; `PooledVFX` devuelve el efecto al terminar (Stop Action = Callback).
- Mapa: «Balas (pool)» incluye la recogida con efectos; nuevos `Cue_Pickup_Ammo` y `vfx_AmmoPickup`; 37 pruebas.

## 5. Funcionamiento: ¿cuál es el flujo principal, paso a paso?
1. Física: el trigger del coleccionable llama `OnTriggerEnter2D` → `TryCollect(other.gameObject)`.
2. Sale si `_collected` ya es true, si no tiene el tag Player o si no tiene `PlayerBlackboard`.
3. Marca `_collected` y suma `ammoAmount` a `bulletCount`.
4. `SpawnEffects` lanza el canal VFX y el SFX (cada uno si su referencia no es null).
5. El `VFXManager` saca un destello del pool; el `AudioManager` elige una de las 3 variantes sin repetir la anterior (`AudioCue` línea 56).
6. `SetActive(false)` apaga el coleccionable; a los 1,5 s el destello vuelve al pool.

## 6. Decisiones: ¿qué se decidió y por qué, con ventajas, desventajas, alternativa y cómo cambiarlo?
### D1 · Guardia `_collected` en la recogida
- Tipo y autor: código · patrón · decidió la IA · Dónde: `CollectibleAmmo.cs · _collected, TryCollect`
- Por qué: la doc de Unity 2022.3 dice que los triggers llegan también a componentes desactivados y no garantiza uno solo por paso de física.
- Ventajas: munición, destello y sonido salen una sola vez. Desventajas: si un coleccionable se reactiva (respawn) hay que reiniciar la guardia.
- Alternativa descartada: confiar en `SetActive(false)`; la doc no lo garantiza. Cómo cambiarlo: poco (un método que ponga `_collected = false`).
### D2 · Sonido: 3 variantes Spark de Electrified SFX Library
- Tipo y autor: diseño · decidió el usuario · Dónde: `Cue_Pickup_Ammo.asset · clips` (Spark_01, Sparks_07, Sparks_03)
- Por qué: no le gustaron 4 maquetas con clips del proyecto ni 43 candidatos de Kenney; trajo este pack («Royalty free», productId 2823) y eligió 3 de 13 Spark.
- Ventajas: 18 recogidas no suenan idénticas. Desventajas: hay que elegir varios de la misma familia.
- Alternativa descartada: un solo clip (repetitivo); sintetizar campanitas. Cómo cambiarlo: poco (`clips` del cue, sin código).
### D3 · `TryCollect` como método público, no función estática
- Tipo y autor: código · patrón · decidió la IA · Dónde: `CollectibleAmmo.cs · TryCollect`; `CollectibleAmmoTests.cs`
- Por qué: las pruebas lo llaman sin escena ni física y cubren guardia, tag y suma reales. Relación: se aparta de la regla pura estática de ciclos anteriores (`MenuSounds.ShouldPlayMove`), que aquí sería trivial.
- Ventajas: prueba el comportamiento real. Desventajas: un método público más.
- Alternativa descartada: `ShouldCollect(bool, bool)` estático; no probaría suma ni guardia. Cómo cambiarlo: poco.
### D4 · Destello: copia completa de vfx_Heal_02 en cian
- Tipo y autor: diseño · el usuario decidió el aspecto; la IA, la forma de copiarlo · Dónde: `vfx_AmmoPickup.prefab`
- Por qué: al usuario le gustó la cruz y el cian une el efecto con la llama del coleccionable. La IA copió el prefab (como con vfx_Impact_01) y cambió color, escala 0,3, duración de 5 a 1,5 s y Stop Action = Callback + PooledVFX.
- Ventajas: muy visible, reutiliza arte, no toca el pack. Desventajas: ~29 000 líneas duplicadas; la cruz sugiere «vida»; en la zona azul el cian puede destacar menos.
- Alternativa descartada: implosión azul (la recomendada por el estudio), LootDrop_01, dorado, blanco azulado; Prefab Variant (frágil a mano). Cómo cambiarlo: poco el color; medio otro efecto.
### D5 · `VFXType.AmmoPickup` al final del enum
- Tipo y autor: código · decidió la IA · Dónde: `VFXManager.cs · VFXType`
- Por qué: Unity guarda el enum como número; insertarlo en medio cambiaría los efectos ya asignados.
- Ventajas: nada existente cambia. Desventajas: nada relevante que haya encontrado.
- Alternativa descartada: insertarlo en otro orden. Cómo cambiarlo: poco, pero reordenar rompería escenas y prefabs.
### D6 · WAV importados: Vorbis 100, 44,1 kHz, mono, precarga
- Tipo y autor: rendimiento · decidió la IA · Dónde: `.meta` de los 3 WAV
- Por qué: efectos cortos como los del Small Sound Kit; sin precarga, la primera recogida podría dar un tirón.
- Ventajas: menos memoria, mismo pico. Desventajas: se pierde el estéreo de Spark_01 (sin valor con spatialBlend 0).
- Alternativa descartada: (inferido) dejar 96 kHz/24 bits; no consta en el registro. Cómo cambiarlo: poco, en el Inspector del clip.
### D7 · Volumen inicial 0,45-0,5, pitch 0,95-1,05
- Tipo y autor: diseño · decidió la IA; el usuario lo dejó igual tras jugar · Dónde: `Cue_Pickup_Ammo.asset`
- Por qué: medido con ffmpeg, queda ~2 dB bajo el disparo del Brujo (C3).
- Ventajas: no compite con el disparo. Desventajas: puede perderse en momentos ruidosos (no medido).
- Alternativa descartada: no consta. Cómo cambiarlo: poco.
### D8 · Sin contador de munición
- Tipo y autor: diseño · decidió el usuario (opción recomendada)
- Por qué: ciclo corto; el playtest dirá si hace falta. Ventajas: menos piezas. Desventajas: el jugador no ve cuántas balas tiene.
- Alternativa descartada: no consta cuál era. Cómo cambiarlo: medio (UI suscrita a un canal).
### D9 · De un pack solo entra lo que se usa
- Tipo y autor: arquitectura de proyecto · decidió el usuario · Dónde: `Assets/Electrified SFX Library/`; `docs/decisiones.md`
- Por qué: los WAV van por LFS y el repositorio crecería 1,2 GB por 3 sonidos; el resto fue a la papelera.
- Ventajas: libera 1,2 GB. Desventajas: otro sonido exige reimportar.
- Alternativa descartada: moverlo a Documentos (recomendada) o al Escritorio; dejarlo entero. Cómo cambiarlo: poco.
### D10 · C3 reescrito: «licencia libre con procedencia»
- Tipo y autor: diseño · decidió el usuario (opción recomendada)
- Por qué: el pack es libre de regalías, no CC0. Ventajas: el criterio refleja la elección real. Desventajas: un criterio cambiado después de acordado.
- Alternativa descartada: dejarlo y anotar la excepción.
### Decisiones menores
- Conexión por canales: el prefab lleva los dos canales y `pickupVFX = AmmoPickup`; ninguna de las 18 copias los sobrescribe.
- `playOnAwake` del destello a 0 (como vfx_Impact_01), corregido tras la revisión.

## 7. Conceptos: ¿qué conceptos de videojuegos aparecen?
- **Callback de trigger repetible** · física · `CollectibleAmmo · OnTriggerEnter2D`: Unity puede avisar más de una vez; se protege con una guardia.
- **Stop Action = Callback** · rendimiento · `vfx_AmmoPickup.prefab`, `PooledVFX`: el efecto avisa al terminar y vuelve al pool; su duración decide cuánto lo ocupa.
- **Variantes de un cue** · audio · `Cue_Pickup_Ammo`: varios clips y rango de pitch/volumen evitan el «efecto ametralladora».
- **Pooling de efectos** · rendimiento · `VFXManager`: reutiliza destellos en vez de instanciarlos.

## 8. Riesgos: ¿qué problemas, dependencias o limitaciones tiene?
- `Circle01_v1.png` (5,3 MB, RGBA sin comprimir) · baja · comprimir con BC7 como las Mossy.
- (inferido) `VFXManager` existe por escena: sin la entrada en `vfxList` de otra escena, el destello no sale · baja · añadirla.
- C4 solo vale a ritmo normal de recogida (el destello vive 1,5 s) · baja · no medido.

## 9. Deuda técnica: ¿qué atajos se tomaron a propósito y cuándo habrá que cambiarlos?
- Guardia sin reinicio (un coleccionable reactivado no recogería nada) · cuando pueda reaparecer.
- Prefab copiado entero, con textura sin comprimir · cuando haya otro efecto de pack o se quiera recortar el tamaño.
- Sin contador de balas · si el playtest muestra que el jugador no sabe cuánta munición tiene.

## 10. Rendimiento: ¿hay posibles problemas de rendimiento, memoria o asignaciones?
- Medido (`docs/rendimiento.md`, mediana de 3 corridas): carga 0,174 s, promedio 0,62 ms, p99 0,84 ms, peor 2,71 ms; presupuesto 16,6 ms. Frente al ciclo 05: promedio +17 %, p95 +14 %, bajo el umbral del 20 %. Exportación 353,0 MB (+5,5: 5,3 de la textura). No se recoge al medir: el coste de recoger no está medido.
- (inferido) La subida es variación del equipo. GPU sin dato válido en 2 de 3 corridas: `FrameTimeRecorder` no filtra valores sin dato.
- Posible: 5 emisores por destello en recogidas seguidas · `vfx_AmmoPickup.prefab` · medir con el Profiler recogiendo muchos.

## 11. Verificación: ¿qué se probó y qué conviene probar a mano?
- Criterios: C1 cumple: 5 pruebas pasan (37 en total) y jugando suma una vez · C2 cumple: el usuario vio el destello cian en su sitio, consola limpia · C3 cumple: 3 Spark libres de regalías, ReadMe y procedencia anotada · C4 cumple: lo comprobó el usuario en la Hierarchy (a ritmo normal).
- Hecho: compila · 37 pruebas (Test Runner del usuario) · qa-playtest sin hallazgos bloqueantes · exportado y medido · playtest sin bugs. Captura no aplica.
- A mano: recoger 10 seguidos y contar los destellos bajo el VFXManager.
- A mano: acercar a Squishy a un coleccionable; no debe recogerlo.
- A mano: recoger en la zona azul (¿se ve el cian?) y mientras se dispara (¿tapa el disparo?).
- A mano: tocarlo por dos lados a la vez; debe sumar 2 balas, no 4.

## 12. Mantenimiento: ¿qué hay que entender para mantenerlo y ampliarlo?
- Recogida con efectos · `CollectibleAmmo.cs`: guardia + canales; otros coleccionables pueden copiar el patrón.
- Efecto nuevo de recogida · `VFXType` al final + prefab + entrada en `vfxList` de cada escena; el sonido se cambia en `clips` del cue.
- Prueba: sube `volumeRange` de `Cue_Pickup_Ammo` a 0,9-1; tapará el disparo. Devuélvelo a 0,45-0,5.
- Pregunta: ¿por qué `TryCollect` devuelve true solo la primera vez aunque Unity avise dos veces?
- Pregunta: ¿qué pasaría con los efectos ya asignados si `AmmoPickup` estuviera entre `PlayerHit` y `EnemyHit`?

## Delegación: ¿qué se encargó a cada agente y qué le faltó al encargo?
- estudio:qa-playtest (modelo por defecto: encajaba, revisión acotada de solo lectura) · revisar el ciclo y proponer hasta 5 puntos de playtest · cumplió, sin salirse de los límites · le faltó: nada; supuso (inferido) que `playOnAwake` no se veía por el Clear del VFXManager · lo escribió el estudio.

## Correcciones
- ninguna
