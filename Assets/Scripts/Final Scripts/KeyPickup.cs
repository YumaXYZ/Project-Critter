using UnityEngine;
using System.Collections;

public class KeyPickup : MonoBehaviour
{
    [Header("ID única de la llave (ej: 'Cemetery', 'Cabin')")]
    public string keyID;

    [Header("Objeto visual del mundo (la llave física)")]
    public GameObject keyModel;

    [Header("Objeto en el inventario (icono u objeto UI)")]
    public GameObject inventoryIcon;

    [Header("Audio del item")]
    public AudioClip itemAudio;

    IEnumerator Start()
    {
        // Espera hasta que GameState.Instance esté disponible
        yield return new WaitUntil(() => GameState.Instance != null);

        // Si ya la tiene, desactiva el modelo y activa el ícono de inventario
        if (GameState.Instance.HasKey(keyID))
        {
            if (keyModel != null) keyModel.SetActive(false);
            if (inventoryIcon != null) inventoryIcon.SetActive(true);
        }
        else
        {
            if (inventoryIcon != null) inventoryIcon.SetActive(false);
        }
    }

    public void Interact()
    {
        AudioSource.PlayClipAtPoint(itemAudio, Camera.main.transform.position, 1.0f);
        if (!GameState.Instance.HasKey(keyID))
        {
            GameState.Instance.AddKey(keyID);
            if (keyModel != null) keyModel.SetActive(false);
            if (inventoryIcon != null) inventoryIcon.SetActive(true);
            Debug.Log($"Recogida llave: {keyID}");
        }
    }
}
