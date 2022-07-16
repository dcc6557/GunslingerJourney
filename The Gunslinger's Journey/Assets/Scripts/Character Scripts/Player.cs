using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum flowMove { Unselected, Attack, Heal }

public class Player : Character
{
    [SerializeField] GameObject weapon;
    [SerializeField] GameObject armor;
    [SerializeField] GameObject accessory;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI flowText;
    [SerializeField] public flowMove flowMove = flowMove.Unselected;

    private Weapon weaponScript;

    public override void SetUpCharacter()
    {
        base.SetUpCharacter();
        healthText.text = GetHitPoints() + " / " + GetMaxHitPoints();
        flowText.text = GetFlowPoints() + " / " + GetMaxFlowPoints();

    }
    public void Attack(out int totalDamage, out int accuracy)
    {
        weaponScript = GetWeapon();

        accuracy = Random.Range(accuracySkillPoints - (accuracySkillPoints / 5), accuracySkillPoints + (accuracySkillPoints / 5));

        int baseDamage = weaponScript.GetBaseDamage() + powerSkillPoints;
        totalDamage = Random.Range(baseDamage - (baseDamage / 5), baseDamage + (baseDamage / 5));
    }
    public void FlowHeal(int baseHeal, out int totalRecovery)
    {
        flowMove = flowMove.Heal;
        baseHeal += fluiditySkillPoints;
        totalRecovery = Random.Range(baseHeal - (baseHeal / 5), baseHeal + (baseHeal / 5));
    }
    public void FlowAttack(int baseDamage, out int totalDamage, out int accuracy)
    {
        flowMove = flowMove.Attack;
        weaponScript = weapon.GetComponent<Weapon>();

        baseDamage += fluiditySkillPoints;

        accuracy = Random.Range(accuracySkillPoints - (accuracySkillPoints / 5), accuracySkillPoints + (accuracySkillPoints / 5));

        totalDamage = Random.Range(baseDamage - (baseDamage / 5), baseDamage + (baseDamage / 5));
    }

    public override void ModifyHealth(int modifier)
    {
        base.ModifyHealth(modifier);
        healthText.text = GetHitPoints() + " / " + GetMaxHitPoints();
    }
    public override void ModifyFlow(int modifier)
    {
        base.ModifyFlow(modifier);
        flowText.text = GetFlowPoints() + " / " + GetMaxFlowPoints();
    }
    public Weapon GetWeapon() { return weapon.GetComponent<Weapon>(); }

    public void SetFlowMove(flowMove flow) { flowMove = flow; }
}


