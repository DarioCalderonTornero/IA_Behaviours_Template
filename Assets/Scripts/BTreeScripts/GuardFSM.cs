using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(GuardSenses))]
public class GuardFSM : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Referencias")]
    public Transform player;

    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public GuardSenses senses;

    private IGuardState currentState;

    [HideInInspector] public Vector3 investigationPoint;

    [Header("Debug")]
    public bool showDebugInfo = true;
    public string currentStateName = "None";   

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        senses = GetComponent<GuardSenses>();
    }

    void Start()
    {
        ChangeState(new PatrolState());
    }

    void Update()
    {
        currentState?.Update(this);
    }

    public void ChangeState(IGuardState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentStateName = currentState.GetType().Name;  
        currentState.Enter(this);
    }


    public void MoveTo(Vector3 destination)
    {
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public void StopMoving()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }

    public bool HasReachedDestination(float tolerance = 0.5f)
    {
        if (agent.pathPending) return false;
        return agent.remainingDistance <= tolerance;
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;

        // Línea al jugador
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }

        // Punto de investigación
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(investigationPoint, 0.3f);

        // Waypoints
        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);
            Gizmos.DrawLine(
                waypoints[i].position,
                waypoints[(i + 1) % waypoints.Length].position
            );
        }
    }
}