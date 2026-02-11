using TMPro;
using UnityEngine;

public class BatteryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI batteryText;
    [SerializeField] private TextMeshProUGUI timerText;

    void Update()
    {
        if (BatteryManager.Instance == null)
            return;

        int current = BatteryManager.Instance.GetBatteryCount();
        int max = BatteryManager.Instance.maxBatteries;

        batteryText.text = current.ToString();

        if (current >= max)
        {
            timerText.text = "FULL";
        }
        else
        {
            float seconds = BatteryManager.Instance.GetSecondsUntilNextBattery();

            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);

            timerText.text = $"{minutes:00}:{secs:00}";
        }
    }
}
