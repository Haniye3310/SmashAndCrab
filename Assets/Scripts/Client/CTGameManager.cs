using UnityEngine;

public class CTGameManager : MonoBehaviour
{
    public StateMachine MainStateMachine { get; private set; }
    public static CTGameManager Instance { get; private set; }
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MainStateMachine = new StateMachine(new CTInitState());
        MainStateMachine.OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        MainStateMachine.OnUpdate();
    }
    private void OnDestroy()
    {
        MainStateMachine.OnFinished();
    }
}
