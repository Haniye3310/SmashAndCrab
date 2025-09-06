using NUnit.Framework;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class SRRuntimeData 
{

}
public struct SRConnectionData
{
    public NetworkConnection NetworkConnection;
    public int ID;
}
public class SRPlayerData
{
    public int ID;
    public GameObject Player;
    public Vector2 Input;
}