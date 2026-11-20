# Revisión del ciclo 04: Sonido en los botones

## Para revisar primero
- Volumen de los disparos (D8, sección 1): el usuario lo fijó jugando tras tres playtests (−33, −30, −27 dB). Es lo único que cambia el juego fuera de los menús: el disparo enemigo bajó 6 dB y los impactos 6 y 10 dB. Confirma que el enemigo sigue sirviendo de aviso.
- `MenuSounds` toma la lista de botones en `Awake` (D4, sección 8): el registro dice que vale «para botones nuevos sin tocarlos», pero un botón creado por código después de `Awake` no sonaría (inferido al leer el código).
- Riesgo leve señalado por qa-playtest (sección 8): al cargar una pantalla con el cursor quieto sobre otro botón, el hover lo selecciona y puede sonar «moverse» solo. En el playtest no molestó y no se añadió código.

## 1. Juego: ¿qué cambia para el jugador y qué valores se ajustan?
- Menú y pantalla final: un «plip» al cambiar de botón (teclado, mando o ratón) y un «plop» al pulsar. No suenan al abrir la pantalla, al recuperar la selección tras un clic en el vacío, ni al abrir o cerrar Controles.
- Los disparos y los impactos suenan más bajos (el enemigo y los impactos, sobre todo).
- Ajuste: `Cue_UI_Move.asset · volumeRange` = 0,12-0,13 (moverse; el «plip» se repite mucho y va unos 6 dB por debajo de pulsar). Subirlo lo hace cansar; bajarlo lo pierde frente a la música (0,1 en el menú).
- Ajuste: `Cue_UI_Press.asset · volumeRange` = 0,10-0,11 (pulsar). El «plop» original ya era ~7 dB más fuerte, por eso el número es menor.
- Ajuste: `Cue_UI_Move/Press.asset · pitchRange` = 0,95-1,05: más ancho suena a desafinado; 1-1 suena a máquina.
- Ajuste: `Cue_Magic_Throw_Player.asset · volumeRange` = 0,75-0,88 (antes 0,85-1); `Cue_Magic_Throw_Enemy` = 0,41-0,48; `Cue_Magic_Impact_Player` = 0,45-0,52; `Cue_Magic_Impact_Enemy` = 0,27-0,32. Objetivo de diseño: unos −27 dB en el juego, sobre la música sin taparla. Cada paso de 3 dB se nota; subir el enemigo acerca el aviso, bajarlo lo arriesga.
- Los grupos del mixer están a 0 dB: la mezcla vive en los cues.

## 2. Alcance: ¿se hizo más o menos de lo pedido?
- Pedido: que moverse y pulsar en el menú y en la pantalla final se oiga, sin dobles ni sonido al entrar.
- De más: bajar los disparos e impactos (pedido por el usuario en el playtest, no estaba en el ciclo) y podar el paquete de clics (D2). Ambos decididos por el usuario.
- De menos: nada.

## 3. Archivos: ¿qué se creó o modificó y qué responsabilidad tiene cada uno?
- `Assets/Scripts/UI/MenuSounds.cs` · creado · léelo entero (patrón nuevo: vigilar la selección del EventSystem; 83 líneas; la regla de cuándo NO suena es lo sutil).
- `Assets/Tests/EditMode/MenuSoundsTests.cs` · creado · basta el resumen: 7 pruebas de `ShouldPlayMove` (cambio de botón, primera selección, clic en el vacío, mismo botón, 0 y 1 frames tras pulsar, 2 frames).
- `Assets/Audio/Cue_UI_Move.asset`, `Cue_UI_Press.asset` · creados · basta el resumen: un clip cada uno, grupo UI del mixer, `spatialBlend` 0.
- `Assets/Free UI Click Sound Effects Pack/` · creado y podado · basta el resumen: solo los 2 wav usados con sus `.meta` (144 KB).
- `Assets/Scenes/SceneMenu.unity`, `SceneGameOver.unity` · modificados · basta el resumen: +16 líneas cada una, el componente `MenuSounds` en el Canvas con `SelectionKeeper`, con `_sfxChannel`, `_moveCue` y `_pressCue`.
- `Assets/Audio/Cue_Magic_*.asset` (4) · modificados · basta el resumen: una línea cada uno, `volumeRange`.
- `docs/decisiones.md` · modificado: tres decisiones del 2026-11-19.

