using UnityEngine;

public enum EventClient
{
    SendInput,
    SendJump,
}
public enum ServerEvent
{
    SendID,
    SendPos,
    SendAllIdsToAllConnections,
    SendGeneratorPos,
    SendItem,
    SendRemovedItem,
    SendCollectedItem
}