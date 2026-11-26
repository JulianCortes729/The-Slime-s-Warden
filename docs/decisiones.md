# Decisiones

### 2026-11-13 · El color de fondo cambia por zona, no por altura
- Qué: BackgroundZones elige la zona de la cámara con un margen de 2 u y funde el color en el tiempo (1 s). Tonos oscuros: verde, vino y añil.
- Por qué: ciclo 01. Con el color según la altura parpadeaba, porque la cámara sube y baja con cada salto.
- Descartado: suavizar la altura (saltando en la frontera seguiría oscilando).

### 2026-11-14 · Cada zona tiene su propia capa de pilares con parallax
- Qué: ZoneParallaxLayer por zona, al 80 % de la velocidad del nivel. Cada pilar se desvanece entero en las 4 u antes de salir de su franja.
- Por qué: ciclo 01, playtests 1 a 6. Con una sola capa se mezclaban las zonas, y fijos en el mundo faltaba profundidad.
- Descartado: una capa única (15 % o 40 %), pilares fijos y el corte por partes con un shader propio (funcionaba, pero no gustó).

### 2026-11-15 · En los diálogos se bloquean los controles y se avanza con teclado
- Qué: PlayerInput ignora moverse, saltar y disparar si IsControlLocked (habla o muerto). DialogueUI avanza con Jump, Shoot o clic, y no en el frame en que se abre.
- Por qué: ciclo 02. El salto pulsado al hablar salía al cerrar, el Brujo caminaba en el sitio y el diálogo solo avanzaba con clic.
- Descartado: comprobar isTalking en cada sistema; teclas fijas en vez de las acciones del jugador.

### 2026-11-15 · Squishy flota siempre a los pies del Brujo
- Qué: SlimeCompanion sin raycast: SmoothDamp hacia el lado del jugador según mira y a la altura de sus pies.
- Por qué: ciclo 02. La detección de suelo estaba invertida y nunca se ejecutaba; así es como se jugó y gusta.
- Descartado: corregir la condición para que se pose en plataformas (cambia el feel).

### 2026-11-15 · Solo el GameManager carga escenas
- Qué: OutroDialogueTrigger lanza el canal de fin del juego (DeathChannel) en vez de llamar a SceneManager.LoadScene.
- Por qué: ciclo 02. Un único sitio carga, en segundo plano y sin cargas dobles. Requiere empezar desde el menú.
- Descartado: un canal LevelCompleted nuevo (mismo destino, más cambios de escena).

### 2026-11-15 · Música en Streaming y texturas con tope de 2048
- Qué: las 6 pistas en Streaming; las 9 texturas de más de 2048 px a 2048, salvo las 3 hojas de tiles (siguen a 4096).
- Por qué: ciclo 02. Medido: 10 MB de RAM por pista; texturas del nivel de ≈764 a ≈439 MB sin cambio visible a 1080p.
- Descartado: comprimir las texturas (puede verse en degradados) y reducir las hojas de tiles (líneas entre tiles).

### 2026-11-18 · La pantalla final tiene una variante de victoria y otra de derrota
- Qué: SceneGameOver muestra la cima azul con el Brujo, Squishy y su familia ("Jugar de nuevo") o el abismo con Squishy solo ("Reintentar"). Título y botones centrados.
- Por qué: ciclo 03. Ganar y morir llevaban a la misma pantalla negra con la fuente por defecto.
- Descartado: una pantalla neutra, dos escenas separadas y repetir el claro del menú.

### 2026-11-18 · El resultado de la partida viaja en un asset y la victoria tiene su canal
- Qué: el GameManager anota GameResult (Victory o Defeat) antes de cargar; la victoria llega por LevelCompletedChannel y la muerte por DeathChannel.
- Por qué: ciclo 03. Ninguna escena busca a otra, y DeathChannel ya no sirve para dos cosas (cambia el final por DeathChannel del 2026-11-15).
- Descartado: un estático global (acoplamiento oculto).

### 2026-11-18 · Pruebas automáticas con Unity Test Framework
- Qué: el juego compila en su asmdef (SlimesWarden) y las pruebas EditMode en otro; 8 pruebas del resultado y de la pantalla final.
- Por qué: ciclo 03, a pedido del usuario: base para probar vidas, daño y estados. Al crear un asmdef, reimportar Assets/Scripts.
- Descartado: esperar a una regla más delicada.

### 2026-11-18 · Un fallo de audio no rompe el juego
- Qué: si al AudioManager le falta su prefab, avisa una vez y sigue sin sonido.
- Por qué: ciclo 03. La excepción del audio cortaba el disparo antes de crear la bala.
- Descartado: cambiar el orden en cada tirador.

### 2026-11-19 · Sonido en los botones del menú y de la pantalla final
- Qué: MenuSounds, en el Canvas de cada pantalla, toca un «plip» al cambiar de botón y un «plop» al pulsar (juego «Gota de slime» del Free UI Click Sound Effects Pack), por SFXCueChannel y el grupo UI del mixer.
- Por qué: ciclo 04. Confirma la navegación con teclado y la pulsación; el sonido no se corta al cambiar de escena.
- Descartado: sonar en OnSelect de MenuButtonFeedback (sonaría al abrir la pantalla y al recuperar la selección).

