using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class MovementSystem : SystemBase
{
    /* ------------------------------------------ */

    protected override void OnUpdate()
    {
        var job = new MovementJob()
        {
            DeltaTime = Time.DeltaTime
        };
        
        Dependency = job.ScheduleParallel(Dependency);
        Dependency.Complete();
    }

    /* ------------------------------------------ */

    [BurstCompile]
    public partial struct MovementJob : IJobEntity
    {
        /* ------------------------------------------ */
        // Variables

        public float DeltaTime;

        /* ------------------------------------------ */
        // Functions

        public void Execute(ref Translation translation, ref MovingTargetData targetData, in MoveData moveData)
        {
            // If there is no target, get me a target.
            if (targetData.TargetPosition.Equals(float3.zero))
                targetData.TargetPosition =
                    new float3(translation.Value.x + targetData.RandomValue.NextFloat(-50f, 50f), 0,
                        translation.Value.z + targetData.RandomValue.NextFloat(-50f, 50f));

            // If I'm not there yet, keep moving
            if (math.distance(translation.Value, targetData.TargetPosition) > .1f)
                translation.Value += math.normalize(targetData.TargetPosition - translation.Value) *
                                     moveData.MoveSpeed * DeltaTime;
            else // If I'm already there or passed there, get me a new target
                targetData.TargetPosition =
                    new float3(translation.Value.x + targetData.RandomValue.NextFloat(-50f, 50f), 0,
                        translation.Value.z + targetData.RandomValue.NextFloat(-50f, 50f));
        }

        /* ------------------------------------------ */
    }

    /* ------------------------------------------ */
}