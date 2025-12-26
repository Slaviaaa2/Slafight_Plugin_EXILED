using Slafight_Plugin_EXILED.API.Features;
using Exiled.API.Features;

namespace Slafight_Plugin_EXILED.Extensions;

public static class PlayerAbilityExtensions
{
    // アビリティ追加（スロット制限付き）
    public static bool AddAbility<TAbility>(this Player player)
        where TAbility : AbilityBase, new()
    {
        var loadout = AbilityManager.GetLoadout(player);
        var ability = new TAbility();
        return loadout.AddAbility(ability);
    }

    // 直接インスタンス渡し版（必要なら）
    public static bool AddAbility(this Player player, AbilityBase ability)
    {
        var loadout = AbilityManager.GetLoadout(player);
        return loadout.AddAbility(ability);
    }

    // アビリティ削除（型指定）
    public static void RemoveAbility<TAbility>(this Player player)
        where TAbility : AbilityBase
    {
        var loadout = AbilityManager.GetLoadout(player);
        for (int i = 0; i < AbilityLoadout.MaxSlots; i++)
        {
            if (loadout.Slots[i] is TAbility)
                loadout.Slots[i] = null;
        }
    }

    // 全アビリティ削除
    public static void ClearAbilities(this Player player)
    {
        AbilityManager.ClearPlayer(player);
    }

    // 現在のアクティブアビリティ発動
    public static void UseActiveAbility(this Player player)
    {
        var loadout = AbilityManager.GetLoadout(player);
        loadout.ActiveAbility?.TryActivateFromInput(player);
    }

    // アクティブアビリティ切り替え
    public static void NextAbility(this Player player)
    {
        var loadout = AbilityManager.GetLoadout(player);
        loadout.CycleNext();
    }
}
