using Cysharp.Threading.Tasks;

public interface IState
{
    public UniTask OnEnter(object arg);
    public void OnUpdate();
    public UniTask OnExit();
}