# Revisión del ciclo 05: Texturas

## Para revisar primero
- Las Hanging Plants bajan de 1423 a 1024 px de ancho efectivo (D2, sección 1). El usuario no vio problema jugando, pero es la única pérdida real de resolución del ciclo.
- Se editaron 9 PNG del paquete Mossy, binarios que no se pueden revisar línea a línea (D1 y D4, sección 3): relleno a potencia de 2 y color oculto en negro. El registro dice que se comprobó píxel a píxel; conviene mirar el diff de LFS si te preocupa.
- Lo inferido por mí, no del registro: las deudas de la sección 9, parte de «Cómo cambiarlo» y los riesgos de BC7 en otras plataformas. Lo demás sale del registro.

## 1. Juego: ¿qué cambia para el jugador y qué valores se ajustan?
- El jugador no debería notar nada: mismo arte, menos memoria de vídeo y el nivel carga en 0,168 s en vez de 0,259 s (medianas de 3). El único cambio visible posible son las Hanging Plants, algo menos nítidas.
- Ajuste: `Mossy - *.png.meta · bloque Standalone · textureFormat` = 25 (BC7) en 15 hojas del juego; -1 devuelve la hoja a sin comprimir (más MB, mismo aspecto).
- Ajuste: `Mossy - *.png.meta · compressionQuality` = 100 (Standalone): con 50 apareció una sombra bajo las plataformas verdes; bajarlo acelera la importación pero puede devolverla.
- Ajuste: `Mossy - Hanging Plants*.png.meta · maxTextureSize` = 2048: a 4096 la planta ganaría detalle, pero cada hoja pasa de 2,7 a 10,7 MB y C1 no se cumple.
- Ajuste: `LevelCapture.cs · Heights` = −14, −6, 4, 12, 24, 35 (alturas de captura; no afectan al juego normal).

## 2. Alcance: ¿se hizo más o menos de lo pedido?
- Pedido: pasar las hojas Mossy a BC7 (632 → menos de 360 MB), sin cambios visibles y con la carga del nivel medida.
- De más: nada que no sirva al objetivo. El relleno de PNG y el color oculto no estaban en el plan: salieron del hallazgo de que BC7 no se aplica sin potencia de 2 y de la sombra verde (D1, D4).
- De menos: nada. Las 3 hojas Mossy sin uso quedan sin podar por decisión del usuario (D10).

## 3. Archivos: ¿qué se creó o modificó y qué responsabilidad tiene cada uno?
- `Assets/Scripts/Diagnostics/LevelCapture.cs` · creado · léelo entero (patrón nuevo: captura automática con el tiempo congelado; 77 líneas): coloca al Brujo a 6 alturas y guarda un PNG por altura.
- `Assets/Scripts/Diagnostics/FrameTimeRecorder.cs` · modificado · léelo entero (punto de entrada de las dos herramientas): acepta `--capture`, mide la carga del nivel y la anota en el informe.
- `Assets/Tests/EditMode/FrameTimeRecorderTests.cs` · modificado · basta el resumen: 2 pruebas nuevas (`Capturas_CubrenLasTresZonas`, `NombreDeCaptura_OrdenadoYConSuAltura`); el archivo suma 8 y el total es 32.
- 17 `Assets/Mossy Tileset/**/*.png.meta` · modificados · basta el resumen con un ejemplo (`Mossy - TileSet.png.meta`): solo el bloque Standalone (`textureFormat 25`, `compressionQuality 100`, `overridden 1`; en 8, `maxTextureSize` de 2048 a 4096).
- 9 `Assets/Mossy Tileset/**/*.png` · modificados · basta el resumen: 5 rellenados a potencia de 2 (TileSet verde y azul, 3 Hanging Plants) y las 6 verdes con el color bajo alfa 0 en negro (TileSet y Hanging Plants verdes tienen ambas cosas).
- `docs/ciclos/05-texturas.md` (creado), `docs/rendimiento.md`, `docs/decisiones.md` (modificados) · basta el resumen: ciclo, fila de medición con carga y MB, tres decisiones del 2026-11-23.

## 4. Arquitectura: ¿cómo se integra con lo que ya existía?
- `FrameTimeRecorder` sigue siendo la única entrada: se crea con `RuntimeInitializeOnLoadMethod` solo si hay `--measure` o `--capture`. Pulsa Jugar vía `MainMenuUI.OnPlayButtonClicked`, mide la carga y delega en `LevelCapture.Run`.
- `LevelCapture` busca `PlayerMovement`, saca su `Rigidbody2D` de la física y apaga los `Canvas`; la cámara de Cinemachine y `BackgroundZones` lo siguen como al jugar.
- Las texturas no tocan código: todo vive en los `.meta` de importación.
- Mapa: fila Diagnóstico con `LevelCapture` y la carga; 32 pruebas; BC7 en «Datos»; cómo comparar arte en «Para cambiar X»; deuda de texturas ajustada.

