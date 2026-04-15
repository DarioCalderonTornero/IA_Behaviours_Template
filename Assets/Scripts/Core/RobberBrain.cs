using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum RobberStateType
{
    Wander,
    Pursue,
    Evade,
    Hide,
    Complex
}

public class RobberBrain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform eyesPoint;
    [SerializeField] private NavMeshAgent agent;

    [Header("Testing")]
    [SerializeField] private RobberStateType initialState = RobberStateType.Wander;
    [SerializeField] private bool enableKeyboardStateTesting = true;

    [Header("Target Tracking")]
    [SerializeField, Min(0f)] private float trackedTargetVelocitySmoothing = 12f;

    [Header("Perception Settings")]
    [SerializeField] private LayerMask visionObstacleMask;
    [SerializeField, Min(0.1f)] private float robberViewDistance = 12f;
    [SerializeField, Range(1f, 360f)] private float robberViewAngle = 120f;
    [SerializeField, Min(0f)] private float robberEyeHeightFallback = 1.2f;
    [SerializeField, Min(0.1f)] private float targetViewDistance = 12f;
    [SerializeField, Range(1f, 360f)] private float targetViewAngle = 90f;
    [SerializeField, Min(0f)] private float targetEyeHeight = 1.2f;

    [Header("Wander Settings")]
    [SerializeField, Min(0.1f)] private float wanderDistance = 4f;
    [SerializeField, Min(0.1f)] private float wanderRadius = 2f;
    [SerializeField, Min(0f)] private float wanderJitter = 0.75f;
    [SerializeField, Min(0.02f)] private float wanderRepathInterval = 0.20f;
    [SerializeField, Min(0.1f)] private float wanderNavMeshSampleDistance = 2f;
    [SerializeField, Min(0f)] private float wanderDestinationMinChange = 0.75f;
    [SerializeField, Min(0.2f)] private float wanderMinDistanceFromSelf = 1.25f;
    [SerializeField, Min(1)] private int wanderCandidateAttempts = 8;

    [Header("Pursue Settings")]
    [SerializeField, Min(0.02f)] private float pursueRepathInterval = 0.10f;
    [SerializeField, Min(0.1f)] private float pursueNavMeshSampleDistance = 2f;
    [SerializeField, Min(0f)] private float pursueDestinationMinChange = 0.5f;
    [SerializeField, Min(0f)] private float pursuePredictionMultiplier = 1f;
    [SerializeField, Min(0f)] private float pursueMaxPredictionTime = 1.5f;
    [SerializeField, Min(0f)] private float pursueMinTargetSpeedForPrediction = 0.1f;

    [Header("Evade Settings")]
    [SerializeField, Min(0.02f)] private float evadeRepathInterval = 0.10f;
    [SerializeField, Min(0.5f)] private float evadeDistance = 6f;
    [SerializeField, Min(0.1f)] private float evadeNavMeshSampleDistance = 2.5f;
    [SerializeField, Min(0f)] private float evadeDestinationMinChange = 0.5f;
    [SerializeField, Min(0f)] private float evadePredictionMultiplier = 1f;
    [SerializeField, Min(0f)] private float evadeMaxPredictionTime = 1.5f;
    [SerializeField, Min(0f)] private float evadeMinTargetSpeedForPrediction = 0.1f;
    [SerializeField, Min(1)] private int evadeCandidateAttempts = 7;
    [SerializeField, Range(5f, 90f)] private float evadeAngleStep = 25f;
    [SerializeField, Min(0.2f)] private float evadeMinDistanceFromSelf = 1.25f;
    [SerializeField, Min(0.5f)] private float evadeMinDistanceFromThreat = 3.5f;

    [Header("Hide Settings")]
    [SerializeField] private bool autoFindHideObstaclesFromTag = true;
    [SerializeField] private string hideObstacleTag = "HideObstacle";
    [SerializeField] private LayerMask hideObstacleMask;
    [SerializeField] private Collider[] hideObstacleColliders;
    [SerializeField, Min(0.02f)] private float hideRepathInterval = 0.15f;
    [SerializeField, Min(0.1f)] private float hideNavMeshSampleDistance = 2.5f;
    [SerializeField, Min(0f)] private float hideDestinationMinChange = 0.5f;
    [SerializeField, Min(0.1f)] private float hideObstaclePadding = 1.2f;
    [SerializeField, Min(0.2f)] private float hideMinDistanceFromSelf = 1.0f;
    [SerializeField, Min(0.2f)] private float hideMinDistanceFromThreat = 2.5f;
    [SerializeField, Min(0f)] private float hideThreatEyeHeight = 1.2f;
    [SerializeField, Min(0f)] private float hideVisionProbeHeight = 1.0f;

    [Header("Complex Settings")]
    [SerializeField, Min(0.1f)] private float complexEnterRange = 10f;
    [SerializeField, Min(0.1f)] private float complexExitRange = 12f;
    [SerializeField, Min(0f)] private float complexPanicMemoryTime = 2f;
    [SerializeField, Min(0f)] private float complexMinSubStateDuration = 0.5f;
    [SerializeField] private bool complexPreferHideOverEvade = true;

    [Header("Runtime Debug")]
    [SerializeField] private string currentStateName;
    [SerializeField] private Vector3 trackedTargetVelocity;

    [Header("Wander Debug")]
    [SerializeField] private Vector3 debugWanderCircleCenter;
    [SerializeField] private Vector3 debugWanderRawTarget;
    [SerializeField] private Vector3 debugWanderSampledTarget;
    [SerializeField] private bool debugWanderHasValidTarget;

    [Header("Pursue Debug")]
    [SerializeField] private Vector3 debugPursueTargetPosition;
    [SerializeField] private Vector3 debugPursuePredictedPosition;
    [SerializeField] private Vector3 debugPursueSampledPosition;
    [SerializeField] private float debugPursuePredictionTime;
    [SerializeField] private bool debugPursueHasValidTarget;

    [Header("Evade Debug")]
    [SerializeField] private Vector3 debugEvadeThreatPosition;
    [SerializeField] private Vector3 debugEvadePredictedThreatPosition;
    [SerializeField] private Vector3 debugEvadeRawDestination;
    [SerializeField] private Vector3 debugEvadeSampledDestination;
    [SerializeField] private float debugEvadePredictionTime;
    [SerializeField] private bool debugEvadeHasValidDestination;

    [Header("Hide Debug")]
    [SerializeField] private Vector3 debugHideThreatPosition;
    [SerializeField] private Vector3 debugHideObstacleCenter;
    [SerializeField] private float debugHideObstacleRadius;
    [SerializeField] private Vector3 debugHideRawDestination;
    [SerializeField] private Vector3 debugHideSampledDestination;
    [SerializeField] private bool debugHideHasValidDestination;

    [Header("Complex Debug")]
    [SerializeField] private bool debugComplexCanSeeTarget;
    [SerializeField] private bool debugComplexCanTargetSeeMe;
    [SerializeField] private bool debugComplexIsTargetInRange;
    [SerializeField] private string debugComplexSubStateName;
    [SerializeField] private float debugComplexDefensiveLockTimer;

    private StateMachine stateMachine;
    private Vector3 lastTargetPosition;

    public Transform Target => target;
    public Transform EyesPoint => eyesPoint;
    public NavMeshAgent Agent => agent;
    public StateMachine StateMachine => stateMachine;

    public LayerMask VisionObstacleMask => visionObstacleMask;
    public float RobberViewDistance => robberViewDistance;
    public float RobberViewAngle => robberViewAngle;
    public float RobberEyeHeightFallback => robberEyeHeightFallback;
    public float TargetViewDistance => targetViewDistance;
    public float TargetViewAngle => targetViewAngle;
    public float TargetEyeHeight => targetEyeHeight;

    public float WanderDistance => wanderDistance;
    public float WanderRadius => wanderRadius;
    public float WanderJitter => wanderJitter;
    public float WanderRepathInterval => wanderRepathInterval;
    public float WanderNavMeshSampleDistance => wanderNavMeshSampleDistance;
    public float WanderDestinationMinChange => wanderDestinationMinChange;
    public float WanderMinDistanceFromSelf => wanderMinDistanceFromSelf;
    public int WanderCandidateAttempts => wanderCandidateAttempts;

    public float PursueRepathInterval => pursueRepathInterval;
    public float PursueNavMeshSampleDistance => pursueNavMeshSampleDistance;
    public float PursueDestinationMinChange => pursueDestinationMinChange;
    public float PursuePredictionMultiplier => pursuePredictionMultiplier;
    public float PursueMaxPredictionTime => pursueMaxPredictionTime;
    public float PursueMinTargetSpeedForPrediction => pursueMinTargetSpeedForPrediction;

    public float EvadeRepathInterval => evadeRepathInterval;
    public float EvadeDistance => evadeDistance;
    public float EvadeNavMeshSampleDistance => evadeNavMeshSampleDistance;
    public float EvadeDestinationMinChange => evadeDestinationMinChange;
    public float EvadePredictionMultiplier => evadePredictionMultiplier;
    public float EvadeMaxPredictionTime => evadeMaxPredictionTime;
    public float EvadeMinTargetSpeedForPrediction => evadeMinTargetSpeedForPrediction;
    public int EvadeCandidateAttempts => evadeCandidateAttempts;
    public float EvadeAngleStep => evadeAngleStep;
    public float EvadeMinDistanceFromSelf => evadeMinDistanceFromSelf;
    public float EvadeMinDistanceFromThreat => evadeMinDistanceFromThreat;

    public bool AutoFindHideObstaclesFromTag => autoFindHideObstaclesFromTag;
    public string HideObstacleTag => hideObstacleTag;
    public LayerMask HideObstacleMask => hideObstacleMask;
    public Collider[] HideObstacleColliders => hideObstacleColliders;
    public float HideRepathInterval => hideRepathInterval;
    public float HideNavMeshSampleDistance => hideNavMeshSampleDistance;
    public float HideDestinationMinChange => hideDestinationMinChange;
    public float HideObstaclePadding => hideObstaclePadding;
    public float HideMinDistanceFromSelf => hideMinDistanceFromSelf;
    public float HideMinDistanceFromThreat => hideMinDistanceFromThreat;
    public float HideThreatEyeHeight => hideThreatEyeHeight;
    public float HideVisionProbeHeight => hideVisionProbeHeight;

    public float ComplexEnterRange => complexEnterRange;
    public float ComplexExitRange => complexExitRange;
    public float ComplexPanicMemoryTime => complexPanicMemoryTime;
    public float ComplexMinSubStateDuration => complexMinSubStateDuration;
    public bool ComplexPreferHideOverEvade => complexPreferHideOverEvade;

    public string CurrentStateName => currentStateName;
    public Vector3 TargetVelocity => trackedTargetVelocity;

    public Vector3 DebugWanderCircleCenter => debugWanderCircleCenter;
    public Vector3 DebugWanderRawTarget => debugWanderRawTarget;
    public Vector3 DebugWanderSampledTarget => debugWanderSampledTarget;
    public bool DebugWanderHasValidTarget => debugWanderHasValidTarget;

    public Vector3 DebugPursueTargetPosition => debugPursueTargetPosition;
    public Vector3 DebugPursuePredictedPosition => debugPursuePredictedPosition;
    public Vector3 DebugPursueSampledPosition => debugPursueSampledPosition;
    public float DebugPursuePredictionTime => debugPursuePredictionTime;
    public bool DebugPursueHasValidTarget => debugPursueHasValidTarget;

    public Vector3 DebugEvadeThreatPosition => debugEvadeThreatPosition;
    public Vector3 DebugEvadePredictedThreatPosition => debugEvadePredictedThreatPosition;
    public Vector3 DebugEvadeRawDestination => debugEvadeRawDestination;
    public Vector3 DebugEvadeSampledDestination => debugEvadeSampledDestination;
    public float DebugEvadePredictionTime => debugEvadePredictionTime;
    public bool DebugEvadeHasValidDestination => debugEvadeHasValidDestination;

    public Vector3 DebugHideThreatPosition => debugHideThreatPosition;
    public Vector3 DebugHideObstacleCenter => debugHideObstacleCenter;
    public float DebugHideObstacleRadius => debugHideObstacleRadius;
    public Vector3 DebugHideRawDestination => debugHideRawDestination;
    public Vector3 DebugHideSampledDestination => debugHideSampledDestination;
    public bool DebugHideHasValidDestination => debugHideHasValidDestination;

    public bool DebugComplexCanSeeTarget => debugComplexCanSeeTarget;
    public bool DebugComplexCanTargetSeeMe => debugComplexCanTargetSeeMe;
    public bool DebugComplexIsTargetInRange => debugComplexIsTargetInRange;
    public string DebugComplexSubStateName => debugComplexSubStateName;
    public float DebugComplexDefensiveLockTimer => debugComplexDefensiveLockTimer;

    public RobberWanderState WanderState { get; private set; }
    public RobberPursueState PursueState { get; private set; }
    public RobberEvadeState EvadeState { get; private set; }
    public RobberHideState HideState { get; private set; }
    public RobberComplexState ComplexState { get; private set; }

    private void OnValidate()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        stateMachine = new StateMachine();

        WanderState = new RobberWanderState(this, stateMachine);
        PursueState = new RobberPursueState(this, stateMachine);
        EvadeState = new RobberEvadeState(this, stateMachine);
        HideState = new RobberHideState(this, stateMachine);
        ComplexState = new RobberComplexState(this, stateMachine);

        if (target != null)
            lastTargetPosition = target.position;

        RefreshHideObstacles();
    }

    private void Start()
    {
        stateMachine.Initialize(GetStateByType(initialState));
    }

    private void Update()
    {
        HandleStateKeyboardInput();
        UpdateTrackedTargetVelocity();

        stateMachine.Tick();
        currentStateName = stateMachine.CurrentState?.GetType().Name ?? "None";
    }

    private void HandleStateKeyboardInput()
    {
        if (!enableKeyboardStateTesting)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            ChangeState(WanderState);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            ChangeState(PursueState);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            ChangeState(EvadeState);
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            ChangeState(HideState);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeState(ComplexState);
        }
    }

    public void ChangeState(RobberState newState)
    {
        stateMachine.ChangeState(newState);
    }

    public RobberState GetStateByType(RobberStateType stateType)
    {
        switch (stateType)
        {
            case RobberStateType.Pursue:
                return PursueState;
            case RobberStateType.Evade:
                return EvadeState;
            case RobberStateType.Hide:
                return HideState;
            case RobberStateType.Complex:
                return ComplexState;
            case RobberStateType.Wander:
            default:
                return WanderState;
        }
    }

    public void RefreshHideObstacles()
    {
        if (!autoFindHideObstaclesFromTag)
            return;

        if (string.IsNullOrWhiteSpace(hideObstacleTag))
        {
            hideObstacleColliders = new Collider[0];
            return;
        }

        List<Collider> results = new List<Collider>();

        try
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(hideObstacleTag);

            foreach (GameObject go in taggedObjects)
            {
                if (go == gameObject)
                    continue;

                Collider col = go.GetComponent<Collider>();

                if (col == null)
                    col = go.GetComponentInChildren<Collider>();

                if (col != null && !results.Contains(col))
                    results.Add(col);
            }
        }
        catch (UnityException)
        {
            Debug.LogWarning($"The tag '{hideObstacleTag}' does not exist. Create it or disable auto-find and assign colliders manually.");
        }

        hideObstacleColliders = results.ToArray();
    }

    public Vector3 GetRobberEyePosition()
    {
        if (eyesPoint != null)
            return eyesPoint.position;

        return transform.position + Vector3.up * robberEyeHeightFallback;
    }

    public Vector3 GetTargetEyePosition()
    {
        if (target == null)
            return Vector3.zero;

        return target.position + Vector3.up * targetEyeHeight;
    }

    public float DistanceToTargetXZ()
    {
        if (target == null)
            return float.MaxValue;

        Vector3 from = transform.position;
        Vector3 to = target.position;
        from.y = 0f;
        to.y = 0f;

        return Vector3.Distance(from, to);
    }

    public bool IsTargetInRange()
    {
        return DistanceToTargetXZ() <= complexEnterRange;
    }

    public bool CanSeeTarget()
    {
        if (target == null)
            return false;

        Vector3 from = GetRobberEyePosition();
        Vector3 to = GetTargetEyePosition();

        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.0001f)
            flatForward = Vector3.forward;
        flatForward.Normalize();

        Vector3 flatDirectionToTarget = to - from;
        flatDirectionToTarget.y = 0f;

        float distance = flatDirectionToTarget.magnitude;
        if (distance > robberViewDistance)
            return false;

        if (distance > 0.0001f)
        {
            float angle = Vector3.Angle(flatForward, flatDirectionToTarget.normalized);
            if (angle > robberViewAngle * 0.5f)
                return false;
        }

        return !Physics.Linecast(from, to, visionObstacleMask, QueryTriggerInteraction.Ignore);
    }

    public bool CanTargetSeeMe()
    {
        if (target == null)
            return false;

        Vector3 from = GetTargetEyePosition();
        Vector3 to = GetRobberEyePosition();

        Vector3 flatForward = target.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.0001f)
            flatForward = Vector3.forward;
        flatForward.Normalize();

        Vector3 flatDirectionToMe = to - from;
        flatDirectionToMe.y = 0f;

        float distance = flatDirectionToMe.magnitude;
        if (distance > targetViewDistance)
            return false;

        if (distance > 0.0001f)
        {
            float angle = Vector3.Angle(flatForward, flatDirectionToMe.normalized);
            if (angle > targetViewAngle * 0.5f)
                return false;
        }

        return !Physics.Linecast(from, to, visionObstacleMask, QueryTriggerInteraction.Ignore);
    }

    private void UpdateTrackedTargetVelocity()
    {
        if (target == null)
        {
            trackedTargetVelocity = Vector3.zero;
            return;
        }

        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);

        Vector3 rawVelocity = (target.position - lastTargetPosition) / deltaTime;
        rawVelocity.y = 0f;

        float lerpFactor = 1f - Mathf.Exp(-trackedTargetVelocitySmoothing * Time.deltaTime);
        trackedTargetVelocity = Vector3.Lerp(trackedTargetVelocity, rawVelocity, lerpFactor);

        lastTargetPosition = target.position;
    }

    public void SetWanderDebugData(Vector3 circleCenter, Vector3 rawTarget, Vector3 sampledTarget, bool hasValidTarget)
    {
        debugWanderCircleCenter = circleCenter;
        debugWanderRawTarget = rawTarget;
        debugWanderSampledTarget = sampledTarget;
        debugWanderHasValidTarget = hasValidTarget;
    }

    public void ClearWanderDebugData()
    {
        debugWanderCircleCenter = Vector3.zero;
        debugWanderRawTarget = Vector3.zero;
        debugWanderSampledTarget = Vector3.zero;
        debugWanderHasValidTarget = false;
    }

    public void SetPursueDebugData(Vector3 targetPosition, Vector3 predictedPosition, Vector3 sampledPosition, float predictionTime, bool hasValidTarget)
    {
        debugPursueTargetPosition = targetPosition;
        debugPursuePredictedPosition = predictedPosition;
        debugPursueSampledPosition = sampledPosition;
        debugPursuePredictionTime = predictionTime;
        debugPursueHasValidTarget = hasValidTarget;
    }

    public void ClearPursueDebugData()
    {
        debugPursueTargetPosition = Vector3.zero;
        debugPursuePredictedPosition = Vector3.zero;
        debugPursueSampledPosition = Vector3.zero;
        debugPursuePredictionTime = 0f;
        debugPursueHasValidTarget = false;
    }

    public void SetEvadeDebugData(
        Vector3 threatPosition,
        Vector3 predictedThreatPosition,
        Vector3 rawDestination,
        Vector3 sampledDestination,
        float predictionTime,
        bool hasValidDestination)
    {
        debugEvadeThreatPosition = threatPosition;
        debugEvadePredictedThreatPosition = predictedThreatPosition;
        debugEvadeRawDestination = rawDestination;
        debugEvadeSampledDestination = sampledDestination;
        debugEvadePredictionTime = predictionTime;
        debugEvadeHasValidDestination = hasValidDestination;
    }

    public void ClearEvadeDebugData()
    {
        debugEvadeThreatPosition = Vector3.zero;
        debugEvadePredictedThreatPosition = Vector3.zero;
        debugEvadeRawDestination = Vector3.zero;
        debugEvadeSampledDestination = Vector3.zero;
        debugEvadePredictionTime = 0f;
        debugEvadeHasValidDestination = false;
    }

    public void SetHideDebugData(
        Vector3 threatPosition,
        Vector3 obstacleCenter,
        float obstacleRadius,
        Vector3 rawDestination,
        Vector3 sampledDestination,
        bool hasValidDestination)
    {
        debugHideThreatPosition = threatPosition;
        debugHideObstacleCenter = obstacleCenter;
        debugHideObstacleRadius = obstacleRadius;
        debugHideRawDestination = rawDestination;
        debugHideSampledDestination = sampledDestination;
        debugHideHasValidDestination = hasValidDestination;
    }

    public void ClearHideDebugData()
    {
        debugHideThreatPosition = Vector3.zero;
        debugHideObstacleCenter = Vector3.zero;
        debugHideObstacleRadius = 0f;
        debugHideRawDestination = Vector3.zero;
        debugHideSampledDestination = Vector3.zero;
        debugHideHasValidDestination = false;
    }

    public void SetComplexDebugData(bool canSeeTarget, bool canTargetSeeMe, bool isTargetInRange, string subStateName, float defensiveLockTimer)
    {
        debugComplexCanSeeTarget = canSeeTarget;
        debugComplexCanTargetSeeMe = canTargetSeeMe;
        debugComplexIsTargetInRange = isTargetInRange;
        debugComplexSubStateName = subStateName;
        debugComplexDefensiveLockTimer = defensiveLockTimer;
    }

    public void ClearComplexDebugData()
    {
        debugComplexCanSeeTarget = false;
        debugComplexCanTargetSeeMe = false;
        debugComplexIsTargetInRange = false;
        debugComplexSubStateName = string.Empty;
        debugComplexDefensiveLockTimer = 0f;
    }
}