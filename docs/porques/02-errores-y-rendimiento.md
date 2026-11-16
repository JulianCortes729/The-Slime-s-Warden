# Revisión del ciclo 02: Errores, arquitectura y rendimiento

## Para revisar primero
- Las cifras de memoria (nivel ≈764→439 MB, música 10,1 MB por pista) son estimaciones sobre los .meta y las dimensiones de los PNG, no medidas con el Profiler (secciones 10 y 11).
- El final del juego ya no carga escena por su cuenta: usa el canal `DeathChannel`, que solo escucha el GameManager (vive en SceneMenu). Probar el final exige empezar desde el menú, y el nombre del asset ya no refleja su uso (D5).
- Squishy no se posa nunca en plataformas, y el GDD (PDF) sigue describiendo el radar, el salto de fe y un cooldown de disparo que el código no tiene (D4, sección 9).

## 1. Juego: ¿qué cambia para el jugador y qué valores se ajustan?
- Durante un diálogo el Brujo queda quieto: no gira con A/D, no camina en el sitio y un salto pulsado antes no sale al cerrar.
- El diálogo avanza con Espacio, Enter o clic (antes solo clic). La primera frase no se salta aunque la tecla caiga en el frame de apertura.
- Tras el diálogo final se carga "Fin del Juego" (desde el menú). Squishy, el aspecto y la música se ven y suenan igual (playtest del usuario, 5 de 5). Los pasos ya no suenan durante el retroceso por golpe.
- Ajuste: `SlimeCompanion.cs · xOffset` = −0,2 en la escena (código: 0,5): más alto, Squishy va delante del Brujo; negativo, detrás.
- Ajuste: `SlimeCompanion.cs · playerFeetOffset` = 0,22 en la escena (código: 0,8): más alto, Squishy flota más abajo.
- Ajuste: `SlimeCompanion.cs · smoothTimeX` y `smoothTimeY` = 0,15 en la escena: más alto, Squishy va más rezagado; más bajo, más pegado.

## 2. Alcance: ¿se hizo más o menos de lo pedido?
- Pedido: diálogos firmes y mucha menos memoria, sin cambiar lo que ya gusta.
- De más: nada relevante que haya encontrado. La reescritura de `SlimeCompanion` y quitar el Input Manager antiguo de `DialogueUI` son poda aprobada ("podar el código muerto").
- De menos: comprimir texturas, managers en todas las escenas y unificar su instancia única quedaron fuera por decisión del usuario. Las 3 hojas de tiles no se redujeron (D7).

## 3. Archivos: ¿qué se creó o modificó y qué responsabilidad tiene cada uno?
- `Assets/Scripts/Player/PlayerInput.cs` · modificado · léelo entero (capa de entrada, base del control del jugador; patrón nuevo de bloqueo): lee acciones, rellena el blackboard, se bloquea con `IsControlLocked` y libera sus acciones.
- `Assets/Scripts/UI/DialogueUI.cs` · modificado · léelo entero (primer uso del Input System fuera del jugador, ciclo de vida de acciones, regla del frame de apertura): avanza con Jump, Shoot o clic.
- `Assets/Scripts/Slime/SlimeCompanion.cs` · modificado · léelo entero (reescrito, ~40 líneas útiles; en el diff sale entero por los finales de línea): SmoothDamp en Update sin raycast.
- `PlayerBlackboard.cs` (añade `IsControlLocked`; borra `maxLives` e `isKnockedBack`), `PlayerMovement.cs` (al hablar, velocidad X y buffer de salto a 0), `PlayerCombat.cs` (descarta `shootIntent` si está bloqueado) y `PlayerFootsteps.cs` (mira `knockbackTimer`), todos en `Assets/Scripts/Player/` · modificados · basta el resumen (cambios pequeños).
- `Assets/Scripts/Dialogue/OutroDialogueTrigger.cs` · modificado · basta el resumen: lanza `_gameOverEventChannel` en vez de `SceneManager.LoadScene`.
- `SampleScene.unity` (1 línea: el trigger final apunta a `DeathChannel.asset`), 6 `.ogg.meta` de `Assets/Audio/Music` (loadType 0→2) y 9 `.png.meta` de `Assets/Mossy Tileset` (maxTextureSize 4096→2048) · modificados · basta el resumen. Más la documentación del ciclo.

## 4. Arquitectura: ¿cómo se integra con lo que ya existía?
- Input → PlayerBlackboard → física sigue igual. El bloqueo se añade en el primer eslabón: `PlayerInput` pone `moveInput` a 0 e ignora saltar y disparar; visuales, pasos y Squishy leen `moveInput` sin cambiar.
- `DialogueUI` crea su propia `PlayerInputActions` (otra copia del asset de acciones) en `Awake` y la activa en `OnEnable`.
- Las escenas se cargan solo desde el GameManager (`LoadSceneAsynchronously`, con `_isLoading`). Muerte y victoria llegan por el mismo `VoidEventChannel`.
- `SlimeCompanion` sigue el Transform del jugador, que tiene Interpolate.