## 4. Arquitectura: ¿cómo se integra con lo que ya existía?
- `MenuSounds` no llama al `AudioManager`: lanza `AudioCueChannel.RaiseEvent(cue, Vector3.zero)` por `SFXCueChannel`, el mismo canal que usan las balas.
- Lee `EventSystem.current.currentSelectedGameObject` cada frame y escucha el `onClick` de los botones hijos del Canvas. Convive con `SelectionKeeper` (que recupera la selección) sin llamarlo.
- Los cues van por el grupo UI del `MasterMixer`; el `AudioManager` (DontDestroyOnLoad) los reproduce.
- Mapa: añadidos `MenuSounds` en la fila UI, los cues UI y la regla «la mezcla vive en los AudioCue».

## 5. Funcionamiento: ¿cuál es el flujo principal, paso a paso?
1. `Awake`: guarda todos los botones hijos, también los de paneles ocultos (`GetComponentsInChildren(true)`).
2. `OnEnable`: suscribe `OnButtonPressed` a cada `onClick` y recuerda la selección actual como `_lastSelected`.
3. Cada `Update`: compara la selección actual con `_lastSelected` usando `ShouldPlayMove`, con `Time.frameCount - _pressFrame`. Si cumple, pide `_moveCue`.
4. Si hay selección, la guarda como `_lastSelected`. Con un clic en el vacío se conserva el anterior y `SelectionKeeper` lo devuelve sin sonido.
5. Al pulsar: `OnButtonPressed` anota el frame y pide `_pressCue`. Durante 1 frame (`PressFocusFrames`) el cambio de foco no suena.
6. El `AudioManager` reproduce el cue; si la escena cambia, el sonido sigue.

