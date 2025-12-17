using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class GameFieldManager : NetworkBehaviour
{
    public static GameFieldManager Instance;
    
    public GameObject nodePrefab;
    public int gridSize = 3;
    public float spacing = 3f;
    
    private List<NodeLogic> allNodes = new List<NodeLogic>();
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("✅ GameFieldManager: Сервер стартовал");
        CreateGrid();
    }
    
    void CreateGrid()
    {
        Debug.Log($"🔧 Создаю сетку {gridSize}x{gridSize}");
        
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                float offset = (gridSize - 1) * spacing / 2f;
                Vector3 position = new Vector3(
                    x * spacing - offset,
                    y * spacing - offset,
                    0
                );
                
                GameObject nodeObj = Instantiate(nodePrefab, position, Quaternion.identity);
                NetworkServer.Spawn(nodeObj);
                
                NodeLogic nodeLogic = nodeObj.GetComponent<NodeLogic>();
                allNodes.Add(nodeLogic);
                nodeObj.name = $"Node_{x}_{y}";
            }
        }
        
        SetUpConnections();
        Debug.Log($"✅ Создано {allNodes.Count} узлов");
    }
    
    void SetUpConnections()
    {
        Debug.Log("🔗 Устанавливаю связи...");
        
        for (int i = 0; i < allNodes.Count; i++)
        {
            int x = i % gridSize;
            int y = i / gridSize;
            
            allNodes[i].ConnectedNodes.Clear();
            
            // Лево
            if (x > 0) allNodes[i].ConnectedNodes.Add(allNodes[i - 1]);
            // Право
            if (x < gridSize - 1) allNodes[i].ConnectedNodes.Add(allNodes[i + 1]);
            // Низ
            if (y > 0) allNodes[i].ConnectedNodes.Add(allNodes[i - gridSize]);
            // Верх
            if (y < gridSize - 1) allNodes[i].ConnectedNodes.Add(allNodes[i + gridSize]);
            
            Debug.Log($"Узел {allNodes[i].name} имеет {allNodes[i].ConnectedNodes.Count} соседей");
        }
    }
    
    [Server]
    public void GiveNodeToPlayer(int playerId)
    {
        Debug.Log($"🎮 Выдаю узел игроку {playerId}");
        
        foreach (NodeLogic node in allNodes)
        {
            if (node.OwnerId == -1)
            {
                node.CaptureNode(playerId);
                Debug.Log($"✅ Узел {node.name} выдан игроку {playerId}");
                return;
            }
        }
    }
    
    // МЕТОД ДЛЯ ПРОВЕРКИ СВЯЗЕЙ
    public void DebugLogConnections()
    {
        Debug.Log("=== ПРОВЕРКА СВЯЗЕЙ МЕЖДУ УЗЛАМИ ===");
        
        foreach (NodeLogic node in allNodes)
        {
            string neighborNames = "";
            foreach (NodeLogic neighbor in node.ConnectedNodes)
            {
                neighborNames += neighbor.name + " ";
            }
            
            Debug.Log($"Узел {node.name} (Владелец: {node.OwnerId}, Значение: {node.Value})");
            Debug.Log($"  Соседи: {neighborNames}");
        }
    }
}