## 5. Funcionamiento: ¿cuál es el flujo principal, paso a paso?
1. Se abre un diálogo y `isTalking = true`: `IsControlLocked` pasa a true.
2. Cada frame (`PlayerInput.Update`): `moveInput = 0`. Los callbacks de Jump y Shoot salen sin guardar nada.
3. `DialogueUI.StartDialogue` guarda `Time.frameCount`. En `Update` ignora ese frame; después, `AdvancePressed()` (Jump, Shoot o clic) pasa a la siguiente frase.
4. En `FixedUpdate`, `PlayerMovement` con `isTalking` pone buffer de salto a 0 y velocidad X a 0 (conserva Y) y sale.
5. Al acabar el diálogo final, `OutroDialogueTrigger` lanza el canal; el GameManager carga "Fin del Juego" en segundo plano.
6. En paralelo, `SlimeCompanion.Update` suaviza hacia (x del jugador + `xOffset`·lado, y del jugador − `playerFeetOffset`).

## 6. Decisiones: ¿qué se decidió y por qué?
### D1 · El bloqueo de controles vive en la capa de entrada (`IsControlLocked`)
- Tipo y autor: arquitectura · decidió la IA (el usuario aprobó bloquear controles, recomendado)
- Dónde: `PlayerBlackboard.cs · IsControlLocked (línea 41)`, `PlayerInput.cs · Update (línea 38), OnJumpPerformed (línea 44), OnShootPerformed (línea 51)`
- Por qué: el salto pulsado en un diálogo se guardaba (`jumpBufferTimer` congelado) y salía al cerrar; A/D giraban al Brujo y lo animaban. Un solo punto arregla giro, animación, pasos y lado de Squishy, que leen `moveInput`.
- Ventajas: los demás sistemas no cambian. Desventajas: `PlayerMovement` y `PlayerCombat` aún limpian salto y disparo "por si acaso": dos defensas.
- Alternativa descartada: comprobar `isTalking` en `PlayerVisuals`, `PlayerFootsteps` y `SlimeCompanion` por separado.
- Cómo cambiarlo: poco (entrada y blackboard).

### D2 · El diálogo avanza con las acciones Jump y Shoot (y clic) del Input System
- Tipo y autor: código · decidió el usuario (teclado) y la IA (cómo)
- Dónde: `DialogueUI.cs · AdvancePressed (línea 77)`
- Por qué: solo avanzaba con clic aunque el juego es de teclado; leer las acciones hace que, si cambian las teclas en `ActionsPlayer.inputactions`, el diálogo las siga, y desaparece el último uso del Input Manager antiguo.
- Ventajas: un solo sistema de entrada. Desventajas: `DialogueUI` crea su propia copia de las acciones; pulsando deprisa se puede saltar una frase.
- Alternativa descartada: Espacio y Enter fijos con `Keyboard.current`. Cómo cambiarlo: poco.

### D3 · La primera frase se protege con `Time.frameCount`
- Tipo y autor: código · decidió la IA (aviso de QA) · Dónde: `DialogueUI.cs · _startFrame (línea 28), Update (línea 67)`
- Por qué: al entrar saltando en el disparador final, la pulsación cerraba la primera frase (regresión de este ciclo). Ventaja: una línea. Desventaja: ninguna que haya encontrado.
- Alternativa descartada: exigir soltar la tecla antes de avanzar. Cómo cambiarlo: poco.

### D4 · Squishy: dejarlo flotando a los pies y borrar el raycast
- Tipo y autor: diseño y código · decidió el usuario (dejarlo como está) y la IA (reescribirlo)
- Dónde: `SlimeCompanion.cs · Update (línea 25)`
- Por qué: la condición del raycast estaba invertida (`SlimeCompanion.cs:63` antes), así que siempre flotó a los pies del Brujo; así se jugó y así lo describe el GDD. Con los valores de la escena el resultado ya era ese; solo fallaba si el rayo no encontraba suelo en 10 u con el jugador entre y −0,1 y 2,4: entonces Squishy iba a y 0,1 (fallo latente).
- Ventajas: ~70 líneas menos (de 107 a 39) y nada cambia al jugar. Desventajas: Squishy no se posa nunca; el GDD queda desfasado.
- Alternativa descartada: corregir la condición (cambia el feel y pide ajustes).
- Cómo cambiarlo: medio (reintroducir detección de suelo y reajustar offsets).

