using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteract : MonoBehaviour
{
    [Header("IDs de llaves necesarias (mínimo una)")]
    public string[] requiredKeyIDs;

    [Header("Nombre de la escena a cargar")]
    public string sceneToLoad;

    [Header("Canvas sobre el que muestra el texto")]
    public GameObject canvas;
    
    [Header("Audio del item")]
    public AudioClip itemAudio;


    public void Interact()
    {
        if (HasAllRequiredKeys())
        {
            AudioSource.PlayClipAtPoint(itemAudio, Camera.main.transform.position, 1.0f);
            SceneManager.LoadScene(sceneToLoad);
            Time.timeScale = 1f;

            if (canvas != null)
            {
                canvas.SetActive(false);
            }
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
