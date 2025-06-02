using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteract : MonoBehaviour
{
    [Header("IDs de llaves necesarias (mínimo una)")]
    public string[] requiredKeyIDs;

    [Header("Nombre de la escena a cargar")]
    public string sceneToLoad;

    public GameObject canvas;

    public void Interact()
    {
        if (HasAllRequiredKeys())
        {
            SceneManager.LoadScene(sceneToLoad);
            Time.timeScale = 1f;
            canvas.SetActive(false);
        }
    }

    private bool HasAllRequiredKeys()
    {
        foreach (string keyID in requiredKeyIDs)
        {
            if (!GameState.Instance.HasKey(keyID))
            {
                return false;
            }
        }
        return true;
    }
}
