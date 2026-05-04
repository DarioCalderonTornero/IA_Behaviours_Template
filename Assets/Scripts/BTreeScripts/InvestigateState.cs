using UnityEngine;

public class InvestigateState : IGuardState
{
    private float waitDuration = 4f;
    private float timer = 0f;
    private bool hasArrived = false;

    public void Enter(GuardFSM guard)
    {
        guard.agent.speed = 3.5f;
        timer = 0f;
        hasArrived = false;
        guard.MoveTo(guard.investigationPoint);
    }

    public void Update(GuardFSM guard)
    {
        if (guard.senses.CanSeePlayer())
        {
            guard.investigationPoint = guard.senses.GetPlayerPosition();
            guard.ChangeState(new ChaseState());
            return;
        }

        if (!hasArrived)
        {
            if (guard.HasReachedDestination())
                hasArrived = true;
            return;
        }

        LookAround(guard);

        timer += Time.deltaTime;
        if (timer >= waitDuration)
            guard.ChangeState(new PatrolState());
    }

    private void LookAround(GuardFSM guard)
    {
        float angle = Mathf.Sin(Time.time * 1.5f) * 60f;
        guard.transform.rotation = Quaternion.Euler(
            0f,
            guard.transform.eulerAngles.y + angle * Time.deltaTime,
            0f
        );
    }

    public void Exit(GuardFSM guard)
    {
        guard.StopMoving();
    }
}