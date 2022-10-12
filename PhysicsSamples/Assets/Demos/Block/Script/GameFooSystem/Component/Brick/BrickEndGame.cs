using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
/// <summary>
/// 此类方块如果碰到局外区域会导致游戏结束, 碰到局外区后地面掉落局外去前进到接触位置
/// </summary>
[Serializable]
[GenerateAuthoringComponent]
public struct BrickEndGame : IComponentData
{
}
