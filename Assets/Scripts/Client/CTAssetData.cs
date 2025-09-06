using UnityEngine;

[CreateAssetMenu(fileName = "CTAssetData", menuName = "ScriptableObjects/CTAssetData")]

public class CTAssetData : ScriptableObject
{
    [SerializeField] GameObject _playerPrefab;
    public GameObject PlayerPrefab => _playerPrefab;
}
