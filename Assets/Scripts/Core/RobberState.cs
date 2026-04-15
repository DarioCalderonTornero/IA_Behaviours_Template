public abstract class RobberState : IState
{
    protected readonly RobberBrain brain;
    protected readonly StateMachine stateMachine;

    protected RobberState(RobberBrain brain, StateMachine stateMachine)
    {
        this.brain = brain;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter()
    {
    }

    public virtual void Tick()
    {
    }

    public virtual void Exit()
    {
    }
}