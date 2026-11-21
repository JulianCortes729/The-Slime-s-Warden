# Rendimiento
<!-- Una fila por medición; la añade /estudio:playtest al correr el juego. Límite: 40 líneas: si se pasa, borra las filas más viejas. Compara solo mediciones del mismo equipo. -->
- Presupuesto: 16,6 ms por frame (60 FPS); docs/vision.md no fija otro (PC, 1920x1080).
- Equipo: i7-9700 · RTX 3050 · 16 GB · pantalla 1920x1080 a 75 Hz · Dónde: juego exportado para Windows, calidad Ultra, sin vsync.
- Cómo se mide: comando «Medir» de CLAUDE.md (FrameTimeRecorder: 3 s de calentamiento y 30 s en SampleScene con el jugador quieto al inicio).

| Fecha | Ciclo | Escena | Promedio (ms) | p95 | p99 | Peor frame | Nota |
|---|---|---|---|---|---|---|---|
| 2026-11-20 | 04 | SampleScene (inicio) | 0,46 | 0,56 | 0,67 | 1,61 | Primera medición. CPU hilo principal 0,45 · GPU 0,16. Exportado: 632 MB, de ellos 551 MB de texturas. |
