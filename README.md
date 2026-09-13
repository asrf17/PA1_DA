# Berie · Rumbo al faro

Continuación del proyecto PA1_DA para PA2 y Evaluación Parcial.

## Jugar

Abre `Assets/Scenes/PA2_Costa.unity` y pulsa Play en Unity 6000.6.0f1.
Presiona **Comenzar** o Enter. Reúne al menos 10 de las 24 monedas y llega al faro.

- A/D o flechas: movimiento.
- Espacio, W o flecha arriba: salto. Soltar antes reduce la altura.
- Escape: pausar o continuar.
- R: reiniciar la partida.
- Tres vidas. Las púas y las caídas consumen una vida; los cristales guardan el punto de reaparición.

`SampleScene.unity` conserva el prototipo PA1 con sus scripts en `Assets/PA1Legacy`.

## Organización

- `Assets/Scripts`: movimiento, vida, animación, feedback, audio, HUD y objetos interactivos.
- `Assets/PA2`: tiles derivados de los sprites existentes, animaciones, materiales y audio.
- `Assets/Editor/PA2LevelBuilder.cs`: construcción reproducible del escenario desde el menú PA2.
- `Assets/Editor/PA2Validation.cs`: comprobaciones de integración y recorrido usando la física de Unity en Play.
- `GameManager` y `AudioManager`: Singleton. `GameHUD` escucha eventos de partida y vida (Observer).
- Cámara: Cinemachine 3.1.5, Position Composer con zona muerta y Confiner 2D.
- Animator: Idle, Run, Jump y Fall; parámetros booleanos y transiciones inmediatas.

## Créditos

Arte de **Crusenho**, Berie's Adventure Seaside Asset Pack, ya incluido en el proyecto.
Se conserva su licencia en la carpeta de Sprites. Los tiles de PA2 son recortes de ese recurso.
Música y efectos de PA2: síntesis original creada para este proyecto.

El proyecto excluye Library, Temp, Logs y carpetas de compilación mediante .gitignore.