## 6. Decisiones: ¿qué se decidió y por qué?
### D1 · Suenan moverse y pulsar
- Tipo y autor: diseño · decidió el usuario
- Dónde: `MenuSounds.cs · _moveCue, _pressCue (líneas 14-16)`
- Por qué: un toque al cambiar de botón confirma la navegación con teclado y otro al pulsar confirma la acción.
- Ventajas: es lo habitual en menús. Desventajas: dos sonidos que afinar para que no cansen.
- Alternativa descartada: solo al pulsar (moverse con teclado seguiría mudo); un tercer sonido de «empezar» en Jugar y Reintentar (más trabajo).
- Cómo cambiarlo: poco; nuevo cue y campo en `MenuSounds`.
### D2 · Sonidos del Free UI Click Sound Effects Pack, podado a 2 wav
- Tipo y autor: diseño · decidió el usuario
- Dónde: `Assets/Free UI Click Sound Effects Pack/AUDIO/Liquid/`
- Por qué: el Small Sound Kit no trae sonidos de interfaz. Se podó porque 98 sonidos, un mp3, 2 PDF y una imagen no se usaban y seguían fuera de git.
- Ventajas: 100 clics hechos para menús, de menos de 0,3 s; el repo gana 12 MB → 144 KB. Desventajas: probar otro sonido obliga a reimportar desde Package Manager > My Assets.
- Alternativa descartada: recortar el Small Sound Kit (pasos y magia no suenan a interfaz); un pack CC0 (descarga externa y otro estilo); mantener el paquete entero.
- Cómo cambiarlo: poco; cambiar el clip del cue.
### D3 · Juego «Gota de slime» (Generic_1 al moverse, Pop_Generic_1 al pulsar)
- Tipo y autor: diseño · decidió el usuario, tras escuchar 4 juegos en una maqueta con la música del menú
- Por qué: encaja con Squishy.
- Ventajas y desventajas: nada relevante que haya encontrado.
- Alternativa descartada: Burbuja (pop), Plástico suave y Madera.
- Cómo cambiarlo: poco.
### D4 · `MenuSounds`, un componente por pantalla que vigila la selección y los onClick
- Tipo y autor: patrón · decidió la IA
- Dónde: `MenuSounds.cs · Update (línea 42), Awake (línea 22)`
- Por qué: en un solo sitio se distinguen los cambios que no hace el jugador (primera selección, selección recuperada, foco que salta al abrir o cerrar Controles).
- Ventajas: se configura una vez por pantalla. Desventajas: mira la selección cada frame (coste mínimo); (inferido) no engancha botones creados tras `Awake`.
- Alternativa descartada: sonar en `OnSelect` de `MenuButtonFeedback` (sonaría al abrir y al recuperar, y mezclaría lo visual con el sonido).
- Cómo cambiarlo: medio; mover la lógica a otro oyente y reconfigurar las 2 escenas.
### D5 · Sonidos por `SFXCueChannel` y grupo UI del mixer
- Tipo y autor: arquitectura · decidió la IA
- Dónde: `MenuSounds.cs · Play (línea 77)`; `Cue_UI_*.asset · mixerGroup`
- Por qué: el `AudioManager` sobrevive al cambio de escena, así que el «plop» de Jugar no se corta.
- Ventajas: mismo canal que las balas, sin acoplar. Desventajas: nada relevante que haya encontrado.
- Alternativa descartada: un `AudioSource` en el Canvas (se destruiría al cargar la escena siguiente).
- Cómo cambiarlo: poco.
### D6 · La regla vive en `ShouldPlayMove` estática, con la ventana de «acaba de pulsar»
- Tipo y autor: patrón · decidió la IA (la ventana, a raíz de la revisión de qa-playtest)
- Dónde: `MenuSounds.cs · ShouldPlayMove (línea 65), PressFocusFrames (línea 58)`
- Por qué: es lo sutil del ciclo (cuándo NO suena) y se prueba sin escena; la ventana evita el sonido doble y no tenía prueba.
- Ventajas: 7 pruebas EditMode, 15 en total. Desventajas: la ventana de 1 frame depende del orden entre el EventSystem y `MenuSounds`; el código lo cubre en cualquier orden.
- Alternativa descartada: nada registrado.
- Cómo cambiarlo: poco; el valor de `PressFocusFrames` y sus pruebas.
### D7 · Volúmenes de partida de los sonidos de interfaz
- Tipo y autor: diseño · decidió la IA
- Dónde: `Cue_UI_Move.asset · volumeRange (0,12-0,13)`; `Cue_UI_Press.asset (0,10-0,11)`; tono ±5 %
- Por qué: moverse se repite y no debe tapar a pulsar; el tono al azar evita que suene a máquina. En el juego la música del menú suena a 0,1 y en la maqueta a 0,35: se mantuvo la proporción que oyó el usuario.
- Ventajas: igualados midiendo el RMS de cada clip. Desventajas: medidos fuera de Unity; valores bajos. El playtest los dio por buenos.
- Alternativa descartada: el volumen original de cada archivo (el «plop» es ~7 dB más fuerte).
- Cómo cambiarlo: poco; Inspector.
### D8 · Disparos e impactos bajan hasta unos −27 dB
- Tipo y autor: diseño · decidió el usuario, jugando
- Dónde: `Cue_Magic_*.asset · volumeRange` (valores en la sección 1)
- Por qué: se oían muy fuertes. Medidos con ffmpeg: disparo jugador −26 dB, enemigo −21, impactos −21 y −17, música −39 de media (−31 en picos). Tres rondas: −33 (muy bajo), −30 (bajo), −27 («bien así»).
- Ventajas: más parejos y sobre la música. Desventajas: el disparo enemigo baja 6 dB, a vigilar como aviso.
- Alternativa descartada: −30 y −33 (bajos en juego), −36 (perdería el aviso) y dejar los impactos (el enemigo era lo más fuerte).
- Cómo cambiarlo: poco; 4 assets.
### D9 · Se baja el volumen de cada cue y no el grupo SFX
- Tipo y autor: arquitectura · decidió la IA
- Por qué: los pasos van por el mismo grupo y ya suenan bajos (−47 dB); el problema eran cuatro sonidos. Se mantiene la variación al azar del volumen (±8 %).
- Ventajas: no toca los pasos. Desventajas: la mezcla queda repartida en muchos assets, con el mixer a 0 dB.
- Alternativa descartada: bajar el grupo SFX.
- Cómo cambiarlo: medio.

