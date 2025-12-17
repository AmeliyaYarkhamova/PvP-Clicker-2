using Mirror;
using UnityEngine;

public class PlayerClicker : NetworkBehaviour
{
    public static PlayerClicker LocalPlayer { get; private set; }
    public static int localPlayerId = -1;
    
    [SyncVar(hook = nameof(OnMyPlayerIdChanged))]
    public int myPlayerId = -1;
    
    [SyncVar(hook = nameof(OnIsDefeatedChanged))]
    public bool isDefeated = false;
    
    private Camera mainCamera;
    
    void Awake()
    {
        Debug.Log($"PlayerClicker Awake: {gameObject.name}, isLocalPlayer: {isLocalPlayer}");
    }
    
    public override void OnStartLocalPlayer()
    {
        Debug.Log("🎮 PlayerClicker: Локальный игрок стартовал!");
        LocalPlayer = this;
        
        // Находим камеру
        FindCamera();
        
        // Просим сервер выдать ID
        Debug.Log("🔄 Запрашиваю ID у сервера...");
        CmdRequestPlayerId();
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"🎮 PlayerClicker OnStartClient: {gameObject.name}, netId: {netId}");
    }
    
    void FindCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Камера не найдена, ищу в сцене...");
            Camera[] cameras = FindObjectsOfType<Camera>();
            if (cameras.Length > 0)
            {
                mainCamera = cameras[0];
                Debug.Log($"Найдена камера: {mainCamera.gameObject.name}");
            }
            else
            {
                Debug.LogError("Камеры в сцене не найдены!");
            }
        }
    }
    
    void OnDestroy()
    {
        if (LocalPlayer == this) 
        {
            LocalPlayer = null;
            localPlayerId = -1;
            Debug.Log($"PlayerClicker уничтожен: {myPlayerId}");
        }
    }
    
    [Command]
    void CmdRequestPlayerId()
    {
        // Используем connectionId как Player ID
        myPlayerId = (int)connectionToClient.connectionId;
        Debug.Log($"СЕРВЕР: Игрок подключился! ID={myPlayerId}");
        
        // Выдаем стартовый узел
        if (GameFieldManager.Instance != null)
        {
            GameFieldManager.Instance.GiveNodeToPlayer(myPlayerId);
            Debug.Log($"СЕРВЕР: Выдал узел игроку {myPlayerId}");
        }
        
        // Регистрируем в GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(myPlayerId);
            Debug.Log($"СЕРВЕР: Зарегистрировал игрока {myPlayerId} в GameManager");
        }
        
        // Отправляем ID клиенту
        TargetSetPlayerId(connectionToClient, myPlayerId);
    }
    
    [TargetRpc]
    void TargetSetPlayerId(NetworkConnection target, int playerId)
    {
        myPlayerId = playerId;
        localPlayerId = playerId;
        Debug.Log($"КЛИЕНТ: Я получил ID {myPlayerId} от сервера!");
        
        // Обновляем UI
        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.UpdatePlayerInfo();
        }
    }
    
    // Хуки для SyncVar
    void OnMyPlayerIdChanged(int oldId, int newId)
    {
        Debug.Log($"PlayerClicker: ID изменился {oldId} -> {newId}");
        myPlayerId = newId;
        
        if (isLocalPlayer)
        {
            localPlayerId = newId;
            Debug.Log($"🎮 КЛИЕНТ: Мой ID установлен: {localPlayerId}");
        }
    }
    
    void OnIsDefeatedChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"PlayerClicker: isDefeated {oldValue} -> {newValue}");
    }
    
    void Update()
    {
        if (!isLocalPlayer || isDefeated) return;
        
        // Проверяем GameManager
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.gameOver) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }
    
    void HandleClick()
    {
        if (mainCamera == null) 
        {
            FindCamera();
            if (mainCamera == null) return;
        }
        
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        
        if (hit.collider != null)
        {
            NodeLogic clickedNode = hit.collider.GetComponent<NodeLogic>();
            
            if (clickedNode != null)
            {
                Debug.Log($"Игрок {myPlayerId} кликнул на узел: {clickedNode.gameObject.name}");
                CmdProcessClick(clickedNode.gameObject);
            }
        }
    }
    
    [Command]
    void CmdProcessClick(GameObject nodeObject)
    {
        if (isDefeated) return;
        
        NodeLogic node = nodeObject.GetComponent<NodeLogic>();
        if (node == null) return;
        
        Debug.Log($"🎮 СЕРВЕР: Игрок {myPlayerId} кликает по узлу {node.name}");
        
        // 1. Если узел наш - увеличиваем
        if (node.OwnerId == myPlayerId)
        {
            node.IncreaseValue(1);
            Debug.Log($"Игрок {myPlayerId} увеличил свой узел");
        }
        else
        {
            // 2. Проверяем, есть ли у нас соседние узлы
            bool hasNeighbor = false;
            foreach (NodeLogic connectedNode in node.ConnectedNodes)
            {
                if (connectedNode.OwnerId == myPlayerId)
                {
                    hasNeighbor = true;
                    break;
                }
            }
            
            // Если нет соседних узлов и узел не нейтральный - нельзя атаковать
            if (!hasNeighbor && node.OwnerId != -1)
            {
                Debug.Log($"Игрок {myPlayerId}: Нельзя атаковать - нет своих узлов рядом");
                return;
            }
            
            // 3. Если узел нейтральный
            if (node.OwnerId == -1)
            {
                node.CaptureNode(myPlayerId);
                Debug.Log($"🏴 Игрок {myPlayerId} захватил нейтральный узел");
            }
            else // 4. Чужой узел
            {
                // ТОЛЬКО уменьшаем значение, не захватываем!
                if (node.Value > 0)
                {
                    node.DecreaseValue(1);
                    Debug.Log($"Игрок {myPlayerId} атаковал узел игрока {node.OwnerId}");
                }
            }
        }
    }
    
    [ClientRpc]
    public void RpcDefeat()
    {
        isDefeated = true;
        Debug.Log($"Игрок {myPlayerId} проиграл (RPC)");
        enabled = false;
    }
}