# Revisión del ciclo 03: Pantalla final

## Para revisar primero
- D2 reemplaza una decisión del ciclo 02: entonces se descartó un canal `LevelCompleted` y el final usaba `DeathChannel`; ahora existe `LevelCompletedChannel`. Venía implícito en la opción que elegiste ("una escena, dos variantes": su descripción decía que había que separar el canal de la victoria); el cómo lo decidió la IA (sección 6).
- La causa del "Instantiate is null" (MonoScript de `PooledAudio` asociado a `Assembly-CSharp`) es una hipótesis que explica los logs y el arreglo, no algo visto dentro de Unity (D5). Puede repetirse con otros scripts: Unity solo reimportó 23 de ~52 (secciones 8 y 12).
- Los 4 `onClick` de SceneMenu siguen guardando `MainMenuUI, Assembly-CSharp`; los de SceneGameOver ya dicen `GameOverUI, SlimesWarden`. Funcionan hoy, pero conviene reasignarlos (sección 8).

## 1. Juego: ¿qué cambia para el jugador y qué valores se ajustan?
- Al terminar el diálogo final se ve la cima azul con el Brujo, Squishy y su familia: "¡Llegamos a la cima!" y botón "Jugar de nuevo". Al morir, el abismo oscuro con Squishy solo: "Squishy sigue esperando..." y botón "Reintentar".
- Título y botones centrados (maqueta A). Botones "Jugar de nuevo/Reintentar" y "Menú", elegibles con teclado y ratón. "Salir" desaparece de esta pantalla.
- Tras ganar y morir después, sale la derrota (pruebas EditMode y playtest).
- Ajuste: `GameOverScreen.cs · _victory / _defeat` (campos de la escena, struct Variant: objects, background, retryText): fondo y texto del botón por variante, editables en el Inspector. No hay valores numéricos de game feel nuevos en este ciclo.

## 2. Alcance: ¿se hizo más o menos de lo pedido?
- Pedido: que el final premie al ganar y anime a reintentar al morir, con el acabado del menú, más las primeras pruebas automáticas.
- De más: `AudioManager` falla en blando (D4) y se tocó `PooledAudio.cs` (D5). Nacieron del fallo del playtest 1, no estaban pedidos.
- De menos: nada. No hay prueba de que el `GameManager` anote el resultado (necesita escena). Sonido en botones quedó para el siguiente ciclo.

## 3. Archivos: ¿qué se creó o modificó y qué responsabilidad tiene cada uno?
- `Assets/Scripts/ScriptableObjects/GameResult.cs` · creado · léelo entero (patrón nuevo, 27 líneas): enum `GameOutcome` y asset con el resultado de la última partida.
- `Assets/Scripts/UI/GameOverScreen.cs` · creado · léelo entero (lógica central de la pantalla, probada): activa la variante según el resultado.
- `Assets/Scripts/Managers/GameManager.cs` · modificado · léelo entero (base de carga de escenas): `OnLevelCompleted`, `RecordOutcome`, 2 campos nuevos.
- `Assets/Scripts/SlimesWarden.asmdef` · creado · léelo entero (cambia cómo compila todo el juego): ensamblado del juego.
- `Assets/Tests/EditMode/` (asmdef, `GameResultTests.cs`, `GameOverScreenTests.cs`) · creados · léelo entero el asmdef (primera vez), basta el resumen en los tests: 8 pruebas.
- `Assets/Scripts/UI/SelectionKeeper.cs` · creado · basta el resumen (su `Update` salió tal cual de `MainMenuUI`).
- `MainMenuUI.cs` (pierde el `Update`), `OutroDialogueTrigger.cs` (`_levelCompletedChannel` en vez de `_gameOverEventChannel`), `AudioManager.cs` (2 guardas), `PooledAudio.cs` (solo un comentario) · modificados · basta el resumen.
- `Assets/GameResult.asset`, `Assets/LevelCompletedChannel.asset`, `Assets/Animations/Brujo Idle.controller` · creados · basta el resumen.
- `SceneMenu.unity` (+2 campos en el GameManager, `SelectionKeeper` en el Canvas), `SampleScene.unity` (1 línea: el trigger final) · modificados · basta el resumen.
- `SceneGameOver.unity` · reconstruida con `build_gameover.py` (~9.900 líneas de diff, 359 documentos) · basta el resumen: mundos Victoria y Derrota, textos por variante, `ButtonRetry` y `ButtonMenu` copiados del menú.

