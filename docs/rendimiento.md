# Rendimiento
<!-- Una fila por medición; la añade /estudio:playtest al correr el juego. Límite: 40 líneas: si se pasa, borra las filas más viejas. Compara solo mediciones del mismo equipo. -->
- Presupuesto: 16,6 ms por frame (60 FPS); docs/vision.md no fija otro (PC, 1920x1080).
- Equipo: i7-9700 · RTX 3050 · 16 GB · pantalla 1920x1080 a 75 Hz · Dónde: juego exportado para Windows, calidad Ultra, sin vsync.
- Cómo se mide: comando «Medir» de CLAUDE.md (FrameTimeRecorder: carga del nivel, 3 s de calentamiento y 30 s en SampleScene con el jugador quieto al inicio). Desde el ciclo 05, 3 ejecuciones: se anota la mediana de cada columna (la primera carga, con la caché de disco fría, sale más lenta).

| Fecha | Ciclo | Escena | Carga (s) | Promedio (ms) | p95 | p99 | Peor frame | Exportado (MB) | Nota |
|---|---|---|---|---|---|---|---|---|---|
| 2026-11-20 | 04 | SampleScene (inicio) | — | 0,46 | 0,56 | 0,67 | 1,61 | 632 | Primera medición. CPU hilo principal 0,45 · GPU 0,16. 551 MB de texturas. |
| 2026-11-23 | 05 | SampleScene (inicio) | 0,259 | 0,53 | 0,66 | 0,75 | 1,66 | 632 | Antes de comprimir (con el medidor nuevo). |
| 2026-11-23 | 05 | SampleScene (inicio) | 0,168 | 0,53 | 0,66 | 0,76 | 3,03 | 347,5 | Hojas Mossy en BC7: texturas de 551 a 266 MB. Un pico aislado de 25 ms en una de las 3 ejecuciones. |
