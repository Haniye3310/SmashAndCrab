using System;
using UnityEngine;

public class CTSceneData : MonoBehaviour
{
    [SerializeField] UIData _uiData;
    public UIData UIData => _uiData;

    [SerializeField] CTAssetData _assetData;
    public CTAssetData AssetData => _assetData;
}
[Serializable]
public class UIData
{
    [SerializeField] FixedJoystick _joystick;
    public FixedJoystick Joystick => _joystick;
}