## 4. Arquitectura: ¿cómo se integra con lo que ya existía?
- Victoria: `OutroDialogueTrigger` → `LevelCompletedChannel` → `GameManager.OnLevelCompleted`. Muerte: `PlayerHealth` → `DeathChannel` → `GameManager.OnGameOver`. Ambos anotan en `GameResult` y cargan SceneGameOver.
- `GameOverScreen` lee `GameResult` en `Awake`. Los botones llaman a `GameOverUI`, que lanza los canales de reiniciar o volver al menú; solo el `GameManager` carga escenas (regla del ciclo 02).
- `GameResult` sobrevive a la carga porque el `GameManager` (DontDestroyOnLoad) lo referencia. `SelectionKeeper` va en el Canvas del menú y en el de la pantalla final.
- El juego compila en `SlimesWarden`; las pruebas, en `SlimesWarden.Tests.EditMode` (solo Editor), que referencia al primero.
- Mapa: creado `docs/arquitectura.md` (no existía).

## 5. Funcionamiento: ¿cuál es el flujo principal, paso a paso?
1. Termina el diálogo final (o el Brujo muere): se lanza `LevelCompletedChannel` (o `DeathChannel`).
2. `GameManager` comprueba `_isLoading`, anota `Victory` o `Defeat` en `GameResult` y empieza `LoadSceneAsynchronously("SceneGameOver")`.
3. Al cargar, `GameOverScreen.Awake` llama a `OutcomeToShow` y `Show`: enciende la variante, apaga la otra, pone el fondo y el texto del botón.
4. `SelectionKeeper.Update` mantiene el foco; First Selected = `ButtonRetry`, así el teclado funciona desde el primer frame.
5. "Jugar de nuevo/Reintentar" lanza el canal de reinicio (carga SampleScene); "Menú", el de volver al menú.