## 5. Funcionamiento: ¿cuál es el flujo principal, paso a paso?
1. El exe se abre con `--measure <informe>` y/o `--capture <carpeta>`; `CreateIfRequested` crea el recorder.
2. `Start` quita vsync, pulsa Jugar, espera a SampleScene y a un frame dibujado: eso es la carga.
3. Con `--measure`: 3 s de calentamiento, 30 s de frames y se escribe el informe (con la carga).
4. Con `--capture` (después de medir, porque mueve al Brujo): `LevelCapture.Run` deja al Brujo sin física y sin Canvas.
5. Por cada altura: mueve al Brujo, espera 2,5 s reales, congela `Time.timeScale`, espera fin de frame, guarda el PNG (`try/catch/finally`) y descongela.
6. `Application.Quit`.

## 6. Decisiones: ¿qué se decidió y por qué, con ventajas, desventajas, alternativa y cómo cambiarlo?
### D1 · Rellenar con margen transparente las hojas que no miden potencia de 2
- Tipo y autor: arte/rendimiento · decidió el usuario
- Dónde: 5 PNG (TileSet verde y azul: 3584² → 4096²; 3 Hanging Plants: 2048x2948 → 2048x4096)
- Por qué: en Unity 2022.3 BC7 no se aplica sin potencia de 2 (con mipmaps), sin aviso en el log. Se probó exportando: el tamaño no cambió.
- Ventajas: todo en BC7 y sin cambio visible; los sprites no se mueven porque Unity cuenta su rect desde abajo a la izquierda. Verificado píxel a píxel: contenido idéntico y margen transparente.
- Desventajas: 5 PNG del paquete editados (en LFS, reversibles).
- Alternativa descartada: dejarlas sin comprimir (≈470 MB, C1 no se cumplía) y quitar mipmaps (los tiles, dibujados a menos de un tercio, parpadearían).
- Cómo cambiarlo: medio; restaurar los PNG y sus `.meta` desde LFS y perder ≈110 MB de ahorro (inferido).
### D2 · Hanging Plants con tope de 2048 sobre 2048x4096
- Tipo y autor: rendimiento · decidió la IA
- Dónde: `Mossy - Hanging Plants*.png.meta · maxTextureSize`
- Por qué: quedan a 1024x2048 (2,7 MB cada una frente a 10,7) y aun así 1,7 veces más detalle del que dibuja la pantalla (148 px por unidad frente a 512 de la hoja).
- Ventajas: ahorro necesario para C1. Desventajas: menos resolución que antes (1423x2048).
- Alternativa descartada: tope 4096 (≈372 MB, C1 no cumplía).
- Cómo cambiarlo: poco; el tope en 3 `.meta`.
### D3 · BC7 por override de Standalone, no en Default
- Tipo y autor: rendimiento · decidió la IA
- Dónde: 17 `.png.meta`, bloque `buildTarget: Standalone` (línea 84 en `Mossy - TileSet.png.meta`)
- Por qué: BC7 pesa 1 byte por píxel como DXT5 pero conserva mejor los degradados; el override solo toca PC (WebGL no cambia). El override que ya traían los `.meta` llevaba 2048 y reduciría las hojas de tiles, devolviendo las líneas entre tiles: por eso 4096 en 8.
- Relación: respeta D7 del ciclo 02 (hojas de tiles a 4096).
- Ventajas: ahorro de 284,5 MB exportados. Desventajas: solo vale para PC; otra plataforma sigue en RGBA32 (inferido).
- Alternativa descartada: comprimir en Default (afectaría a todas las plataformas).
- Cómo cambiarlo: poco; el bloque en los `.meta`.
### D4 · Color oculto bajo alfa 0 de las 6 hojas verdes pasa a negro
- Tipo y autor: arte · decidió la IA (arreglo de lo que vio el usuario)
- Dónde: 6 PNG verdes del paquete Mossy
- Por qué: las verdes guardaban blanco bajo lo transparente; las rojas y azules, negro. BC7 codifica color y alfa en bloques de 4x4 y ese blanco dejaba una sombra tenue en los bordes. Con negro ya no aparece (recorte ×4 y comparación del usuario).
- Ventajas: lo visible no cambia (alfa idéntico y mismo color en todo píxel visible). Desventajas: otra edición binaria del paquete.
- Alternativa descartada: dejar las verdes sin comprimir (perdían los MB) y DXT5 (alfa más exacto, color de menos calidad en degradados).
- Cómo cambiarlo: medio; restaurar los PNG, pero la sombra volvería.
### D5 · BC7 en calidad máxima (compressionQuality 100)
- Tipo y autor: rendimiento · decidió la IA
- Dónde: los 17 `.meta` (Standalone)
- Por qué: con calidad normal, un recorte ×4 mostraba sombra bajo las plataformas verdes; con la máxima casi desaparece (D4 la quitó del todo).
- Ventajas: sin coste en juego (mismo tamaño). Desventajas: importar tarda algo más (la exportación siguió en ~30 s).
- Alternativa descartada: nada registrado.
- Cómo cambiarlo: poco; bajar a 50 y repetir la comparación (inferido: tras D4 quizá ya no haga falta el 100).
### D6 · Comparar con capturas automáticas del juego exportado (`--capture`)
- Tipo y autor: arquitectura/herramienta · decidió el usuario
- Dónde: `FrameTimeRecorder.cs · CaptureArgument (línea 19)`, `LevelCapture.cs`
- Por qué: misma vista exacta a 1080p como la ve el jugador, y sirve para futuros cambios de arte.
- Ventajas: comparable y repetible. Desventajas: ~50 líneas más y una exportación por paso.
- Alternativa descartada: mirarlo a ojo en el Editor (otra vista; los degradados finos se comparan mal).
- Cómo cambiarlo: poco; es una herramienta aparte.
### D7 · `LevelCapture` mueve al Brujo sin física y congela el tiempo
- Tipo y autor: patrón · decidió la IA
- Dónde: `LevelCapture.cs · Run (línea 19), SettleSeconds (línea 17)`
- Por qué: la cámara sigue al Brujo y su vista mide el ancho del nivel, así que basta la altura; sin física no dispara el final ni recibe balas.
- Ventajas: cámara, fondo por zonas y parallax se actualizan como jugando. Desventajas: animaciones, Squishy y balas varían entre capturas: se compara a ojo (PSNR 45-50 dB), no píxel a píxel.
- Alternativa descartada: un script de Editor que renderiza (determinista, pero el fondo y el parallax solo se actualizan jugando) y mover la cámara a mano apagando Cinemachine.
- Cómo cambiarlo: poco; `Heights` y `SettleSeconds`.
### D8 · Alcance escalonado: primero rojo, luego verde y azul
- Tipo y autor: diseño de ciclo · decidió el usuario
- Por qué: si en las capturas no se notaba, el ahorro completo cabía en un ciclo (632 → ~340 MB).
- Ventajas: ahorro completo. Desventajas: más capturas que revisar. Alternativa descartada: solo el rojo (dejaba 309 MB).
### D9 · El medidor anota la carga del nivel
- Tipo y autor: rendimiento · decidió la IA
- Dónde: `FrameTimeRecorder.cs · Start (líneas 57-64)`
- Por qué: C3 pide comparar la carga. Va de pulsar Jugar al primer frame dibujado.
- Desventajas: una carga por ejecución y la caché de disco acelera tras la primera; se miden 3 veces y se toma la mediana.
### D10 · Decisiones menores
- Dejar las 3 hojas Mossy sin uso (el usuario): quedan a mano para el nivel; pesan en el repositorio.
- Siguiente paso elegido por el usuario: partículas y sonido al recoger coleccionables, antes que H2 o más texturas.

