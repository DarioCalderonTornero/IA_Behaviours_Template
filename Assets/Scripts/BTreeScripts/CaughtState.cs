using UnityEngine;

public class CaughtState : IGuardState
{
    public void Enter(GuardFSM guard)
    {
        guard.StopMoving();
        GameManager.Instance.GameOver();
    }

    public void Update(GuardFSM guard) { }

    public void Exit(GuardFSM guard) { }
}