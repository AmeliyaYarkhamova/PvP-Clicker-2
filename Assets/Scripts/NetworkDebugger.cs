using UnityEngine;
using Mirror;

public class NetworkDebugger : MonoBehaviour
{
    void Update()
    {
        // F5 - принудительная диагностика
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("=== СЕТЕВАЯ ДИАГНОСТИКА ===");
            
            // Проверяем всех игроков
            PlayerClicker[] players = FindObjectsOfType<PlayerClicker>();
            Debug.Log($"Найдено игроков: {players.Length}");
            
            foreach (PlayerClicker player in players)
            {
                Debug.Log($"  Игрок: {player.name}");
                Debug.Log($"    netId: {player.netId}");
                Debug.Log($"    isLocalPlayer: {player.isLocalPlayer}");
                Debug.Log($"    myPlayerId: {player.myPlayerId}");
                Debug.Log($"    isServer: {player.isServer}");
                Debug.Log($"    isClient: {player.isClient}");
            }
            
            // Проверяем локального игрока
            if (PlayerClicker.LocalPlayer != null)
            {
                Debug.Log($"✅ Локальный игрок: {PlayerClicker.LocalPlayer.myPlayerId}");
                Debug.Log($"✅ localPlayerId: {PlayerClicker.localPlayerId}");
            }
            else
            {
                Debug.Log("❌ Локальный игрок не найден!");
            }
        }
        
        // F6 - принудительно обновить ID
        if (Input.GetKeyDown(KeyCode.F6) && PlayerClicker.LocalPlayer != null)
        {
            if (PlayerClicker.LocalPlayer.myPlayerId != -1 && PlayerClicker.localPlayerId == -1)
            {
                PlayerClicker.localPlayerId = PlayerClicker.LocalPlayer.myPlayerId;
                Debug.Log($"🔄 Принудительно установил localPlayerId = {PlayerClicker.localPlayerId}");
            }
        }
    }
}