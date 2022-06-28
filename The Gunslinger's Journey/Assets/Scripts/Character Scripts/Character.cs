using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    [SerializeField] protected int level;
    [SerializeField] protected int hitPoints, flowPoints;
    [SerializeField] protected int maxHitPoints, maxFlowPoints, powerSkillPoints, fluiditySkillPoints, accuracySkillPoints, resistanceSkillPoints, evasionSkillPoints, speedSkillPoints, charismaSkillPoints, defenseSkillPoints;
    [SerializeField] protected int swiftnessStat, gritStat, eloquenceStat, aptitudeStat, insightStat, vigorStat;
    [SerializeField] public Slider healthBar;
    [SerializeField] public Slider flowBar;
    protected bool myTurn = false;
    protected bool isDead = false;

    virtual public void SetUpCharacter()
    {
        float hp = (gritStat + ((float)vigorStat / 2) + level) * 8;
        maxHitPoints = (int)hp;

        float fp = (insightStat + ((float)vigorStat / 2) + level) * 7;
        maxFlowPoints = (int)fp;

        float pow = vigorStat + ((float)aptitudeStat / 2) + ((float)level / 3);
        powerSkillPoints = (int)pow;

        float flu = insightStat + ((float)vigorStat / 2) + ((float)level / 3);
        fluiditySkillPoints = (int)flu;

        float acc = aptitudeStat + ((float)insightStat / 2) + ((float)(level+vigorStat) / 4);
        accuracySkillPoints = (int)acc;

        float res = gritStat + ((float)(insightStat + level) / 2);
        resistanceSkillPoints = (int)res;

        float ev = swiftnessStat + ((float)eloquenceStat / 2) + ((float)(level+gritStat) / 4);
        evasionSkillPoints = (int)ev;

        float sp = swiftnessStat + ((float)eloquenceStat / 2) + ((float)level / 4);
        speedSkillPoints = (int)sp;

        float cha = eloquenceStat + ((float)(aptitudeStat + level) / 4);
        charismaSkillPoints = (int)cha;

        float def = gritStat + ((float)vigorStat / 2) + ((float)level / 3);
        defenseSkillPoints = (int)def;

        hitPoints = maxHitPoints;
        flowPoints = maxFlowPoints;
    }


    public int GetHitPoints() { return hitPoints; }
    public void SetHitPoints(int hp) { hitPoints = hp; }
    
    public int GetFlowPoints() { return flowPoints; }
    public void SetFlowPoints(int fp) { flowPoints = fp; }

    public int GetMaxHitPoints() { return maxHitPoints; }
    public void SetMaxHitPoints(int mHp) { maxHitPoints = mHp; }

    public int GetMaxFlowPoints() { return maxFlowPoints; }
    public void SetMaxFlowPoints(int mFp) { maxFlowPoints = mFp; }

    public int GetSwiftnessStat() { return swiftnessStat; }
    public void SetSwiftnessStat(int swift) { swiftnessStat = swift; }

    public int GetGritStat() { return gritStat; }
    public void SetGritStat(int grit) { gritStat = grit; }

    public int GetAptitudeStat() { return aptitudeStat; }
    public void SetAptitudeStat(int apt) { aptitudeStat = apt; }
    public int GetEloquenceStat() { return eloquenceStat; }
    public void SetEloquenceStat(int eloquence) { eloquenceStat = eloquence; }

    public int GetFlowStat() { return insightStat; }
    public void SetFlowStat(int insight) { insightStat = insight; }

    public int GetVigorStat() { return vigorStat; }
    public void SetVigorStat(int vigor) { vigorStat = vigor; }

    public int GetPowerSkill() { return powerSkillPoints; }
    public void SetPowerSkill(int power) { powerSkillPoints = power; }

    public int GetFluiditySkill() { return fluiditySkillPoints; }
    public void SetFluiditySkill(int fluid) { fluiditySkillPoints = fluid; }

    public int GetAccuracySkill() { return accuracySkillPoints; }
    public void SetAccuracySkill(int accuracy) { accuracySkillPoints = accuracy; }

    public int GetResistanceSkill() { return resistanceSkillPoints; }
    public void SetResistanceSkill(int res) { resistanceSkillPoints = res; }

    public int GetEvasionSkill() { return evasionSkillPoints; }
    public void SetEvasionSkill(int evasion) { evasionSkillPoints = evasion; }

    public int GetSpeedSkill() { return speedSkillPoints; }
    public void SetSpeedSkill(int speed) { speedSkillPoints = speed; }

    public int GetCharismaSkill() { return charismaSkillPoints; }
    public void SetCharismaSkill(int charisma) { charismaSkillPoints = charisma; }
    public int GetDefenseSkill() { return defenseSkillPoints; }
    public void SetDefenseSkill(int defense) { defenseSkillPoints = defense; }

    public bool GetTurn() { return myTurn; }
    public void SetTurn(bool turn) { myTurn = turn; }
    public bool GetDead() { return isDead; }
    public void SetDead(bool dead) { isDead = dead; }

    virtual public void ModifyHealth(int modifier)
    {
        hitPoints += modifier;
        if (hitPoints <= 0)
            isDead = true;
        else if (hitPoints > GetMaxHitPoints())
            hitPoints = GetMaxHitPoints();
        healthBar.value = (float)GetHitPoints() / GetMaxHitPoints();
    }
    virtual public void ModifyFlow(int modifier)
    {
        flowPoints += modifier;
        if (flowPoints < 0)
            flowPoints = 0;
        else if (flowPoints > GetMaxFlowPoints())
            flowPoints = GetMaxFlowPoints();
        flowBar.value = (float)GetFlowPoints() / GetMaxFlowPoints();
    }
    public void GetEvasionRoll(out int evasion)
    {
        evasion = Random.Range(evasionSkillPoints - (evasionSkillPoints / 5), evasionSkillPoints + (evasionSkillPoints / 5));
    }
}
