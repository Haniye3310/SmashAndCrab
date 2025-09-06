using Cysharp.Threading.Tasks;
using UnityEngine;

public class InitState : IState
{
    public UniTask OnEnter(object arg)
    {
        GameManager.Instance.MainStateMachine.SwitchState(new GameplayState(),null);
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