## 6. Decisiones: ¿qué se decidió y por qué?
### D1 · Adoptar Unity Test Framework ya, con un asmdef para el juego
- Tipo y autor: arquitectura · decidió el usuario (contra la recomendación de esperar)
- Dónde: `SlimesWarden.asmdef`, `SlimesWarden.Tests.EditMode.asmdef` (`overrideReferences` true, `UNITY_INCLUDE_TESTS`)
- Por qué: base de pruebas para reglas futuras (vidas, daño, estados): la primera regla nueva sale probada.
- Ventajas: pruebas desde ya; scripts del juego aislados en su ensamblado.
- Desventajas: cambia cómo compila el proyecto (causó D5); las pruebas las corre el usuario en el Test Runner, el estudio no puede; el asmdef depende de 4 ensamblados (Cinemachine, Unity.InputSystem, Unity.TextMeshPro, UnityEngine.UI).
- Alternativa descartada: esperar a una regla más delicada.
- Cómo cambiarlo: medio (borrar los asmdef y reimportar Assets/Scripts; los `onClick` volverían a `Assembly-CSharp`).
### D2 · El resultado viaja en un asset `GameResult` y la victoria tiene su canal
- Tipo y autor: arquitectura · decidió la IA
- Dónde: `GameResult.cs · _outcome (línea 22)`, `GameManager.cs · OnLevelCompleted (línea 69), RecordOutcome (línea 77)`
- Por qué: ninguna escena busca a otra, y `DeathChannel` dejaba de servir para dos cosas (deuda del ciclo 02).
- Relación: reemplaza D5 del ciclo 02 (final por `DeathChannel`).
- Ventajas: se prueba sin abrir escena. Desventajas: un asset más que asignar en el `GameManager`; `OnEnable` comprueba null en `_levelCompletedEventChannel` pero no en `_gameOverEventChannel` (inconsistente, sin efecto hoy).
- Alternativa descartada: estático global (acoplamiento oculto); canal con dato (la escena final aún no existe al lanzarlo).
- Cómo cambiarlo: medio (2 escenas y el `GameManager`).
### D3 · Una escena, dos variantes, con escenario propio y distribución centrada
- Tipo y autor: diseño · decidió el usuario (recomendado) y la IA (maquetas)
- Dónde: `GameOverScreen.cs · Variant, Show (línea 41)`; SceneGameOver (mundos Victoria y Derrota, maqueta A)
- Por qué: el final debe premiar y la muerte invitar a reintentar. Escena propia por resultado: más emoción que repetir el claro del menú.
- Ventajas: una sola escena; victoria y derrota se sienten distintas. Desventajas: hay que pasar el resultado a la escena; el texto tapa el centro del escenario; más composición.
- Alternativa descartada: pantalla neutra, dos escenas (fondo y botones duplicados), el claro del menú, distribución B al costado.
- Cómo cambiarlo: poco (textos y fondo en Inspector); medio (rehacer escenarios).
### D4 · `AudioManager` falla en blando
- Tipo y autor: código · decidió la IA
- Dónde: `AudioManager.cs · Awake (línea 62), PlayCue (línea 177)`
- Por qué: la excepción de `Instantiate(null)` subía por el canal y cortaba `Shoot` antes de crear la bala.
- Ventajas: el juego sigue sin audio. Desventajas: si el prefab falla, queda mudo y solo hay un `LogError`.
- Alternativa descartada: cambiar el orden en los tiradores (otro oyente podría fallar igual). Cómo cambiarlo: poco.
### D5 · Reimportar `PooledAudio.cs` con un comentario
- Tipo y autor: motor · decidió la IA
- Dónde: `PooledAudio.cs` (comentario de la línea 6)
- Por qué: Unity reimportó 23 de ~52 scripts al crear el asmdef y ninguno de Audio/; el prefab quedó con "missing scripts". La causa (MonoScript asociado a `Assembly-CSharp`) es hipótesis; el playtest 2 confirmó que el arreglo funcionó.
- Ventajas: reimporta el script, no el prefab. Desventajas: arreglo manual; si hay otros scripts sin reimportar, no se ve.
- Alternativa descartada: Reimport manual del script o reasignar el script en el prefab (quedaron de respaldo). Cómo cambiarlo: poco.
### D6 · `GameOverScreen.OutcomeToShow` estática y pura
- Tipo y autor: código · decidió la IA (aviso de QA)
- Dónde: `GameOverScreen.cs · OutcomeToShow (línea 37)`
- Por qué: "sin resultado → derrota" era la regla sin probar y la más delicada; ahora tiene 2 pruebas (8 en total).
- Ventajas: se puede probar sin reflexión. Desventajas: ninguna que haya encontrado.
- Alternativa descartada: llamar a `Awake` por reflexión. Cómo cambiarlo: poco.
### D7 · `SelectionKeeper` como componente propio
- Tipo y autor: patrón · decidió la IA
- Dónde: `SelectionKeeper.cs · Update (línea 13)`; quitado de `MainMenuUI.cs`
- Por qué: segunda pantalla con botones; una responsabilidad por script.
- Ventajas: se reutiliza. Desventajas: hay que acordarse de añadirlo y de poner First Selected en cada pantalla.
- Alternativa descartada: copiar el `Update` en `GameOverUI`. Cómo cambiarlo: poco.
### D8 · Decisiones menores
- Botones Reintentar y Menú, sin "Salir del juego" · usuario (recomendado): "Salir" llevaba al menú y confundía; cerrar el juego exige pasar por el menú.
- Botón principal "Jugar de nuevo" al ganar y "Reintentar" al morir · usuario (recomendado); un texto más que cambia por variante.
- Textos de victoria y derrota en voseo · usuario (los propuestos).
- Controlador "Brujo Idle" con un estado (`Player_Idle`) · IA: el del jugador, sin suelo, pasa a la animación de salto.
- Texto con ContentSizeFitter y Squishy hijo del texto · IA: el marcador sigue a cualquier longitud; Squishy crece un 10 % al seleccionar.
- Derrota sin viñeta, solo con tintes · IA: la viñeta era un efecto de imagen; no se creó arte nuevo.
- Escena generada con `build_gameover.py` desde las maquetas · IA (el script vive en la carpeta temporal del estudio, fuera del repositorio).
- Aviso de Domain Reload descartado · IA: `m_EnterPlayModeOptionsEnabled: 0`, cada Play recarga el dominio.
- Cerrar el ciclo tras el playtest 2 y elegir como siguiente "sonido en botones" (se recomendó "managers en todas las escenas") · usuario.

