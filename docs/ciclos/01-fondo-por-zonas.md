# Ciclo 01: Fondo por zonas
<!-- Límite: 20 líneas. Archivo: docs/ciclos/01-fondo-por-zonas.md -->

## Objetivo
- Que cada zona del nivel (verde, roja y azul) se sienta como un lugar distinto y que la subida se note, sin estorbar la lectura de las plataformas.

## Qué se construye (plan inicial: lo que quedó está en "Resultado")
- El color de fondo de la cámara cambia según su altura: verde oscuro, azul marino en la frontera, vino y añil, con un fundido suave de ±3 unidades.
- Una capa lejana de pilares de musgo (BackgroundDecoration del color de cada zona), semitransparentes y con parallax: sigue a la cámara al 60 %.
- Dos scripts nuevos: ZoneBackgroundColor (en la cámara) y ParallaxLayer (en la capa).

## Cómo sabremos que funciona
- Al subir se nota el cambio de zona en el fondo, y los pilares se mueven más lentos que las plataformas.
- Las plataformas y los enemigos se distinguen del fondo a primera vista.
- La capa no tiembla cuando la cámara se mueve.

## Resultado
- Playtest: 6 rondas. El color por altura parpadeaba con los saltos, una sola capa lenta mezclaba zonas y los pilares fijos quitaban profundidad. Al final: parallax por zona al 80 % y desvanecido del pilar entero en 4 u. Al usuario le gusta con sus valores.
- Crece: el color por zona con margen y fundido (BackgroundZones) y una capa de parallax por zona (ZoneParallaxLayer) · Se ajusta: tono, transparencia 0,3 y los valores finales a ojo · Se poda: ZoneBackgroundColor, ParallaxLayer, los pilares fijos y el shader de corte por partes.
