using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class CTGameplayState : IState
{
    private NetworkDriver _driver;
    private NetworkConnection _connection;
    CTSceneData _sceneData;
    int _id;
    List<CTPlayer> _players = new List<CTPlayer>();
    private float _dataSyncingInterval = 0.05f;
    float _timerCounter;

    public UniTask OnEnter(object arg)
    {
        _sceneData = GameObject.FindObjectOfType<CTSceneData>();
        _driver = NetworkDriver.Create();

        var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);
        _connection = _driver.Connect(endpoint);
        _timerCounter = Time.time;
        return UniTask.CompletedTask;
    }

    public void OnUpdate()
    {
        UpdateCharacterPosition();
        _driver.ScheduleUpdate().Complete();

        if (!_connection.IsCreated)
        {
            return;
        }
        Unity.Collections.DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = _connection.PopEvent(_driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server.");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                byte[] inData = new byte[stream.Length];
                for(int x = 0; x < stream.Length; x++)
                {
                    inData[x]= stream.ReadByte();
                }
                byte[] eventByte = new byte[2];
                Buffer.BlockCopy(inData,0, eventByte, 0, 2);
                Int16 @event = BitConverter.ToInt16(eventByte);
                if ((ServerEvent)@event == ServerEvent.SendAllIdsToAllConnections)
                {
                    RecieveAllIDsOfAllConnections(inData);
                }
                if ((ServerEvent)@event == ServerEvent.SendID)
                {
                    RecieveIDAndInstantiate(inData);
                }
                if ((ServerEvent)@event == ServerEvent.SendPos)
                {
                    RecievePosition(inData);
                }
            }
            
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server.");
                _connection = default;
            }
        }
        if (_timerCounter + _dataSyncingInterval < Time.time)
        {
            SendInputAndID();
            _timerCounter = Time.time;
        }
    }

    public UniTask OnExit()
    {
        _driver.Dispose();
        return UniTask.CompletedTask;
    }
    private void SendInputAndID()
    { 
        byte[] sentBytes = new byte[14];
        float moveHorizontal = _sceneData.UIData.Joystick.Horizontal;

        float moveVertical = _sceneData.UIData.Joystick.Vertical;

        byte[] evBytes = BitConverter.GetBytes((Int16)EventClient.SendInput);
        byte[] moveHorizontalBytes = BitConverter.GetBytes(moveHorizontal);
        byte[] moveVerticalBytes = BitConverter.GetBytes(moveVertical);
        byte[] idByte = BitConverter.GetBytes(_id);

        _driver.BeginSend(_connection, out var writer);
        Buffer.BlockCopy(evBytes, 0, sentBytes, 0, 2);
        Buffer.BlockCopy(moveHorizontalBytes, 0, sentBytes, 2, 4);
        Buffer.BlockCopy(moveVerticalBytes, 0, sentBytes, 6, 4);
        Buffer.BlockCopy(idByte, 0, sentBytes, 10, 4);

        writer.WriteBytes(sentBytes);

        _driver.EndSend(writer);
    }
    public void RecieveIDAndInstantiate(byte[] inData)
    {
        byte[] IDByte = new byte[4];
        Buffer.BlockCopy(inData, 2, IDByte, 0, 4);

        _id = BitConverter.ToInt32(IDByte);
        var go = GameObject.Instantiate(_sceneData.AssetData.PlayerPrefab);
        _players.Add(new CTPlayer { ID = _id, Player = go });
    }
    void RecieveAllIDsOfAllConnections(byte[] inData)
    {
        byte[] numberOfIDsByte = new byte[4];
        Buffer.BlockCopy(inData, 2, numberOfIDsByte, 0, 4);
        int numberOFIds = BitConverter.ToInt32(numberOfIDsByte);


        for (int i = 0; i < numberOFIds; i++)
        {
            byte[] idByte = new byte[4];
            Buffer.BlockCopy(inData, 6 + (i * 4), idByte, 0, 4);
            int id = BitConverter.ToInt32(idByte);
            bool isIdExistInList = false;
            foreach ( CTPlayer p in _players)
            {
                if (p.ID == id)
                {
                    isIdExistInList = true;
                    break;
                }
            }
            if (!isIdExistInList)
            {
                var go = GameObject.Instantiate(_sceneData.AssetData.PlayerPrefab);
                _players.Add(new CTPlayer { ID = id, Player = go });
            }
        }

    }
    void RecievePosition(byte[] inData)
    {
        byte[] posXByte = new byte[4];
        byte[] posYByte = new byte[4];
        byte[] posZByte = new byte[4];
        byte[] IDByte = new byte[4];

        Buffer.BlockCopy(inData, 2, posXByte, 0, 4);
        Buffer.BlockCopy(inData, 6, posYByte, 0, 4);
        Buffer.BlockCopy(inData, 10, posZByte, 0, 4);
        Buffer.BlockCopy(inData, 14, IDByte, 0, 4);

        float posX = BitConverter.ToSingle(posXByte);
        float posY = BitConverter.ToSingle(posYByte);
        float posZ = BitConverter.ToSingle(posZByte);
        int ID = BitConverter.ToInt32(IDByte);

        foreach (CTPlayer p in _players)
        {
            if (p.ID == ID)
            {
                p.MovementDirection = new Vector3(posX, posY, posZ) - p.Player.transform.position;
            }
        }

    }
    void UpdateCharacterPosition()
    {
        foreach (CTPlayer p in _players)
        {
            p.Player.transform.position += p.MovementDirection * 10 * Time.deltaTime;

            if (p.MovementDirection.magnitude > 0.01)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(p.MovementDirection.x, 0, p.MovementDirection.z));
                p.Player.transform.rotation = Quaternion.Lerp(p.Player.transform.rotation, targetRotation, 10 * Time.deltaTime);

            }

        }
    }
}
