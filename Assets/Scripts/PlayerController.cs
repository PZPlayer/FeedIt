using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float additionalMoveSpeed = 3f;
    public float jumpForce = 8f;
    public float SprintTime = 8f;
    public float gravity = 20f;
    public float normalFOV = 60f;
    public float speedupFov = 80f;

    [Header("Tilt Settings")]
    public float tiltAngle = 20f;
    public float tiltSpeed = 4f;

    [SerializeField] private Image _sprintImg;
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _feet;
    [SerializeField] private LayerMask _layerGround;
    [SerializeField] private Animator _anmtr;
    [SerializeField] private AudioClip _walkingSound;
    [SerializeField] private AudioClip _runningSound;

    private CharacterController controller;
    private Vector3 movement;
    private float verticalVelocity;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction jumpAction;
    private Coroutine coroutineSpeedUp;
    private Coroutine coroutineSlowDown;
    private CinemachineCamera cinemachineCamera;
    private float targetTilt = 0f;
    private float currentTilt = 0f;
    private bool stoping = false;
    private float sprintingSpeedTime = 0f;
    private AudioSource source;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        source = GetComponent<AudioSource>();

        // Получаем Actions напрямую
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];

        cinemachineCamera = _camera.transform.GetComponent<CinemachineCamera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (stoping) return;

        HandleGravity();
        RotatePlayerWithCamera();
        Jump();
        Move();
        UpdateTilt();
    }

    private void RotatePlayerWithCamera() => transform.rotation = Quaternion.Euler(new Vector3(0, _camera.rotation.eulerAngles.y, 0));

    private void HandleTilt(float horizontalInput)
    {
        if (horizontalInput > 0.1f) // Вправо
        {
            targetTilt = -tiltAngle;
        }
        else if (horizontalInput < -0.1f) // Влево
        {
            targetTilt = tiltAngle;
        }
        else // Прямо или назад
        {
            targetTilt = 0f;
        }
    }

    public void ResetMovement(bool stop)
    {
        stoping = stop;
    }

    private void UpdateTilt()
    {
        // Плавно интерполируем к целевому наклону
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);

        // Применяем наклон к камере
        if (cinemachineCamera != null)
        {
            var lens = cinemachineCamera.Lens;
            lens.Dutch = currentTilt; // Dutch - это наклон камеры в Cinemachine
            cinemachineCamera.Lens = lens;
        }
    }

    private void Move()
    {
        Vector2 inputVector = moveAction.ReadValue<Vector2>();
        float boost_speed = 0;

        bool isSprinting = sprintAction.IsPressed() && (inputVector != Vector2.zero) && (sprintingSpeedTime < SprintTime);
        bool isMoving = inputVector != Vector2.zero;

        // Обработка наклона
        HandleTilt(inputVector.x);

        // остальной код Move() без изменений...
        if (isSprinting && ((sprintingSpeedTime < SprintTime) == true))
        {
            if (source.clip != _runningSound)
            {
                source.Stop();
                source.clip = _runningSound;
            }
            else if (!source.isPlaying)
            {
                source.Play();
            }

            sprintingSpeedTime += Time.deltaTime;
            boost_speed = additionalMoveSpeed;

            if (coroutineSlowDown != null)
            {
                StopCoroutine(coroutineSlowDown);
                coroutineSlowDown = null;
            }
            if (coroutineSpeedUp == null)
                coroutineSpeedUp = StartCoroutine(ChangeFOV(speedupFov));
        }
        else if (isMoving)
        {
            boost_speed = 0;

            if (source.clip != _walkingSound)
            {
                source.Stop();
                source.clip = _walkingSound;
            }
            else if (!source.isPlaying)
            {
                source.Play();
            }
            
            if (coroutineSpeedUp != null)
            {
                StopCoroutine(coroutineSpeedUp);
                coroutineSpeedUp = null;
            }

            if (coroutineSlowDown == null)
                coroutineSlowDown = StartCoroutine(ChangeFOV(normalFOV));
        }
        else
        {
            boost_speed = 0;
            source.Stop();

            if (coroutineSpeedUp != null)
            {
                StopCoroutine(coroutineSpeedUp);
                coroutineSpeedUp = null;
            }
            if (coroutineSlowDown != null)
            {
                StopCoroutine(coroutineSlowDown);
                coroutineSlowDown = null;
            }
        }

        source.volume = (!controller.isGrounded) ? 0 : 1;
        if (!isSprinting && !sprintAction.IsPressed()) sprintingSpeedTime -= Time.deltaTime * 0.5f;
        sprintingSpeedTime = Mathf.Clamp(sprintingSpeedTime, 0, SprintTime);
        _sprintImg.fillAmount = Mathf.Clamp(1 - sprintingSpeedTime / SprintTime, 0, 1);
        _anmtr.SetBool("IsRunning", controller.isGrounded && isMoving);
        movement = transform.TransformDirection(new Vector3(inputVector.x, 0, inputVector.y));
        movement *= moveSpeed + boost_speed;
        controller.Move(movement * Time.deltaTime);
    }

    private void Jump()
    {
        controller.Move(movement * Time.deltaTime);

        // Проверяем прыжок
        if (jumpAction.triggered && controller.isGrounded)
        {
            verticalVelocity = jumpForce;
            _anmtr.SetTrigger("Jump");
        }
        else if (controller.isGrounded)
        {
            _anmtr.SetBool("IsGround", true);
        }
        else
        {
            _anmtr.SetBool("IsGround", false);
        }
    }

    private IEnumerator ChangeFOV(float newFov)
    {
        float time = 0.4f;
        float currentFov = cinemachineCamera.Lens.FieldOfView;
        float timer = 0;

        while (time > timer)
        {
            timer += Time.deltaTime;
            cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(currentFov, newFov, timer / time);
            yield return null;
        }

        // Сбрасываем соответствующую переменную корутины
        if (newFov == speedupFov)
            coroutineSpeedUp = null;
        else
            coroutineSlowDown = null;
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
    
        movement.y = verticalVelocity;
        controller.Move(movement * Time.deltaTime);
    }
}