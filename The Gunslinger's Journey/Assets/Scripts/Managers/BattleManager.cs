using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public enum playerAction { Unselected, Attack, Block, Flow }

public class BattleManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject diamondPrefab;
    [SerializeField] private Canvas BattleUI;
    [SerializeField] private Image flowPanel;
    [SerializeField] private Image targetPanel;
    [SerializeField] private TextMeshProUGUI battleText;
    [SerializeField] private TextMeshProUGUI weaponName;
    [SerializeField] private List<GameObject> turnOrder;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button flowButton;
    [SerializeField] private Button flowHeal;
    [SerializeField] private Button flowAttack;
    [SerializeField] private Button cancelFlow;
    bool endConditionsMet = false;
    bool gotAttackRolls = false;
    bool criticalHit = false;
    public int currentTurn;
    playerAction action = playerAction.Unselected;
    bool introTextSeen = false;
    private Player playerScript;
    private Enemy enemyScript;
    private List<GameObject> allEnemies;
    GameObject playerObject;
    GameObject enemy;
    GameObject target;
    public float timer = 0.0f;
    public float buffer = 3.0f;
    float healChance = 0.0f;
    bool gotHealChance = false;
    bool healedLastTurn = false;
    int damage;
    int accuracy;
    int evasion;




    // Start is called before the first frame update
    void Start()
    {
        currentTurn = 0;
        turnOrder = new List<GameObject>();
        playerObject = Instantiate(playerPrefab);
        playerObject.transform.position = new Vector3(-75.0f, -10.0f);
        playerScript = playerObject.GetComponent<Player>();
        playerScript.SetUpCharacter();
        turnOrder.Add(playerObject);

        //Make the enemies
        allEnemies = new List<GameObject>();
        enemy = Instantiate(diamondPrefab);
        enemyScript = enemy.GetComponent<Enemy>();
        enemy.transform.position = new Vector3(50.0f, 25.0f);
        allEnemies.Add(enemy);

        attackButton.onClick.AddListener(() => SetPlayerAction(playerAction.Attack));
        flowButton.onClick.AddListener(() => SetPlayerAction(playerAction.Flow));
        cancelFlow.onClick.AddListener(() => SetPlayerAction(playerAction.Unselected));
        cancelFlow.onClick.AddListener(() => flowPanel.gameObject.SetActive(false));
        flowAttack.onClick.AddListener(() => playerScript.SetFlowMove(flowMove.Attack));
        flowHeal.onClick.AddListener(() => playerScript.SetFlowMove(flowMove.Heal));




        flowPanel.gameObject.SetActive(false);
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

    // Update is called once per frame
    void Update()
    {
        IsBattleOver();
        if (introTextSeen && !endConditionsMet)
        {
            turnOrder[currentTurn].GetComponent<Character>().SetTurn(true);
            if (playerScript.GetTurn())
                PlayerTurn(action);
            if (enemyScript.GetTurn() && !enemyScript.GetDead())
                EnemyTurn();
            for (int x = allEnemies.Count - 1; x >= 0; x--)
            {
                Enemy foe = allEnemies[x].GetComponent<Enemy>();
                if (foe.GetDead())
                {
                    allEnemies[x].SetActive(false);
                    allEnemies.RemoveAt(x);
                }
            }
        }
        else if (!introTextSeen && !endConditionsMet)
        {
            timer += Time.deltaTime;
            if (timer >= buffer)
            {
                introTextSeen = true;
                timer = 0.0f;
            }
        }
    }

    public void SetPlayerAction(playerAction act) { action = act; }

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
        timer = 0.0f;
        action = playerAction.Unselected;
        playerScript.flowMove = flowMove.Unselected;
        gotAttackRolls = false;
        criticalHit = false;
        gotHealChance = false;
        if (!healedLastTurn)
            healChance = 0;
        else
            healChance = -1;
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
            ProcessAttack();
        else if (act == playerAction.Flow)
            FlowLoop();
    }

    public void ProcessAttack()
    {
        attackButton.interactable = false;
        flowButton.interactable = false;
        timer += Time.deltaTime;
        if (timer > 0 && timer < buffer)
            battleText.text = "You attack!";
        else if (timer >= buffer)
        {
            if (!gotAttackRolls)
            {
                playerScript.Attack(out damage, out accuracy);
                enemyScript.GetEvasionRoll(out evasion);
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
                battleText.text += " The enemy dodged your attack!";
            if (timer >= buffer * 1.75)
            {
                enemyScript.ModifyHealth(-damage);
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
        }
        else if (playerScript.flowMove == flowMove.Attack)
        {
            if (playerScript.GetFlowPoints() >= 9)
                ProcessFlowAttack();
            else
            {
                timer += Time.deltaTime;
                if (timer > 0 && timer < buffer)
                    battleText.text = "Not enough Flow!";
                else
                {
                    playerScript.flowMove = flowMove.Unselected;
                    action = playerAction.Unselected;
                    timer = 0;
                }
            }
        }
        else if (playerScript.flowMove == flowMove.Heal)
        {
            if (playerScript.GetFlowPoints() >= 12 && playerScript.GetHitPoints() < playerScript.GetMaxHitPoints())
                ProcessFlowHeal();
            else if (playerScript.GetFlowPoints() < 12)
            {
                timer += Time.deltaTime;
                if (timer > 0 && timer < buffer)
                    battleText.text = "Not enough Flow!";
                else
                {
                    playerScript.flowMove = flowMove.Unselected;
                    action = playerAction.Unselected;
                    timer = 0;
                }
            }
            else if (playerScript.GetHitPoints() >= playerScript.GetMaxHitPoints())
            {
                timer += Time.deltaTime;
                if (timer > 0 && timer < buffer)
                    battleText.text = "Your health is already full!";
                else
                {
                    playerScript.flowMove = flowMove.Unselected;
                    action = playerAction.Unselected;
                    timer = 0;
                }
            }
        }
    }
    public void ProcessFlowAttack()
    {
        timer += Time.deltaTime;
        if (timer > 0 && timer < buffer)
            battleText.text = "You fire your revolver!";
        else if (timer >= buffer)
        {
            if (!gotAttackRolls)
            {
                playerScript.FlowAttack(15, 9, out damage, out accuracy);
                enemyScript.GetEvasionRoll(out evasion);
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
                battleText.text += " The enemy dodged your attack!";
            if (timer >= buffer * 1.75)
            {
                enemyScript.ModifyHealth(-damage);
                if (!endConditionsMet)
                    NextTurn();
            }
        }
    }
    public void ProcessFlowHeal()
    {
        timer += Time.deltaTime;
        if (timer > 0 && timer < buffer)
            battleText.text = "You heal yourself!";
        else if (timer >= buffer)
        {
            if (!gotAttackRolls)
            {
                playerScript.FlowHeal(12, 9, out damage);
                gotAttackRolls = true;
            }
            battleText.text = "You healed yourself for " + damage + " points!";
            if (timer >= buffer * 1.75)
            {
                playerScript.ModifyHealth(damage);
                if (!endConditionsMet)
                    NextTurn();
            }
        }
    }

    public void EnemyTurn()
    {
        float healthPercentage = (float)enemyScript.GetHitPoints() / enemyScript.GetMaxHitPoints();

        if (!gotHealChance)
        {
            if (!healedLastTurn)
            {
                if (healthPercentage == 1)
                    healChance = 0;
                else if (healthPercentage >= 0.75)
                    healChance = 0.1f;
                else if (healthPercentage >= 0.5)
                    healChance = 0.35f;
                else if (healthPercentage >= 0.25)
                    healChance = 0.55f;
                else
                    healChance = 0.85f;
                healChance += Random.Range(0.0f, 1.0f);
            }
            else
                healedLastTurn = false;
            gotHealChance = true;
        }

        if (healChance >= 1)
        {
            timer += Time.deltaTime;
            if (timer > 0 && timer < buffer)
                battleText.text = "The enemy heals themselves!\nHeal Chance: " + healChance;
            else if (timer >= buffer)
            {
                if (!gotAttackRolls)
                {
                    enemyScript.FlowHeal(8, out damage);
                    gotAttackRolls = true;
                    healedLastTurn = true;
                }
                battleText.text = "The enemy healed for " + damage + " points!";
                if (timer >= buffer * 1.75)
                {
                    enemyScript.ModifyHealth(damage);
                    if (!endConditionsMet)
                        NextTurn();
                }
            }
        }
        else
        {
            timer += Time.deltaTime;
            if (timer > 0 && timer < buffer)
                battleText.text = "The enemy attacks!\nHeal Chance: " + healChance;
            else if (timer >= buffer)
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
                battleText.text = "The enemy dealt " + damage + " damage!\nAccuracy Roll: " + accuracy + " Evasion Roll: " + evasion + "\n";
                if (criticalHit)
                    battleText.text += " A critical hit!!!";
                if (damage == 0)
                    battleText.text += " You dodged the enemy's attack!";
                if (timer >= buffer * 1.75)
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
        if (allEnemies.Count == 0)
        {
            endConditionsMet = true;
            battleText.text = "You win!";
        }
        if (playerScript.GetDead())
        {
            endConditionsMet = true;
            battleText.text = "You lose!";
        }
        if (endConditionsMet)
        {
            attackButton.interactable = false;
            flowButton.interactable = false;
        }
    }
}

