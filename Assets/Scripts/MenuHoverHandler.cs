using UnityEngine;
using UnityEngine.EventSystems;
public class MenuHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject generalImg;

    public GameObject highlightImg;

    private void Awake()
    {
        highlightImg.SetActive(false);
        generalImg.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlightImg.SetActive(true);
        generalImg.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlightImg.SetActive(false);
        generalImg.SetActive(true);
    }
}