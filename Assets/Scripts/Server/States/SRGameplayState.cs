using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class SRGameplayState : IState
{
    private NetworkDriver _driver;
    private SRRuntimeData _runtimeData;
    private SRSceneData _sceneData;
    private int _idCounter;
    private NativeList<SRConnectionData> _connections;
    private List<SRPlayerData> _players = new List<SRPlayerData>();
    float _timerCounter;
    private float _dataSyncingInterval=0.05f;
    public UniTask OnEnter(object arg)
    {
        _timerCounter = Time.time;
        _sceneData = GameObject.FindAnyObjectByType<SRSceneData>();
        _driver = NetworkDriver.Create();
        _connections = new NativeList<SRConnectionData>(16, Allocator.Persistent);

        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);
        if (_driver.Bind(endpoint) != 0)
        {
            Debug.LogError("Failed to bind to port 7777.");
            return UniTask.CompletedTask;
        }
        _driver.Listen();
        Debug.Log("serverIsListening");
        return UniTask.CompletedTask;
    }


    public void OnUpdate()
    {
        _driver.ScheduleUpdate().Complete();
        // Clean up connections.
        for (int i = 0; i < _connections.Length; i++)
        {
            if (!_connections[i].NetworkConnection.IsCreated)
            {
                _connections.RemoveAtSwapBack(i);
                i--;
            }
        }
        // Accept new connections.
        NetworkConnection c;
        while ((c = _driver.Accept()) != default)
        {
            Debug.Log("Accepted a connection.");

            _connections.Add(new SRConnectionData { NetworkConnection = c, ID = _idCounter });
            var go = GameObject.Instantiate(_sceneData.SRAssetData.PlayerPrefab, new Vector3(0f, 2, 0f), Quaternion.identity);
            _players.Add(new SRPlayerData() { Player = go, ID = _idCounter });
            SendID(_connections[_connections.Length - 1]);
            SendAllIDsToAllConnections();
            _idCounter++;
        }
        for (int i = 0; i < _connections.Length; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = _driver.PopEventForConnection(_connections[i].NetworkConnection, out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    // Convert DataStreamReader to byte array
                    byte[] inData = new byte[stream.Length];
                    for (int x = 0; x < stream.Length; x++)
                    {
                        inData[x] = stream.ReadByte();
                    }

                    byte[] eventByte = new byte[2];
                    Buffer.BlockCopy(inData, 0, eventByte, 0, 2);
                    int eve = BitConverter.ToInt16(eventByte);


                    if ((EventClient)eve == EventClient.SendInput)
                    {
                        float horizontal;
                        float vertical;
                        int idToMove;
                        RecieveInputByID(inData, out horizontal, out vertical, out idToMove);
                        foreach (SRPlayerData p in _players)
                        {
                            if (idToMove == p.ID)
                            {
                                p.Input = new Vector2(horizontal, vertical);
                            }
                        }
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from the server.");
                    _connections[i] = default;
                    break;
                }

            }
            if (_timerCounter + _dataSyncingInterval < Time.time)
            {
                for (int j = 0; j < _connections.Length; j++)
                {
                    foreach (SRPlayerData p in _players)
                    {
                        SendPositionAndID(
                            p.Player.transform.position,
                            p.ID,
                            _connections[j].NetworkConnection);
                    }

                }
                _timerCounter = Time.time;
            }
        }
        MoveCharacters();
    }
    public UniTask OnExit()
    {
        if (_driver.IsCreated)
        {
            _driver.Dispose();
            _connections.Dispose();
        }
        return UniTask.CompletedTask;
    }
    void SendID(SRConnectionData connectionData)
    {
        byte[] @event = BitConverter.GetBytes((Int16)ServerEvent.SendID);
        byte[] IDBytes = BitConverter.GetBytes(connectionData.ID);


        _driver.BeginSend(NetworkPipeline.Null, connectionData.NetworkConnection, out var writer);

        byte[] sentBytes = new byte[6];

        Buffer.BlockCopy(@event, 0, sentBytes, 0, 2);
        Buffer.BlockCopy(IDBytes, 0, sentBytes, 2, 4);


        writer.WriteBytes(sentBytes);
        _driver.EndSend(writer);
    }
    void SendAllIDsToAllConnections()
    {
        byte[] @event = BitConverter.GetBytes((int)ServerEvent.SendAllIdsToAllConnections);
        byte[] numberOfIDs = BitConverter.GetBytes(_connections.Length);


        byte[] sentBytes = new byte[6 + _connections.Length * 4];

        Buffer.BlockCopy(@event, 0, sentBytes, 0, 2);
        Buffer.BlockCopy(numberOfIDs, 0, sentBytes, 2, 4);

        for (int i = 0; i < _connections.Length; i++)
        {
            byte[] idByte = BitConverter.GetBytes(_connections[i].ID);
            Buffer.BlockCopy(idByte, 0, sentBytes, 6 + (i * 4), 4);
        }
        for (int i = 0; i < _connections.Length; i++)
        {
            _driver.BeginSend(NetworkPipeline.Null, _connections[i].NetworkConnection, out var writer);
            writer.WriteBytes(sentBytes);
            _driver.EndSend(writer);
        }

    }
    void MoveCharacters()
    {
        foreach (SRPlayerData p in _players)
        {

            Vector3 direction = new Vector3(p.Input.x, 0f, p.Input.y);

            direction = direction.normalized * direction.magnitude;

            if (direction != Vector3.zero)
            {
                Vector3 newPosition = p.Player.transform.position + direction * 2 * Time.deltaTime;
                // Update the player's position
                p.Player.transform.position = newPosition;
            }
        }

    }
    void RecieveInputByID(byte[] inData, out float moveHorizontal, out float moveVertical, out int idToMove)
    {
        byte[] moveHorizontalByte = new byte[4];
        byte[] moveVerticalByte = new byte[4];
        byte[] idByte = new byte[4];

        Buffer.BlockCopy(inData, 2, moveHorizontalByte, 0, 4);
        Buffer.BlockCopy(inData, 6, moveVerticalByte, 0, 4);
        Buffer.BlockCopy(inData, 10, idByte, 0, 4);

        moveHorizontal = BitConverter.ToSingle(moveHorizontalByte);
        moveVertical = BitConverter.ToSingle(moveVerticalByte);
        idToMove = BitConverter.ToInt32(idByte);

    }
     void SendPositionAndID(Vector3 position, int iD, NetworkConnection networkConnection)
    {
        byte[] @event = BitConverter.GetBytes((Int16)ServerEvent.SendPos);
        byte[] posX = BitConverter.GetBytes(position.x);
        byte[] posY = BitConverter.GetBytes(position.y);
        byte[] posZ = BitConverter.GetBytes(position.z);
        byte[] IDByte = BitConverter.GetBytes(iD);

        byte[] sentBytes = new byte[18];
        _driver.BeginSend(NetworkPipeline.Null, networkConnection, out var writer);

        Buffer.BlockCopy(@event, 0, sentBytes, 0, 2);
        Buffer.BlockCopy(posX, 0, sentBytes, 2, 4);
        Buffer.BlockCopy(posY, 0, sentBytes, 6, 4);
        Buffer.BlockCopy(posZ, 0, sentBytes, 10, 4);
        Buffer.BlockCopy(IDByte, 0, sentBytes, 14, 4);
        writer.WriteBytes(sentBytes);
        _driver.EndSend(writer);
    }
}
