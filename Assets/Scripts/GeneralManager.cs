using UnityEngine;
using UnityEngine.UI;

public class GeneralManager : MonoBehaviour
{
    public GameObject[] highQualityGameObjects, mediumQualityGameObjects;
    public Image toolbar;
    public GameObject bg;
    private const int DefaultRefreshRate = 60;

    void Awake()
    {
        ApplyVSync();
        HighQuality();
    }

    private void SetGameObjectsActive(GameObject[] objects, bool active)
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(active);
        }
    }

    public void LowQuality()
    {
        SetGameObjectsActive(highQualityGameObjects, false);
        SetGameObjectsActive(mediumQualityGameObjects, false);
        AnimManager.isLowQuality = true;
        print("🪦 for high and midium quality objs.");
    }

    public void MediumQuality()
    {
        SetGameObjectsActive(highQualityGameObjects, false);
        SetGameObjectsActive(mediumQualityGameObjects, true);
        print("🪦 for high quality objs.");
    }

    public void HighQuality()
    {
        SetGameObjectsActive(highQualityGameObjects, true);
        SetGameObjectsActive(mediumQualityGameObjects, true);
    }

    private int GetSafeRefreshRate()
    {
        int refreshRate;
        try
        {
            refreshRate = (int)Screen.currentResolution.refreshRateRatio.value;

            if (refreshRate <= 0)
            {
                refreshRate = (int)Screen.currentResolution.refreshRateRatio.value;
            }
        }
        catch
        {
            refreshRate = DefaultRefreshRate;
        }

        if (refreshRate <= 0)
            refreshRate = DefaultRefreshRate;

        return refreshRate;
    }

    private void ApplyVSync()
    {
        int refreshRate = GetSafeRefreshRate();

        Application.targetFrameRate = refreshRate + 5;
        QualitySettings.vSyncCount = 0;
    }
}
