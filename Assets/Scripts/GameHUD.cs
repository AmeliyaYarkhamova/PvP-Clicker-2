using UnityEngine;
using TMPro;

public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance;
    
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private GameObject rulesPanel;
    [SerializeField] private TMP_Text playerStatusText;
    
    private float updateTimer = 0f;
    private float updateInterval = 0.5f;
    private int lastPlayerId = -2;
    private int lastNodeCount = -1;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        Debug.Log("GameHUD Start");
        
        FindUIElements();
        
        if (turnText != null)
        {
            turnText.text = "Подключение...";
            turnText.enabled = true;
        }
        
        if (progressText != null)
        {
            progressText.text = "Узлы: 0/0";
            progressText.enabled = true;
        }
        
        if (playerStatusText != null)
        {
            playerStatusText.text = "Ожидание подключения...";
        }
        
        if (rulesPanel != null && rulesPanel.activeSelf)
        {
            rulesPanel.SetActive(false);
        }
    }
    
    void FindUIElements()
    {
        if (turnText == null)
        {
            GameObject turnObj = GameObject.Find("TurnText");
            if (turnObj != null) turnText = turnObj.GetComponent<TMP_Text>();
        }
        
        if (progressText == null)
        {
            GameObject progressObj = GameObject.Find("ProgressText");
            if (progressObj != null) progressText = progressObj.GetComponent<TMP_Text>();
        }
        
        if (playerStatusText == null)
        {
            GameObject statusObj = GameObject.Find("PlayerStatusText");
            if (statusObj != null) playerStatusText = statusObj.GetComponent<TMP_Text>();
        }
        
        if (rulesPanel == null)
        {
            GameObject rulesPanelObj = GameObject.Find("RulesPanel");
            if (rulesPanelObj != null) rulesPanel = rulesPanelObj;
        }
    }
    
    void Update()
    {
        updateTimer += Time.deltaTime;
        
        if (updateTimer >= updateInterval)
        {
            UpdateHUD();
            updateTimer = 0f;
        }
    }
    
    void UpdateHUD()
    {
        // Получаем ID локального игрока
        int currentPlayerId = PlayerClicker.localPlayerId;
        
        // Обновляем статус игрока
        if (playerStatusText != null)
        {
            if (currentPlayerId != -1)
            {
                playerStatusText.text = $"Вы игрок {currentPlayerId}";
            }
            else
            {
                playerStatusText.text = "Подключение...";
            }
        }
        
        // Обновляем информацию об игроке
        if (turnText != null)
        {
            if (currentPlayerId != -1)
            {
                string colorName = GetPlayerColorName(currentPlayerId);
                string playerText = $"Игрок {currentPlayerId} ({colorName})";
                
                if (currentPlayerId != lastPlayerId)
                {
                    turnText.text = playerText;
                    
                    // Устанавливаем цвет текста
                    if (currentPlayerId == 0)
                        turnText.color = Color.blue;
                    else if (currentPlayerId == 1)
                        turnText.color = Color.red;
                    else if (currentPlayerId == 2)
                        turnText.color = Color.green;
                    else
                        turnText.color = Color.yellow;
                        
                    lastPlayerId = currentPlayerId;
                }
            }
            else if (turnText.text != "Ожидание подключения...")
            {
                turnText.text = "Ожидание подключения...";
                turnText.color = Color.white;
            }
        }
        
        // Считаем узлы
        if (progressText != null)
        {
            NodeLogic[] allNodes = FindObjectsOfType<NodeLogic>();
            int totalNodes = allNodes.Length;
            int playerNodes = 0;
            
            if (currentPlayerId != -1)
            {
                foreach (NodeLogic node in allNodes)
                {
                    if (node.OwnerId == currentPlayerId)
                        playerNodes++;
                }
            }
            
            string progressString = $"Узлы: {playerNodes}/{totalNodes}";
            
            if (progressText.text != progressString || playerNodes != lastNodeCount)
            {
                progressText.text = progressString;
                lastNodeCount = playerNodes;
                
                if (playerNodes == totalNodes)
                    progressText.color = Color.green;
                else if (playerNodes == 0)
                    progressText.color = Color.red;
                else if (playerNodes > totalNodes / 2)
                    progressText.color = Color.yellow;
                else
                    progressText.color = Color.white;
            }
        }
    }
    
    public void UpdatePlayerInfo()
    {
        Debug.Log($"GameHUD: Обновляю информацию для игрока {PlayerClicker.localPlayerId}");
        UpdateHUD();
    }
    
    string GetPlayerColorName(int playerId)
    {
        switch (playerId)
        {
            case 0: return "Синий";
            case 1: return "Красный";
            case 2: return "Зеленый";
            case 3: return "Желтый";
            default: return "Игрок";
        }
    }
    
    public void ShowRules()
    {
        if (rulesPanel != null)
        {
            rulesPanel.SetActive(true);
            Debug.Log("Показать правила");
        }
    }
    
    public void HideRules()
    {
        if (rulesPanel != null)
        {
            rulesPanel.SetActive(false);
            Debug.Log("Скрыть правила");
        }
    }
}