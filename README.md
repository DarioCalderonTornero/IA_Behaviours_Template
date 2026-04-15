# 🕵️ Robber AI – Unity (State Machine + NavMesh)

## 📌 Descripción general
Este proyecto implementa una **IA de ladrón** en Unity utilizando una **máquina de estados finita (FSM)** combinada con **NavMesh** para navegación.

El objetivo es simular un comportamiento creíble donde el ladrón:
- Deambula por el escenario
- Persigue al jugador cuando le conviene
- Huye cuando está en peligro
- Se esconde usando obstáculos reales
- Toma decisiones inteligentes combinando todos los comportamientos

---

## 🧠 Arquitectura

La IA está basada en una **máquina de estados modular**, donde cada comportamiento está separado en su propio script.

### Componentes principales

- `RobberBrain` → controlador central de la IA
- `StateMachine` → gestiona los estados
- `RobberState` → clase base de todos los estados
- Estados concretos:
  - `RobberWanderState`
  - `RobberPursueState`
  - `RobberEvadeState`
  - `RobberHideState`
  - `RobberComplexState`

### Ventajas de esta arquitectura
- Código modular y reutilizable
- Fácil de extender
- Separación clara de responsabilidades
- Permite depuración visual por estado

---

## 🤖 Estados implementados

### 1. 🟢 Wander (Deambular)
- Movimiento aleatorio usando NavMesh
- Generación de puntos con jitter
- Evita quedarse bloqueado en bordes

👉 Tecla: `R`

---

### 2. 🔴 Pursue (Perseguir)
- Persigue al objetivo
- Usa **predicción de movimiento** basada en la velocidad del target
- Recalcula destino periódicamente

👉 Tecla: `P`

---

### 3. 🟠 Evade (Huir)
- Se aleja del target
- También usa **predicción**
- Prueba múltiples direcciones para evitar quedarse atrapado

👉 Tecla: `E`

---

### 4. 🔵 Hide (Esconderse)
- Busca obstáculos en la escena
- Calcula una posición detrás del obstáculo respecto al enemigo
- Verifica ocultación con raycast

👉 Tecla: `H`

---

### 5. 🧩 Complex (Comportamiento inteligente)
Combina todos los estados anteriores:

- **Lejos del target** → Wander  
- **Cerca y no lo ve** → Pursue  
- **El target lo ve** → Hide / Evade  
- Usa memoria temporal para evitar cambios bruscos

👉 Tecla: `C`

---

## 👁️ Sistema de percepción

Se implementan funciones clave:

- `CanSeeTarget()`
- `CanTargetSeeMe()`
- `IsTargetInRange()`

### Características:
- Campo de visión (ángulo + distancia)
- Raycast para comprobar visibilidad real
- Altura de ojos configurable

---

## 🧭 Navegación

Se utiliza:
- `NavMesh`
- `NavMeshAgent`

### Características:
- Pathfinding automático
- Repath inteligente
- Validación de rutas completas

---

## 🧪 Debug visual

Se ha implementado un sistema de debug (`RobberDebugView`) que permite visualizar:

- Estados activos
- Destinos del agente
- Predicciones
- Líneas de visión
- Obstáculos usados para esconderse
- Path del NavMesh

Esto facilita:
- Ajuste de parámetros
- Detección de errores
- Explicación del comportamiento

---

## 🧱 Configuración de escena

Para que la IA funcione correctamente:

### Ladrón
- `NavMeshAgent`
- `RobberBrain`
- `RobberDebugView`

### Policía (target)
- Transform válido
- Forward correctamente orientado

### Obstáculos
- Collider (NO trigger)
- Tag: `HideObstacle`
- Layer incluida en:
  - `HideObstacleMask`
  - `VisionObstacleMask`

### NavMesh
- Correctamente horneado
- Sin huecos inesperados

---

## ⚙️ Controles

| Tecla | Estado |
|------|--------|
| R | Wander |
| P | Pursue |
| E | Evade |
| H | Hide |
| C | Complex |

---

## 🎯 Objetivos cumplidos

✔ Máquina de estados modular  
✔ Navegación con NavMesh  
✔ Persecución con predicción  
✔ Huida inteligente  
✔ Sistema de escondite con obstáculos reales  
✔ Sistema de percepción (visión)  
✔ Estado complejo con toma de decisiones  
✔ Debug visual completo  

---

## 🚀 Posibles mejoras

- Animaciones (Animator)
- Sistema de sonido/percepción auditiva
- Behavior Trees en lugar de FSM
- IA multi-agente
- Sistema de memoria más avanzado

---

## 📌 Conclusión

El proyecto demuestra cómo implementar una IA completa en Unity combinando:

- Máquina de estados
- Navegación
- Percepción
- Toma de decisiones

El resultado es un agente que responde de forma creíble al entorno y al jugador.

---

