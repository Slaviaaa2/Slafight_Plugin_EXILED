using Slafight_Plugin_EXILED.API.Features;
using Exiled.API.Features;

namespace Slafight_Plugin_EXILED.Extensions;

public static class PlayerAbilityExtensions
{
    // アビリティ追加（スロット制限付き）
    public static bool AddAbility<TAbility>(this Player player)
        where TAbility : AbilityBase, new()
    {
        Log.Debug($"[Ability] Add {typeof(TAbility).Name} to {player.Nickname} ({player.Role.Type})");
        var loadout = AbilityManager.GetOrCreateLoadout(player);
        var ability = new TAbility();
        return loadout.AddAbility(ability);
    }

    // 直接インスタンス渡し版
    public static bool AddAbility(this Player player, AbilityBase ability)
    {
        var loadout = AbilityManager.GetOrCreateLoadout(player);
        return loadout.AddAbility(ability);
    }

    // アビリティ削除（型指定）
    public static void RemoveAbility<TAbility>(this Player player)
        where TAbility : AbilityBase
    {
        if (!AbilityManager.TryGetLoadout(player, out var loadout))
            return;

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
        if (!AbilityManager.TryGetLoadout(player, out var loadout))
            return;

        loadout.ActiveAbility?.TryActivateFromInput(player);
    }

    // アクティブアビリティ切り替え
    public static void NextAbility(this Player player)
    {
        if (!AbilityManager.TryGetLoadout(player, out var loadout))
            return;

        loadout.CycleNext();
    }
}