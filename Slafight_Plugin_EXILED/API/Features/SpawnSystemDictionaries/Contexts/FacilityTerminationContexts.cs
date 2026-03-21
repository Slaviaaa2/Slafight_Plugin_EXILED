using Slafight_Plugin_EXILED.API.Enums;

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
                { SpawnTypeId.GOI_ChaosNormal, 30 },
                { SpawnTypeId.GOI_GoCNormal,   70 },
            },
            // FoundationStaffMiniWaveWeights
            new()
            {
                { SpawnTypeId.MTF_LastOperationBackup, 100 },
            },
            // FoundationEnemyMiniWaveWeights
            new()
            {
                { SpawnTypeId.GOI_ChaosBackup, 30 },
                { SpawnTypeId.GOI_GoCBackup,   70 },
            },
            lastOpPack,
            gocPack,
            chaosPack
        );

        SpawnContextRegistry.Register(ctx);
    }
}