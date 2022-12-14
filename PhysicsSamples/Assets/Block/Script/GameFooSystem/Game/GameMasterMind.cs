using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameMasterMind : MonoBehaviour
{
    /// <summary>
    /// 波次相关
    /// </summary>
    ///
    [Serializable]
    public class GameWave
    {
        /// <summary>
        /// nextwave 的回调
        /// </summary>
        public UnityAction<int> NextWaveCallBack;
        /// <summary>
        /// 第几波
        /// </summary>
        int waveNum;

        /// <summary>
        /// 下拨开始时间
        /// </summary>
        float nextWaveTime = Time.time;
        /// <summary>
        /// 推进游戏的时间间隔
        /// </summary>
        ///
        [SerializeField]
        float waveTimeInterval;
        public float WaveTime { get => waveTimeInterval; set => waveTimeInterval = value; }
        public GameWave(float waveTimeInterval)
        {
            this.waveTimeInterval = waveTimeInterval;
            this.nextWaveTime = Time.time;
        }

        public void NextWave()
        {
            waveNum++;
            nextWaveTime += waveTimeInterval;
            NextWaveCallBack?.Invoke(1);
        }

        public bool IsNextWaveComing()
        {
            return Time.time >= nextWaveTime;
        }

        public float GetWaveProess()
        {
            return (nextWaveTime - Time.time) / waveTimeInterval;
        }
    }
    [SerializeField]
    GameWave wave;
    //boradcast on
    [SerializeField] FloatEventChannelSO UpdateWaveEvent;
    [SerializeField] IntEventChannelSO WaveStartEvnet;
    void Start()
    {
        wave.NextWaveCallBack = WaveStartEvnet.RaiseEvent;
        wave.NextWave();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateWaveEvent.RaiseEvent(wave.GetWaveProess());
        if (wave.IsNextWaveComing())
        {
            wave.NextWave();
        }
    }
}
