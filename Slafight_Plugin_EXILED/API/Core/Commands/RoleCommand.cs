using System;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Commands;

/// <summary>
/// カスタム役職を付与・解除します。
/// </summary>
public sealed class RoleCommand : CommandBase
{
    public override Type Parent => typeof(RootCommand);

    public override string Command => "role";

    public override string Usage => "role <クラス名|clear> [対象]";

    public override string Description => "カスタム役職を付与、または解除します。";

    protected override bool OnExecute(out string response)
    {
        if (!TryGetArgument(0, out string name))
        {
            response = $"使い方: {Usage}\n{CoreCatalog.Names<CustomRole>()}";

            return false;
        }

        if (!TryGetPlayer(1, out Player target))
        {
            response = "対象が見つかりません。";

            return false;
        }

        if (string.Equals(name, "clear", StringComparison.OrdinalIgnoreCase))
        {
            if (CustomRole.Of(target) is not { } current)
            {
                response = $"{target.Nickname} はカスタム役職を持っていません。";

                return false;
            }

            CustomRole.Remove(target);
            response = $"{target.Nickname} の {current.Name} を解除しました。";

            return true;
        }

        if (!CoreCatalog.TryResolve<CustomRole>(name, out Type roleType, out string failure))
        {
            response = failure;

            return false;
        }

        // 付与できなかった理由は役職側が返す。ここで握り潰さない。
        CustomRole role = (CustomRole)Activator.CreateInstance(roleType);

        if (!role.Spawn(target, out string reason))
        {
            response = reason ?? $"{roleType.Name} を付与できませんでした。";

            return false;
        }

        response = $"{target.Nickname} を {role.Name} にしました。";

        return true;
    }
}
