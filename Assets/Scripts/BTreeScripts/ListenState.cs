using UnityEngine;

public class ListenState : IGuardState
{
    private float listenDuration = 2f;
    private float timer = 0f;

    public void Enter(GuardFSM guard)
    {
        guard.StopMoving();
        timer = 0f;
    }

    public void Update(GuardFSM guard)
    {
        if (guard.senses.CanSeePlayer())
        {
            guard.investigationPoint = guard.senses.GetPlayerPosition();
            guard.ChangeState(new ChaseState());
            return;
        }

        Vector3 dirToNoise = (guard.investigationPoint - guard.transform.position).normalized;
        dirToNoise.y = 0f;
        if (dirToNoise != Vector3.zero)
            guard.transform.rotation = Quaternion.Slerp(
                guard.transform.rotation,
                Quaternion.LookRotation(dirToNoise),
                Time.deltaTime * 3f
            );

        timer += Time.deltaTime;
        if (timer >= listenDuration)
            guard.ChangeState(new InvestigateState());
    }

    public void Exit(GuardFSM guard) { }
}