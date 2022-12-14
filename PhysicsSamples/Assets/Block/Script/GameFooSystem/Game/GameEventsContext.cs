using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventsContext : Singleton<GameEventsContext>
{
    [Header("listening in")]
    [SerializeField] IntEventChannelSO WaveStartEvnet;
    public bool TriggleWaveStart;

    private void OnEnable()
    {
        WaveStartEvnet.OnEventRaised += OnWaveStart;
    }

    public void OnWaveStart(int waveStat)
    {
        if (waveStat == 1)
        {
            TriggleWaveStart = true;
        }
    }
}
