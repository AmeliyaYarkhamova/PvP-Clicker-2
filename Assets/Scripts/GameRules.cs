using UnityEngine;
using TMPro;

public class GameRules : MonoBehaviour
{
    [SerializeField] private GameObject rulesPanel;
    [SerializeField] private TMP_Text rulesText;
    
    void Start()
    {
        if (rulesPanel != null)
        {
            rulesPanel.SetActive(true);
            SetupRulesText();
        }
    }
    
    void SetupRulesText()
    {
        if (rulesText != null)
        {
            rulesText.text = "🎮 PvP Clicker - Правила игры:\n\n" +
                            "1. ⚫ Ваши узлы - синего цвета\n" +
                            "2. 🔴 Узлы противника - красного цвета\n" +
                            "3. ⚪ Нейтральные узлы - серого цвета\n\n" +
                            "🎯 Цель:\n" +
                            "Захватить все узлы на карте!\n\n" +
                            "📌 Как играть:\n" +
                            "• Клик по своему узлу → увеличение силы (+1)\n" +
                            "• Клик по соседнему чужому узлу → уменьшение (-1)\n" +
                            "• Когда узел достигает 0 → становится вашим!\n\n" +
                            "⏱️ Игра в реальном времени - кликайте быстрее соперника!";
        }
    }
    
    public void HideRules()
    {
        if (rulesPanel != null)
            rulesPanel.SetActive(false);
    }
    
    public void ShowRules()
    {
        if (rulesPanel != null)
            rulesPanel.SetActive(true);
    }
}