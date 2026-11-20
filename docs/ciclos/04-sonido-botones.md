# Ciclo 04: Sonido en los botones
<!-- Límite: 20 líneas. Archivo: docs/ciclos/04-sonido-botones.md -->

## Objetivo
- Que moverse y pulsar en el menú y en la pantalla final se oiga: la navegación se siente viva y cada acción se confirma.

## Qué se construye
- Dos sonidos del juego «Gota de slime» (Free UI Click Sound Effects Pack, del Asset Store): un «plip» al cambiar de botón y un «plop» al pulsar. Son dos AudioCue que van por el grupo UI del mixer.
- MenuSounds en el Canvas del menú y de la pantalla final: vigila la selección y los clics y pide los sonidos por SFXCueChannel.
- No suena al abrir la pantalla, al recuperar la selección perdida ni cuando el foco salta tras pulsar (abrir o cerrar Controles). La regla tiene su prueba EditMode.

## Cómo sabremos que funciona
- Con teclado y ratón, cada cambio de botón suena una vez y cada pulsación una vez, sin dobles ni sonido al entrar en la pantalla.
- El «plop» de Jugar, Reintentar y Menú se oye entero aunque cambie la escena.
- Las pruebas EditMode pasan.

## Resultado
- Playtest: los sonidos de los botones bien en todo (teclado, ratón, Controles, victoria y derrota). Los disparos se oían muy fuertes: medidos y probados a −33, −30 y −27 dB, quedaron a −27.
- Crece: sonido en los botones · Se ajusta: volumen de disparos e impactos (sobre todo enemigos e impactos) · Se poda: 98 sonidos sin usar del paquete (de 12 MB a 144 KB).
