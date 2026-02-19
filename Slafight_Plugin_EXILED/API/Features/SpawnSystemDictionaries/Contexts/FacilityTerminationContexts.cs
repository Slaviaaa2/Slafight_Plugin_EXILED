using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.MainHandlers;

namespace Slafight_Plugin_EXILED.API.Features.SpawnSystemDictionaries.Contexts;

public static class FacilityTerminationContexts
{
    public static void Register()
    {
        UnitPackRegistry.TryGet("FT_LastOperation", out var lastOpPack);
        UnitPackRegistry.TryGet("FT_GoC",          out var gocPack);
        UnitPackRegistry.TryGet("FT_Chaos",        out var chaosPack);

        var ctx = new SpawnContext(
            "FacilityTerminationCustom",
            // FoundationStaffWaveWeights
            new() 
            { 
                { SpawnTypeId.MTF_LastOperationNormal, 100 },
            },
            // FoundationEnemyWaveWeights
            new() 
            { 
                { SpawnTypeId.GOI_ChaosNormal, 40 },
                { SpawnTypeId.GOI_GoCNormal,   60 },
            },
            // FoundationStaffMiniWaveWeights
            new()
            {
                { SpawnTypeId.MTF_LastOperationBackup, 100 },
            },
            // FoundationEnemyMiniWaveWeights
            new()
            {
                { SpawnTypeId.GOI_ChaosBackup, 40 },
                { SpawnTypeId.GOI_GoCBackup,   60 },
            },
            lastOpPack,
            gocPack,
            chaosPack
        );

        SpawnContextRegistry.Register(ctx);
    }
}