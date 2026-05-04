# 🎨 Museum Heist — Unity 6 3D

## ¿Cómo se juega?

Eres un ladrón que debe robar todos los cuadros de un museo sin ser atrapado por el guardia de seguridad.

- Muévete por el museo con **WASD**
- Corre con **Shift**
- Al acercarte a un cuadro pulsa **X** para robarlo
- Roba todos los cuadros para **ganar**
- Si el guardia te atrapa, **pierdes**

---

## 🗂️ Estructura de scripts

```
Assets/
├── IGuardState.cs
├── GuardFSM.cs
├── GuardSenses.cs
├── PatrolState.cs
├── ListenState.cs
├── InvestigateState.cs
├── ChaseState.cs
├── CaughtState.cs
├── ThiefController.cs
├── PaintingInteract.cs
└── GameManager.cs
```

---

## 🧠 FSM del Guardia

```
                    ┌─────────────────────────────────┐
                    │                                 │
              [oye ruido]                    [pierde al ladrón]
                    │                                 │
[PATRULLAR] ──────►[ESCUCHAR]──[ve al ladrón]──►[PERSEGUIR]
    ▲                  │                              │
    │            [llega al punto]                     │
    │                  ▼                         [pierde vista]
    └────────────[INVESTIGAR]◄────────────────────────┘
                       │
                 [tiempo agotado]
                       │
                 vuelve a PATRULLAR
```

### Estados

| Estado | Comportamiento |
|---|---|
| **Patrol** | Recorre waypoints en bucle por el museo |
| **Listen** | Para, gira hacia el ruido, espera 2s y va a investigar |
| **Investigate** | Corre al punto sospechoso, mira alrededor 4s y vuelve a patrullar |
| **Chase** | Persigue al ladrón a máxima velocidad |
| **Caught** | Avisa al GameManager y termina la partida |

---

## 📋 Scripts — Descripción y Setup

### `IGuardState.cs`
Interfaz base de la FSM. Define `Enter`, `Update` y `Exit`.
> No va en ningún GameObject.

---

### `GuardFSM.cs`
Cerebro de la FSM. Gestiona el estado actual y las transiciones.
> Va en el **Guard**.

| Campo | Valor |
|---|---|
| `waypoints` | Array de GameObjects vacíos por el museo |
| `player` | GameObject del ladrón |
| `showDebugInfo` | `true` durante desarrollo |

---

### `GuardSenses.cs`
Gestiona la vista (raycast con cono) y el oído (overlap esférico) del guardia.
> Va en el **Guard** (se añade automáticamente por `RequireComponent`).

| Campo | Valor |
|---|---|
| `viewRange` | `10` |
| `viewAngle` | `90` |
| `obstacleMask` | Layer `Walls` |
| `playerMask` | Layer `Player` |
| `hearRange` | `4` |

---

### `PatrolState.cs`
El guardia recorre los waypoints en bucle. Transiciona a `ListenState` si oye al ladrón o a `ChaseState` si lo ve.
> No va en ningún GameObject.

---

### `ListenState.cs`
El guardia para y gira hacia el punto de ruido. Tras 2 segundos transiciona a `InvestigateState`.
> No va en ningún GameObject.

---

### `InvestigateState.cs`
El guardia corre al punto sospechoso. Al llegar mira alrededor durante 4 segundos y vuelve a `PatrolState`.
> No va en ningún GameObject.

---

### `ChaseState.cs`
El guardia persigue al ladrón a máxima velocidad. Si lo pierde de vista durante 2 segundos va a `InvestigateState`. Si lo alcanza (menos de 1m) transiciona a `CaughtState`.
> No va en ningún GameObject.

---

### `CaughtState.cs`
Para al guardia y llama a `GameManager.Instance.GameOver()`.
> No va en ningún GameObject.

---

### `ThiefController.cs`
Controla el movimiento del ladrón en top-down. WASD para moverse, Shift para correr. El personaje rota automáticamente hacia la dirección de movimiento.
> Va en el **ladrón**.

| Campo | Valor |
|---|---|
| `walkSpeed` | `4` |
| `runSpeed` | `7` |

Componentes necesarios en el ladrón:

| Componente | Configuración |
|---|---|
| `Rigidbody` | `Is Kinematic = false`, congelar rotaciones X y Z |
| `CapsuleCollider` | `Is Trigger = false` |
| Tag | `Player` |

---

### `PaintingInteract.cs`
Detecta cuando el ladrón entra en el trigger del cuadro. Al pulsar X llama a `GameManager.PaintingStolen()` y desactiva el cuadro.
> Va en **cada cuadro**.

| Componente | Configuración |
|---|---|
| `BoxCollider` | `Is Trigger = true` |

---

### `GameManager.cs`
Singleton que gestiona el contador de cuadros, la condición de victoria y la condición de derrota. Controla los paneles de UI y el `Time.timeScale`.
> Va en un **GameObject vacío** llamado `GameManager`.

| Campo | Valor |
|---|---|
| `totalPaintings` | Número de cuadros en escena |
| `gameOverPanel` | Panel UI de derrota |
| `victoryPanel` | Panel UI de victoria |
| `paintingCounterText` | TextMeshPro del HUD |

---

## 🗺️ Setup de escena

### Layers
Crear en **Edit → Project Settings → Tags and Layers**:

| Layer | Asignar a |
|---|---|
| `Walls` | Todos los muros del laberinto |
| `Player` | El ladrón |

### Waypoints
Crear GameObjects vacíos `WP_01`, `WP_02`... dentro del museo y arrastrarlos al array `waypoints` del `GuardFSM`.

### NavMesh
```
1. Seleccionar suelo y paredes → marcar Static en Inspector
2. Window → AI → Navigation → Bake
3. Verificar que el área azul cubre el suelo del museo
4. En NavMeshAgent del guardia: Is Kinematic = true en Rigidbody
5. Ajustar Base Offset si el guardia se hunde en el suelo
```

### UI (Canvas)
| Elemento | Contenido |
|---|---|
| `GameOverPanel` | Texto "HAS SIDO ATRAPADO" + botón `RestartScene()` + botón `QuitGame()` |
| `VictoryPanel` | Texto "¡HAS ESCAPADO!" + botón `RestartScene()` |
| `PaintingCounterText` | TextMeshPro visible en esquina, muestra `Cuadros: X / Y` |

Los botones en **OnClick()** apuntan al GameObject `GameManager`:
- Reintentar → `GameManager.RestartScene`
- Salir → `GameManager.QuitGame`
