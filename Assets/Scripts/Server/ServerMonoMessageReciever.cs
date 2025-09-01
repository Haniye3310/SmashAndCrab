using UnityEngine;

public class ServerMonoMessageReciever : MonoBehaviour
{
    public ServerDataRepo ServerDataRepo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ServerSystemFunction.Start(ServerDataRepo);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDestroy()
    {
        ServerSystemFunction.OnDestroy(ServerDataRepo);
    }
}
