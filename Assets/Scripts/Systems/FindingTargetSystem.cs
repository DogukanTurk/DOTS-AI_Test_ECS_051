using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(QuadrantSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class FindingTargetSystem : SystemBase
{
    /* ------------------------------------------ */

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    /* ------------------------------------------ */

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter entityCommandBuffer =
            endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();


        RemovingTagJob removingTagJob = new RemovingTagJob()
        {
            entityCommandBuffer = entityCommandBuffer
        };
        Dependency = removingTagJob.ScheduleParallel(Dependency);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        Dependency.Complete();


        FindingTargetJob findingTargetJob = new FindingTargetJob()
        {
            entityCommandBuffer = entityCommandBuffer,

            QuadrantMultiHashMap = QuadrantSystem.QuadrantMultiHashMap
        };

        Dependency = findingTargetJob.ScheduleParallel(Dependency);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        Dependency.Complete();
    }

    /* ------------------------------------------ */

    [BurstCompile]
    private partial struct FindingTargetJob : IJobEntity
    {
        /* ------------------------------------------ */

        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

        [ReadOnly] public NativeParallelMultiHashMap<int, QuadrantData> QuadrantMultiHashMap;

        /* ------------------------------------------ */

        public void Execute(Entity entity, [EntityInQueryIndex] int sortIndex, in Translation translation,
            in IdentityData identityData, in GunData gunData)
        {
            float targetEntityDistance = float.MaxValue;
            float tmpDistance = 0f;

            int hashMapKey = QuadrantSystem.GetPositionHashMapKey(translation.Value);
            
            EnemyTargetData targetEntity = new EnemyTargetData();

            if (QuadrantMultiHashMap.TryGetFirstValue(hashMapKey, out QuadrantData quadrantData,
                    out NativeParallelMultiHashMapIterator<int> nativeMultiHashMapIterator))
            {
                do
                {
                    if (identityData.Team != quadrantData.Identity.Team &&
                        identityData.Entity != quadrantData.Identity.Entity)
                    {
                        tmpDistance = math.distance(translation.Value, quadrantData.Position);

                        if (tmpDistance <= gunData.Range)
                        {
                            if (targetEntity.Target.Identity.Entity == Entity.Null)
                                targetEntity.Target = quadrantData;
                            else
                            {
                                if (targetEntityDistance > tmpDistance)
                                    targetEntity.Target = quadrantData;
                            }
                        }
                    }
                } while (QuadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));


                //If target found set it to the array
                if (targetEntity.Target.Identity.Entity != Entity.Null)
                {
                    targetEntity.Attacker = identityData;
                    entityCommandBuffer.AddComponent(sortIndex, entity,
                        new EnemyTargetData
                        {
                            Target = targetEntity.Target,
                            Attacker = targetEntity.Attacker
                        });
                }
            }
        }

        /* ------------------------------------------ */
    }

    [BurstCompile]
    private partial struct RemovingTagJob : IJobEntity
    {
        /* ------------------------------------------ */

        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

        /* ------------------------------------------ */

        public void Execute(Entity entity, [EntityInQueryIndex] int sortIndex, in EnemyTargetData enemyTargetData)
        {
            entityCommandBuffer.RemoveComponent<EnemyTargetData>(sortIndex, entity);
        }

        /* ------------------------------------------ */
    }

    /* ------------------------------------------ */
}