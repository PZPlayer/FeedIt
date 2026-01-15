using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private Vector3 _resetPoint;
    [SerializeField] private UnityEvent _onDeath;
    [SerializeField] private GameObject _deathPartcl;
    private bool ifAlredyDying = false;

    public void Death(float after, UnityEvent DeathEvents)
    {
        if (ifAlredyDying) { return; }
        DeathEvents.Invoke();
        ifAlredyDying = true;
        transform.GetComponent<PlayerController>().enabled = false;
        transform.GetComponent<CharacterController>().enabled = false;
        Invoke("Die", after);
    }

    private void Die()
    {
        _onDeath.Invoke();
        GameObject t = Instantiate(_deathPartcl, transform.position, transform.rotation);
        Destroy(t, 900);
        Invoke("ToSpawn", 1f);
    }

    private void ToSpawn()
    {
        print($"Going to: {_resetPoint}");
        transform.position = _resetPoint;
        transform.GetComponent<PlayerController>().enabled = true;
        transform.GetComponent<CharacterController>().enabled = true;
        ifAlredyDying = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_resetPoint == Vector3.zero && other.collider.gameObject.layer == 6)
        {
            _resetPoint = transform.position;
        } 
    }
}
