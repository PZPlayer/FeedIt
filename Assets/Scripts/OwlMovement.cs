using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Events;

public class OwlMovement : MonoBehaviour
{
    public enum OwlState
    {
        Sleep,
        WakeUp,
        Follow
    }

    [Header("References")]
    public Transform player;
    public Transform owlHead;
    public Animator animator;

    private NavMeshAgent agent;

    [Header("Settings")]
    public float wakeDistance = 5f;         // рассто€ние пробуждени€
    public float losePlayerDistance = 7f;   // рассто€ние потери игрока
    public float extraFollowTime = 3f;      // врем€ продолжени€ погони после потери
    public float headLookSpeed = 5f;        // скорость поворота головы

    [Header("Head Settings")]
    public float headSideOffset = 90f;  // 90 = смотреть правым боком, -90 = левым, 0 = лицом

    [Header("Kill Settings")]
    public float killDistance = 0.7f;         // рассто€ние убийства игрока
    public string playerScriptName = "PlayerHealth"; // им€ класса игрока, где есть метод Death()
    public UnityEvent OnKillPlayer;

    private OwlState currentState = OwlState.Sleep;
    private bool isLosingPlayer = false;

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.Find("Player").transform;
        }

        agent = GetComponent<NavMeshAgent>();
        GoToSleep();
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case OwlState.Sleep:
                if (distance <= wakeDistance)
                    StartCoroutine(WakeUpRoutine());
                break;

            case OwlState.Follow:
                HandleFollow(distance);
                break;
        }

        if (currentState != OwlState.Sleep)
            LookAtPlayer();
    }

    // ---------------------- FSM STATES ----------------------

    IEnumerator WakeUpRoutine()
    {
        ChangeState(OwlState.WakeUp);
        transform.GetComponent<AudioSource>().Play();
        animator.SetTrigger("WakeUp");

        yield return new WaitForSeconds(2f);

        ChangeState(OwlState.Follow);
        animator.SetBool("Walk", true);
        agent.isStopped = false;
    }

    void HandleFollow(float dist)
    {
        // --- ”бийство игрока ---
        if (dist <= killDistance)
        {
            TryKillPlayer();
            return;
        }

        // --- ѕотер€ игрока ---
        if (!isLosingPlayer && dist > losePlayerDistance)
        {
            StartCoroutine(LosePlayerRoutine());
            return;
        }

        // --- ќбычное следование ---
        if (!isLosingPlayer)
        {
            agent.SetDestination(player.position);
        }
    }

    IEnumerator LosePlayerRoutine()
    {
        isLosingPlayer = true;

        float timer = 0f;

        while (timer < extraFollowTime)
        {
            timer += Time.deltaTime;

            float dist = Vector3.Distance(transform.position, player.position);

            // игрок вернулс€ Ч продолжаем следовать
            if (dist <= losePlayerDistance)
            {
                isLosingPlayer = false;
                yield break;
            }

            // продолжаем идти к игроку ещЄ 3 секунды
            agent.SetDestination(player.position);

            yield return null;
        }

        // игрок так и не вернулс€ Ч засыпаем пр€мо здесь
        FallAsleepInPlace();
    }

    void FallAsleepInPlace()
    {
        ChangeState(OwlState.Sleep);
        isLosingPlayer = false;
        agent.isStopped = true;
        animator.SetBool("Walk", false);
    }

    // ---------------------- UTILITY ----------------------

    void GoToSleep()
    {
        ChangeState(OwlState.Sleep);
        agent.isStopped = true;
        animator.SetBool("Walk", false);
    }

    void ChangeState(OwlState newState)
    {
        currentState = newState;
    }

    void LookAtPlayer()
    {
        Vector3 dir = player.position - owlHead.position;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        // —мещение (правый бок, левый бок и т.д.)
        Quaternion offset = Quaternion.Euler(0, headSideOffset, 0);
        targetRot *= offset;

        owlHead.rotation = Quaternion.Lerp(
            owlHead.rotation,
            targetRot,
            Time.deltaTime * headLookSpeed
        );
    }

    void TryKillPlayer()
    {
        PlayerHealth comp = player.GetComponent<PlayerHealth>();
        if (comp != null)
        {
            comp.Death(0, OnKillPlayer);
        }
    }

    // ---------------------- GIZMOS ----------------------

    void OnDrawGizmosSelected()
    {
        Color wakeColor = new Color(0f, 0.5f, 1f, 0.25f);
        Color loseColor = new Color(1f, 0f, 0f, 0.25f);
        Color dirColor = new Color(1f, 0.9f, 0.1f, 0.8f);

        // –адиус пробуждени€
        Gizmos.color = wakeColor;
        Gizmos.DrawSphere(transform.position, wakeDistance);

        // –адиус потери игрока
        Gizmos.color = loseColor;
        Gizmos.DrawSphere(transform.position, losePlayerDistance);

        // Ћуч к игроку
        if (player != null)
        {
            Gizmos.color = dirColor;
            Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, player.position);
        }

#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 2f,
            $"State: {currentState}"
        );
#endif
    }
}