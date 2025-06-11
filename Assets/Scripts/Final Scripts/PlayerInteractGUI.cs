using UnityEngine;

public class PlayerInteractGUI : MonoBehaviour
{
    [SerializeField] private GameObject containerGameObject;
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private ObjInteractable myInteractableObj;  // Cambiado a ObjInteractable

    private void Update()
    {
        if (playerInteract.GetInteractableObj() == myInteractableObj)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        containerGameObject.SetActive(true);
    }

    private void Hide()
    {
        containerGameObject.SetActive(false);
    }
}