## 7. Conceptos: ¿qué conceptos de videojuegos aparecen?
- **Compresión por bloques (BC7)** · rendimiento · `.meta` de Mossy: la GPU lee la textura comprimida en bloques de 4x4 y ocupa 1 byte por píxel en vez de 4. BC7 conserva mejor los degradados que DXT5.
- **Color oculto bajo la transparencia (alfa 0)** · gráficos · PNG verdes: un píxel invisible aún guarda un color, y la compresión por bloques lo mezcla con los vecinos visibles. Si es blanco, aparece un halo.
- **Textura en potencia de 2** · rendimiento · hojas rellenadas: algunos formatos exigen lados 512, 1024, 2048, 4096… Unity 2022.3 no comprime en BC7 una que no lo sea (con mipmaps).
- **Mipmaps** · rendimiento · hojas de tiles: copias reducidas que se usan al dibujar pequeño y evitan el parpadeo.
- **Override por plataforma** · motor · bloque Standalone de los `.meta`: la importación puede ser distinta por plataforma sin tocar las demás.
- **Captura automática para comparar arte (PSNR)** · motor · `LevelCapture.cs`: se guardan imágenes antes y después de un cambio y se mide cuánto difieren.

## 8. Riesgos: ¿qué problemas, dependencias o limitaciones tiene?
- `SettleSeconds` fijo (2,5 s): en un equipo lento la cámara podría no alcanzar al Brujo · baja · esperar a que se asiente o subir el valor.
- `--capture` sin ruta no avisa y no hace nada · baja · avisar en el log (lo señaló QA).
- `LevelCapture.Run` no tiene prueba automática (depende de la escena) · baja · solo se prueban `Heights` y `FileName`.
- BC7 solo en Standalone: otra plataforma exportaría RGBA32 (inferido) · media · repetir el override.
- Una hoja nueva que no mida potencia de 2 se queda sin comprimir sin aviso · media · medirla al importar.
- Una hoja nueva con blanco bajo lo transparente reproducirá la sombra · media · limpiarlo antes de importar.

