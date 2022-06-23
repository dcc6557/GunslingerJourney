using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Character
{
    [SerializeField] GameObject weapon;
    [SerializeField] GameObject armor;
    [SerializeField] GameObject accessory;

    private Weapon weaponScript;

    public void Attack(out int totalDamage, out int accuracy)
    {
        weaponScript = weapon.GetComponent<Weapon>();

        accuracy = Random.Range(accuracySkillPoints - (accuracySkillPoints / 5), accuracySkillPoints + (accuracySkillPoints / 5));

        int baseDamage = weaponScript.GetBaseDamage() + powerSkillPoints;
        totalDamage = Random.Range(baseDamage - (baseDamage / 5), baseDamage + (baseDamage / 5));
    }
    public void FlowHeal(out int totalRecovery)
    {
        totalRecovery = Random.Range(GetFluiditySkill() - (GetFluiditySkill() / 5), GetFluiditySkill() + (GetFluiditySkill() / 5));
    }
    public void FlowAttack(out int totalDamage, out int accuracy)
    {
        weaponScript = weapon.GetComponent<Weapon>();

        accuracy = Random.Range(accuracySkillPoints - (accuracySkillPoints / 5), accuracySkillPoints + (accuracySkillPoints / 5));

        int baseDamage = weaponScript.GetBaseDamage() + fluiditySkillPoints;
        totalDamage = Random.Range(baseDamage - (baseDamage / 5), baseDamage + (baseDamage / 5));
    }
}


