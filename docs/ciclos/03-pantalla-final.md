# Ciclo 03: Pantalla final
<!-- Límite: 20 líneas. Archivo: docs/ciclos/03-pantalla-final.md -->

## Objetivo
- Que el final premie al ganar y anime a reintentar al morir, con el nivel de acabado del menú.

## Qué se construye
- SceneGameOver con dos variantes: victoria en la cima azul (el Brujo, Squishy y su familia) y derrota en el abismo oscuro (Squishy solo). Título y botones centrados (maqueta A).
- Botones "Jugar de nuevo" o "Reintentar", y "Menú", con el estilo del menú y navegación con teclado.
- La victoria tiene su propio canal (LevelCompleted). El GameManager guarda el resultado (GameResult) antes de cargar la escena.
- Primeras pruebas automáticas (Unity Test Framework): un asmdef para el juego y otro para las pruebas EditMode.

## Cómo sabremos que funciona
- Al morir se ve la derrota; al terminar el diálogo final, la victoria. Tras ganar y morir después, sale la derrota.
- Los botones se eligen con el teclado y el ratón, y llevan al nivel o al menú.
- Las pruebas EditMode pasan en el Test Runner.

## Resultado
- Playtest: 8 pruebas en verde; menú, victoria, derrota y sonido, bien. Fallo arreglado: al pasar al asmdef, PooledAudio quedó como script perdido y el error de audio cortaba los disparos.
- Crece: pantalla con victoria y derrota, GameResult, LevelCompletedChannel, SelectionKeeper y 8 pruebas · Se ajusta: AudioManager falla sin romper el juego · Se poda: la pantalla vieja, DeathChannel para ganar y el Update de MainMenuUI.
