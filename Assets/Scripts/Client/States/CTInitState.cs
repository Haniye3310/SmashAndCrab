using Cysharp.Threading.Tasks;
using UnityEngine;

public class CTInitState : IState
{
    public UniTask OnEnter(object arg)
    {
        CTGameManager.Instance.MainStateMachine.SwitchState(new CTGameplayState(),null);
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
