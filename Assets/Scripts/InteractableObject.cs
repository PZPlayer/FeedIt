using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public interface IInteractable
{
    void Highlight();
    void OnDismiss();
    void Use();
}

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interactable Settings")]
    public UnityEvent onUse; // Событие которое будет вызываться при взаимодействии
    public UnityEvent onHighlight; // Событие которое будет вызываться при взаимодействии
    public UnityEvent onDismiss; // Событие которое будет вызываться при взаимодействии

    [Header("Visual Feedback")]
    public bool showDebugInfo = true;

    [SerializeField] private Material _material;
    [SerializeField] private Texture _startSprite;
    [SerializeField] private Texture _endSprite;

    private void Start()
    {
        if(_material != null) _material.mainTexture = _startSprite;
    }

    public void OnDismiss()
    {
        if (!this.enabled) return;
        onDismiss?.Invoke();
    }

    public virtual void Highlight()
    {
        if (!this.enabled) return;
        onHighlight?.Invoke();
    }

    public virtual void Use()
    {
        if (!this.enabled) return;
        // Вызываем событие
        onUse?.Invoke();

        // Для отладки
        if (showDebugInfo)
        {
            Debug.Log($"Interacted with: {gameObject.name}");
        }
    }

    public void ChangePlayerPosition(Transform position)
    {
        GameObject player = GameObject.Find("Player");

        
        Destroy(player.transform.GetComponent<Rigidbody>());
        StartCoroutine(TeleportPlayer(player, position));
    }

    public void ChangeFace()
    {
        _material.mainTexture = _endSprite;
    }

    private IEnumerator TeleportPlayer(GameObject player, Transform pos)
    {
        yield return new WaitForSeconds(1);
        player.transform.GetComponent<PlayerController>().enabled = false;
        player.transform.GetComponent<CharacterController>().enabled = false;
        player.transform.position = pos.transform.position;
        player.transform.GetComponent<PlayerController>().enabled = true;
        player.transform.GetComponent<CharacterController>().enabled = true;
    }
}