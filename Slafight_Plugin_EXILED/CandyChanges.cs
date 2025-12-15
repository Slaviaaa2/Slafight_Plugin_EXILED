using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Scp330;
using InventorySystem.Items.Usables.Scp330;

namespace Slafight_Plugin_EXILED;

public class CandyChanges
{
    public CandyChanges()
    {
        Exiled.Events.Handlers.Scp330.InteractingScp330 += CandyRoll;
    }

    ~CandyChanges()
    {
        Exiled.Events.Handlers.Scp330.InteractingScp330 -= CandyRoll;
    }

    public void CandyRoll(InteractingScp330EventArgs ev)
    {
        float Random = UnityEngine.Random.Range(1f, 100f);
        List<CandyKindID> RareCandies = new List<CandyKindID>()
        {
            CandyKindID.Black,
            CandyKindID.Brown,
            CandyKindID.Gray,
            CandyKindID.Orange,
            CandyKindID.White,
            CandyKindID.Evil
        };
        if (Random <= 0.1)
        {
            ev.Candy = CandyKindID.Pink;
        }
        else if (Random <= 0.22)
        {
            ev.Candy = RareCandies.RandomItem();
        }
    }

    public void SpecialCandiesEffect(EatenScp330EventArgs ev)
    {
        if (Plugin.Singleton.Config.Season != 1) // !IsHalloween
        {
            if (ev.Candy.Kind == CandyKindID.Black)
            {
                ev.Player.ShowHint("<size=26>※現在は効果がありません。提案してください</size>");
            }
            else if (ev.Candy.Kind == CandyKindID.Brown)
            {
                ev.Player.ShowHint("<size=26>※現在は効果がありません。提案してください</size>");
            }
            else if (ev.Candy.Kind == CandyKindID.Gray)
            {
                ev.Player.ShowHint("<size=26>※現在は効果がありません。提案してください</size>");
            }
            else if (ev.Candy.Kind == CandyKindID.Orange)
            {
                ev.Player.ShowHint("<size=26>※現在は効果がありません。提案してください</size>");
            }
            else if (ev.Candy.Kind == CandyKindID.White)
            {
                ev.Player.ShowHint("<size=26>※現在は効果がありません。提案してください</size>");
            }
            else if (ev.Candy.Kind == CandyKindID.Evil)
            {
                ev.Player.ShowHint("<size=26>※現在は効果がありません。提案してください</size>");
            }
        }
    }
}