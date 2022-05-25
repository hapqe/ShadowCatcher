using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShadowConfig", menuName = "ShadowConfig", order = 1)]
public class ShadowConfig : ScriptableObject
{
    public static ShadowConfig GetConfig() {
        ShadowConfig config = Resources.Load<ShadowConfig>("ShadowConfig");
        if(config == null) {
            config = CreateInstance<ShadowConfig>();
            config.name = "ShadowConfig";
        }
        return config;
    }
    public ComputeShader shadowShader;
    public Material positionMaterial;
    public LayerMask positionsMask;
}
