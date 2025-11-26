using UnityEngine;
using System.Linq;

public class DropTrash : MonoBehaviour, IDragDrop
{
    [HideInInspector] public int x = 100;
    [HideInInspector] public int y = 100;
    [HideInInspector] bool filled = false;


    void Awake()
    {

    }

    public int GetX() => x;
    public int GetY() => y;
    public bool getFilled() => filled;

    public void OnItemDrop(DragScript drop, int evoIndex)
    {
        drop.transform.position = transform.position;
        Destroy(drop.gameObject);
        ObjectManager.Instance.SummonCoinWithRandomOffset(gameObject, GetPreviousCost(SummonGUIManager.Instance.allCreatures[evoIndex]));
    }

    public int GetPreviousCost(CreatureData creature)
    {
        int index = creature.index;
        var dict = GameManager.Instance.creaturePurchaseCount;
        int baseCost = creature.cost;

        int timesBought = dict.ContainsKey(index) ? dict[index] : 0;

        float scaled;

        if (index == 0)
        {
            scaled = 10f;
            dict[index] -= 1;
            if(dict[index] < 0)
                dict[index] = 0;
        }
        else if (timesBought <= 0)
        {
            scaled = baseCost / 2f;
            dict[index] -= 1;
            if(dict[index] < 0)
                dict[index] = 0;
        }
        else
        {
            timesBought -= 1;
            dict[index] -= 1;
            scaled = baseCost *
                (1 + 0.5f * timesBought + 0.1f * timesBought * timesBought)
                / 2f;
        }

        return Mathf.RoundToInt(scaled);
    }




    public void OnItemLeave(DragScript drop)
    {
        filled = false;
    }
}
