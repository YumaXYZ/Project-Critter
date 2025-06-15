using UnityEngine;
using UnityEngine.EventSystems; 
using TMPro; 

public class TextHoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color normalColor = Color.white; 
    public Color hoverColor = Color.red; 
    private TextMeshProUGUI tmpText; 

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>(); 
       
        // Asegúrate de que el color inicial sea el normal
        if (tmpText != null) tmpText.color = normalColor;
    }

    // Se llama cuando el ratón entra en el área del objeto
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tmpText != null) tmpText.color = hoverColor;
    }

    // Se llama cuando el ratón sale del área del objeto
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tmpText != null) tmpText.color = normalColor;
    }
}