# Revisión del ciclo 01: Fondo por zonas

## Para revisar primero
- El archivo del ciclo marca "Qué se construye" como plan inicial: lo que quedó está en su "Resultado" y aquí.
- Los valores finales de ZoneRed (franja −2 a 19) solapan con las zonas vecinas, y los 3 pilares rojos desactivados se desactivaron a mano: ver D20 y sección 8.
- Hay cambios tuyos fuera del fondo en el mismo diff de SampleScene (VirtualCamera, jumpForce 5,4, Player, tilemap, Trampoline (3), prefabs). No son de este ciclo y entran en el mismo commit (sección 3).

## 1. Juego: ¿qué cambia para el jugador y qué valores se ajustan?
- El color de fondo cambia por zona (verde oscuro, vino, añil) con un fundido de 1 s. Detrás hay pilares de musgo semitransparentes que se mueven más despacio que el nivel (parallax) y se desvanecen al acercarse a la frontera de su zona.
- Ajuste: `BackgroundZones · margin` = 2: unidades que la cámara pasa de la frontera antes de cambiar de zona. Más alto, menos cambios por saltos pero más retraso. Más bajo, vuelve el parpadeo.
- Ajuste: `BackgroundZones · fadeDuration` = 1 en la escena (el script trae 1,5): segundos de fundido de color.
- Ajuste: `ZoneParallaxLayer · followFactor` = 0,2 en las 3 capas: 0 = pilares quietos como el nivel, 1 = pegados a la cámara. Subirlo los hace más lentos respecto a la pantalla y más rápidos respecto a las plataformas.
- Ajuste: `ZoneParallaxLayer · fadeDistance` = 4 (unidades de desvanecido antes de la frontera; más alto, más tiempo medio transparentes) y `bandBottom / bandTop`: ZoneGreen −30 a 1, ZoneRed −2 a 19, ZoneBlue 17 a 43,48. Zonas en `BackgroundZones · zones · startHeight`: −16,332, 1 y 17.
- Ajuste: alfa de los pilares = 0,3 (en cada SpriteRenderer, no en un símbolo del código): más alto, compiten con las plataformas.

## 2. Alcance: ¿se hizo más o menos de lo pedido?
- Pedido: que cada zona se sienta distinta y que la subida se note, sin estorbar la lectura de las plataformas.
- De más: nada que haya encontrado. Se construyeron y podaron cosas por el camino (ZoneBackgroundColor, ParallaxLayer, shader de franja), que ya no están.
- De menos: nada. El plan inicial (color por altura, parallax al 60 %) cambió por decisiones tuyas en 6 playtests.

## 3. Archivos: ¿qué se creó o modificó y qué responsabilidad tiene cada uno?
- `Assets/Scripts/Background/BackgroundZones.cs` · creado · léelo entero (patrón nuevo: evento de Cinemachine, histéresis y fundido; 100 líneas): decide la zona de la cámara con margen y mezcla el color de fondo.
- `Assets/Scripts/Background/ZoneParallaxLayer.cs` · creado · léelo entero (mismo patrón, y es donde ajustas el parallax): mueve la capa y desvanece cada pilar según su distancia a la frontera.
- `Assets/Scenes/SampleScene.unity` · modificado · basta el resumen (YAML de 3710 líneas añadidas): BackgroundFar con 3 capas y 38 pilares (orden −100, alfa 0,3, sin colisionador). Incluye cambios tuyos ajenos al fondo.
- Los `.meta` (carpeta y scripts) y `docs/ciclos/`, `docs/porques/` y `docs/decisiones.md` · creados · basta el resumen: identificadores de Unity y documentación.

## 4. Arquitectura: ¿cómo se integra con lo que ya existía?
- Ambos scripts se suscriben a `CinemachineCore.CameraUpdatedEvent` en OnEnable y se desuscriben en OnDisable. No tocan el input, el Player ni la física.
- `BackgroundZones` va en BackgroundFar y escribe `Camera.backgroundColor` de la cámara de salida del Brain.
- `ZoneParallaxLayer` va en ZoneGreen, ZoneRed y ZoneBlue, hijas de BackgroundFar, y mueve su propio transform.
- No usa canales de eventos ScriptableObject ni pooling: no hacen falta (no se crean objetos en ejecución). Depende de Cinemachine: no sigue la convención de canales, pero es un evento del propio motor.

