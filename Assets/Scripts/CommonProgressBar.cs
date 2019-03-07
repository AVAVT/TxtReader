using UnityEngine;
using UnityEngine.UI;

public class CommonProgressBar : MonoBehaviour
{
  public RectTransform bar;
  public RectTransform fill;
  public Text percentageText;
  public void UpdateProgress(float ratio)
  {
    fill.sizeDelta = bar.sizeDelta.WithX(bar.rect.width * ratio);
    if (percentageText != null) percentageText.text = $"{(ratio * 100).ToString("0.0")}%";
  }
}