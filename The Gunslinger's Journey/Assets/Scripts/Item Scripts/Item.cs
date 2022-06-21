using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    [SerializeField] private int goldValue;
    [SerializeField] private bool questItem;


    public int GetGoldValue() { return goldValue; }
    public void SetGoldValue(int v) { goldValue = v; }
    public bool IsQuestItem() { return questItem; }
    public void SetQuestItem(bool quest) { questItem = quest; }
}
