using System.Collections;
using UnityEditor.Animations;
using UnityEngine;
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
        int index = 1;
        while (index < transform.childCount)
        {
            var signalObj = transform.GetChild(index).GetChild(1).gameObject;
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