## 7. Conceptos: ¿qué conceptos de videojuegos aparecen?
- **Assembly Definition (asmdef)** · motor · `SlimesWarden.asmdef`: agrupa scripts en un ensamblado propio con sus dependencias; permite que las pruebas lo referencien.
- **Prueba EditMode** · arquitectura · `Assets/Tests/EditMode`: se ejecuta en el Editor sin dar Play, ideal para reglas puras.
- **Data as Asset** · arquitectura · `GameResult.cs`: el dato compartido vive en un ScriptableObject que las escenas referencian.
- **MonoScript** · motor · `PooledAudio.cs.meta`: el asset interno que Unity asocia a cada script; si apunta al ensamblado equivocado, el componente queda "missing".
- **Fallo en blando** · arquitectura · `AudioManager.cs`: el audio puede fallar sin romper la jugabilidad.

## 8. Riesgos: ¿qué problemas, dependencias o limitaciones tiene?
- Otros scripts podrían tener el mismo MonoScript desasociado (solo se vio en `PooledAudio`) · media · en Unity, clic derecho en Assets/Scripts > Reimport (más rápido que Reimport All, que reimporta también las texturas).
- `onClick` del menú con `Assembly-CSharp` · baja (funciona) · reasignarlos en el Inspector.
- Crear `TextMeshProUGUI` en EditMode es frágil (aviso de QA) · baja · si falla, probar solo `OutcomeToShow`.
- Managers solo en SceneMenu: Play directo en SampleScene no carga escenas ni música; SceneGameOver sin resultado muestra derrota · media · managers en todas las escenas.
- Nada prueba que `GameManager` anote el resultado ni que `OutroDialogueTrigger` use el canal correcto · media · playtest o prueba de integración.
- El panel de controles dice "Clic izquierdo" para avanzar el diálogo, pero también valen Espacio y Enter (desde el ciclo 02) · baja · actualizar el texto.

## 9. Deuda técnica: ¿qué atajos se tomaron a propósito?
- Texturas sin comprimir (32 en el nivel) · cuando el Profiler muestre memoria o carga altas.
- Managers solo en SceneMenu · al querer probar el final sin el menú.
- GDD desfasado (radar, salto de fe, cooldown de disparo) · al actualizar el GDD.
- Faltan `docs/motor.md` y `docs/rendimiento.md` · con /estudio:motor y las primeras mediciones.

## 10. Rendimiento: ¿hay posibles problemas de rendimiento, memoria o asignaciones?
- Medido: no medido (no existe `docs/rendimiento.md`).
- Posible: SceneGameOver carga los dos mundos aunque uno esté apagado · escena final · mirar el Memory Profiler (el ciclo 02 la estimó en ≈50 MB, antes de rehacerla).
- `SelectionKeeper.Update` compara un objeto por frame, sin asignaciones (lectura del código): coste despreciable.

## 11. Verificación: ¿qué se probó y qué conviene probar a mano?
- Hecho: compilación del juego y de las pruebas con el Roslyn de Unity 2022.3.62f2; YAML íntegro (359 documentos); renders del YAML iguales a la maqueta A (salvo el marcador); revisión de QA sin bloqueantes; 2 playtests del usuario (8 pruebas en verde, menú, victoria, derrota y disparos bien tras D5). El estudio no ejecutó el juego.
- A mano: ganar, pulsar "Jugar de nuevo", morir: debe salir "Reintentar" y la derrota.
- A mano: en la pantalla final, flechas y Enter sin ratón; clic en el vacío: el foco no debe perderse.
- A mano: pulsar Espacio varias veces al terminar el diálogo final: la pantalla debe cargarse una sola vez.
- A mano: abrir el prefab `AudioPrefab` y comprobar que no dice "missing scripts".

## 12. Mantenimiento: ¿qué hay que entender para mantenerlo y ampliarlo?
- Un fin nuevo (empate, final secreto) = valor en `GameOutcome`, canal y `Variant` · `GameResult.cs`, `GameOverScreen.cs`, `GameManager.cs`.
- Los scripts nuevos se compilan en `SlimesWarden`; si un componente queda "missing", reimportar el script · `SlimesWarden.asmdef`.
- Prueba: en SceneGameOver cambia `_defeat.retryText` a "Otra vez" y muere: el botón cambia. Deshaz el cambio.
- Prueba: en `GameResultTests.cs` cambia el valor esperado de `SinPartidaAnotada_EsDerrota` y corre el Test Runner: debe fallar. Restáuralo.
- Pregunta: ¿por qué `GameResult._outcome` es `[NonSerialized]` y qué pasaría sin eso?
- Pregunta: ¿qué ocurre en SceneGameOver si el `GameManager` no existe?

## Correcciones
- ninguna