### D5 · Solo el GameManager carga escenas: el final usa `DeathChannel`
- Tipo y autor: arquitectura · decidió el usuario (recomendado) y la IA (qué canal)
- Dónde: `OutroDialogueTrigger.cs · _gameOverEventChannel (línea 13), RaiseEvent (línea 51)`; `SampleScene.unity`
- Por qué: `LoadScene` directo no tenía carga en segundo plano ni protección contra cargas dobles. `DeathChannel` es el único canal que el GameManager convierte en "cargar Fin del Juego".
- Ventajas: un único sitio carga escenas, sin assets nuevos. Desventajas: Play directo en SampleScene no tiene final (ya pasaba con la muerte); el asset se llama `DeathChannel` aunque también lo use la victoria.
- Alternativa descartada: canal `LevelCompleted` nuevo (editar SceneMenu y GameManager para el mismo destino).
- Cómo cambiarlo: medio (crear canal, asignarlo en 2 escenas y suscribir el GameManager).

### D6 · Música en Streaming
- Tipo y autor: rendimiento · decidió el usuario (recomendado) · Dónde: 6 `.ogg.meta` de `Assets/Audio/Music/…` · loadType 2
- Por qué: cada pista (60 s estéreo 44,1 kHz) ocupaba 10,1 MB de PCM, 3 por escena ≈30 MB, descomprimidos al cargar.
- Ventajas: <1 MB por escena (estimado) y cargas más rápidas, sin cambio audible. Desventajas: lee del disco mientras suena; posible retardo al empezar un cue (aviso de QA).
- Alternativa descartada: Compressed In Memory (ahorra menos). Cómo cambiarlo: poco (revertir los .meta).

### D7 · Tope de 2048 px en 9 texturas; las 3 hojas de tiles de 3584 se quedan en 4096
- Tipo y autor: rendimiento · decidió el usuario (tope) y la IA (excepción de los tiles, aviso de QA)
- Dónde: 9 `.png.meta` de `Assets/Mossy Tileset/…` · maxTextureSize en DefaultTexturePlatform
- Por qué: la cámara dibuja ≈148 px por unidad (ortográfica de 3,638 a 1080p) y las hojas tienen 512 PPU, así que el detalle 4096 no llega a pantalla. A 2048, los tiles de 3584 medirían 292,57 px y el filtrado mezclaría tiles vecinos (líneas en el Tilemap).
- Ventajas: ≈4 veces menos memoria por textura sin cambio visible a 1080p. Desventajas: en 4K se vería algo más suave; ahorro menor al anunciado (nivel 764→439 MB, menú 395→244 MB, Fin del Juego 201→50 MB, estimado); siguen 32 texturas sin comprimir.
- Alternativa descartada: comprimirlas (puede notarse en degradados) y probar los tiles a 2048 para revertir si salen líneas.
- Cómo cambiarlo: poco (volver a 4096 en los .meta); reducir los tiles, medio.

### D8 · Decisiones menores
- Liberar acciones con `Dispose` en `OnDestroy` (`PlayerInput.cs línea 35`, `DialogueUI.cs línea 56`) · IA: `new PlayerInputActions()` crea un asset que no se destruye solo.
- Velocidad X a 0 al hablar (`PlayerMovement.cs línea 35`) · IA: sin eso el Brujo se deslizaba al entrar en el disparador final; frena en seco.
- Pasos con `knockbackTimer > 0f` (`PlayerFootsteps.cs línea 116`) y borrar `isKnockedBack` y `maxLives` · IA: nadie activaba el primero (sonaban pasos durante el golpe).
- No unificar el código de instancia única de los 4 managers · usuario: son 6 líneas estables y cada manager difiere; la repetición sigue.
- Auditoría antes de tocar (48 scripts, 3 escenas, assets) · IA: sin Profiler, el rendimiento se midió sobre los assets; el código por frame no tenía trampas.
- Importación editando los .meta · IA: Unity reimporta solo (la primera vez tarda).
- Cerrar el ciclo con 5 de 5 y dejar SceneGameOver para el siguiente · usuario.

## 7. Conceptos: ¿qué conceptos de videojuegos aparecen?
- **Bloqueo de controles en la capa de entrada** · arquitectura · `PlayerInput.cs`: se anula la intención del jugador al leerla, antes de que otros sistemas la usen.
- **Buffer de salto** · bucle y tiempo · `PlayerMovement.cs · jumpBufferTimer`: guarda un salto pulsado un instante antes de poder ejecutarse; congelado en un diálogo, salía al cerrarlo.
- **Streaming de audio** · rendimiento · 6 `.ogg.meta`: el clip se lee del disco mientras suena, en vez de descomprimirse entero en RAM.
- **Mipmaps y tope de textura** · rendimiento · 9 `.png.meta`: la GPU usa versiones reducidas según el tamaño en pantalla; el tope descarta niveles que nunca se ven.
- **Píxeles por unidad (PPU)** · motor · cámara y hojas de 512: relación entre píxeles de arte y unidades del mundo; decide el detalle que realmente se muestra.
- **Interpolate** · física · `SlimeCompanion.cs · Update`: el Rigidbody2D suaviza su posición entre pasos de física, así Squishy lo sigue sin temblar desde Update.
- **Canal de eventos** · arquitectura · `DeathChannel.asset`: un ScriptableObject que desacopla quién avisa de quién escucha.

