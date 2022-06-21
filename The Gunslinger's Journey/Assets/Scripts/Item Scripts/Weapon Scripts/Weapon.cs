using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Store all the enums for the various weapons
public enum Damage { Bash, Pierce, Slash };
public enum Direction { Physical, Mental, Spirtual };
public enum PhysicalEffect { None, Bleeding, Shock, Burn, Freeze };
public enum MentalEffect { None, Sleep, Confusion, Silence };
public enum SpirutalEffect { None, Heal, Calm, Reinforce };

public class Weapon : Item
{
    [SerializeField] private Damage damageType;
    [SerializeField] private PhysicalEffect physEffect;
    [SerializeField] private MentalEffect mentEffect;
    [SerializeField] private SpirutalEffect spiritEffect;
    [SerializeField] private int baseDamage;
    [SerializeField] private string itemName;

    public int GetBaseDamage() { return baseDamage; }
}
