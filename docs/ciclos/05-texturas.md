# Ciclo 05: Texturas
<!-- Límite: 30 líneas. Archivo: docs/ciclos/05-texturas.md -->

## Objetivo
- Que el juego pese y cargue menos sin que se note en pantalla: las hojas del tileset Mossy (388 de los 632 MB exportados) pasan de RGBA32 sin comprimir a BC7.
- Encargo: propuesta del estudio, decidida en la revisión 2026-11-20 (H «texturas», opción a).

## Qué se construye
- El medidor (FrameTimeRecorder) anota también la carga del nivel, y un modo --capture (LevelCapture) guarda capturas a 1920x1080 a 6 alturas del nivel, sin interfaz y con el tiempo congelado.
- Override de Standalone a BC7 en las hojas Mossy: primero el rojo; si en las capturas no se nota, también verde y azul (con las hojas de tiles de 3584 a su tamaño).
- Una página de comparación antes/después con las capturas.

## Criterios de aceptación
- C1: las 15 hojas Mossy que entran en el juego van en BC7 y la exportación baja de 632 a menos de 360 MB · se verifica con: medición (informe de la exportación).
- C2: en las capturas antes/después de las 3 zonas no se ven bandas en los degradados ni líneas entre tiles · se verifica con: captura (página de comparación) y playtest.
- C3: la carga del nivel (de pulsar Jugar a la escena lista) no empeora y queda anotada en docs/rendimiento.md · se verifica con: medición (FrameTimeRecorder).

## Estimación
- Tú: de 1 a 3 h · Estudio: de 1 a 3 h (unas 2,5 h: tres exportaciones con Unity cerrado, la importación BC7, las capturas y el playtest) · Empezó: 2026-11-23 12:29

## Resultado
- Tablero: compila sí · pruebas 32 pasan (Test Runner del usuario) · QA sí · medido sí (docs/rendimiento.md) · captura sí (6 alturas, antes/después en capturas/ciclo05-texturas/)
- Criterios: C1 cumple: 15 hojas en BC7, exportación 632 → 347,5 MB · C2 cumple: el usuario no ve diferencia en la comparación final ni jugando (la sombra verde se corrigió) · C3 cumple: carga 0,259 → 0,168 s (medianas de 3)
- Tiempo: terminó 2026-11-23 21:13 · real unas 2 h de trabajo activo (12:29-14:15 hasta el playtest, más el cierre; 8 h 44 min de reloj con una pausa larga), frente a tu estimación (1-3 h) y la del estudio (1-3 h)
- Playtest: todo funciona; sin bandas, líneas entre tiles ni sombras.
- Crece: medidor con carga del nivel y capturas automáticas (LevelCapture) · Se ajusta: nada más · Se poda: nada (las 3 hojas sin uso se quedan)
- Integración: rama ciclo/05-texturas · descripción del estudio
