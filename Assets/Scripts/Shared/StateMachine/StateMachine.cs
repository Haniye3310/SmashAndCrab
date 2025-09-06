using Cysharp.Threading.Tasks;
using System;
public class StateMachine : IStateMachine
{
    bool _initialized = false;

    public StateMachine(IState initState)
    {
        CurrentState = initState;
    }

    public IState CurrentState { get; private set; }

    public void OnUpdate()
    {
        if (_initialized)
        {
            CurrentState.OnUpdate();
        }
    }

    public void SwitchState(IState to, object arg)
    {
        Func<UniTaskVoid> asyncOp = async () =>
        {
            _initialized = false;
            await CurrentState.OnExit();
            CurrentState = to;
            await CurrentState.OnEnter(arg);
            _initialized = true;
        };
        asyncOp().Forget();
    }

    public async void OnStart()
    {
        await CurrentState.OnEnter(null);
        _initialized = true;
    }

    public void OnFinished() { }
}