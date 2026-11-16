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
