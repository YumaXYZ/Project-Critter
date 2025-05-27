using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [Header("ID única de la llave (ej: 'Cemetery', 'Cabin')")]
    public string keyID;

    [Header("Objeto visual del mundo (la llave física)")]
    public GameObject keyModel;

    [Header("Objeto en el inventario (icono u objeto UI)")]
    public GameObject inventoryIcon;

    void Start()
    {
        // Si ya la tiene, desactiva del mundo y activa en inventario
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
        if (!GameState.Instance.HasKey(keyID))
        {
            GameState.Instance.AddKey(keyID);
            if (keyModel != null) keyModel.SetActive(false);
            if (inventoryIcon != null) inventoryIcon.SetActive(true);
            Debug.Log($"Recogida llave: {keyID}");
        }
    }
}
