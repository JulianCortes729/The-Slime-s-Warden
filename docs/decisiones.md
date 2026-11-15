# Decisiones

### 2026-11-13 · El color de fondo cambia por zona, no por altura
- Qué: BackgroundZones elige la zona de la cámara con un margen de 2 u y funde el color en el tiempo (1 s). Tonos oscuros: verde, vino y añil.
- Por qué: ciclo 01. Con el color según la altura parpadeaba, porque la cámara sube y baja con cada salto.
- Descartado: suavizar la altura (saltando en la frontera seguiría oscilando).

### 2026-11-14 · Cada zona tiene su propia capa de pilares con parallax
- Qué: ZoneParallaxLayer por zona, al 80 % de la velocidad del nivel. Cada pilar se desvanece entero en las 4 u antes de salir de su franja.
- Por qué: ciclo 01, playtests 1 a 6. Con una sola capa se mezclaban las zonas, y fijos en el mundo faltaba profundidad.
- Descartado: una capa única (15 % o 40 %), pilares fijos y el corte por partes con un shader propio (funcionaba, pero no gustó).
