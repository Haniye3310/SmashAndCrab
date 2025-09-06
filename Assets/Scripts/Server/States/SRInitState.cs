using Cysharp.Threading.Tasks;
using UnityEngine;

public class SRInitState : IState
{
    public UniTask OnEnter(object arg)
    {
        SRGameManager.Instance.MainStateMachine.SwitchState(new SRGameplayState(), null);
        return UniTask.CompletedTask;
    }

    public void OnUpdate()
    {

    }
    public UniTask OnExit()
    {
        return UniTask.CompletedTask;
    }
}
