using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    [SerializeField] int attackDamage;
    [SerializeField] bool isBoss;

    public void Attack(out int totalDamage, out int accuracy)
    {
        accuracy = Random.Range(accuracySkillPoints - (accuracySkillPoints / 5), accuracySkillPoints + (accuracySkillPoints / 5));

        int baseDamage = attackDamage + powerSkillPoints;
        totalDamage = Random.Range(baseDamage - (baseDamage / 5), baseDamage + (baseDamage / 5));
    }
    public void FlowHeal(int baseHeal, out int totalRecovery)
    {
        baseHeal += fluiditySkillPoints;
        totalRecovery = Random.Range(baseHeal - (baseHeal / 5), baseHeal + (baseHeal / 5));
        //ModifyFlow(-flowCost);
    }
    public void FlowAttack(int baseDamage, int flowCost, out int totalDamage, out int accuracy)
    {
        baseDamage += fluiditySkillPoints;

        accuracy = Random.Range(accuracySkillPoints - (accuracySkillPoints / 5), accuracySkillPoints + (accuracySkillPoints / 5));

        totalDamage = Random.Range(baseDamage - (baseDamage / 5), baseDamage + (baseDamage / 5));
        //ModifyFlow(-flowCost);
    }
}
