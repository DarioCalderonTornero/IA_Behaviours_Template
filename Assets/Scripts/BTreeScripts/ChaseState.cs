using UnityEngine;

public class ChaseState : IGuardState
{
    private float lostSightTimer = 0f;
    private float lostSightDuration = 2f;

    public void Enter(GuardFSM guard)
    {
        guard.agent.speed = 5f;
    }

    public void Update(GuardFSM guard)
    {
        if (guard.senses.CanSeePlayer())
        {
            lostSightTimer = 0f;
            guard.investigationPoint = guard.senses.GetPlayerPosition();
            guard.MoveTo(guard.investigationPoint);
        }
        else
        {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= lostSightDuration)
                guard.ChangeState(new InvestigateState());
        }

        if (HasCaughtPlayer(guard))
            guard.ChangeState(new CaughtState());
    }

    private bool HasCaughtPlayer(GuardFSM guard)
    {
        return Vector3.Distance(guard.transform.position, guard.player.position) < 1f;
    }

    public void Exit(GuardFSM guard)
    {
        guard.StopMoving();
    }
}