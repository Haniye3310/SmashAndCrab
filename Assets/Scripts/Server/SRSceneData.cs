using UnityEngine;

public class SRSceneData : MonoBehaviour 
{
    [SerializeField] SRAssetData _assetData;
    public SRAssetData SRAssetData => _assetData;
}