## 5. Funcionamiento: ¿cuál es el flujo principal, paso a paso?
1. Cinemachine mueve la cámara en el frame y lanza `CameraUpdatedEvent`.
2. `BackgroundZones.OnCameraUpdated`: si es el primer frame, fija la zona y el color sin fundido. Si no, `ZoneWithMargin(y)` cambia de zona solo si y pasa la frontera más `margin`.
3. Los pesos de cada zona se mueven hacia 1 (la actual) o 0 a razón de `deltaTime / fadeDuration`. Si cambió algo, `Apply` mezcla los colores ponderados.
4. Cada `ZoneParallaxLayer.OnCameraUpdated` coloca la capa en `_basePosition.y + (cameraY − referenceCameraY) · followFactor`.
5. Para cada pilar calcula `gap`, su distancia a la frontera más cercana de la franja. Alfa = alfa base · clamp01(gap / fadeDistance). Si cambió, lo aplica, y con 0 apaga el renderer.

## 6. Decisiones: ¿qué se decidió y por qué?
### D1 · El usuario eligió la maqueta A (color por zona y capa lejana de pilares) [registro D1]
- Tipo y autor: diseño · decidió el usuario (entre 3 maquetas).
- Dónde: `SampleScene · BackgroundFar`.
- Por qué: da profundidad y sensación de subir sin tapar plataformas.
- Ventajas: cada zona tiene identidad; el nivel no se ve vacío.
- Desventajas: un script más y muchos sprites en la escena.
- Alternativa descartada: B (solo color, sigue vacío) y C (dos capas, más cargada y confundible con plataformas).
- Cómo cambiarlo: poco (desactivar BackgroundFar).

### D2 · Color por zona con margen y fundido en el tiempo (BackgroundZones) [registro D10, D2 y D6; D4 podada]
- Tipo y autor: patrón · decidió el usuario (la IA propuso el script de paradas por altura, podado).
- Dónde: `BackgroundZones · ZoneWithMargin (línea 84)`, `OnCameraUpdated (línea 47)`, `Apply (línea 92)`.
- Por qué: la cámara sigue cada salto, así que el color por altura parpadeaba cerca de una frontera.
- Ventajas: los saltos no mueven el color; se edita en el Inspector.
- Desventajas: el fundido dura un tiempo y no sigue cuánto se sube; el color cambia en toda la pantalla a la vez.
- Alternativa descartada: suavizar la altura (saltando en la frontera oscilaría).
- Cómo cambiarlo: poco (valores) o medio (otro modelo de color).

### D3 · Parallax por zona, no una sola capa (ZoneParallaxLayer) [registro D13, D14, D15, D16; D11 ajustada en D20]
- Tipo y autor: diseño · decidió el usuario (la idea de una capa por zona; la IA el cálculo de la capa).
- Dónde: `ZoneParallaxLayer · OnCameraUpdated (línea 62)`; capas ZoneGreen, ZoneRed, ZoneBlue.
- Por qué: con una capa lenta un pilar seguía visible durante todo el nivel y parecía un error; los pilares fijos quitaban profundidad.
- Ventajas: hay parallax y ningún pilar cruza una frontera.
- Desventajas: 38 pilares (más que los 15 de la maqueta) y 3 capas que ajustar.
- Alternativa descartada: capa única al 15 % o 40 %, pilares fijos (D14, probado) y SpriteMask (un corte recto parece un fallo).
- Cómo cambiarlo: medio (rehacer pilares y franjas en la escena).

