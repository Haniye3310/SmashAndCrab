using Unity.Networking.Transport;
using UnityEngine;

public class ClientSystemFunction
{
    public static void Start(ClientDataRepo clientDataRepo)
    {
        clientDataRepo.m_Driver = NetworkDriver.Create();

        var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);
        clientDataRepo.m_Connection = clientDataRepo.m_Driver.Connect(endpoint);

    }
    public static void Update(ClientDataRepo clientDataRepo)
    {
        clientDataRepo.m_Driver.ScheduleUpdate().Complete();

        if (!clientDataRepo.m_Connection.IsCreated)
        {
            return;
        }
        Unity.Collections.DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = clientDataRepo.m_Connection.PopEvent(clientDataRepo.m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server.");

                uint value = 1;
                clientDataRepo.m_Driver.BeginSend(clientDataRepo.m_Connection, out var writer);
                writer.WriteUInt(value);
                clientDataRepo.m_Driver.EndSend(writer);
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                uint value = stream.ReadUInt();
                Debug.Log($"Got the value {value} back from the server.");

                clientDataRepo.m_Connection.Disconnect(clientDataRepo.m_Driver);
                clientDataRepo.m_Connection = default;
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server.");
                clientDataRepo.m_Connection = default;
            }
        }
    }
    public static void OnDestroy(ClientDataRepo clientDataRepo)
    {
        clientDataRepo.m_Driver.Dispose();
    }

}
