using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[GenerateAuthoringComponent]
public struct MoveData : IComponentData
{
    /* ------------------------------------------ */

    public float MoveSpeed;

    public Random RandomValue;

    /* ------------------------------------------ */
}