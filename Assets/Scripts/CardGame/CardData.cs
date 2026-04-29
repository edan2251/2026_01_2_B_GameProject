using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card/Card Data")]
public class CardData : ScriptableObject
{
    public enum CardType
    {
        Attack,
        Heal,
        Buff,
        Utility
    }

    public string cardName;
    public string description;
    public Sprite artwork;
    public int manaCost;
    public int effectAmount;
    public CardType cardType;

    //추가 효과 리스트
    public List<AdditionalEffect> additionalEffects = new List<AdditionalEffect>();

    public enum AdditionalEffectType
    {
        None,                   //추가 효과 없음
        DrawCard,               //카드 드로우
        DiscardCard,            //카드 버리기
        GainMana,               //마나 획득
        ReduceEnemyMana,        //적 마나 감소
        ReduceCardCost          //다음 카드 비용 감소
    }

    public Color GetCardColor()
    {
        switch(cardType)
        {
            case CardType.Attack:
                return new Color(0.9f, 0.3f, 0.3f);

            case CardType.Heal:
                return new Color(0.3f, 0.9f, 0.3f);


            case CardType.Buff:
                return new Color(0.9f, 0.3f, 0.9f);


            case CardType.Utility:
                return new Color(0.9f, 0.9f, 0.3f);
            
            default:
                return Color.white;
        }
    }

    //추가 효과 정보를 문자열로 변환
    public string GetAdditionalEffectDescription()
    {
        if (additionalEffects.Count == 0)
            return "";

        string result = "\n";

        foreach(var effect in additionalEffects)
        {
            result += effect.GetDescription() + "\n";
        }

        return result;
    }
}
