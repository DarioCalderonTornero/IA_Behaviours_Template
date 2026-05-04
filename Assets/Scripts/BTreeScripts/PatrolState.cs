using UnityEngine;
using static UnityEditorInternal.VersionControl.ListControl;

public class PatrolState : IGuardState
{
    private int currentWaypointIndex = 0;

    public void Enter(GuardFSM guard)
    {
        guard.agent.speed = 2f;

        if (guard.waypoints.Length > 0)
            guard.MoveTo(guard.waypoints[currentWaypointIndex].position);
    }

    public void Update(GuardFSM guard)
    {
        if (guard.senses.CanSeePlayer())
        {
            guard.investigationPoint = guard.senses.GetPlayerPosition();
            guard.ChangeState(new ChaseState());
            return;
        }

        if (guard.senses.CanHearPlayer())
        {
            guard.investigationPoint = guard.senses.GetPlayerPosition();
            guard.ChangeState(new ListenState());
            return;
        }

        if (guard.HasReachedDestination())
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % guard.waypoints.Length;
            guard.MoveTo(guard.waypoints[currentWaypointIndex].position);
        }
    }

    public void Exit(GuardFSM guard)
    {
        guard.StopMoving();
    }
}