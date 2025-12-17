using UnityEngine;
using TMPro;
using Mirror;

public class UIInitializer : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== ПРОВЕРКА СЕТИ ===");
        Debug.Log($"Network active: {NetworkManager.singleton != null}");
        
        if (NetworkManager.singleton != null)
        {
            Debug.Log($"Mode: {NetworkManager.singleton.mode}");
            Debug.Log($"Is client: {NetworkManager.singleton.isNetworkActive}");
        }

        Debug.Log("=== UI INITIALIZER ===");
        
        // Находим все текстовые элементы
        TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>(true);
        Debug.Log($"Найдено TMP_Text элементов: {allTexts.Length}");
        
        foreach (TMP_Text text in allTexts)
        {
            Debug.Log($"📝 Текст: '{text.text}', Имя: {text.gameObject.name}, Активен: {text.gameObject.activeInHierarchy}");
        }
        
        // Находим все кнопки
        UnityEngine.UI.Button[] allButtons = FindObjectsOfType<UnityEngine.UI.Button>(true);
        Debug.Log($"Найдено кнопок: {allButtons.Length}");
        
        foreach (var button in allButtons)
        {
            Debug.Log($"🔄 Кнопка: {button.gameObject.name}, Активна: {button.gameObject.activeInHierarchy}");
        }
        
        // Проверяем GameHUD
        GameHUD hud = FindObjectOfType<GameHUD>();
        if (hud != null)
        {
            Debug.Log($"GameHUD найден: {hud.name}");
        }
        else
        {
            Debug.LogError("GameHUD не найден!");
        }
        
        Debug.Log("=== UI INITIALIZER END ===");
    }
}