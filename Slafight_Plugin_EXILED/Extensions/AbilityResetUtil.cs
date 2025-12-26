using Slafight_Plugin_EXILED.API.Features;

namespace Slafight_Plugin_EXILED.Extensions;

// どこか共通のUtilityに置く or Plugin.cs から直接呼ぶ用

public static class AbilityResetUtil
{
    // 全アビリティ状態＋ロードアウトを全消し
    public static void ResetAllAbilities()
    {
        AbilityBase.RevokeAllPlayers();   // AbilityBase.playerStates を全クリア
        AbilityManager.ClearAllLoadouts(); // 全プレイヤーの AbilityLoadout 削除
    }
}