### D4 · Mover con `CinemachineCore.CameraUpdatedEvent` y no con LateUpdate [registro D7]
- Tipo y autor: arquitectura · decidió la IA.
- Dónde: ambos scripts, `OnEnable (línea 36 y 50)`.
- Por qué: llega justo tras colocar la cámara, en el mismo frame, así no tiembla (el temblor ya fue un problema). El orden de LateUpdate respecto al Brain no está garantizado.
- Ventajas: sin temblor ni frame de retraso.
- Desventajas: depende de Cinemachine. El evento llega una vez por frame, por cada Brain (en CinemachineBrain.cs:1026, al final de PushStateToUnityCamera; comprobado por el director). Aquí solo hay uno.
- Alternativa descartada: LateUpdate propio, o hacer la capa hija de la cámara (sin parallax).
- Cómo cambiarlo: poco por script.

### D5 · Desvanecido del pilar entero, 4 u, tras probar el corte por shader [registro D17, D18, D19]
- Tipo y autor: diseño · decidió el usuario.
- Dónde: `ZoneParallaxLayer · fadeDistance (línea 22)`; lógica en las líneas 70-84.
- Por qué: el corte por partes con shader propio funcionaba, pero no le gustó. Se podó (shader, material, carpeta Shaders).
- Ventajas: sin shader propio que mantener con URP.
- Desventajas: pilares más tiempo medio transparentes y tramos con pocos pilares cerca de las fronteras.
- Alternativa descartada: el corte por partes (D17).
- Cómo cambiarlo: poco (fadeDistance) o mucho (volver al shader: hay que recrearlo).

### D6 · Referencia y relleno de cada capa calculados por el generador [registro D8, D16]
- Tipo y autor: código · decidió la IA.
- Dónde: `ZoneParallaxLayer · referenceCameraY (línea 15)`: 9, −5,847 y 25,852 en la escena.
- Por qué: el Player estaba guardado en y 20,4 (ahora lo pusiste en −17,7). La cámara no empieza siempre abajo, así que la referencia es fija. Cada capa se llena para todo lo que su franja recorre.
- Ventajas: la franja nunca se queda sin pilares.
- Desventajas: si cambian los límites de la cámara o el followFactor, hay que recalcular la referencia y la colocación de los pilares a mano. En el Editor se ven todos los pilares a la vez.
- Alternativa descartada: referencia en el inicio de la zona (media franja vacía).
- Cómo cambiarlo: medio.

### D7 · Pilares como SpriteRenderers sueltos, orden −100 y sin colisionador [registro D9, D5]
- Tipo y autor: arquitectura y diseño · decidió la IA (D5: transparencia y escala; el alfa pasó de 0,45 a 0,3 por el usuario, D12).
- Dónde: hijos de `BackgroundFar`; alfa en cada SpriteRenderer.
- Por qué: se pueden mover a mano; el resultado coincide con la maqueta que eligió el usuario.
- Ventajas: editable en el Editor · Desventajas: 38 objetos en la jerarquía.
- Alternativa descartada: Tilemap (no encaja en cuadrícula).
- Cómo cambiarlo: medio.

### D8 · Valores finales ajustados a ojo por el usuario [registro D20]
- Tipo y autor: diseño · decidió el usuario.
- Dónde: ZoneRed `bandBottom` −2 y `bandTop` 19; ZoneGreen `bandBottom` −30; `fadeDuration` 1; 3 pilares rojos desactivados.
- Por qué: es como más le gusta con su cámara (VirtualCamera, lookahead 0,5, damping 2).
- Ventajas: ajustado en el juego real · Desventajas: el solape deja que pilares rojos asomen un poco en zonas vecinas; las franjas ya no son las de las zonas (1 y 17).
- Alternativa descartada: las franjas exactas.
- Cómo cambiarlo: poco (Inspector).

- Decisiones menores del registro (D4, D11 y D12) quedaron absorbidas en D2, D3 y D7.