## 9. Deuda técnica: ¿qué atajos se tomaron a propósito?
- Comparación a ojo y PSNR, no píxel a píxel (por animaciones y balas) · cuando un cambio de arte sea tan fino que no se vea.
- 3 hojas Mossy sin uso en el repositorio · cuando pesen en el clon o se necesiten.
- Texturas no Mossy (slimes 48 MB, otras 112 MB) sin comprimir (inferido) · cuando el peso importe.
- PNG del paquete editados en vez de reexportar el original · si llega una versión nueva del paquete (inferido).

## 10. Rendimiento: ¿hay posibles problemas de rendimiento, memoria o asignaciones?
- Medido (`docs/rendimiento.md`, medianas de 3, presupuesto 16,6 ms): exportación 632 → 347,5 MB y texturas 551 → 266 MB; carga 0,259 → 0,168 s; frame medio 0,53 ms (igual), p99 0,75 → 0,76 ms, peor 1,66 → 3,03 ms por un pico aislado de 25 ms en una de las 3 ejecuciones. Frente al ciclo 04 (0,46 ms) sube con el medidor nuevo, no con BC7 (la «antes» ya da 0,53).
- Las asignaciones de `LevelCapture` (textura y PNG por captura) ocurren solo con `--capture`; la textura se libera en `finally`.
- Posible: el pico de 25 ms podría ser ruido del sistema; no se investigó (inferido) · repetir la medición y mirar el Profiler.

## 11. Verificación: ¿qué se probó y qué conviene probar a mano?
- Criterios: C1 cumple: 15 hojas en BC7, 632 → 347,5 MB (< 360) · C2 cumple: capturas de las 3 zonas (PSNR 45-50 dB) y comparación y playtest del usuario sin bandas, líneas entre tiles ni sombras · C3 cumple: carga 0,259 → 0,168 s.
- Hecho: compilación con el Roslyn de Unity; 3 exportaciones medidas (antes, rojo, final); capturas antes/después; revisión de qa-playtest (nada bloquea; corregidas la fila de rendimiento y el try/catch); 32 pruebas en verde en el Test Runner del usuario; playtest del usuario. Las 12 capturas (antes/después) quedan en `capturas/ciclo05-texturas/`, fuera de git (se regeneran con `--capture`).
- A mano: mirar de cerca las Hanging Plants (1423 → 1024 px) y los bordes de las plataformas verdes; subir y bajar rápido buscando parpadeo de tiles; abrir el exe sin argumentos (sin medidor) y con `--capture <carpeta>` (6 PNG a 1920x1080 sin interfaz).

## 12. Mantenimiento: ¿qué hay que entender para mantenerlo y ampliarlo?
- Regla de importación · `Mossy - *.png.meta · Standalone`: una hoja nueva debe medir potencia de 2 y no guardar blanco bajo lo transparente, o se queda sin comprimir o con halo.
- Comparar arte · `FrameTimeRecorder.cs`, `LevelCapture.cs`: exporta antes, exporta después con `--capture` y compara las 6 capturas.
- Prueba: en `LevelCapture.cs · Heights` cambia −14 por 0, exporta con `--capture` y mira la captura nueva; vuelve a −14.
- Preguntas: ¿por qué rellenar el PNG con margen transparente no mueve los sprites? ¿Por qué la sombra aparecía solo en las plataformas verdes?

## Delegación: ¿qué se encargó a cada agente y qué le faltó al encargo?
- estudio:qa-playtest (modelo del agente: encajaba, revisión acotada con criterios claros) · revisar medidor, capturas y `.meta` y preparar ≤5 puntos de playtest · cumplió, sin salirse de los límites (solo lectura, sin reabrir BC7, el relleno ni el color oculto) · le faltó: poder usar git diff y Unity, que suplió el director · lo escribió el estudio.

## Correcciones
- ninguna
