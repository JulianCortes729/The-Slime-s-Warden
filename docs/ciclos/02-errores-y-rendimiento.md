# Ciclo 02: Errores, arquitectura y rendimiento
<!-- Límite: 20 líneas. Archivo: docs/ciclos/02-errores-y-rendimiento.md -->

## Objetivo
- Que los diálogos se sientan firmes y el juego cargue con mucha menos memoria, sin cambiar lo que ya gusta.

## Qué se construye
- Controles bloqueados en los diálogos: el Brujo se queda quieto y no guarda saltos ni disparos. El diálogo avanza con Espacio, Enter o clic.
- Squishy: se poda la detección de suelo que nunca se ejecutaba; sigue flotando a los pies del Brujo.
- El final del nivel pide la carga de escena al GameManager, como ya hace la muerte.
- Música en Streaming y tope de 2048 px en las texturas de más de 2048 (menos las hojas de tiles). Limpieza: pasos durante el retroceso y campos sin uso.

## Cómo sabremos que funciona
- Pulsar Espacio, A/D o Enter durante un diálogo no gira ni mueve al Brujo, y al cerrarlo no salta.
- Squishy, el nivel y la música se ven y suenan igual; tras el diálogo final se carga "Fin del Juego".
- Memoria estimada: texturas del nivel de ≈760 a ≈440 MB; música de ≈30 a <1 MB por escena.

## Resultado
- Playtest: 5 de 5 bien. El final carga "Fin del Juego" empezando desde el menú. Todo se ve y suena igual. SceneGameOver sigue vieja (siguiente ciclo).
- Crece: bloqueo de controles en la entrada, diálogo con teclado, escenas solo vía GameManager · Se ajusta: música en Streaming, texturas a 2048 salvo los tiles · Se poda: el raycast muerto de Squishy, isKnockedBack, maxLives y el Input Manager antiguo.
