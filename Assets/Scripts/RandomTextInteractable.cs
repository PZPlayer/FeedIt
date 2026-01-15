using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomTextInteractable : InteractableObject
{
    [Header("Text Settings")]
    public TMPro.TextMeshProUGUI textComponent; // Ссылка на TextMeshPro компонент
    public List<string> randomTexts = new List<string>
    {
        "Hello World!",
        "You found me!",
        "This is random text",
        "Interaction works!",
        "Feed the anomaly..."
    };

    [Header("Timing Settings")]
    public float displayTime = 3f; // Время отображения текста

    private Coroutine textCoroutine;

    public override void Use()
    {
        base.Use(); // Вызываем базовый метод для события onUse

        // Запускаем корутину для показа текста
        if (textCoroutine != null)
        {
            StopCoroutine(textCoroutine);
        }
        textCoroutine = StartCoroutine(ShowRandomText());
    }

    private IEnumerator ShowRandomText()
    {
        // Показываем случайный текст
        if (textComponent != null && randomTexts.Count > 0)
        {
            string randomText = randomTexts[Random.Range(0, randomTexts.Count)];
            textComponent.text = randomText;
            textComponent.gameObject.SetActive(true);

            // Ждем указанное время
            yield return new WaitForSeconds(displayTime);

            // Прячем текст
            textComponent.gameObject.SetActive(false);
        }

        textCoroutine = null;
    }

    // Автоматически скрываем текст при старте
    private void Start()
    {
        if (textComponent != null)
        {
            textComponent.gameObject.SetActive(false);
        }
    }

    // Останавливаем корутину при уничтожении объекта
    private void OnDestroy()
    {
        if (textCoroutine != null)
        {
            StopCoroutine(textCoroutine);
        }
    }
}