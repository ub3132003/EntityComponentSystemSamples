using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SystemToggleManager : MonoBehaviour
{
    [SerializeField] bool RunTestPyhsicsSyt;
    void Start()
    {
        var testSys = World.DefaultGameObjectInjectionWorld.GetExistingSystem<TestPhysicsEventSystem>();

        testSys.Enabled = RunTestPyhsicsSyt;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
