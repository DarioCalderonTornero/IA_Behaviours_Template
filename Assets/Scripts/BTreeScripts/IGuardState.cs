// IGuardState.cs
public interface IGuardState
{
    void Enter(GuardFSM guard);  
    void Update(GuardFSM guard);  
    void Exit(GuardFSM guard);    
}