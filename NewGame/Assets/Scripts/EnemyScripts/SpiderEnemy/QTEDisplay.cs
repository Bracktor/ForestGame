using UnityEngine;
using UnityEngine.UI;

public class QTEDisplay : MonoBehaviour
{
    [Header("UI References")]
    public GameObject qtePanel;
    public Text[] keyLabels;
    public Color defaultColor = Color.white;
    public Color completedColor = Color.green;
    public Color timedOutColor = Color.red;

    private void Start()
    {
        if (qtePanel != null)
            qtePanel.SetActive(false);
    }

    public void ShowSequence(KeyCode[] sequence)
    {
        qtePanel.SetActive(true);

        for (int i = 0; i < keyLabels.Length; i++)
        {
            if (i < sequence.Length)
            {
                keyLabels[i].text = sequence[i].ToString();
                keyLabels[i].color = defaultColor;
                keyLabels[i].gameObject.SetActive(true);
            }
            else
            {
                keyLabels[i].gameObject.SetActive(false);
            }
        }
    }

    public void HighlightStep(int stepIndex)
    {
        if (stepIndex < keyLabels.Length)
            keyLabels[stepIndex].color = completedColor;
    }

    public void ResetSequence()
    {
        foreach (var label in keyLabels)
            label.color = timedOutColor;

        // Brief flash then reset to white
        Invoke(nameof(ResetColors), 0.3f);
    }

    private void ResetColors()
    {
        foreach (var label in keyLabels)
            label.color = defaultColor;
    }

    public void Hide()
    {
        qtePanel.SetActive(false);
    }
}