using UnityEngine;

public class RobberComplexState : RobberState
{
    public RobberComplexState(RobberBrain brain, StateMachine stateMachine) : base(brain, stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("Entering Complex State");
    }

    public override void Tick()
    {
        // Aquí irá la lógica del comportamiento complejo.
    }

    public override void Exit()
    {
        Debug.Log("Exiting Complex State");
    }
}