## 8. Riesgos: ¿qué problemas, dependencias o limitaciones tiene?
- Probar el final o la muerte empezando en SampleScene no hace nada (managers solo en SceneMenu) · media · cargar los managers en cada escena.
- Posible retardo al empezar un cue de música en Streaming · baja · probar en un equipo lento o precargar si molesta.
- `DeathChannel` mezcla muerte y victoria: si la muerte suma efectos propios, la victoria los heredaría · media · crear un canal propio.
- Si falta el canal, `OutroDialogueTrigger` solo avisa con `Debug.LogError` y el final no carga · baja · vigilar la consola.

## 9. Deuda técnica: ¿qué atajos se tomaron a propósito?
- Texturas sin comprimir (32 en el nivel) · cuando el Profiler muestre memoria o carga altas, o se apunte a otra plataforma.
- Instancia única repetida en 4 managers · cuando aparezca un quinto manager.
- Managers solo en SceneMenu · cuando se quiera probar el final en SampleScene sin el menú.
- Asset `DeathChannel` con doble uso · al añadir un efecto exclusivo de la muerte o la victoria.
- El GDD describe radar, salto de fe y cooldown de disparo que el código no tiene · al actualizar el GDD.
- `PlayerMovement` y `PlayerCombat` limpian la intención aunque la entrada ya la bloquea · si se quiere una sola defensa.

## 10. Rendimiento: ¿hay posibles problemas de rendimiento, memoria o asignaciones?
- Las cifras de memoria son estimaciones, no medidas · medir con el Memory Profiler (Window > Analysis) en Play, escena por escena.
- Posible hipo al empezar un cue en Streaming (inferido, no medido) · `MusicManager.cs` · mirar el Profiler de audio al cambiar de cue.
- Las 32 texturas sin comprimir siguen siendo lo más pesado del nivel · `Assets/Mossy Tileset` · comparar en el Memory Profiler antes y después de comprimir.
- Lectura del código por frame: pools, sin `GetComponent` ni `Find` en Update, física NonAlloc; no se tocó la CPU sin medir.

## 11. Verificación: ¿qué se probó y qué conviene probar a mano?
- Hecho: los 48 scripts compilan con el Roslyn de Unity 2022.3.62f2 contra los ensamblados del proyecto, sin errores. Revisión de QA sin bloqueantes (orden de eventos, ciclo de vida, campos borrados inocuos). Medición estática de assets. Playtest del usuario 5 de 5. El estudio no ejecutó el juego.
- A mano: entrar saltando al disparador final con Espacio pulsado y comprobar que se lee la primera frase.
- A mano: mantener D durante un diálogo; el Brujo no debe deslizarse ni girar, y al cerrar no debe saltar.
- A mano: partida completa desde el menú hasta el final, pulsando Espacio varias veces al cerrar: "Fin del Juego" debe cargar una sola vez.
- A mano: cambiar de cue de música varias veces y escuchar si hay cortes; morir y comprobar que el Brujo no responde.

## 12. Mantenimiento: ¿qué hay que entender para mantenerlo y ampliarlo?
- Todo lo que bloquee al jugador pasa por `PlayerBlackboard.cs · IsControlLocked`: un estado nuevo (cinemática, pausa) se añade ahí.
- Dos sistemas lanzan `DeathChannel` y solo el GameManager lo escucha · `GameManager.cs · OnGameOver`.
- Cada `new PlayerInputActions()` debe liberarse con `Dispose` · `PlayerInput.cs`, `DialogueUI.cs`.
- Prueba: en la escena, pon `SlimeCompanion · xOffset` a 1: Squishy irá delante del Brujo. Vuelve a −0,2.
- Prueba: en `DialogueUI.cs · Update`, comenta `if (Time.frameCount == _startFrame) return;` y entra saltando al disparador final: la primera frase se salta. Restáurala.
- Pregunta: ¿por qué `PlayerInput` pone `moveInput` a 0 en vez de comprobar `isTalking` en `PlayerVisuals`?
- Pregunta: ¿qué pasaría con el final si el GameManager no existiera en la escena?

## Correcciones
- ninguna
