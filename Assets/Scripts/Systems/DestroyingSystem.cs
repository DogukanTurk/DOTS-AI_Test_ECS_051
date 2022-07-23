using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(AttackingSystem))]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class DestroyingSystem : SystemBase
{
    /* ------------------------------------------ */

    BeginInitializationEntityCommandBufferSystem ecbSystem;

    /* ------------------------------------------ */

    protected override void OnCreate()
    {
        ecbSystem = World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter entityCommandBuffer = ecbSystem.CreateCommandBuffer().AsParallelWriter();

        DestroyingJob destroyingJob = new DestroyingJob()
        {
            entityCommandBuffer = entityCommandBuffer,
            ExplosionEntity = PrefabEntities.ExplosionEntity
        };
        Dependency = destroyingJob.ScheduleParallel(Dependency);

        ecbSystem.AddJobHandleForProducer(Dependency);
        Dependency.Complete();
    }

    /* ------------------------------------------ */

    [BurstCompile]
    public partial struct DestroyingJob : IJobEntity
    {
        /* ------------------------------------------ */

        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

        public Entity ExplosionEntity;

        /* ------------------------------------------ */

        public void Execute(Entity entity, [EntityInQueryIndex] int sortIndex, in StatsData statsData,
            in LocalToWorld localToWorld)
        {
            if (statsData.Health <= 0f)
            {
                entityCommandBuffer.DestroyEntity(sortIndex, entity);

                // var explosion = entityCommandBuffer.Instantiate(sortIndex, ExplosionEntity);
                // entityCommandBuffer.SetComponent(sortIndex, explosion, new Translation
                // {
                //     Value = localToWorld.Position
                // });
            }
        }

        /* ------------------------------------------ */
    }
}