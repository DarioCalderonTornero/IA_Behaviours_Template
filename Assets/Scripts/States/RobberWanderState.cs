using UnityEngine;
using UnityEngine.AI;

public class RobberWanderState : RobberState
{
    private Vector3 wanderOffset;
    private Vector3 lastSampledDestination;
    private float repathTimer;

    public RobberWanderState(RobberBrain brain, StateMachine stateMachine) : base(brain, stateMachine)
    {
    }

    public override void Enter()
    {
        if (brain.Agent == null)
            return;

        brain.Agent.isStopped = false;
        brain.Agent.autoBraking = false;

        repathTimer = 0f;

        wanderOffset = RandomDirectionXZ() * brain.WanderRadius;
        UpdateDestination(forceSetDestination: true);
    }

    public override void Tick()
    {
        if (brain.Agent == null)
            return;

        repathTimer -= Time.deltaTime;
        if (repathTimer > 0f)
            return;

        repathTimer = brain.WanderRepathInterval;

        if (brain.Agent.pathPending)
            return;

        bool reachedCurrentDestination =
            brain.Agent.hasPath &&
            brain.Agent.remainingDistance <= brain.Agent.stoppingDistance + 0.15f;

        bool stuckNearEdge =
            !brain.Agent.pathPending &&
            brain.Agent.velocity.sqrMagnitude < 0.01f &&
            (!brain.Agent.hasPath || brain.Agent.remainingDistance <= brain.Agent.stoppingDistance + 0.25f);

        UpdateDestination(forceSetDestination: reachedCurrentDestination || stuckNearEdge || !brain.Agent.hasPath);
    }

    public override void Exit()
    {
        brain.ClearWanderDebugData();
    }

    private void UpdateDestination(bool forceSetDestination)
    {
        Vector3 agentPosition = brain.transform.position;

        Vector3 bestCircleCenter = Vector3.zero;
        Vector3 bestRawTarget = Vector3.zero;
        Vector3 bestSampledTarget = Vector3.zero;
        bool foundValidTarget = false;

        float minDistanceFromSelfSqr = brain.WanderMinDistanceFromSelf * brain.WanderMinDistanceFromSelf;
        float minDestinationChangeSqr = brain.WanderDestinationMinChange * brain.WanderDestinationMinChange;

        for (int attempt = 0; attempt < brain.WanderCandidateAttempts; attempt++)
        {
            float jitterMultiplier = 1f + (attempt * 0.35f);

            Vector3 jitter = Random.insideUnitSphere * (brain.WanderJitter * jitterMultiplier);
            jitter.y = 0f;

            wanderOffset += jitter;

            if (attempt > 0)
            {
                // Si está atascado en el borde, forzamos más variedad direccional.
                wanderOffset += RandomDirectionXZ() * (brain.WanderRadius * 0.5f);
            }

            if (wanderOffset.sqrMagnitude < 0.0001f)
                wanderOffset = RandomDirectionXZ() * brain.WanderRadius;

            wanderOffset = wanderOffset.normalized * brain.WanderRadius;

            Vector3 forwardReference = GetForwardReference(attempt);
            Vector3 circleCenter = agentPosition + forwardReference * brain.WanderDistance;
            Vector3 rawTarget = circleCenter + wanderOffset;

            if (!NavMesh.SamplePosition(rawTarget, out NavMeshHit hit, brain.WanderNavMeshSampleDistance, NavMesh.AllAreas))
            {
                bestCircleCenter = circleCenter;
                bestRawTarget = rawTarget;
                bestSampledTarget = rawTarget;
                continue;
            }

            float distanceFromSelfSqr = (hit.position - agentPosition).sqrMagnitude;
            if (distanceFromSelfSqr < minDistanceFromSelfSqr)
            {
                bestCircleCenter = circleCenter;
                bestRawTarget = rawTarget;
                bestSampledTarget = hit.position;
                continue;
            }

            if (!HasCompletePath(hit.position))
            {
                bestCircleCenter = circleCenter;
                bestRawTarget = rawTarget;
                bestSampledTarget = hit.position;
                continue;
            }

            bestCircleCenter = circleCenter;
            bestRawTarget = rawTarget;
            bestSampledTarget = hit.position;
            foundValidTarget = true;
            break;
        }

        brain.SetWanderDebugData(bestCircleCenter, bestRawTarget, bestSampledTarget, foundValidTarget);

        if (!foundValidTarget)
        {
            // Rompemos la tendencia a mirar siempre hacia fuera del borde.
            wanderOffset = RandomDirectionXZ() * brain.WanderRadius;
            return;
        }

        bool targetMovedEnough =
            (bestSampledTarget - lastSampledDestination).sqrMagnitude >= minDestinationChangeSqr;

        if (forceSetDestination || targetMovedEnough)
        {
            if (brain.Agent.SetDestination(bestSampledTarget))
            {
                lastSampledDestination = bestSampledTarget;
            }
        }
    }

    private Vector3 GetForwardReference(int attempt)
    {
        if (brain.Agent.desiredVelocity.sqrMagnitude > 0.01f)
            return brain.Agent.desiredVelocity.normalized;

        if (brain.Agent.velocity.sqrMagnitude > 0.01f)
            return brain.Agent.velocity.normalized;

        if (attempt > 0)
            return RandomDirectionXZ();

        Vector3 forward = brain.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f)
            return Vector3.forward;

        return forward.normalized;
    }

    private bool HasCompletePath(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        bool pathFound = brain.Agent.CalculatePath(destination, path);

        return pathFound && path.status == NavMeshPathStatus.PathComplete;
    }

    private Vector3 RandomDirectionXZ()
    {
        Vector2 random2D = Random.insideUnitCircle.normalized;

        if (random2D.sqrMagnitude < 0.0001f)
            random2D = Vector2.right;

        return new Vector3(random2D.x, 0f, random2D.y);
    }
}