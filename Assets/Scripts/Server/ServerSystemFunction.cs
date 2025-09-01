using Unity.Collections;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine;

public class ServerSystemFunction
{
    public static void Start(ServerDataRepo serverDataRepo)
    {
        serverDataRepo.m_Driver = NetworkDriver.Create();
        serverDataRepo.m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);
        if (serverDataRepo.m_Driver.Bind(endpoint) != 0)
        {
            Debug.LogError("Failed to bind to port 7777.");
            return;
        }
        serverDataRepo.m_Driver.Listen();
    }
    public static void Update(ServerDataRepo serverDataRepo)
    {
        serverDataRepo.m_Driver.ScheduleUpdate().Complete();
        // Clean up connections.
        for (int i = 0; i < serverDataRepo.m_Connections.Length; i++)
        {
            if (!serverDataRepo.m_Connections[i].IsCreated)
            {
                serverDataRepo.m_Connections.RemoveAtSwapBack(i);
                i--;
            }
        }
        // Accept new connections.
        NetworkConnection c;
        while ((c = serverDataRepo.m_Driver.Accept()) != default)
        {
            serverDataRepo.m_Connections.Add(c);
            Debug.Log("Accepted a connection.");
        }
        for (int i = 0; i < serverDataRepo.m_Connections.Length; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = serverDataRepo.m_Driver.PopEventForConnection(serverDataRepo.m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    uint number = stream.ReadUInt();
                    Debug.Log($"Got {number} from a client, adding 2 to it.");
                    number += 2;

                    serverDataRepo.m_Driver.BeginSend(NetworkPipeline.Null, serverDataRepo.m_Connections[i], out var writer);
                    writer.WriteUInt(number);
                    serverDataRepo.m_Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from the server.");
                    serverDataRepo.m_Connections[i] = default;
                    break;
                }

            }
        }
    }
    public static void OnDestroy(ServerDataRepo serverDataRepo)
    {
        if (serverDataRepo.m_Driver.IsCreated)
        {
            serverDataRepo.m_Driver.Dispose();
            serverDataRepo.m_Connections.Dispose();
        }
    }

}
