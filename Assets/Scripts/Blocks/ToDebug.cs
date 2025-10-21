using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToDebug : MonoBehaviour
{
    public Button button;

    public bool shouldStop;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(StartToDebug);
    }
    public void StartToDebug()
    {
        _ = StartCoroutine(StartRunning());
    }

    IEnumerator StartRunning()
    {
        var startTime = Time.time;
        float currentTime = 0f;
        while (currentTime - startTime < 0.5f)
        {
            currentTime = Time.time;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                yield break;
            }
            else
            {
                yield return null;
            }
        }

        int index = 1;
        while (index < transform.GetChild(1).childCount)
        {
            var signalObj = transform.GetChild(1).GetChild(index).GetChild(1).gameObject;
            signalObj.SetActive(true);

            while (signalObj.activeSelf)
            {
                if (shouldStop) yield break;
                yield return null;
            }

            index++;
        }
    }
}
