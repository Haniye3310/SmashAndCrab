using UnityEngine.UIElements;

public interface IStateMachine
{
    public IState CurrentState { get; }
    public void SwitchState(IState to, object arg = null);
    public void OnStart();
    public void OnUpdate();
    public void OnFinished();
}