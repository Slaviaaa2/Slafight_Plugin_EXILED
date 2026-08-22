using System;
using System.Collections.Generic;
using PlayerRoles;
using Respawning.NamingRules;
using Slafight_Plugin_EXILED.API.Core.Features;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.ForceSystem;

/// <summary>
/// 隊の名前を作ります。
/// </summary>
/// <remarks>
/// <b>バニラが表示している部隊名と衝突させないための層です。</b>
///
/// <see cref="Team.FoundationForces"/> の <c>UsesUnitNames</c> は true 固定なので、
/// NTF の名札にはバニラの <c>(ALPHA-01)</c> が必ず出ます。これは避けられません。
/// そこで分隊名と非 NTF 部隊名を作るときは、
/// <b>バニラが実際に配った名前を突き合わせて、空いている組み合わせだけを使います</b>。
///
/// 生成数を offset にする方式は使いません。バニラの
/// <see cref="NineTailedFoxNamingRule.GenerateNew"/> は NATO コードも番号も
/// <c>Random.Range</c> で選ぶので、数から名前を決定的に計算しても
/// 実際に使われた名前とは何の関係もなく、衝突を避けたことになりません。
/// </remarks>
public static class ForceNaming
{
    /// <summary>
    /// バニラと同じ NATO コードです。見た目を揃えるために同じ表を使います。
    /// </summary>
    private static readonly string[] Codes = NineTailedFoxNamingRule.PossibleCodes;

    /// <summary>
    /// バニラの採番が使えないときに使う連番です。
    /// </summary>
    private static int fallbackIndex;

    /// <summary>
    /// 既にこのラウンドで配った名前です。同じ名前を 2 度出さないために持ちます。
    /// </summary>
    private static readonly HashSet<string> Issued = [];

    /// <summary>
    /// バニラが採番した部隊名をそのまま採用します。名前が無ければこちらで作ります。
    /// </summary>
    /// <remarks>
    /// NTF の本隊用です。番号と名前は <c>SpawnSystem</c> が波を出すときに確保済みなので、
    /// ここでは<b>受け取るだけ</b>で新しく採番しません。
    /// 二重に採番すると名札の <c>(ALPHA-01)</c> と食い違います。
    /// </remarks>
    public static string Adopt(string vanillaName)
    {
        if (string.IsNullOrEmpty(vanillaName))
            return IssueLocalName();

        Issued.Add(vanillaName);

        return vanillaName;
    }

    /// <summary>
    /// バニラの採番を消費せずに、こちら側だけで名前を作ります。
    /// </summary>
    /// <remarks>
    /// 分隊と非 NTF の隊に使います。
    ///
    /// <b>「バニラの生成数を offset にする」方式はやめました。</b>
    /// バニラの <see cref="NineTailedFoxNamingRule.GenerateNew"/> は
    /// NATO コードも番号も <c>Random.Range</c> で選ぶので、
    /// 生成数から名前を決定的に計算しても、実際に使われた名前とは何の関係もありません。
    /// 数を進めても衝突を避けたことにならないのです。
    ///
    /// 代わりに<b>バニラが実際に配った名前を突き合わせて</b>、
    /// 空いている組み合わせだけを返します。これなら確実に重なりません。
    /// </remarks>
    public static string IssueLocalName()
    {
        SyncVanillaNames();

        // バニラと同じ体系からランダムに選ぶ。見た目を揃えるため。
        for (int attempt = 0; attempt < RandomAttempts; attempt++)
        {
            string candidate = Compose(
                Random.Range(0, Codes.Length),
                Random.Range(1, MaxUnitNumber + 1));

            if (Issued.Add(candidate))
                return candidate;
        }

        // 運が悪かっただけかもしれないので、最後は総当たりで空きを探す。
        for (int nato = 0; nato < Codes.Length; nato++)
        {
            for (int number = 1; number <= MaxUnitNumber; number++)
            {
                string candidate = Compose(nato, number);

                if (Issued.Add(candidate))
                    return candidate;
            }
        }

        // 表を使い切った。ここまで来ることはまず無いが、名前を空にはしない。
        fallbackIndex++;

        return $"UNIT-{fallbackIndex:00}";
    }

    /// <summary>
    /// バニラが既に配った部隊名を取り込みます。
    /// </summary>
    /// <remarks>
    /// <see cref="NamingRulesManager.GeneratedNames"/> はホストクライアントが
    /// <c>UnitNameMessage</c> を処理してから増えるので、採番直後は載っていないことがあります。
    /// そのぶんは <see cref="Adopt"/> 側が先に登録しているので取りこぼしません。
    /// </remarks>
    /// <summary>
    /// 名前をそのまま表示できる形にします。
    /// </summary>
    /// <remarks>
    /// タグの記号が入っているとリッチテキストとして解釈され、HUD と名札が崩れます。
    /// 名札はバニラの検証も通らなくなります。
    /// <see cref="ForceBase.Name"/> が自動で通すので、隊の側で呼ぶ必要はありません。
    /// </remarks>
    internal static string Sanitize(string name) =>
        string.IsNullOrEmpty(name) ? name : name.Replace("<", "＜").Replace(">", "＞");

    private static void SyncVanillaNames()
    {
        if (!NamingRulesManager.GeneratedNames.TryGetValue(Team.FoundationForces, out List<string> names))
            return;

        foreach (string name in names)
        {
            if (!string.IsNullOrEmpty(name))
                Issued.Add(name);
        }
    }

    /// <summary>
    /// ラウンドをまたいで名前を持ち越さないようにします。
    /// </summary>
    internal static void Reset()
    {
        Issued.Clear();
        fallbackIndex = 0;
    }

    /// <summary>
    /// バニラと同じ <c>ALPHA-01</c> 形式に組みます。
    /// </summary>
    private static string Compose(int nato, int number) => $"{Codes[nato]}-{number:00}";

    /// <summary>
    /// バニラの <c>NineTailedFoxNamingRule</c> が使う番号の上限です。
    /// </summary>
    private const int MaxUnitNumber = NineTailedFoxNamingRule.MaxUnitNumber;

    /// <summary>
    /// ランダムに空きを探す回数です。これを超えたら総当たりに切り替えます。
    /// </summary>
    private const int RandomAttempts = 32;
}
