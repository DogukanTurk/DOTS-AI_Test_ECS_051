using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("DOTS/IJobEntityBatch/Converter")]
public class Converter : MonoBehaviour, IConvertGameObjectToEntity
{
    /* ------------------------------------------ */

    public ShipBase ShipBase;

    /* ------------------------------------------ */

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new IdentityData { Entity = entity, Team = ShipBase.Team });
        dstManager.AddComponentData(entity, new GunData { Range = ShipBase.GunRange, Damage = ShipBase.GunDamage });
        dstManager.AddComponentData(entity, new MoveData { MoveSpeed = ShipBase.MoveSpeed });
        dstManager.AddComponentData(entity,
            new StatsData
                { Health = ShipBase.Health, Identity = new IdentityData { Entity = entity, Team = ShipBase.Team } });
        dstManager.AddComponentData(entity,
            new MovingTargetData
            {
                RandomValue = Unity.Mathematics.Random.CreateFromIndex((uint)UnityEngine.Random.Range(1, 9999999999))
            });
        dstManager.AddComponentData(entity, new EnemyTargetData() { });

        RenderMesh _newRenderMesh = new RenderMesh();
        _newRenderMesh.material = ShipBase.Team == 0 ? ShipBase.MatBlue : ShipBase.MatRed;
        _newRenderMesh.mesh = ShipBase.Mesh;
        _newRenderMesh.castShadows = ShadowCastingMode.Off;
        _newRenderMesh.receiveShadows = false;
        _newRenderMesh.needMotionVectorPass = true;
        _newRenderMesh.layerMask = 1;

        dstManager.SetSharedComponentData(entity, _newRenderMesh);
    }

    /* ------------------------------------------ */
}