## 7. Conceptos: ¿qué conceptos de videojuegos aparecen?
- **Parallax** · diseño · `ZoneParallaxLayer · followFactor`: capas que se mueven a distinta velocidad que la cámara para dar profundidad. Aquí 0,2 significa que la capa sigue el 20 % del movimiento de la cámara, así que se ve a un 80 % de la velocidad del nivel.
- **Orden de actualización de la cámara** · motor · `CameraUpdatedEvent`: el movimiento visual debe ocurrir después de que la cámara se coloque, o tiembla un frame. Cinemachine avisa con un evento.
- **Histéresis** · diseño · `BackgroundZones · margin`: se necesita pasar la frontera con margen para cambiar de estado, así un salto no lo cambia y lo devuelve.
- **Perspectiva atmosférica** · diseño · alfa 0,3 de los pilares: lo lejano se ve más pálido y transparente (ya está en el glosario).
- **Orden de dibujado (sorting order)** · motor · SpriteRenderer −100: un número menor se dibuja detrás.
- **Fundido por pesos** · diseño · `_weights`: se mezcla cada color según su peso, y los pesos se mueven en el tiempo.

## 8. Riesgos: ¿qué problemas, dependencias o limitaciones tiene?
- El evento de Cinemachine es del paquete 2.10.7 (firma confirmada en CinemachineCore.cs:127 por el director): si se sube a Cinemachine 3, el código no compila · media · adaptar a la nueva API.
- Una `startHeight` de la primera zona no se usa en `ZoneWithMargin` y la referencia es fija: si cambian los límites de cámara o se añade una zona, hay que recalcular todo · media · recalcular a mano, o un generador.
- `ZoneParallaxLayer · Awake` captura con GetComponentsInChildren(true) y alfa/bounds una sola vez: si un sprite cambia de tamaño en Play, el cálculo queda mal · baja.
- `pillar.enabled = alpha > 0f` (línea 80) puede reactivar un pilar si lo apagaste desactivando solo el componente SpriteRenderer. Con el GameObject desactivado (como hiciste con los 3 rojos) no se dibuja · baja.

## 9. Deuda técnica: ¿qué atajos se tomaron a propósito?
- Referencias, franjas y posición de pilares fijas en el Inspector y la escena · cuando cambien los límites de cámara o el followFactor, o se añada una zona.
- Textos y valores por capa duplicados (followFactor y fadeDistance en cada una de las 3) · cuando haya una cuarta zona.
- Parallax solo vertical (el Confiner impide el movimiento horizontal) · si se añade scroll horizontal.

## 10. Rendimiento: ¿hay posibles problemas de rendimiento, memoria o asignaciones?
- Cada capa recorre sus pilares por cada evento de cámara (bucle de unos 12 a 20 pilares) · `ZoneParallaxLayer · OnCameraUpdated` · Profiler, CPU, buscar "OnCameraUpdated". Posible, no medido, y probablemente despreciable.
- Sin asignaciones por frame que haya encontrado leyendo (los arrays se crean en Awake) · mirar GC Alloc en el Profiler.
- El alfa de cada pilar va en el color de vértice del sprite, así que no rompe el batching · se puede mirar en el Frame Debugger.

## 11. Verificación: ¿qué se probó y qué conviene probar a mano?
- Hecho: compilación con el compilador de Unity contra Cinemachine.dll, 0 errores. YAML con ids únicos y 0 referencias rotas. Cuatro revisiones de estudio:qa-playtest, sin bloqueos. Renders y GIFs de verificación. 6 playtests tuyos. El estudio no ejecutó el juego en Unity.
- A mano: subir y bajar saltando justo en cada frontera: el color no debe parpadear.
- A mano: reintentar con el Player arriba (por ejemplo, en y 20): el primer frame debe salir ya con el color de su zona.
- A mano: parar en una frontera y mirar si algún pilar sigue visible cruzándola.

## 12. Mantenimiento: ¿qué hay que entender para mantenerlo y ampliarlo?
- Franjas y `referenceCameraY` · `ZoneParallaxLayer`: si cambian la cámara, las zonas o el followFactor, deben cambiar juntos.
- Margen y zonas · `BackgroundZones · ZoneWithMargin`: la banda muerta es el doble del margen (4 unidades) y no debe pasar de la mitad de la distancia entre fronteras.
- Prueba: sube `ZoneParallaxLayer · followFactor` a 0,6 en las 3 capas. Los pilares se sentirán casi pegados a la pantalla y se verán huecos en las franjas. Vuelve a 0,2.
- Pregunta: ¿por qué el color depende de la zona con margen y no de la altura, y qué problema del salto lo causaba?

## Correcciones
- ninguna
