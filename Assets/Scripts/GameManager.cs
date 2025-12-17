using Mirror;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    
    [Header("UI Elements")]
    public GameObject gameOverPanel;
    public TMP_Text winnerText;
    public TMP_Text playerStatusText;
    
    [SyncVar] public bool gameOver = false;
    [SyncVar] public int winnerId = -1;
    
    private HashSet<int> activePlayers = new HashSet<int>();
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        Debug.Log("GameManager Start");
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Запускаем игру через 3 секунды после старта
        if (isServer)
        {
            Invoke(nameof(StartGame), 3f);
        }
    }
    
    [Server]
    void StartGame()
    {
        Debug.Log("🎮 Игра начинается!");
        
        RpcGameStarted();
    }
    
    [ClientRpc]
    void RpcGameStarted()
    {
        Debug.Log("🎮 Игра началась!");
        
        if (playerStatusText != null)
        {
            playerStatusText.text = "Игра началась!";
        }
    }
    
    [Server]
    public void RegisterPlayer(int playerId)
    {
        if (activePlayers.Contains(playerId))
            return;
        
        activePlayers.Add(playerId);
        Debug.Log($"🎮 Игрок {playerId} зарегистрирован. Активных: {activePlayers.Count}");
        UpdatePlayerStatus();
    }
    
    [Server]
    public void PlayerDefeated(int playerId)
    {
        if (!activePlayers.Contains(playerId))
            return;
        
        activePlayers.Remove(playerId);
        Debug.Log($"Игрок {playerId} проиграл. Осталось: {activePlayers.Count}");
        
        UpdatePlayerStatus();
        RpcPlayerLost(playerId);
        CheckForWinner();
    }
    
    [Server]
    public void CheckForWinner()
    {
        if (gameOver) return;
        
        Debug.Log($"Проверка победителя. Активных игроков: {activePlayers.Count}");
        
        if (activePlayers.Count == 1)
        {
            foreach (int playerId in activePlayers)
            {
                winnerId = playerId;
                gameOver = true;
                Debug.Log($"🏆 Игрок {playerId} победил!");
                RpcGameOver(winnerId);
                break;
            }
        }
        else if (activePlayers.Count == 0)
        {
            winnerId = -1;
            gameOver = true;
            Debug.Log("Ничья!");
            RpcGameOver(winnerId);
        }
    }
    
    [ClientRpc]
    void RpcPlayerLost(int playerId)
    {
        Debug.Log($"Игрок {playerId} проиграл");
        
        if (PlayerClicker.localPlayerId == playerId)
        {
            Debug.Log("Вы проиграли!");
            if (playerStatusText != null)
                playerStatusText.text = "Вы проиграли!";
        }
    }
    
    [ClientRpc]
    void RpcGameOver(int winnerId)
    {
        Debug.Log($"Игра окончена! Победитель: {(winnerId == -1 ? "Ничья" : $"Игрок {winnerId}")}");
        
        StartCoroutine(ShowGameOverDelayed(winnerId));
    }
    
    private IEnumerator ShowGameOverDelayed(int winnerId)
    {
        yield return new WaitForSeconds(1f);
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("GameOverPanel активирован");
            
            if (winnerText != null)
            {
                if (winnerId == -1)
                    winnerText.text = "НИЧЬЯ!\nВсе игроки проиграли";
                else if (PlayerClicker.localPlayerId == winnerId)
                    winnerText.text = "ПОБЕДА!\n🏆 Вы победили! 🏆";
                else
                    winnerText.text = $"ИГРА ОКОНЧЕНА\nПобедил Игрок {winnerId}";
            }
        }
        
        // Отключаем управление у всех игроков
        PlayerClicker[] players = FindObjectsOfType<PlayerClicker>();
        foreach (PlayerClicker player in players)
        {
            if (player.isLocalPlayer)
            {
                player.enabled = false;
            }
        }
    }
    
    [Server]
    public void UpdatePlayerStatus()
    {
        RpcUpdatePlayerStatus(activePlayers.Count);
    }
    
    [ClientRpc]
    void RpcUpdatePlayerStatus(int activeCount)
    {
        if (playerStatusText != null)
            playerStatusText.text = $"Активных игроков: {activeCount}";
    }
}