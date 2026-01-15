using UnityEngine;
using UnityEngine.Events;

public class OnForestEnter : MonoBehaviour
{
    [SerializeField] private UnityEvent _event;

    private void Start()
    {
        RenderSettings.fog = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            FogActive(true);
            _event.Invoke();
        }
    }

    public void FogActive(bool on = false)
    {
        RenderSettings.fog = on;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.045f;
    }
}
