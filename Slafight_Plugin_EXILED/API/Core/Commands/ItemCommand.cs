using System;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Core.Features;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.API.Core.Commands;

/// <summary>
/// カスタムアイテムを渡す、または足元に出します。
/// </summary>
public sealed class ItemCommand : CommandBase
{
    public override Type Parent => typeof(RootCommand);

    public override string Command => "item";

    public override string Usage => "item <クラス名> [対象] [drop]";

    public override string Description => "カスタムアイテムを渡します。drop を付けると足元に出します。";

    protected override bool OnExecute(out string response)
    {
        if (!TryGetArgument(0, out string name))
        {
            response = $"使い方: {Usage}\n{CoreCatalog.Names<CustomItem>()}";

            return false;
        }

        if (!CoreCatalog.TryResolve<CustomItem>(name, out Type itemType, out string failure))
        {
            response = failure;

            return false;
        }

        if (!TryGetPlayer(1, out Player target))
        {
            response = "対象が見つかりません。";

            return false;
        }

        bool drop = TryGetArgument(2, out string mode) &&
                    mode is "drop" or "spawn" or "floor";

        if (drop)
        {
            if (CustomItem.Spawn(itemType, target.Position) is not { } spawned)
            {
                response = $"{itemType.Name} を出せませんでした。";

                return false;
            }

            response = $"{target.Nickname} の足元に {spawned.Name} を出しました。";

            return true;
        }

        // 手持ちが一杯なら足元に落ちる。
        if (target.GiveOrDrop(itemType) is not { } given)
        {
            response = $"{itemType.Name} を渡せませんでした。";

            return false;
        }

        response = $"{target.Nickname} に {given.Name} を渡しました。";

        return true;
    }
}
