using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleButtons : MonoBehaviour
{
    [SerializeField] private int sceneGoTo = 1;

    public void FastGoToScene()
    {
        SceneManager.LoadScene(sceneGoTo);
    }

    public void ChangeScene(int num)
    {
        SceneManager.LoadScene(num);
    }
}
