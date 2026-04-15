using UnityEngine;
using UnityEngine.AI;

public class RobberEvadeState : RobberState
{
    private Vector3 lastSampledDestination;
    private float repathTimer;

    public RobberEvadeState(RobberBrain brain, StateMachine stateMachine) : base(brain, stateMachine)
    {
    }

    public override void Enter()
    {
        if (brain.Agent == null)
            return;

        brain.Agent.isStopped = false;
        brain.Agent.autoBraking = false;

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

        repathTimer = brain.EvadeRepathInterval;

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
        brain.ClearEvadeDebugData();
    }

    private void UpdateDestination(bool forceSetDestination)
    {
        if (brain.Target == null || brain.Agent == null)
            return;

        Vector3 agentPosition = brain.transform.position;
        agentPosition.y = 0f;

        Vector3 threatPosition = brain.Target.position;
        threatPosition.y = 0f;

        Vector3 targetVelocity = brain.TargetVelocity;
        targetVelocity.y = 0f;

        float targetSpeed = targetVelocity.magnitude;
        float distanceToThreat = Vector3.Distance(agentPosition, threatPosition);

        float predictionTime = 0f;
        if (targetSpeed >= brain.EvadeMinTargetSpeedForPrediction)
        {
            float denominator = brain.Agent.speed + targetSpeed;
            if (denominator > 0.001f)
            {
                predictionTime = distanceToThreat / denominator;
                predictionTime *= brain.EvadePredictionMultiplier;
                predictionTime = Mathf.Clamp(predictionTime, 0f, brain.EvadeMaxPredictionTime);
            }
        }

        Vector3 predictedThreatPosition = threatPosition + targetVelocity * predictionTime;
        predictedThreatPosition.y = 0f;

        Vector3 baseFleeDirection = agentPosition - predictedThreatPosition;
        baseFleeDirection.y = 0f;

        if (baseFleeDirection.sqrMagnitude < 0.0001f)
            baseFleeDirection = GetFallbackDirection();

        baseFleeDirection.Normalize();

        bool foundValidDestination = false;
        Vector3 bestRawDestination = Vector3.zero;
        Vector3 bestSampledDestination = Vector3.zero;
        float bestScore = float.NegativeInfinity;

        float minSelfDistanceSqr = brain.EvadeMinDistanceFromSelf * brain.EvadeMinDistanceFromSelf;
        float minThreatDistanceSqr = brain.EvadeMinDistanceFromThreat * brain.EvadeMinDistanceFromThreat;

        for (int attempt = 0; attempt < brain.EvadeCandidateAttempts; attempt++)
        {
            float angleOffset = GetAngleOffsetForAttempt(attempt);
            Vector3 candidateDirection = Quaternion.Euler(0f, angleOffset, 0f) * baseFleeDirection;

            Vector3 rawDestination = agentPosition + candidateDirection * brain.EvadeDistance;

            if (!NavMesh.SamplePosition(rawDestination, out NavMeshHit hit, brain.EvadeNavMeshSampleDistance, NavMesh.AllAreas))
                continue;

            Vector3 sampled = hit.position;
            sampled.y = 0f;

            if ((sampled - agentPosition).sqrMagnitude < minSelfDistanceSqr)
                continue;

            if ((sampled - predictedThreatPosition).sqrMagnitude < minThreatDistanceSqr)
                continue;

            if (!HasCompletePath(hit.position))
                continue;

            float score = Vector3.Distance(sampled, predictedThreatPosition);

            if (!foundValidDestination || score > bestScore)
            {
                foundValidDestination = true;
                bestScore = score;
                bestRawDestination = rawDestination;
                bestSampledDestination = hit.position;
            }
        }

        if (foundValidDestination)
        {
            brain.SetEvadeDebugData(
                threatPosition,
                predictedThreatPosition,
                bestRawDestination,
                bestSampledDestination,
                predictionTime,
                true);

            float minChangeSqr = brain.EvadeDestinationMinChange * brain.EvadeDestinationMinChange;
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
        else
        {
            Vector3 fallbackRaw = agentPosition + baseFleeDirection * brain.EvadeDistance;

            brain.SetEvadeDebugData(
                threatPosition,
                predictedThreatPosition,
                fallbackRaw,
                fallbackRaw,
                predictionTime,
                false);
        }
    }

    private float GetAngleOffsetForAttempt(int attempt)
    {
        if (attempt == 0)
            return 0f;

        int ring = (attempt + 1) / 2;
        float sign = (attempt % 2 == 1) ? 1f : -1f;

        return ring * brain.EvadeAngleStep * sign;
    }

    private bool HasCompletePath(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        bool pathFound = brain.Agent.CalculatePath(destination, path);

        return pathFound && path.status == NavMeshPathStatus.PathComplete;
    }

    private Vector3 GetFallbackDirection()
    {
        Vector3 fallback = brain.transform.forward;
        fallback.y = 0f;

        if (fallback.sqrMagnitude < 0.0001f)
            fallback = RandomDirectionXZ();

        return fallback.normalized;
    }

    private Vector3 RandomDirectionXZ()
    {
        Vector2 random2D = Random.insideUnitCircle.normalized;

        if (random2D.sqrMagnitude < 0.0001f)
            random2D = Vector2.right;

        return new Vector3(random2D.x, 0f, random2D.y);
    }
}