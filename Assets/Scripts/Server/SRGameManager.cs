using UnityEngine;

public class SRGameManager : MonoBehaviour
{
    public StateMachine MainStateMachine { get; private set; }
    public static SRGameManager Instance { get; private set; }
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MainStateMachine = new StateMachine(new SRInitState());
        MainStateMachine.OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        MainStateMachine.OnUpdate();
    }
}
