using UnityEngine;
using UnityEngine.EventSystems;
public class MenuHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("默认状态")]
    public GameObject generalImg;

    [Header("高亮状态")]
    public GameObject highlightImg;

    private void Awake()
    {
        if (highlightImg)
            highlightImg.SetActive(false);
        if (generalImg) generalImg.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightImg)
            highlightImg.SetActive(true);
        if (generalImg) generalImg.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightImg)
            highlightImg.SetActive(false);
        if (generalImg) generalImg.SetActive(true);
    }
}
