using UnityEngine;

[RequireComponent(typeof(RobberBrain))]
public class RobberDebugView : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private bool drawAgentPath = true;
    [SerializeField] private float pointSize = 0.20f;

    [Header("Wander Debug")]
    [SerializeField] private bool drawWanderGizmos = true;

    [Header("Pursue Debug")]
    [SerializeField] private bool drawPursueGizmos = true;
    [SerializeField] private bool drawTargetVelocity = true;

    [Header("Evade Debug")]
    [SerializeField] private bool drawEvadeGizmos = true;

    [Header("Hide Debug")]
    [SerializeField] private bool drawHideGizmos = true;

    [Header("Complex Debug")]
    [SerializeField] private bool drawComplexGizmos = true;
    [SerializeField] private bool drawComplexRanges = true;

    private RobberBrain brain;

    private void OnValidate()
    {
        if (brain == null)
            brain = GetComponent<RobberBrain>();
    }

    private void Awake()
    {
        if (brain == null)
            brain = GetComponent<RobberBrain>();
    }

    private void OnDrawGizmosSelected()
    {
        if (brain == null)
            brain = GetComponent<RobberBrain>();

        if (brain == null)
            return;

        if (drawComplexGizmos)
            DrawComplexDebug();

        if (drawWanderGizmos)
            DrawWanderDebug();

        if (drawPursueGizmos)
            DrawPursueDebug();

        if (drawEvadeGizmos)
            DrawEvadeDebug();

        if (drawHideGizmos)
            DrawHideDebug();

        if (drawAgentPath)
            DrawAgentPath();
    }

    private bool ShouldDrawState(string stateName)
    {
        if (brain == null)
            return false;

        if (brain.CurrentStateName == stateName)
            return true;

        return brain.CurrentStateName == nameof(RobberComplexState) &&
               brain.DebugComplexSubStateName == stateName;
    }

    private void DrawComplexDebug()
    {
        if (brain.CurrentStateName != nameof(RobberComplexState))
            return;

        Vector3 robberEyes = brain.GetRobberEyePosition();
        Vector3 targetEyes = brain.GetTargetEyePosition();

        Gizmos.color = brain.DebugComplexCanSeeTarget ? Color.green : Color.red;
        Gizmos.DrawLine(robberEyes, targetEyes);
        Gizmos.DrawSphere(robberEyes, pointSize * 0.65f);

        Gizmos.color = brain.DebugComplexCanTargetSeeMe ? Color.green : Color.red;
        Gizmos.DrawLine(targetEyes, robberEyes);
        Gizmos.DrawSphere(targetEyes, pointSize * 0.65f);

        if (drawComplexRanges)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.7f);
            Gizmos.DrawWireSphere(transform.position, brain.ComplexEnterRange);

            Gizmos.color = new Color(1f, 0.6f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, brain.ComplexExitRange);
        }
    }

    private void DrawWanderDebug()
    {
        if (!ShouldDrawState(nameof(RobberWanderState)))
            return;

        Vector3 origin = transform.position;

        if (brain.DebugWanderCircleCenter != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, brain.DebugWanderCircleCenter);
            Gizmos.DrawWireSphere(brain.DebugWanderCircleCenter, brain.WanderRadius);
        }

        if (brain.DebugWanderRawTarget != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(brain.DebugWanderRawTarget, pointSize * 0.75f);
            Gizmos.DrawLine(brain.DebugWanderCircleCenter, brain.DebugWanderRawTarget);
        }

        if (brain.DebugWanderSampledTarget != Vector3.zero)
        {
            Gizmos.color = brain.DebugWanderHasValidTarget ? Color.green : Color.red;
            Gizmos.DrawSphere(brain.DebugWanderSampledTarget, pointSize);
            Gizmos.DrawLine(origin, brain.DebugWanderSampledTarget);
        }
    }

    private void DrawPursueDebug()
    {
        if (!ShouldDrawState(nameof(RobberPursueState)))
            return;

        Vector3 origin = transform.position;

        if (brain.Target != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(origin, brain.DebugPursueTargetPosition);
            Gizmos.DrawSphere(brain.DebugPursueTargetPosition, pointSize * 0.8f);
        }

        if (drawTargetVelocity && brain.Target != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(brain.DebugPursueTargetPosition, brain.DebugPursueTargetPosition + brain.TargetVelocity);
        }

        if (brain.DebugPursuePredictedPosition != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(brain.DebugPursuePredictedPosition, pointSize * 0.9f);
            Gizmos.DrawLine(brain.DebugPursueTargetPosition, brain.DebugPursuePredictedPosition);
        }

        if (brain.DebugPursueSampledPosition != Vector3.zero)
        {
            Gizmos.color = brain.DebugPursueHasValidTarget ? Color.green : Color.red;
            Gizmos.DrawSphere(brain.DebugPursueSampledPosition, pointSize);
            Gizmos.DrawLine(origin, brain.DebugPursueSampledPosition);
        }
    }

    private void DrawEvadeDebug()
    {
        if (!ShouldDrawState(nameof(RobberEvadeState)))
            return;

        Vector3 origin = transform.position;

        if (brain.Target != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(origin, brain.DebugEvadeThreatPosition);
            Gizmos.DrawSphere(brain.DebugEvadeThreatPosition, pointSize * 0.8f);
        }

        if (drawTargetVelocity && brain.Target != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(brain.DebugEvadeThreatPosition, brain.DebugEvadeThreatPosition + brain.TargetVelocity);
        }

        if (brain.DebugEvadePredictedThreatPosition != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(brain.DebugEvadePredictedThreatPosition, pointSize * 0.9f);
            Gizmos.DrawLine(brain.DebugEvadeThreatPosition, brain.DebugEvadePredictedThreatPosition);
        }

        if (brain.DebugEvadeRawDestination != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(brain.DebugEvadeRawDestination, pointSize * 0.75f);
            Gizmos.DrawLine(brain.DebugEvadePredictedThreatPosition, brain.DebugEvadeRawDestination);
        }

        if (brain.DebugEvadeSampledDestination != Vector3.zero)
        {
            Gizmos.color = brain.DebugEvadeHasValidDestination ? Color.green : Color.red;
            Gizmos.DrawSphere(brain.DebugEvadeSampledDestination, pointSize);
            Gizmos.DrawLine(origin, brain.DebugEvadeSampledDestination);
        }
    }

    private void DrawHideDebug()
    {
        if (!ShouldDrawState(nameof(RobberHideState)))
            return;

        Vector3 origin = transform.position;

        if (brain.Target != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(origin, brain.DebugHideThreatPosition);
            Gizmos.DrawSphere(brain.DebugHideThreatPosition, pointSize * 0.8f);
        }

        if (brain.DebugHideObstacleCenter != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(brain.DebugHideObstacleCenter, pointSize * 0.9f);
            Gizmos.DrawWireSphere(brain.DebugHideObstacleCenter, brain.DebugHideObstacleRadius);
        }

        if (brain.DebugHideRawDestination != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(brain.DebugHideRawDestination, pointSize * 0.75f);
            Gizmos.DrawLine(brain.DebugHideObstacleCenter, brain.DebugHideRawDestination);
        }

        if (brain.DebugHideSampledDestination != Vector3.zero)
        {
            Gizmos.color = brain.DebugHideHasValidDestination ? Color.green : Color.red;
            Gizmos.DrawSphere(brain.DebugHideSampledDestination, pointSize);
            Gizmos.DrawLine(origin, brain.DebugHideSampledDestination);
            Gizmos.DrawLine(brain.DebugHideThreatPosition, brain.DebugHideSampledDestination);
        }
    }

    private void DrawAgentPath()
    {
        if (brain.Agent == null || !brain.Agent.hasPath)
            return;

        Vector3[] corners = brain.Agent.path.corners;
        if (corners == null || corners.Length < 2)
            return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Gizmos.DrawLine(corners[i], corners[i + 1]);
        }
    }
}