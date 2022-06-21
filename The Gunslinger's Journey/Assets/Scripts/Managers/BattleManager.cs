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
    [SerializeField] private TextMeshProUGUI battleText;
    [SerializeField] private List<GameObject> turnOrder;
    [SerializeField] private Button attackButton;
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
    public float timer = 0.0f;
    public float buffer = 3.0f;
    int damage;
    int accuracy;
    int evasion;




    // Start is called before the first frame update
    void Start()
    {
        currentTurn = 0;
        turnOrder = new List<GameObject>();
        playerObject = Instantiate(playerPrefab);
        playerObject.transform.position = new Vector3(-5.0f, -1.0f);
        playerScript = playerObject.GetComponent<Player>();
        playerScript.SetUpCharacter();
        turnOrder.Add(playerObject);

        //Make the enemies
        allEnemies = new List<GameObject>();
        enemy = Instantiate(diamondPrefab);
        enemyScript = enemy.GetComponent<Enemy>();
        enemy.transform.position = new Vector3(1.0f, 1.0f);
        allEnemies.Add(enemy);

        attackButton.onClick.AddListener(() => SetPlayerAction(playerAction.Attack));

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

        playerScript.healthBar.value = (float)playerScript.GetHitPoints() / (float)playerScript.GetMaxHitPoints();
        foreach (GameObject foe in allEnemies)
        {
            Enemy enemyScript = foe.GetComponent<Enemy>();
            enemyScript.healthBar.value = (float)enemyScript.GetHitPoints() / (float)enemyScript.GetMaxHitPoints();

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (introTextSeen && !endConditionsMet)
        {
            turnOrder[currentTurn].GetComponent<Character>().SetTurn(true);
            if (playerScript.GetTurn())
            {
                battleText.text = "Your turn!";
                if (action != playerAction.Attack)
                    attackButton.interactable = true;
            }
            if (enemyScript.GetTurn() && !enemyScript.GetDead())
            {
                timer += Time.deltaTime;
                if(timer > 0 && timer < buffer)
                    battleText.text = "The enemy attacks!";
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
                    battleText.text = "The enemy dealt " + damage + " damage!";
                    if(criticalHit)
                        battleText.text += " A critical hit!!!";
                    if (damage == 0)
                        battleText.text += " You dodged the enemy's attack!";
                    if (timer >= buffer * 1.75)
                    {
                        playerScript.ModifyHealth(-damage);
                        if (!endConditionsMet)
                            NextTurn();
                        gotAttackRolls = false;
                        criticalHit = false;
                    }
                }
            }
            if (playerScript.GetTurn() && action == playerAction.Attack)
            {
                attackButton.interactable = false;
                timer += Time.deltaTime;
                if(timer > 0 && timer < buffer)
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
                    battleText.text = "You dealt " + damage + " damage!";
                    if (criticalHit)
                        battleText.text += " A critical hit!!!";
                    if (damage == 0)
                        battleText.text += " The enemy dodged your attack!";
                    if (timer >= buffer * 1.75)
                    {
                        enemyScript.ModifyHealth(-damage);
                        if (!endConditionsMet)
                            NextTurn();
                        action = playerAction.Unselected;
                        gotAttackRolls = false;
                        criticalHit = false;
                    }
                }
            }
            foreach (GameObject foe in allEnemies)
            {
                if (foe.GetComponent<Enemy>().GetDead())
                {
                    foe.SetActive(false);
                    allEnemies.Remove(foe);
                }
            }
        }
        else if(!introTextSeen && !endConditionsMet)
        {
            timer += Time.deltaTime;
            if (timer >= buffer)
            {
                introTextSeen = true;
                timer = 0.0f;
            }
        }
        IsBattleOver();

    }

    public void SetPlayerAction(playerAction act) { action = act; }
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



    }
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
            attackButton.interactable = false;
    }
    public void CriticalHitCheck()
    {
        float critChance;
        float critRoll;
        if (accuracy >= (evasion * 2.5))
            critChance = 1f;
        else if (accuracy >= (evasion * 2))
            critChance = 0.9f;
        else if (accuracy >= (evasion + (evasion / 1.5)))
            critChance = 0.75f;
        else if (accuracy >= (evasion + (evasion / 3)))
            critChance = 0.5f;
        else if (accuracy >= (evasion + (evasion / 5)))
            critChance = 0.25f;
        else
            critChance = 0.05f;
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
        else if (evasion >= (accuracy + (accuracy / 5)))
            dodgeChance = 0.25f;
        else
            dodgeChance = 0.05f;
        dodgeRoll = Random.Range(0f, 1.0f);
        dodgeRoll += dodgeChance;

        if (dodgeRoll >= 1.0f)
            damage = 0;
    }
}
