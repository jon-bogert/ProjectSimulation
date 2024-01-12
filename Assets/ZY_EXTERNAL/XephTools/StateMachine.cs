using System.Collections.Generic;

namespace XephTools
{
    public interface IState<AgentType> where AgentType : class
    {
        public void Enter(AgentType agent) { }
        public void Update(AgentType agent, float deltaTime) { }
        public void Exit(AgentType agent) { }
    }

    public class StateMachine<AgentType> where AgentType : class
    {
        protected AgentType _agent;
        protected int _currentState = -1;
        protected List<IState<AgentType>> _states;

        public int currentState { get { return _currentState; } }

        public StateMachine(AgentType agent)
        {
            _agent = agent;
            _states = new List<IState<AgentType>>();
        }

        public void AddState<StateType>() where StateType : IState<AgentType>, new()
        {
            _states.Add(new StateType());
        }

        public void Update(float deltaTime)
        {
            if (_currentState < 0)
                return;

            _states[_currentState].Update(_agent, deltaTime);
        }

        public void ChangeState(int index)
        {
            if (_currentState >= 0)
                _states[_currentState].Exit(_agent);

            _currentState = index;
            _states[_currentState].Enter(_agent);
        }
    }
}