### 2026-11-19 · Disparos e impactos más bajos
- Qué: los 4 cues de magia bajan de 0,85-1 a unos −27 dB en el juego (jugador 0,75-0,88, enemigo 0,41-0,48, impactos 0,45-0,52 y 0,27-0,32).
- Por qué: ciclo 04, playtest: se oían muy fuertes. Medidos, tapaban la música por 10-20 dB; jugando, −33 y −30 dB quedaron bajos.
- Descartado: bajar el grupo SFX del mixer (bajaría también los pasos, que ya suenan flojos).

### 2026-11-19 · Del paquete de clics solo quedan los 2 sonidos usados
- Qué: se borraron 98 wav, el mp3 de muestra, los PDF y la imagen del Free UI Click Sound Effects Pack (de 12 MB a 144 KB), antes de entrar en git.
- Por qué: ciclo 04. Podar lo que no se usa; las referencias (guid de los .meta) no cambian.
- Descartado: mantenerlo entero. Otro sonido se reimporta desde Package Manager > My Assets.

### 2026-11-20 · Medir el rendimiento en el juego exportado
- Qué: FrameTimeRecorder mide 30 s en SampleScene con `--measure` (promedio, p95, p99 y peor frame; CPU y GPU). Primera medición: 0,46 ms de 16,6.
- Por qué: revisión 2026-11-20. Sin números no se optimiza; el Editor falsea la medición.
- Descartado: el Profiler del Editor (manual y mide también el Editor).

### 2026-11-21 · El rebote del trampolín manda sobre el salto
- Qué: tras saltar o rebotar, el suelo no cuenta 0,1 s (`groundIgnoreTime`), y un trampolín no es suelo mientras el Brujo cae sobre él.
- Por qué: revisión 2026-11-20 (H1). Un salto en el buffer pisaba el rebote y un doble toque saltaba en el aire.
- Descartado: un rebote mayor al pulsar a tiempo (diseño nuevo, habría que revisar las alturas del nivel).

### 2026-11-21 · Poda del código sin uso
- Qué: fuera IBulletPool, PetFollower, AudioSettingsBinder/SO y lo no usado de HealthSystem, BulletPool, MusicManager y PlayerFootsteps.
- Por qué: revisión 2026-11-20 (H4). Lo que no se usa confunde sobre qué está vivo; queda en git.
- Descartado: guardarlo para unas opciones de volumen que el juego no tiene.

### 2026-11-23 · Hojas del tileset Mossy en BC7
- Qué: las 15 hojas Mossy del juego, en BC7 de calidad máxima por override de Standalone (mismo tope de tamaño). Exportación de 632 a 347,5 MB; carga de 0,26 a 0,17 s.
- Por qué: ciclo 05 (revisión 2026-11-20). Eran el 61 % de la exportación; en las capturas antes/después no se nota.
- Descartado: comprimir en Default (afectaría a WebGL) y la calidad normal (dejaba una sombra tenue en los bordes).

### 2026-11-23 · Hojas en potencia de 2 y sin color oculto blanco
- Qué: las 2 hojas de tiles (3584) y las 3 Hanging Plants (2948 de alto) se rellenan con margen transparente arriba y a la derecha hasta 4096; en las 6 verdes, el color bajo alfa 0 pasa a negro.
- Por qué: ciclo 05. Unity no comprime en BC7 lo que no es potencia de 2, y el blanco oculto dejaba sombra bajo las plataformas verdes. Lo visible no cambia (verificado píxel a píxel).
- Descartado: dejarlas sin comprimir (≈470 MB) y quitarles los mipmaps (parpadeo de los tiles).

### 2026-11-23 · Capturas automáticas para cambios de arte
- Qué: `--capture <carpeta>` en el juego exportado guarda 6 capturas a alturas fijas, sin interfaz y con el tiempo congelado; el medidor anota también la carga del nivel.
- Por qué: ciclo 05. Comparar antes/después con la misma vista, como la ve el jugador.
- Descartado: mirarlo a ojo en el Editor.

### 2026-11-25 · Recoger munición se ve y se oye
- Qué: al tocarlo el Brujo, el coleccionable pide por canal el destello vfx_AmmoPickup (vfx_Heal_02 en cian, con pool) y Cue_Pickup_Ammo (3 variantes Spark de Electrified SFX Library); suma su munición una sola vez.
- Por qué: ciclo 06; en el playtest el destello y el sonido «gustan bastante», sin bugs.
- Descartado: implosión azul, sonidos derivados del proyecto y de Kenney (no gustaron) y un contador de munición (fuera de alcance).

### 2026-11-25 · Packs de sonido: solo entra lo que se usa
- Qué: de un pack importado se quedan los clips elegidos y su licencia; el resto sale de Assets (Electrified: 1,2 GB a la papelera, quedan 0,9 MB). Basta una licencia libre con su procedencia anotada; no hace falta CC0.
- Por qué: ciclo 06. Los WAV van por Git LFS: el repositorio crecería 1,2 GB por 3 sonidos.
- Descartado: dejar el pack entero (la exportación no crece, pero el repositorio sí).
