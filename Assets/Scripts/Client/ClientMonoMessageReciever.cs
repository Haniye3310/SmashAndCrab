using UnityEngine;

public class ClientMonoMessageReciever : MonoBehaviour
{
    public ClientDataRepo ClientDataRepo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ClientSystemFunction.Start(ClientDataRepo);
    }

    // Update is called once per frame
    void Update()
    {
        ClientSystemFunction.Update(ClientDataRepo);
    }
    private void OnDestroy()
    {
        ClientSystemFunction.OnDestroy(ClientDataRepo);
    }
}
