using Mirror;
using UnityEngine;
using TMPro;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_Text statusText;
    
    private bool menuHidden = false;
    
    void Start()
    {
        Debug.Log("NetworkUI: Start вызван");
        
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError(" NetworkManager не найден в сцене!");
                return;
            }
        }
        
        // Убедимся, что меню видно при старте
        if (menuPanel != null && !menuPanel.activeSelf)
        {
            menuPanel.SetActive(true);
            Debug.Log(" MenuPanel активирован при старте");
        }
        
        // Прячем меню Mirror HUD
        NetworkManagerHUD hud = networkManager.GetComponent<NetworkManagerHUD>();
        if (hud != null) hud.enabled = false;
        
        // Устанавливаем дефолтный IP
        if (ipInputField != null)
            ipInputField.text = "localhost";
            
        // Показываем начальный статус
        UpdateStatus("Готов к подключению");
    }
    
    void UpdateStatus(string message)
    {
        Debug.Log("NetworkUI: " + message);
        
        if (statusText != null)
            statusText.text = "Статус: " + message;
    }
    
    public void OnHostButton()
    {
        Debug.Log("NetworkUI: Нажата кнопка Host");
        
        if (networkManager == null) 
        {
            Debug.LogError(" NetworkManager не найден!");
            UpdateStatus("Ошибка: NetworkManager не найден");
            return;
        }
        
        UpdateStatus("Запуск хоста...");
        
        try
        {
            Debug.Log("Пытаюсь запустить Host...");
            networkManager.StartHost();
            Debug.Log(" Host запущен успешно!");
            
            // Проверяем состояние
            if (networkManager.isNetworkActive)
            {
                Debug.Log($" Сеть активна, режим: {networkManager.mode}");
                UpdateStatus("Хост запущен");
                
                // Ждем немного и скрываем меню
                Invoke(nameof(HideMenu), 0.5f);
            }
            else
            {
                Debug.LogError(" Сеть не активна после StartHost!");
                UpdateStatus("Ошибка: сеть не активна");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($" Ошибка при запуске Host: {e.Message}");
            UpdateStatus($"Ошибка: {e.Message}");
        }
    }
    
    public void OnClientButton()
    {
        if (networkManager == null) 
        {
            Debug.LogError(" NetworkManager не найден!");
            UpdateStatus("Ошибка: NetworkManager не найден");
            return;
        }
        
        UpdateStatus("Подключение...");
        
        // Устанавливаем IP из поля ввода
        if (ipInputField != null && !string.IsNullOrEmpty(ipInputField.text))
        {
            networkManager.networkAddress = ipInputField.text;
            Debug.Log($"🔗 Подключение к {networkManager.networkAddress}");
        }
        
        try
        {
            networkManager.StartClient();
            UpdateStatus($"Подключение к {networkManager.networkAddress}");
            
            // Ждем подключения и скрываем меню
            Invoke(nameof(HideMenu), 1f);
        }
        catch (System.Exception e)
        {
            Debug.LogError($" Ошибка подключения: {e.Message}");
            UpdateStatus($"Ошибка: {e.Message}");
        }
    }
    
    private void HideMenu()
    {
        if (menuHidden) return;
        
        Debug.Log("Скрываем меню");
        
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            menuHidden = true;
            Debug.Log(" Меню скрыто");
        }
        else
        {
            Debug.LogWarning("menuPanel не назначен!");
        }
    }
    
    // Кнопка выхода
    public void OnQuitButton()
    {
        Debug.Log("Выход из игры");
        Application.Quit();
    }
    
    // Кнопка показа правил (если нужно)
// В NetworkUI.cs, в конце класса добавьте или замените метод ShowRules:

public void ShowRules()
{
    if (GameHUD.Instance != null)
    {
        GameHUD.Instance.ShowRules();
    }
    else
    {
        Debug.LogWarning("GameHUD.Instance не найден, пытаюсь найти...");
        GameHUD hud = FindObjectOfType<GameHUD>();
        if (hud != null)
        {
            hud.ShowRules();
        }
        else
        {
            Debug.LogError(" GameHUD не найден в сцене!");
        }
    }
}


}