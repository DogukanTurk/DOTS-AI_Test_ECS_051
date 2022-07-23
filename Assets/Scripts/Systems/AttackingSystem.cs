using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class AttackingSystem : SystemBase
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
        var attackingJob = new AttackingJob()
        {
            entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),

            Time = (float)Time.ElapsedTime,

            StatsData = GetComponentDataFromEntity<StatsData>()
        };

        Dependency = attackingJob.ScheduleParallel(Dependency);
        Dependency.Complete();
    }

    /* ------------------------------------------ */

    [BurstCompile]
    public partial struct AttackingJob : IJobEntity
    {
        /* ------------------------------------------ */

        public float Time;

        /* ------------------------------------------ */

        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

        [NativeDisableContainerSafetyRestriction]
        public ComponentDataFromEntity<StatsData> StatsData;

        /* ------------------------------------------ */

        public void Execute(Entity entity, [EntityInQueryIndex] int sortIndex, ref GunData gunData,
            ref StatsData statsData, in EnemyTargetData enemyTargetData)
        {
            if (StatsData.HasComponent(enemyTargetData.Target.Identity.Entity))
            {
                StatsData statData = StatsData[enemyTargetData.Target.Identity.Entity];

                // Every 3/4 second, it can fire
                if (Time > gunData.NextFireTime)
                {
                    gunData.NextFireTime = Time + .75f;

                    statData.Health -= gunData.Damage;

                    if (enemyTargetData.Target.Identity.Entity != Entity.Null)
                        entityCommandBuffer.SetComponent(sortIndex, enemyTargetData.Target.Identity.Entity,
                            new StatsData { Health = statData.Health, Identity = statData.Identity });

                    if (statData.Health <= 0)
                        if (enemyTargetData.Attacker.Entity != Entity.Null)
                            entityCommandBuffer.RemoveComponent<EnemyTargetData>(sortIndex,
                                enemyTargetData.Attacker.Entity);
                }
            }
        }

        /* ------------------------------------------ */
    }

    /* ------------------------------------------ */
}