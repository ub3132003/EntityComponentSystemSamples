using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;
using Unity.Entities;

public class CameraShakeSouce : MonoBehaviour
{
    [SerializeField] CinemachineImpulseSource impulseSource;
    [SerializeField] Vector3 Force;
    //lisent in
    [SerializeField] EntityChannelSO createExplodeEvent;
    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnEnable()
    {
        createExplodeEvent.OnEventRaised += Shake;
    }

    private void OnDisable()
    {
        createExplodeEvent.OnEventRaised -= Shake;
    }

    void Shake(Entity e)
    {
        impulseSource.GenerateImpulse(Force);
    }

    [Button]
    void Shark()
    {
        impulseSource.GenerateImpulse(Force);
    }
}
