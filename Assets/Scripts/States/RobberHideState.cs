using UnityEngine;
using UnityEngine.AI;

public class RobberHideState : RobberState
{
    private Vector3 lastSampledDestination;
    private float repathTimer;

    public RobberHideState(RobberBrain brain, StateMachine stateMachine) : base(brain, stateMachine)
    {
    }

    public override void Enter()
    {
        if (brain.Agent == null)
            return;

        brain.RefreshHideObstacles();
        brain.ClearWanderDebugData();
        brain.ClearPursueDebugData();
        brain.ClearEvadeDebugData();

        brain.Agent.isStopped = false;
        brain.Agent.autoBraking = true;

        repathTimer = 0f;
        UpdateDestination(forceSetDestination: true);
    }

    public override void Tick()
    {
        if (brain.Agent == null || brain.Target == null)
            return;

        repathTimer -= Time.deltaTime;
        if (repathTimer > 0f)
            return;

        repathTimer = brain.HideRepathInterval;

        if (brain.Agent.pathPending)
            return;

        bool forceRepath =
            !brain.Agent.hasPath ||
            brain.Agent.remainingDistance <= brain.Agent.stoppingDistance + 0.25f ||
            brain.TargetVelocity.sqrMagnitude > 0.01f;

        UpdateDestination(forceSetDestination: forceRepath);
    }

    public override void Exit()
    {
        brain.ClearHideDebugData();
    }

    private void UpdateDestination(bool forceSetDestination)
    {
        if (brain.Target == null || brain.Agent == null)
            return;

        Collider[] obstacles = brain.HideObstacleColliders;
        if (obstacles == null || obstacles.Length == 0)
        {
            brain.RefreshHideObstacles();
            obstacles = brain.HideObstacleColliders;
        }

        Vector3 agentPosition = brain.transform.position;
        Vector3 threatPosition = brain.Target.position;
        Vector3 flatThreatPosition = new Vector3(threatPosition.x, agentPosition.y, threatPosition.z);
        Vector3 threatEyePosition = threatPosition + Vector3.up * brain.HideThreatEyeHeight;

        if (obstacles == null || obstacles.Length == 0)
        {
            brain.SetHideDebugData(flatThreatPosition, Vector3.zero, 0f, Vector3.zero, Vector3.zero, false);
            return;
        }

        bool foundValidDestination = false;
        Vector3 bestObstacleCenter = Vector3.zero;
        float bestObstacleRadius = 0f;
        Vector3 bestRawDestination = Vector3.zero;
        Vector3 bestSampledDestination = Vector3.zero;
        float bestScore = float.NegativeInfinity;

        float minSelfDistanceSqr = brain.HideMinDistanceFromSelf * brain.HideMinDistanceFromSelf;
        float minThreatDistanceSqr = brain.HideMinDistanceFromThreat * brain.HideMinDistanceFromThreat;

        foreach (Collider obstacle in obstacles)
        {
            if (obstacle == null)
                continue;

            Bounds bounds = obstacle.bounds;
            Vector3 obstacleCenter = bounds.center;
            obstacleCenter.y = agentPosition.y;

            Vector3 threatToObstacle = obstacleCenter - flatThreatPosition;
            threatToObstacle.y = 0f;

            if (threatToObstacle.sqrMagnitude < 0.0001f)
                continue;

            float obstacleRadius = Mathf.Max(bounds.extents.x, bounds.extents.z);
            Vector3 hideDirection = threatToObstacle.normalized;
            Vector3 rawDestination = obstacleCenter + hideDirection * (obstacleRadius + brain.HideObstaclePadding);

            if (!NavMesh.SamplePosition(rawDestination, out NavMeshHit hit, brain.HideNavMeshSampleDistance, NavMesh.AllAreas))
                continue;

            Vector3 sampledDestination = hit.position;

            if ((sampledDestination - agentPosition).sqrMagnitude < minSelfDistanceSqr)
                continue;

            if ((sampledDestination - flatThreatPosition).sqrMagnitude < minThreatDistanceSqr)
                continue;

            if (!HasCompletePath(hit.position))
                continue;

            if (!IsPointOccluded(threatEyePosition, hit.position))
                continue;

            float distanceFromAgent = Vector3.Distance(agentPosition, hit.position);
            float distanceFromThreat = Vector3.Distance(flatThreatPosition, hit.position);

            float score = distanceFromThreat - (distanceFromAgent * 0.65f) + (obstacleRadius * 0.25f);

            if (!foundValidDestination || score > bestScore)
            {
                foundValidDestination = true;
                bestScore = score;
                bestObstacleCenter = obstacleCenter;
                bestObstacleRadius = obstacleRadius;
                bestRawDestination = rawDestination;
                bestSampledDestination = hit.position;
            }
        }

        brain.SetHideDebugData(
            flatThreatPosition,
            bestObstacleCenter,
            bestObstacleRadius,
            bestRawDestination,
            bestSampledDestination,
            foundValidDestination);

        if (!foundValidDestination)
            return;

        float minChangeSqr = brain.HideDestinationMinChange * brain.HideDestinationMinChange;
        bool destinationChangedEnough =
            (bestSampledDestination - lastSampledDestination).sqrMagnitude >= minChangeSqr;

        if (forceSetDestination || destinationChangedEnough)
        {
            if (brain.Agent.SetDestination(bestSampledDestination))
            {
                lastSampledDestination = bestSampledDestination;
            }
        }
    }

    private bool IsPointOccluded(Vector3 threatEyePosition, Vector3 candidateGroundPosition)
    {
        Vector3 candidateVisionPoint = candidateGroundPosition + Vector3.up * brain.HideVisionProbeHeight;

        return Physics.Linecast(
            threatEyePosition,
            candidateVisionPoint,
            out _,
            brain.HideObstacleMask,
            QueryTriggerInteraction.Ignore);
    }

    private bool HasCompletePath(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        bool pathFound = brain.Agent.CalculatePath(destination, path);

        return pathFound && path.status == NavMeshPathStatus.PathComplete;
    }
}