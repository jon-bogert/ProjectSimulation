using XephTools;

public enum PlayerStates
{
    Grounded,
    Airborn
}
public class PlayerGrounded : IState<PlayerMovement>
{
    public void Update(PlayerMovement agent, float deltaTime)
    {
        if (agent.moveAxis.sqrMagnitude > 0)
        {
            agent.Move(agent.moveSpeedGround);
        }
        else
        {
            agent.Deccelerate(agent.frictionGround);
        }

        if (agent.jumpThisFrame)
        {
            agent.jumpThisFrame = false;
            agent.DoJump();
        }

        if (!agent.isGrounded)
        {
            agent.stateMachine.ChangeState((int)PlayerStates.Airborn);
        }
    }
}

public class PlayerAirborn : IState<PlayerMovement>
{
    public void Update(PlayerMovement agent, float deltaTime)
    {
        if (agent.moveAxis.sqrMagnitude > 0)
        {
            agent.Move(agent.moveSpeedAir);
        }
        else
        {
            agent.Deccelerate(agent.frictionAir);
        }

        if (agent.isGrounded)
        {
            agent.OnLand();
            agent.stateMachine.ChangeState((int)PlayerStates.Grounded);
        }
        else
        {
            agent.ApplyGravity();
        }
    }
}
