using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Lean.Touch;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json.Linq;

public class ReadSceneManager : MonoBehaviour
{
  const float SCROLL_SPEED = 115f;
  public GameObject textPrefab;
  public RectTransform container;
  public InputField chapterNumberInput;
  public ScrollRect scrollRect;
  public Slider speedSlider;
  public Button speedMultiplierButton;
  public Text autoText;
  int currentChapter;
  float scrollPositionSaveTimer = 1;
  float normalizedAutoScrollSpeed = 0f;
  bool isAuto = false;
  float speedMultiplier = 1;
  public Color speedButtonActiveColor;
  public Color speedButtonInactiveColor;

  List<string> chapterNames;
  string bookName;
  void Start()
  {
    if (!PlayerPrefs.HasKey(PlayerPrefKeys.CURRENT_BOOK_PREF))
    {
      ToMenu();
      return;
    }

    bookName = PlayerPrefs.GetString(PlayerPrefKeys.CURRENT_BOOK_PREF);
    var path = $"{Application.persistentDataPath}{BooksManager.BOOK_DIRECTORY}/{bookName}/";
    if (!Directory.Exists(path))
    {
      ToMenu();
      return;
    }
    try
    {
      chapterNames = BooksManager.Instance.GetChapterList(bookName);
      int chapter = PlayerPrefs.GetInt($"{bookName}{PlayerPrefKeys.CURRENT_CHAPTER_SUFFIX_PREF}", 1);
      GoToChapter(chapter);
      float scrollPos = PlayerPrefs.GetFloat(PlayerPrefKeys.CURRENT_SCROLL_PREF, 1);
      scrollRect.verticalNormalizedPosition = scrollPos;
      SetSpeedMultiplier(PlayerPrefs.GetFloat(PlayerPrefKeys.SPEED_MULTIPLIER_PREF, 1));
    }
    catch
    {
      ToMenu();
      return;
    }
  }

  private void Update()
  {
    if (isAuto && Mathf.Abs(scrollRect.velocity.y) < 0.1f)
    {
      scrollRect.verticalNormalizedPosition -= normalizedAutoScrollSpeed * Time.deltaTime * speedMultiplier;
      if (scrollRect.verticalNormalizedPosition <= 0) Next();
    }

    scrollPositionSaveTimer -= Time.deltaTime;
    if (scrollPositionSaveTimer <= 0)
    {
      PlayerPrefs.SetFloat(PlayerPrefKeys.CURRENT_SCROLL_PREF, scrollRect.verticalNormalizedPosition);
      scrollPositionSaveTimer += 1;
    }
  }

  public void OnValueChanged(string value)
  {
    try
    {
      int clampedVal = Mathf.Clamp(int.Parse(value), 1, chapterNames.Count);
      chapterNumberInput.text = clampedVal.ToString();
    }
    catch { }
  }

  void GoToChapter(int chapterIndexBase1)
  {
    if (chapterIndexBase1 == currentChapter && chapterIndexBase1 != 1) return;

    try
    {
      var content = BooksManager.Instance.GetChapter(bookName, chapterNames[chapterIndexBase1 - 1]);
      for (int i = 0; i < container.childCount; i++) Destroy(container.GetChild(i).gameObject);
      var paragraphText = Instantiate(textPrefab, container);
      paragraphText.GetComponent<TMP_Text>().text = $"\n\n{content}";

      chapterNumberInput.text = chapterIndexBase1.ToString();
      currentChapter = chapterIndexBase1;
      PlayerPrefs.SetInt($"{bookName}{PlayerPrefKeys.CURRENT_CHAPTER_SUFFIX_PREF}", currentChapter);
      GoToTop();
      StartCoroutine(UpdateAutoSpeed());
    }
    catch
    {
      if (currentChapter != 1) GoToChapter(1);
    }
  }
  IEnumerator UpdateAutoSpeed()
  {
    normalizedAutoScrollSpeed = 0;
    yield return null;
    normalizedAutoScrollSpeed = SCROLL_SPEED / container.rect.height;
  }

  public void GoToTop()
  {
    scrollRect.velocity = Vector2.zero;
    scrollRect.verticalNormalizedPosition = 1;
  }

  public void Go()
  {
    GoToChapter(int.Parse(chapterNumberInput.text));
  }

  public void Next()
  {
    if (currentChapter < chapterNames.Count) GoToChapter(currentChapter + 1);
  }

  public void Prev()
  {
    if (currentChapter > 1) GoToChapter(currentChapter - 1);
  }

  public void ToggleAutoScroll()
  {
    isAuto = !isAuto;
    autoText.text = isAuto ? "Stop" : "Auto";
    Screen.sleepTimeout = isAuto ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
    speedMultiplierButton.gameObject.SetActive(isAuto);
    chapterNumberInput.gameObject.SetActive(!isAuto);
    SetSpeedSliderVisibility(isAuto);
  }

  public void SetSpeedMultiplier(float value)
  {
    speedMultiplier = value;
    if (value != speedSlider.value) speedSlider.value = value;
    PlayerPrefs.SetFloat(PlayerPrefKeys.SPEED_MULTIPLIER_PREF, value);
    speedMultiplierButton.GetComponentInChildren<Text>().text = $"{value.ToString("0.0")}x";
  }

  public void ToggleSpeedSlider()
  {
    SetSpeedSliderVisibility(!speedSlider.gameObject.activeSelf);
  }

  void SetSpeedSliderVisibility(bool shown)
  {
    speedSlider.gameObject.SetActive(shown);
    speedMultiplierButton.image.color = shown ? speedButtonActiveColor : speedButtonInactiveColor;
  }

  IEnumerator UpdateSliderPosition()
  {
    yield return null;
    SetSpeedMultiplier(PlayerPrefs.GetFloat(PlayerPrefKeys.SPEED_MULTIPLIER_PREF, 1));
  }

  public void ToMenu()
  {
    PlayerPrefs.DeleteKey(PlayerPrefKeys.CURRENT_BOOK_PREF);
    SceneManager.LoadScene(0);
  }
}
