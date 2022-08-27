using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;

public class CameraShakeSouce : MonoBehaviour
{
    [SerializeField] CinemachineImpulseSource impulseSource;
    [SerializeField] Vector3 Force;
    //lisent in
    [SerializeField] EntityChannelSO createExplodeEvent;
    private void OnEnable()
    {
        createExplodeEvent.OnEventRaised += (e) => Shark();
    }

    private void OnDisable()
    {
        createExplodeEvent.OnEventRaised -= (e) => Shark();
    }

    [Button]
    void Shark()
    {
        impulseSource.GenerateImpulse(Force);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
