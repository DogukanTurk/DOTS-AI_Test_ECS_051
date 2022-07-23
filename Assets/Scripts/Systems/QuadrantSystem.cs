using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class QuadrantSystem : SystemBase
{
    /* ------------------------------------------ */

    public static NativeParallelMultiHashMap<int, QuadrantData> QuadrantMultiHashMap;

    /* ------------------------------------------ */

    private const int _quadrantYMultiplier = 1500;
    private const int _quadrantCellSize = 5;

    /* ------------------------------------------ */

    public static int GetPositionHashMapKey(float3 position)
    {
        return (int)(math.floor(position.x / _quadrantCellSize) +
                     (_quadrantYMultiplier * math.floor(position.z / _quadrantCellSize)));
    }

    /* ------------------------------------------ */

    protected override void OnCreate()
    {
        QuadrantMultiHashMap = new NativeParallelMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
    }

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(IdentityData));

        QuadrantMultiHashMap.Clear();

        if (entityQuery.CalculateEntityCount() > QuadrantMultiHashMap.Capacity)
            QuadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();

        SetQuadrantDataHashMapJob job = new SetQuadrantDataHashMapJob
        {
            QuadrantMultiHashMap = QuadrantMultiHashMap.AsParallelWriter()
        };

        Dependency = job.ScheduleParallel(Dependency);
        Dependency.Complete();
    }

    protected override void OnDestroy()
    {
        QuadrantMultiHashMap.Dispose();
    }

    /* ------------------------------------------ */

    [BurstCompile]
    public partial struct SetQuadrantDataHashMapJob : IJobEntity
    {
        /* ------------------------------------------ */

        public NativeParallelMultiHashMap<int, QuadrantData>.ParallelWriter QuadrantMultiHashMap;

        /* ------------------------------------------ */

        public void Execute(in Translation translation, in IdentityData identityData)
        {
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            QuadrantMultiHashMap.Add(hashMapKey,
                new QuadrantData { Identity = identityData, Position = translation.Value });
        }

        /* ------------------------------------------ */
    }

    /* ------------------------------------------ */
}