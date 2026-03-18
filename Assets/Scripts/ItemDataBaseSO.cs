using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataBaseSO", menuName = "Inventory/ItemDataBaseSO")]
public class ItemDataBaseSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();

    //캐싱을 위한 Dictionary
    private Dictionary<int, ItemSO> itemsByID;      //아이템 ID로 검색할 수 있도록 추가
    private Dictionary<string, ItemSO> itemsByName; //아이템 이름으로도 검색할 수 있도록 추가

    public void Initialze()     //위에 선언만 했기 때문에 초기화 해주는 함수 추가
    {
        itemsByID = new Dictionary<int, ItemSO>();  
        itemsByName = new Dictionary<string, ItemSO>();

        foreach(var item in items)
        {
            itemsByID[item.id] = item;
            itemsByName[item.itemName] = item;
        }
    }

    //아이템 ID로 검색하는 함수 추가
    public ItemSO GetItemByID(int id)
    {
        if (itemsByID == null)      //캐싱이 되어있는지 확인
        {
            Initialze();
        }
        if(itemsByID.TryGetValue(id, out ItemSO item))  //id 값을 찾아서 ItenSO를 리턴
        {
            return item;
        }
        return null;    //아이템이 없는 경우 null 반환
    }

    //아이템 이름으로 검색하는 함수 추가
    public ItemSO GetItemByName(string name)
    {
        if(itemsByName == null) //캐싱이 되어있는지 확인
        {
            Initialze();
        }
        if (itemsByName.TryGetValue(name, out ItemSO item))  //name 값을 찾아서 ItenSO를 리턴
        {
            return item;
        }
        return null;    //아이템이 없는 경우 null 반환
    }

    //타입으로 아이템 필터링
    public List<ItemSO> GetItemByType(ItemType type)
    {
        return items.FindAll(item => item.itemType == type);   
    }
}
