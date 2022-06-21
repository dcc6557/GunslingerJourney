using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    [SerializeField] int attackDamage;
    public void Attack(out int totalDamage, out int accuracy)
    {
        accuracy = Random.Range(accuracySkillPoints - (accuracySkillPoints / 5), accuracySkillPoints + (accuracySkillPoints / 5));

        int baseDamage = attackDamage + powerSkillPoints;
        totalDamage = Random.Range(baseDamage - (baseDamage / 5), baseDamage + (baseDamage / 5));
    }
}
