using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armor : Item
{
    public class Weapon : Item
    {
        [SerializeField] private int baseDefense;
        [SerializeField] private string itemName;

        public int GetBaseDamage() { return baseDefense; }
    }
}
