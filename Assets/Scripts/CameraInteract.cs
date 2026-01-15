using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInteract : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float interactDistance = 3f; // Максимальная дистанция взаимодействия
    public LayerMask interactableLayer = ~0; // Слои для взаимодействия
    public LayerMask obstacleLayer = ~0; // Слои которые блокируют луч

    [Header("Input Settings")]
    public KeyCode interactKey = KeyCode.E;
    public PlayerInput InteractInput;

    [Header("Visual Settings")]
    public bool showDebugRay = true;
    public Color rayColor = Color.green;
    public Color hitColor = Color.red;

    private Camera playerCamera;
    private IInteractable currentInteractable;
    private InputAction onInteract;

    [Header("Input Settings")]
    public InputActionReference interactAction;



    private void Start()
    {
        playerCamera = GetComponent<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        onInteract = InteractInput.actions["Interact"];

        // Включаем Input Action
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
    }

    void Update()
    {
        CheckForInteractable();

        if (onInteract.triggered)
        {
            Click();
        }
    }

    void CheckForInteractable()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // Визуализация луча в редакторе
        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, rayColor);
        }

        // Проверяем столкновение с interactable объектами
        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
        {
            // Проверяем нет ли препятствий на пути
            if (!HasObstacle(ray, hit.distance))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    if(currentInteractable != null)
                    {
                        currentInteractable.OnDismiss();
                    }

                    currentInteractable = interactable;
                    currentInteractable.Highlight();

                    // Визуализация попадания
                    if (showDebugRay)
                    {
                        Debug.DrawRay(ray.origin, ray.direction * hit.distance, hitColor);
                    }

                    return;
                }
                else if (currentInteractable != null)
                {
                    currentInteractable.OnDismiss();
                }
            }
            else if (currentInteractable != null)
            {
                currentInteractable.OnDismiss();
            }
        }
        else if (currentInteractable != null)
        {
            currentInteractable.OnDismiss();
        }

        currentInteractable = null;
    }

    void OnDestroy()
    {
        // Отписываемся от событий
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
        }
    }

    bool HasObstacle(Ray ray, float distanceToTarget)
    {
        RaycastHit obstacleHit;

        // Проверяем есть ли препятствия между камерой и целью
        if (Physics.Raycast(ray, out obstacleHit, distanceToTarget, obstacleLayer))
        {
            if (obstacleHit.collider.gameObject.GetComponent<IInteractable>() != null) return false;
            return true;
        }

        return false;
    }

    void OnInteractPerformed(InputAction.CallbackContext context)
    {
        Click();
    }

    private void Click()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Use();
        }
    }

    // Для отображения в инспекторе текущего активного interactable объекта
    void OnGUI()
    {
        if (currentInteractable != null)
        {
            GUI.Label(new Rect(10, 10, 200, 25), "Press E to interact");
        }
    }
}