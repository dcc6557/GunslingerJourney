using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Enemy : Character
{
    [SerializeField] int attackDamage;
    [SerializeField] bool isBoss;
    [SerializeField] string enemyName;
    [SerializeField] TextMeshProUGUI enemyNameTMP;
    [SerializeField] bool canHeal;
    public float healChance = 0.0f;
    public bool healedLastTurn = false;

    public void SetUpEnemy()
    {
        enemyNameTMP.text = enemyName;
    }

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
    public void FlowAttack(int baseDamage, out int totalDamage, out int accuracy)
    {
        baseDamage += fluiditySkillPoints;

        accuracy = Random.Range(accuracySkillPoints - (accuracySkillPoints / 5), accuracySkillPoints + (accuracySkillPoints / 5));

        totalDamage = Random.Range(baseDamage - (baseDamage / 5), baseDamage + (baseDamage / 5));
        //ModifyFlow(-flowCost);
    }
    public void SetEnemyName(string name) { enemyName = name; enemyNameTMP.text = enemyName; }
    public string GetEnemyName() { return enemyName; }
    public bool CanHeal() { return canHeal; }
}
