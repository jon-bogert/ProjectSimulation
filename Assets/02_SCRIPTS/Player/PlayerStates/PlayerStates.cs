﻿using XephTools;
using UnityEngine;

public enum PlayerStates
{
    Grounded = 0,
    Airborn = 1,
    Climbing = 2
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

public class PlayerClimbing : IState<PlayerMovement>
{
    private bool lastHandLeft = true;
    public void Update(PlayerMovement agent, float deltaTime)
    {
        bool isLeftClimbing = agent.leftClimb.isClimbing;
        bool isRightClimbing = agent.rightClimb.isClimbing;
        Vector3 moveAmount = Vector3.zero;

        ClimbController activeHand = null;
        // Check which hands are climbing
        if (isLeftClimbing && isRightClimbing)
        {
            moveAmount = agent.leftClimb.moveDelta + agent.rightClimb.moveDelta / 2f;
            moveAmount *= -1f;
        }
        else if (isLeftClimbing)
        {
            lastHandLeft = true;
            activeHand = agent.leftClimb;
            moveAmount = -activeHand.moveDelta;
        }
        else if (isRightClimbing)
        {
            lastHandLeft = false;
            activeHand = agent.rightClimb;
            moveAmount = -activeHand.moveDelta;
        }
        else
        {
            ((lastHandLeft) ? agent.leftClimb : agent.rightClimb).EndClimb();
            return;
        }

        agent.AbsoluteMove(moveAmount);

        // TODO Update hand position to grip point?
    }
}