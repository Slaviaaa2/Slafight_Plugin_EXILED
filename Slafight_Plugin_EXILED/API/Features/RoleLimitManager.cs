using System.Collections.Generic;
using System.Linq;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;

namespace Slafight_Plugin_EXILED.API.Features;

public static class RoleLimitManager
{
    private static readonly Dictionary<RoleTypeId, (int Current, int Max)> VanillaLimits = new();
    private static readonly Dictionary<CRoleTypeId, (int Current, int Max)> CustomLimits  = new();

    public static void Reset()
    {
        // Keys を配列にコピーしてから回す
        foreach (var key in VanillaLimits.Keys.ToArray())
        {
            var entry = VanillaLimits[key];
            VanillaLimits[key] = (0, entry.Max);
        }

        foreach (var key in CustomLimits.Keys.ToArray())
        {
            var entry = CustomLimits[key];
            CustomLimits[key] = (0, entry.Max);
        }
    }

    public static void ClearAll()
    {
        VanillaLimits.Clear();
        CustomLimits.Clear();
    }

    public static void SetLimit(RoleTypeId role, int max)
        => VanillaLimits[role] = (VanillaLimits.TryGetValue(role, out var v) ? v.Current : 0, max);

    public static void SetLimit(CRoleTypeId role, int max)
        => CustomLimits[role] = (CustomLimits.TryGetValue(role, out var v) ? v.Current : 0, max);

    // チェックのみ（カウントは進めない）
    public static bool CanAssign(object choice) =>
        choice switch
        {
            RoleTypeId r   => !VanillaLimits.TryGetValue(r, out var v) || v.Current < v.Max,
            CRoleTypeId cr => !CustomLimits.TryGetValue(cr, out var c) || c.Current < c.Max,
            _              => false,
        };

    // 実際に割り当てるときに呼び、カウントを+1する
    public static void Consume(object choice)
    {
        switch (choice)
        {
            case RoleTypeId r when VanillaLimits.TryGetValue(r, out var v):
                VanillaLimits[r] = (v.Current + 1, v.Max);
                break;

            case CRoleTypeId cr when CustomLimits.TryGetValue(cr, out var c):
                CustomLimits[cr] = (c.Current + 1, c.Max);
                break;
        }
    }
}