## 7. Conceptos: ¿qué conceptos de videojuegos aparecen?
- **Feedback sonoro de interfaz** · diseño · `MenuSounds.cs`: un sonido confirma que el juego recibió la acción. Mover y pulsar son los dos momentos clásicos.
- **Grupo del Audio Mixer (bus)** · motor · `MasterMixer.mixer`, `Cue_UI_*.asset · mixerGroup`: los sonidos se envían a un grupo (Music, SFX, UI) donde se ajusta su volumen en conjunto.
- **Mezcla de sonido (niveles relativos)** · diseño · `Cue_Magic_*.asset`: importa el volumen de unos sonidos frente a otros (disparo, impacto, música), no el valor absoluto. Se ajusta jugando.

## 8. Riesgos: ¿qué problemas, dependencias o limitaciones tiene?
- Cursor quieto sobre otro botón al cargar: el hover selecciona y puede sonar «moverse» solo · baja · probar con el ratón antes de añadir código.
- Botones creados tras `Awake` no suenan (inferido) · baja · volver a pedir la lista o suscribir al crearlos.
- Depende de `EventSystem.current` y de `SFXCueChannel`: sin ellos no suena; `Play` comprueba `null` y sigue · baja.
- Los volúmenes se midieron fuera de Unity y la maqueta del navegador no iguala el volumen del juego · media · afinar jugando, en pasos de 3 dB.
- El disparo enemigo, 6 dB más bajo: podría perder su valor de aviso · media · probar un nivel con enemigos fuera de pantalla.

## 9. Deuda técnica: ¿qué atajos se tomaron a propósito?
- Un `MenuSounds` por pantalla con los mismos cues enlazados a mano en dos escenas · cuando haya una tercera pantalla con botones, considerar un prefab (inferido).
- Mezcla en cada cue con el mixer a 0 dB · cuando haya opciones de volumen del jugador o más sonidos, volver a repartirla por grupos (inferido).

## 10. Rendimiento: ¿hay posibles problemas de rendimiento, memoria o asignaciones?
- Medido: no medido (no existe `docs/rendimiento.md`).
- `MenuSounds.cs · Update`: dos lecturas por frame, sin asignaciones (posible, leído del código, no medido) · Profiler > CPU, solo en menú.

## 11. Verificación: ¿qué se probó y qué conviene probar a mano?
- Hecho: medición de los 100 sonidos (duración, pico, RMS); el usuario eligió el juego en una maqueta; compilación con el Roslyn de Unity; script de escenas probado sobre copias; revisión de qa-playtest (cableado y guids bien, sin dobles en ningún orden); 15 pruebas EditMode; 3 playtests del usuario (botones bien en los 5 puntos; disparos a −27 dB, «bien así»). No se ejecutaron las pruebas en el Test Runner desde aquí.
- A mano: Enter en el menú, un sonido por pulsación.
- A mano: Controles abre y cierra, sin «plip» tras el «plop».
- A mano: clic en el vacío y flechas: la selección vuelve sin sonido.
- A mano: Jugar y Reintentar: el «plop» se oye entero.
- A mano: cursor quieto sobre otro botón al cargar la pantalla final.

## 12. Mantenimiento: ¿qué hay que entender para mantenerlo y ampliarlo?
- Regla de «moverse» · `MenuSounds.cs · ShouldPlayMove`: toda excepción nueva (otro panel) va aquí, con su prueba.
- Mezcla · `Cue_*.asset · volumeRange`: el volumen se ajusta en el cue, no en el mixer.
- Prueba: sube `Cue_UI_Move.asset · volumeRange` a 0,3 en el Inspector y navega el menú; el «plip» tapa al «plop». Vuelve a 0,12-0,13.
- Pregunta: ¿por qué el «plop» de Jugar no se corta al cargar la escena?
- Pregunta: ¿qué pasaría con `PressFocusFrames = 0` al abrir Controles?

## Correcciones
- ninguna
