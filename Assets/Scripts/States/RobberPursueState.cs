using UnityEngine;
using UnityEngine.AI;

public class RobberPursueState : RobberState
{
    private Vector3 lastSampledDestination;
    private float repathTimer;

    public RobberPursueState(RobberBrain brain, StateMachine stateMachine) : base(brain, stateMachine)
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

        repathTimer = brain.PursueRepathInterval;

        if (brain.Agent.pathPending)
            return;

        bool needsNewDestination =
            !brain.Agent.hasPath ||
            brain.Agent.remainingDistance <= brain.Agent.stoppingDistance + 0.2f ||
            brain.TargetVelocity.sqrMagnitude > 0.01f;

        UpdateDestination(forceSetDestination: needsNewDestination);
    }

    public override void Exit()
    {
        brain.ClearPursueDebugData();
    }

    private void UpdateDestination(bool forceSetDestination)
    {
        if (brain.Target == null || brain.Agent == null)
            return;

        Vector3 targetPosition = brain.Target.position;
        targetPosition.y = brain.transform.position.y;

        Vector3 targetVelocity = brain.TargetVelocity;
        targetVelocity.y = 0f;

        float targetSpeed = targetVelocity.magnitude;
        float distanceToTarget = Vector3.Distance(brain.transform.position, targetPosition);

        float predictionTime = 0f;
        if (targetSpeed >= brain.PursueMinTargetSpeedForPrediction)
        {
            float denominator = brain.Agent.speed + targetSpeed;
            if (denominator > 0.001f)
            {
                predictionTime = distanceToTarget / denominator;
                predictionTime *= brain.PursuePredictionMultiplier;
                predictionTime = Mathf.Clamp(predictionTime, 0f, brain.PursueMaxPredictionTime);
            }
        }

        Vector3 predictedPosition = targetPosition + targetVelocity * predictionTime;

        if (NavMesh.SamplePosition(predictedPosition, out NavMeshHit hit, brain.PursueNavMeshSampleDistance, NavMesh.AllAreas))
        {
            brain.SetPursueDebugData(targetPosition, predictedPosition, hit.position, predictionTime, true);

            float minChangeSqr = brain.PursueDestinationMinChange * brain.PursueDestinationMinChange;
            bool destinationChangedEnough = (hit.position - lastSampledDestination).sqrMagnitude >= minChangeSqr;

            if (forceSetDestination || destinationChangedEnough)
            {
                if (brain.Agent.SetDestination(hit.position))
                {
                    lastSampledDestination = hit.position;
                }
            }
        }
        else
        {
            brain.SetPursueDebugData(targetPosition, predictedPosition, predictedPosition, predictionTime, false);
        }
    }
}