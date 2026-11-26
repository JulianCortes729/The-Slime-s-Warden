# Ciclo 06: Recoger munición
<!-- Límite: 30 líneas. Archivo: docs/ciclos/06-recoger-municion.md -->

## Objetivo
- Que recoger munición se note: hoy el coleccionable suma 2 balas y desaparece en silencio, sin ningún aviso.
- Encargo: siguiente paso de MEMORIA.md (CollectibleAmmo con partículas y sonido al tocarlo el Brujo).

## Qué se construye
- Al tocarlo el Brujo: el destello de vfx_Heal_02 recoloreado en cian (el de la llama), pedido al VFXManager por su canal (con pool).
- Un sonido con 3 variantes Spark del pack Electrified SFX Library (elegidas escuchando maquetas), como AudioCue lanzado por SFXCueChannel.
- La regla de recogida (solo el Brujo y una sola vez) como función pura con su prueba. Sin contador de munición en pantalla.

## Criterios de aceptación
- C1: cada coleccionable suma su munición una sola vez (aunque el Brujo lo toque dos veces en el mismo paso de física) y solo si lo toca el Brujo · se verifica con: prueba CollectibleAmmoTests
- C2: al recogerlo aparece en su sitio el destello de vfx_Heal_02 en cian, y la consola queda sin errores tras recoger varios · se verifica con: playtest
- C3: al recogerlo suena el sonido elegido de un pack de licencia libre, con su procedencia anotada (reescrito: decía «CC0»; el usuario eligió un pack libre de regalías); se oye sobre la música sin tapar el disparo · se verifica con: playtest (volumen en pasos de 3 dB)
- C4: los efectos se reutilizan: tras recoger 10 coleccionables, bajo el VFXManager hay como mucho 3 copias del destello · se verifica con: playtest (mirar la Hierarchy)

## Estimación
- Tú: de 1 a 3 h · Estudio: de 1 a 3 h (unas 2,5 h: buscar el sonido con maquetas, el prefab del efecto, editar la escena con Unity guardado, la prueba y el playtest) · Empezó: 2026-11-24 19:41

## Resultado
- Tablero: compila sí · pruebas 37 pasan (Test Runner del usuario) · QA sí · medido sí (docs/rendimiento.md: carga 0,174 s, frame 0,62 ms, exportación 353,0 MB) · captura no aplica: el destello solo aparece al recoger y `--capture` no juega; C2 lo verificó el usuario jugando
- Criterios: C1 cumple: CollectibleAmmoTests (5 pruebas) pasan y jugando suma una vez · C2 cumple: el usuario vio el destello cian en su sitio y la consola sin errores · C3 cumple: 3 Spark de Electrified SFX Library (libre de regalías; ReadMe junto a los sonidos y procedencia en docs/porques), se oyen y se distinguen del disparo · C4 cumple: el usuario lo comprobó en la Hierarchy («todo funciona correctamente»)
- Playtest: destello «bien», sonido «bien»; «el sonido y el destello me gustan bastante». Sin bugs.
- Crece: la recogida con destello y sonido · Se ajusta: nada (volumen 0,45-0,5) · Se poda: nada; del pack Electrified solo quedan los 3 sonidos
- Tiempo: terminó 2026-11-26 16:05 · real de 1 a 3 h de trabajo (según el usuario; unos 2 días de reloj con pausas), igual que tu estimación (1-3 h) y la del estudio (1-3 h)
- Integración: rama ciclo/06-recoger-municion · descripción del estudio
