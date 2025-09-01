using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class ServerDataRepo : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NativeList<NetworkConnection> m_Connections;
}
