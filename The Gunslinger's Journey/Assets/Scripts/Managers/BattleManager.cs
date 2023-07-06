using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public enum playerAction { Unselected, Attack, Block, Flow }

public class BattleManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject diamondPrefab;
    [SerializeField] private Button targetButtonPrefab;
    [SerializeField] private Canvas BattleUI;
    [SerializeField] private Image flowPanel;
    [SerializeField] private Image infoBackground;
    [SerializeField] private TextMeshProUGUI battleText;
    [SerializeField] private TextMeshProUGUI weaponName;
    [SerializeField] private List<GameObject> turnOrder;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button flowButton;
    [SerializeField] private Button flowHeal;
    [SerializeField] private Button flowAttack;
    [SerializeField] private Button cancelFlow;
    [SerializeField] private Button cancelTarget;
    private OverworldManager overworldScript;
    bool endConditionsMet = false;
    bool gotAttackRolls = false;
    bool criticalHit = false;
    bool targetSelected = false;
    public int currentTurn;
    playerAction action = playerAction.Unselected;
    bool introTextSeen = false;
    private Player playerScript;
    private List<GameObject> allEnemies;
    private List<Button> allEnemyTargets;
    GameObject playerObject;
    GameObject target;
    bool gotHealChance = false;
    int damage;
    int accuracy;
    int evasion;

    void Start()
    {
        currentTurn = 0;
        turnOrder = new List<GameObject>();
        playerObject = Instantiate(playerPrefab);
        playerObject.transform.position = new Vector3(-3.75f, -0.5f);
        playerScript = playerObject.GetComponent<Player>();
        playerScript.SetUpCharacter(PlayerStats.Health, PlayerStats.Flow);
        turnOrder.Add(playerObject);

        //Make the enemies
        allEnemies = new List<GameObject>();
        allEnemyTargets = new List<Button>();

        //AddEnemyToList(diamondPrefab, new Vector3(2.5f, 1.75f), "DiaMan", targetButtonPrefab);

        AddEnemyToList(diamondPrefab, new Vector3(2.1f, -1.0f), targetButtonPrefab);

        for (int x = 0; x < allEnemies.Count; x++)
        {
            GameObject thisEnemy = allEnemies[x];
            Button thisButton = allEnemyTargets[x];
            thisButton.transform.SetParent(infoBackground.transform);
            thisButton.transform.localScale = Vector3.one;

            thisButton.GetComponentInChildren<TextMeshProUGUI>().text = thisEnemy.name;
            thisButton.transform.localPosition = new Vector3(-120 + (x * 120), 0);
            SetUpTargetButton(thisEnemy, thisButton);
        }
        

        attackButton.onClick.AddListener(() => SetPlayerAction(playerAction.Attack));
        flowButton.onClick.AddListener(() => SetPlayerAction(playerAction.Flow));
        cancelFlow.onClick.AddListener(() => SetPlayerAction(playerAction.Unselected));
        cancelFlow.onClick.AddListener(() => flowPanel.gameObject.SetActive(false));
        cancelTarget.onClick.AddListener(() => SetPlayerAction(playerAction.Unselected));
        cancelTarget.onClick.AddListener(() => HideTargets());
        cancelTarget.onClick.AddListener(() => playerScript.SetFlowMove(flowMove.Unselected));
        flowAttack.onClick.AddListener(() => playerScript.SetFlowMove(flowMove.Attack));
        flowHeal.onClick.AddListener(() => playerScript.SetFlowMove(flowMove.Heal));

        flowPanel.gameObject.SetActive(false);
        HideTargets();
        weaponName.text = playerScript.GetWeapon().name;

        //Insert all the enemies into the turn order
        foreach (GameObject foe in allEnemies)
        {
            bool turnPositionFound = false;
            int turnIndex = 0;
            while (!turnPositionFound)
            {
                Enemy enemyScript = foe.GetComponent<Enemy>();
                Character thisCharacterScript = turnOrder[turnIndex].GetComponent<Character>();
                enemyScript.SetUpCharacter();

                int thisCharacterSpeed = thisCharacterScript.GetSpeedSkill();
                int enemySpeed = enemyScript.GetSpeedSkill();

                if (thisCharacterSpeed < enemySpeed)
                {
                    turnOrder.Insert(turnIndex, foe);
                    turnPositionFound = true;
                }
                //In a speed tie, the player gets priority
                else if (thisCharacterSpeed == enemySpeed)
                {
                    //Since the Player is in first, the script will always put the enemies after the player if they tie.
                    turnOrder.Insert(turnIndex + 1, foe);
                    turnPositionFound = true;
                }
                else
                {
                    turnIndex++;
                    if (turnIndex == turnOrder.Count)
                    {
                        turnOrder.Add(foe);
                        turnPositionFound = true;
                    }
                }
            }
        }

        if (allEnemies.Count == 1)
            battleText.text = "You've encountered " + allEnemies.Count + " enemy!";
        else
            battleText.text = "You've encountered " + allEnemies.Count + " enemies!";

        playerScript.healthBar.value = (float)playerScript.GetHitPoints() / playerScript.GetMaxHitPoints();
        playerScript.flowBar.value = (float)playerScript.GetFlowPoints() / playerScript.GetMaxFlowPoints();
        foreach (GameObject foe in allEnemies)
        {
            Enemy enemyScript = foe.GetComponent<Enemy>();
            enemyScript.healthBar.value = (float)enemyScript.GetHitPoints() / enemyScript.GetMaxHitPoints();
        }
    }

    void Update()
    {
        IsBattleOver();
        if (introTextSeen && !endConditionsMet)
        {
            turnOrder[currentTurn].GetComponent<Character>().SetTurn(true);
            if (playerScript.GetTurn())
                PlayerTurn(action);
            else
                EnemyTurn(turnOrder[currentTurn]);
            for (int x = allEnemies.Count - 1; x >= 0; x--)
            {
                Enemy foe = allEnemies[x].GetComponent<Enemy>();
                if (foe.GetDead())
                {
                    allEnemies[x].SetActive(false);
                    allEnemies.RemoveAt(x);
                    //This next part is a bad way to do this and will need to be fixed later
                    allEnemyTargets[x].gameObject.SetActive(false);
                    allEnemyTargets.RemoveAt(x);
                }
            }
        }
        else if (!introTextSeen && !endConditionsMet)
        {
            BattleStats.Timer += Time.deltaTime;
            if (BattleStats.Timer >= BattleStats.Buffer)
            {
                introTextSeen = true;
                BattleStats.Timer = 0.0f;
            }
        }
    }

    public void SetPlayerAction(playerAction act) { action = act; }

    public void SetUpTargetButton(GameObject enemyObject, Button button) 
    { 
        button.onClick.AddListener(() => SetTarget(enemyObject));
        button.onClick.AddListener(() => { targetSelected = true; });
        button.onClick.AddListener(() => HideTargets());

    }
    public void AddEnemyToList(GameObject prefab, Vector3 position, Button buttonPrefab)
    {
        allEnemies.Add(Instantiate(prefab));
        int x = allEnemies.Count - 1;
        allEnemies[x].transform.position = position;
        allEnemies[x].GetComponent<Enemy>().SetUpEnemy();
        allEnemies[x].name = allEnemies[x].GetComponent<Enemy>().GetEnemyName();
        allEnemyTargets.Add(Instantiate(buttonPrefab));

    }

    public void SetTarget(GameObject tar) { target = tar; }
    /// <summary>
    /// Go to the character in the turn order
    /// </summary>
    public void NextTurn()
    {
        turnOrder[currentTurn].GetComponent<Character>().SetTurn(false);
        currentTurn++;
        if (currentTurn >= turnOrder.Count)
            currentTurn = 0;
        if (turnOrder[currentTurn].GetComponent<Character>().GetDead() && !endConditionsMet)            
            NextTurn();
        else
            turnOrder[currentTurn].GetComponent<Character>().SetTurn(true);
        BattleStats.Timer = 0.0f;
        action = playerAction.Unselected;
        playerScript.flowMove = flowMove.Unselected;
        gotAttackRolls = false;
        criticalHit = false;
        gotHealChance = false;
        targetSelected = false;
        for (int x = 0; x < allEnemies.Count; x++)
        {
            Enemy thisEnemyScript = allEnemies[x].GetComponent<Enemy>();
            if (!thisEnemyScript.healedLastTurn && thisEnemyScript.GetMaxHitPoints() - thisEnemyScript.GetHitPoints() != 0)
                thisEnemyScript.healChance = 0;
            else
                thisEnemyScript.healChance = -1;
        }
    }
    public void PlayerTurn(playerAction act)
    {
        if (act == playerAction.Unselected)
        {
            battleText.text = "Your turn!";
            attackButton.interactable = true;
            flowButton.interactable = true;
        }
        else if (act == playerAction.Attack)
        {
            if (!targetSelected)
                SelectEnemyTarget();
            else
                ProcessAttack();
        }
        else if (act == playerAction.Flow)
            FlowLoop();
    }

    public void SelectEnemyTarget()
    {
        attackButton.interactable = false;
        flowButton.interactable = false;
        battleText.text = "Target?";
        if (!cancelTarget.gameObject.activeSelf)
        {
            cancelTarget.gameObject.SetActive(true);
            for (int x = 0; x < allEnemyTargets.Count; x++)
            {
                allEnemyTargets[x].gameObject.SetActive(true);
            }
        }
    }

    public void HideTargets()
    {
        cancelTarget.gameObject.SetActive(false);
        for (int x = 0; x < allEnemyTargets.Count; x++)
        {
            allEnemyTargets[x].gameObject.SetActive(false);
        }
        if (flowPanel.gameObject.activeSelf)
            flowPanel.gameObject.SetActive(false);
    }

    public void ProcessAttack()
    {
        Enemy targetScript = target.GetComponent<Enemy>();
        BattleStats.Timer += Time.deltaTime;
        if (BattleStats.Timer > 0 && BattleStats.Timer < BattleStats.Buffer)
            battleText.text = "You attack!";
        else if (BattleStats.Timer >= BattleStats.Buffer)
        {
            if (!gotAttackRolls)
            {
                playerScript.Attack(out damage, out accuracy);
                targetScript.GetEvasionRoll(out evasion);
                if (evasion > accuracy)
                    DodgeCheck();
                else if (accuracy > evasion)
                    CriticalHitCheck();
                gotAttackRolls = true;
            }
            battleText.text = "You dealt " + damage + " damage!\nAccuracy Roll: " + accuracy + " Evasion Roll: " + evasion + "\n";
            if (criticalHit)
                battleText.text += " A critical hit!!!";
            if (damage == 0)
                battleText.text += target.name + " dodged your attack!";
            if (BattleStats.Timer >= BattleStats.Buffer * 1.75)
            {
                targetScript.ModifyHealth(-damage);
                if (!endConditionsMet)
                    NextTurn();
            }
        }
    }
    public void FlowLoop()
    {
        if (playerScript.flowMove == flowMove.Unselected)
        {
            attackButton.interactable = false;
            flowButton.interactable = false;
            flowAttack.interactable = true;
            flowHeal.interactable = true;
            cancelFlow.interactable = true;
        }
        else if (playerScript.flowMove == flowMove.Attack)
        {
            if (playerScript.GetFlowPoints() >= 9)
            {
                if (!targetSelected)
                {
                    if (flowAttack.IsInteractable())
                    {
                        flowAttack.interactable = false;
                        flowHeal.interactable = false;
                        cancelFlow.interactable = false;
                    }
                    SelectEnemyTarget();
                }
                else
                    ProcessFlowAttack();
            }
            else
            {
                flowAttack.interactable = false;
                flowHeal.interactable = false;
                cancelFlow.interactable = false;
                BattleStats.Timer += Time.deltaTime;
                if (BattleStats.Timer > 0 && BattleStats.Timer < BattleStats.Buffer)
                    battleText.text = "Not enough Flow!";
                else
                {
                    flowAttack.interactable = true;
                    flowHeal.interactable = true;
                    cancelFlow.interactable = true;
                    playerScript.SetFlowMove(flowMove.Unselected);
                    BattleStats.Timer = 0;
                }
            }
        }
        else if (playerScript.flowMove == flowMove.Heal)
        {
            flowAttack.interactable = false;
            flowHeal.interactable = false;
            cancelFlow.interactable = false;
            if (playerScript.GetFlowPoints() >= 12 && playerScript.GetHitPoints() < playerScript.GetMaxHitPoints())
            {
                ProcessFlowHeal();
                if(flowPanel.gameObject.activeSelf)
                    flowPanel.gameObject.SetActive(false);
            }
            else if (playerScript.GetFlowPoints() < 12)
            {
                BattleStats.Timer += Time.deltaTime;
                if (BattleStats.Timer > 0 && BattleStats.Timer < BattleStats.Buffer)
                    battleText.text = "Not enough Flow!";
                else
                {
                    flowAttack.interactable = true;
                    flowHeal.interactable = true;
                    cancelFlow.interactable = true;
                    playerScript.SetFlowMove(flowMove.Unselected);
                    BattleStats.Timer = 0;
                }
            }
            else if (playerScript.GetHitPoints() >= playerScript.GetMaxHitPoints())
            {
                BattleStats.Timer += Time.deltaTime;
                if (BattleStats.Timer > 0 && BattleStats.Timer < BattleStats.Buffer)
                    battleText.text = "Your health is already full!";
                else
                {
                    flowAttack.interactable = true;
                    flowHeal.interactable = true;
                    cancelFlow.interactable = true;
                    playerScript.SetFlowMove(flowMove.Unselected);
                    BattleStats.Timer = 0;
                }
            }
        }
    }
    public void ProcessFlowAttack()
    {
        BattleStats.Timer += Time.deltaTime;
        Enemy targetScript = target.GetComponent<Enemy>();
        if (BattleStats.Timer > 0 && BattleStats.Timer < BattleStats.Buffer)
            battleText.text = "You fire your revolver!";
        else if (BattleStats.Timer >= BattleStats.Buffer)
        {
            if (!gotAttackRolls)
            {
                playerScript.FlowAttack(15, out damage, out accuracy);
                targetScript.GetEvasionRoll(out evasion);
                if (evasion > accuracy)
                    DodgeCheck();
                else if (accuracy > evasion)
                    CriticalHitCheck(true);
                gotAttackRolls = true;
            }
            battleText.text = "You dealt " + damage + " damage!\nAccuracy Roll: " + accuracy + " Evasion Roll: " + evasion + "\n";
            if (criticalHit)
                battleText.text += " A critical hit!!!";
            if (damage == 0)
                battleText.text += target.name + " dodged your attack!";
            if (BattleStats.Timer >= BattleStats.Buffer * 1.75)
            {
                targetScript.ModifyHealth(-damage);
                playerScript.ModifyFlow(-9);
                if (!endConditionsMet)
                    NextTurn();
            }
        }
    }
    public void ProcessFlowHeal()
    {
        BattleStats.Timer += Time.deltaTime;
        if (BattleStats.Timer > 0 && BattleStats.Timer < BattleStats.Buffer)
            battleText.text = "You heal yourself!";
        else if (BattleStats.Timer >= BattleStats.Buffer)
        {
            if (!gotAttackRolls)
            {
                playerScript.FlowHeal(9, out damage);
                if (playerScript.GetHitPoints() + damage > playerScript.GetMaxHitPoints())
                    damage = playerScript.GetMaxHitPoints() - playerScript.GetHitPoints();
                gotAttackRolls = true;
            }
            battleText.text = "You healed yourself for " + damage + " points!";
            if (BattleStats.Timer >= BattleStats.Buffer * 1.75)
            {
                playerScript.ModifyHealth(damage);
                playerScript.ModifyFlow(-12);
                if (!endConditionsMet)
                    NextTurn();
            }
        }
    }

    public void EnemyTurn(GameObject enemy)
    {
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        float healthPercentage = (float)enemyScript.GetHitPoints() / enemyScript.GetMaxHitPoints();

        if (!gotHealChance)
        {
            if (!enemyScript.healedLastTurn && enemyScript.GetMaxHitPoints() - enemyScript.GetHitPoints() != 0)
            {
                if (healthPercentage == 1)
                    enemyScript.healChance = 0;
                else if (healthPercentage >= 0.75)
                    enemyScript.healChance = 0.1f;
                else if (healthPercentage >= 0.5)
                    enemyScript.healChance = 0.35f;
                else if (healthPercentage >= 0.25)
                    enemyScript.healChance = 0.55f;
                else
                    enemyScript.healChance = 0.85f;
                enemyScript.healChance += Random.Range(0.0f, 1.0f);
            }
            else
                enemyScript.healedLastTurn = false;
            gotHealChance = true;
        }

        if (enemyScript.healChance >= 1)
        {
            BattleStats.Timer += Time.deltaTime;
            if (BattleStats.Timer > 0 && BattleStats.Timer < BattleStats.Buffer)
                battleText.text = enemyScript.name + " heals themselves!\nHeal Chance: " + enemyScript.healChance;
            else if (BattleStats.Timer >= BattleStats.Buffer)
            {
                if (!gotAttackRolls)
                {
                    enemyScript.FlowHeal(8, out damage);
                    gotAttackRolls = true;
                    enemyScript.healedLastTurn = true;
                }
                battleText.text = enemyScript.name + " healed for " + damage + " points!";
                if (BattleStats.Timer >= BattleStats.Buffer * 1.75)
                {
                    enemyScript.ModifyHealth(damage);
                    if (!endConditionsMet)
                        NextTurn();
                }
            }
        }
        else
        {
            BattleStats.Timer += Time.deltaTime;
            if (BattleStats.Timer > 0 && BattleStats.Timer < BattleStats.Buffer)
                battleText.text = enemyScript.name + " attacks!\nHeal Chance: " + enemyScript.healChance;
            else if (BattleStats.Timer >= BattleStats.Buffer)
            {
                if (!gotAttackRolls)
                {
                    enemyScript.Attack(out damage, out accuracy);
                    playerScript.GetEvasionRoll(out evasion);
                    if (evasion > accuracy)
                        DodgeCheck();
                    else if (accuracy > evasion)
                        CriticalHitCheck();
                    gotAttackRolls = true;
                }
                battleText.text = enemyScript.name + " dealt " + damage + " damage!\nAccuracy Roll: " + accuracy + " Evasion Roll: " + evasion + "\n";
                if (criticalHit)
                    battleText.text += "A critical hit!!!";
                if (damage == 0)
                    battleText.text += "You dodged " + enemyScript.name + "'s attack!";
                if (BattleStats.Timer >= BattleStats.Buffer * 1.75)
                {
                    playerScript.ModifyHealth(-damage);
                    if (!endConditionsMet)
                        NextTurn();
                }
            }
        }
    }

    public void CriticalHitCheck(bool isFlow = false)
    {
        float critChance;
        float critRoll;
        if (isFlow)
        {
            if (accuracy >= (evasion * 2.5))
                critChance = 0.9f;
            else if (accuracy >= (evasion * 2))
                critChance = 0.7f;
            else if (accuracy >= (evasion + (evasion / 1.5)))
                critChance = 0.55f;
            else if (accuracy >= (evasion + (evasion / 3)))
                critChance = 0.25f;
            else if (accuracy >= (evasion + (evasion / 4)))
                critChance = 0.10f;
            else
                critChance = 0.025f;
        }
        else
        {
            if (accuracy >= (evasion * 2.5))
                critChance = 1f;
            else if (accuracy >= (evasion * 2))
                critChance = 0.9f;
            else if (accuracy >= (evasion + (evasion / 1.5)))
                critChance = 0.75f;
            else if (accuracy >= (evasion + (evasion / 3)))
                critChance = 0.5f;
            else if (accuracy >= (evasion + (evasion / 4)))
                critChance = 0.25f;
            else
                critChance = 0.05f;
        }
        critRoll = Random.Range(0f, 1.0f);
        critRoll += critChance;
        if (critRoll >= 1.0f)
        {
            damage *= 2;
            criticalHit = true;
        }
    }
    public void DodgeCheck()
    {
        float dodgeChance;
        float dodgeRoll;
        if (evasion >= (accuracy * 2.5))
            dodgeChance = 1f;
        else if (evasion >= (accuracy * 2))
            dodgeChance = 0.9f;
        else if (evasion >= (accuracy + (accuracy / 1.5)))
            dodgeChance = 0.75f;
        else if (evasion >= (accuracy + (accuracy / 3)))
            dodgeChance = 0.5f;
        else if (evasion >= (accuracy + (accuracy / 4)))
            dodgeChance = 0.25f;
        else
            dodgeChance = 0.05f;
        dodgeRoll = Random.Range(0f, 1.0f);
        dodgeRoll += dodgeChance;

        if (dodgeRoll >= 1.0f)
            damage = 0;
    }
    /// <summary>
    /// Check if the conditions have been met for the battle to be over.
    /// </summary>
    public void IsBattleOver()
    {
        if (playerScript.GetDead())
        {
            endConditionsMet = true;
            battleText.text = "You lose!";
        }
        else if (allEnemies.Count == 0)
        {
            BattleStats.Timer += Time.deltaTime;
            endConditionsMet = true;
            if (BattleStats.Timer > 0 && BattleStats.Timer < BattleStats.Buffer)
                battleText.text = "You win!";
            else if (BattleStats.Timer >= BattleStats.Buffer)
                ToOverworld();
        }
        if (endConditionsMet)
        {
            attackButton.interactable = false;
            flowButton.interactable = false;
        }
    }
    private void ToOverworld()
    {
        PlayerStats.Health = playerScript.GetHitPoints();
        PlayerStats.Flow = playerScript.GetFlowPoints();
        SceneManager.LoadScene(1);
        
    }
}

