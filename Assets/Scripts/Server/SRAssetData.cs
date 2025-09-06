using UnityEngine;

[CreateAssetMenu(fileName = "SRAssetData", menuName = "ScriptableObjects/SRAssetData")]
public class SRAssetData : ScriptableObject
{
    [SerializeField] GameObject _playerPrefab;
    public GameObject PlayerPrefab => _playerPrefab;
}
