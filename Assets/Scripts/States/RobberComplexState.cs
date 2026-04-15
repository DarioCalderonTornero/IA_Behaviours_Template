using UnityEngine;

public class RobberComplexState : RobberState
{
    private RobberState activeSubState;
    private bool targetInRange;
    private float defensiveLockTimer;
    private float timeInCurrentSubState;

    public RobberComplexState(RobberBrain brain, StateMachine stateMachine) : base(brain, stateMachine)
    {
    }

    public override void Enter()
    {
        if (brain.Agent == null)
            return;

        brain.Agent.isStopped = false;

        targetInRange = brain.IsTargetInRange();
        defensiveLockTimer = 0f;
        timeInCurrentSubState = 0f;
        activeSubState = null;

        brain.RefreshHideObstacles();
        brain.ClearComplexDebugData();

        EvaluateAndSwitch(forceSwitch: true);
    }

    public override void Tick()
    {
        if (brain.Agent == null || brain.Target == null)
            return;

        UpdateRangeHysteresis();
        EvaluateAndSwitch(forceSwitch: false);

        timeInCurrentSubState += Time.deltaTime;
        activeSubState?.Tick();
    }

    public override void Exit()
    {
        activeSubState?.Exit();
        activeSubState = null;
        brain.ClearComplexDebugData();
    }

    private void UpdateRangeHysteresis()
    {
        float distance = brain.DistanceToTargetXZ();

        if (!targetInRange && distance <= brain.ComplexEnterRange)
        {
            targetInRange = true;
        }
        else if (targetInRange && distance >= brain.ComplexExitRange)
        {
            targetInRange = false;
        }
    }

    private void EvaluateAndSwitch(bool forceSwitch)
    {
        bool canSeeTarget = brain.CanSeeTarget();
        bool canTargetSeeMe = brain.CanTargetSeeMe();

        if (canTargetSeeMe)
        {
            defensiveLockTimer = brain.ComplexPanicMemoryTime;
        }
        else
        {
            defensiveLockTimer = Mathf.Max(0f, defensiveLockTimer - Time.deltaTime);
        }

        RobberState desiredSubState = DetermineDesiredSubState(canSeeTarget, canTargetSeeMe);

        brain.SetComplexDebugData(
            canSeeTarget,
            canTargetSeeMe,
            targetInRange,
            desiredSubState != null ? desiredSubState.GetType().Name : "None",
            defensiveLockTimer);

        bool canChangeNow = forceSwitch || timeInCurrentSubState >= brain.ComplexMinSubStateDuration;

        if (desiredSubState != activeSubState && canChangeNow)
        {
            SwitchSubState(desiredSubState);
        }
    }

    private RobberState DetermineDesiredSubState(bool canSeeTarget, bool canTargetSeeMe)
    {
        if (!targetInRange)
            return brain.WanderState;

        bool shouldStayDefensive = canTargetSeeMe || defensiveLockTimer > 0f;

        if (shouldStayDefensive)
        {
            brain.RefreshHideObstacles();

            bool hasHideOptions = brain.HideObstacleColliders != null && brain.HideObstacleColliders.Length > 0;

            if (brain.ComplexPreferHideOverEvade && hasHideOptions)
                return brain.HideState;

            return brain.EvadeState;
        }

        if (canSeeTarget)
            return brain.PursueState;

        return brain.WanderState;
    }

    private void SwitchSubState(RobberState newSubState)
    {
        activeSubState?.Exit();

        activeSubState = newSubState;
        timeInCurrentSubState = 0f;

        activeSubState?.Enter();
    }
}