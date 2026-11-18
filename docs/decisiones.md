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
