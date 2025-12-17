using Mirror;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class NodeLogic : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnValueChanged))]
    public int Value;
    
    [SyncVar(hook = nameof(OnOwnerChanged))]
    public int OwnerId = -1;
    
    public List<NodeLogic> ConnectedNodes = new List<NodeLogic>();
    
    private SpriteRenderer spriteRenderer;
    private TextMesh textMesh;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CreateTextMesh();
        
        if (isServer)
        {
            // Убедимся, что значения разные для отладки
            Value = Random.Range(15, 25);
            OwnerId = -1;
            UpdateVisual();
        }
        else
        {
            UpdateVisual();
        }
        
        Debug.Log($"Узел {name} создан. Server: {isServer}, Client: {isClient}");
    }
    
    void CreateTextMesh()
    {
        GameObject textGO = new GameObject("NodeValueText");
        textGO.transform.SetParent(transform);
        textGO.transform.localPosition = new Vector3(0, 0, -0.1f);
        
        textMesh = textGO.AddComponent<TextMesh>();
        textMesh.characterSize = 0.2f;
        textMesh.fontSize = 24;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
        
        MeshRenderer meshRenderer = textGO.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 1;
        }
    }
    
    void OnValueChanged(int oldValue, int newValue)
    {
        Debug.Log($"Узел {name}: Значение изменилось {oldValue} -> {newValue}");
        UpdateVisual();
    }
    
    void OnOwnerChanged(int oldOwner, int newOwner)
    {
        Debug.Log($"Узел {name}: Владелец изменился {oldOwner} -> {newOwner}");
        UpdateVisual();
    }
    
    void UpdateVisual()
    {
        if (spriteRenderer == null) return;
        
        if (OwnerId == -1)
            spriteRenderer.color = Color.gray;
        else if (OwnerId == 0)
            spriteRenderer.color = Color.blue;
        else if (OwnerId == 1)
            spriteRenderer.color = Color.red;
        else if (OwnerId == 2)
            spriteRenderer.color = Color.green;
        else
            spriteRenderer.color = Color.yellow;
        
        if (textMesh != null)
        {
            textMesh.text = Value.ToString();
            
            if (OwnerId == -1 || OwnerId == 3)
                textMesh.color = Color.black;
            else
                textMesh.color = Color.white;
        }
    }
    
    [Server]
    public void DecreaseValue(int amount)
    {
        int oldOwner = OwnerId;
        Value -= amount;
        
        if (Value <= 0)
        {
            Value = 0;
            OwnerId = -1; // Сначала делаем нейтральным
            Debug.Log($"Узел {name}: Стал нейтральным (достиг 0)");
            
            // НЕ захватываем сразу! Ждем отдельного клика
            // Проверяем, не потерял ли старый владелец все узлы
            if (oldOwner != -1)
            {
                StartCoroutine(CheckPlayerNodesAfterDelay(oldOwner));
            }
        }
    }
    
    [Server]
    public void IncreaseValue(int amount)
    {
        Value += amount;
        Debug.Log($"Узел {name}: Значение увеличено на {amount} до {Value}");
    }
    
    [Server]
    public void CaptureNode(int newOwnerId)
    {
        int oldOwner = OwnerId;
        OwnerId = newOwnerId;
        Value = 1; // При захвате ставим минимальное значение
        Debug.Log($"Узел {name}: Захвачен игроком {newOwnerId}");
        
        // Проверяем, не потерял ли старый владелец все узлы
        if (oldOwner != -1 && oldOwner != newOwnerId)
        {
            StartCoroutine(CheckPlayerNodesAfterDelay(oldOwner));
        }
    }
    
    [Server]
    IEnumerator CheckPlayerNodesAfterDelay(int playerId)
    {
        yield return new WaitForSeconds(0.2f);
        
        bool hasNodes = false;
        NodeLogic[] allNodes = FindObjectsOfType<NodeLogic>();
        
        foreach (NodeLogic node in allNodes)
        {
            if (node.OwnerId == playerId)
            {
                hasNodes = true;
                break;
            }
        }
        
        if (!hasNodes && GameManager.Instance != null)
        {
            Debug.Log($"💀 Игрок {playerId} потерял все узлы!");
            GameManager.Instance.PlayerDefeated(playerId);
        }
    }
}