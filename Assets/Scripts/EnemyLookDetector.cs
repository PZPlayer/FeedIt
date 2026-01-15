using UnityEngine;
using UnityEngine.Events;

public class EnemyLookDetector : MonoBehaviour
{
    [Header("Player Settings")]
    public string playerName = "Player";
    public Camera playerCamera;

    [Header("Detection Settings")]
    public float minDistance = 5f;   // Минимальная дистанция
    [Range(0f, 1f)]
    public float screenCenterThreshold = 0.1f; // Насколько близко к центру экрана игрок должен смотреть

    [Header("Raycast Settings")]
    public LayerMask obstacleMask;   // Что считается препятствием

    public UnityEvent OnDeath;

    private Transform playerTransform;

    private void Start()
    {
        // Находим игрока по имени
        GameObject playerObj = GameObject.Find(playerName);
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning("Player not found: " + playerName);

        // Находим камеру
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            Debug.LogError("Camera not found! Assign manually.");
    }

    private void Update()
    {
        if (playerTransform == null || playerCamera == null)
            return;

        // Проверка дистанции
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > minDistance) return;

        // Конвертируем позицию врага в координаты Viewport (0..1)
        Vector3 viewPos = playerCamera.WorldToViewportPoint(transform.position);

        // Проверка, что враг перед камерой
        if (viewPos.z < 0) return;

        // Проверка, что враг в пределах центра экрана
        bool isInCenter =
            Mathf.Abs(viewPos.x - 0.5f) <= screenCenterThreshold &&
            Mathf.Abs(viewPos.y - 0.5f) <= screenCenterThreshold;

        if (!isInCenter) return;

        // Теперь делаем Raycast от камеры к врагу
        Vector3 dir = transform.position - playerCamera.transform.position;
        float rayDist = dir.magnitude;

        if (Physics.Raycast(playerCamera.transform.position, dir.normalized, out RaycastHit hit, rayDist, obstacleMask))
        {
            // Если что-то попало в хит — значит есть препятствие
            if (hit.collider.gameObject != gameObject)
                return; // игрок НЕ видит врага
        }

        // Если дошли сюда — всё чисто, игрок смотрит на врага
        OnPlayerLooking();
    }

    protected virtual void OnPlayerLooking()
    {
        Debug.Log($"{name}: Player is looking at me!");
        GameObject.Find(playerName).GetComponent<PlayerHealth>().Death(1f, OnDeath);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistance);
    }
}