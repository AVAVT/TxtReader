using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

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
  public Color speedButtonActiveColor;
  public Color speedButtonInactiveColor;
  public RectTransform goToTopButton;
  public VerticalLayoutGroup textVerticalLayoutGroup;
  public GameObject translationPart;
  public Translator translator;

  int currentChapter;
  float scrollPositionSaveTimer = 1;
  float normalizedAutoScrollSpeed = 0f;
  bool isAuto = false;
  float speedMultiplier = 1;
  List<string> chapterNames;
  string bookName;
  string bookDir;
  TMP_Text contentText;
  TxtEventHandler txtEventHandler;
  string content;

  void Start()
  {
    if (!PlayerPrefs.HasKey(PlayerPrefKeys.CURRENT_BOOK_PREF))
    {
      ToMenu();
      return;
    }

    bookName = PlayerPrefs.GetString(PlayerPrefKeys.CURRENT_BOOK_PREF);
    bookDir = PlayerPrefs.GetString(PlayerPrefKeys.CURRENT_BOOK_DIR_PREF) ?? BooksManager.BOOK_DIRECTORY;
    var path = $"{Application.persistentDataPath}{bookDir}{bookName}/";
    if (!Directory.Exists(path))
    {
      ToMenu();
      return;
    }
    try
    {
      chapterNames = BooksManager.Instance.GetChapterList(bookDir, bookName);
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

    translationPart.SetActive(bookDir == BooksManager.BOOK_CN_DIRECTORY);

    goToTopButton.sizeDelta = goToTopButton.sizeDelta.WithY(Screen.safeArea.y + 150);
    textVerticalLayoutGroup.padding.top = Mathf.RoundToInt(goToTopButton.sizeDelta.y) + 100;
    textVerticalLayoutGroup.padding.bottom = Mathf.RoundToInt((scrollRect.transform as RectTransform).rect.height * 0.5f);
  }

  private void OnDestroy()
  {
    if (txtEventHandler != null)
    {
      txtEventHandler.onCharacterSelection.RemoveAllListeners();
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

    if (Input.GetKeyDown(KeyCode.Escape)) ToMenu();
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
      content = BooksManager.Instance.GetChapter(bookDir, bookName, chapterNames[chapterIndexBase1 - 1]);
      for (int i = 0; i < container.childCount; i++) Destroy(container.GetChild(i).gameObject);
      var paragraphText = Instantiate(textPrefab, container);
      contentText = paragraphText.GetComponent<TMP_Text>();
      contentText.text = content;

      if (bookDir == BooksManager.BOOK_CN_DIRECTORY)
      {
        txtEventHandler = paragraphText.GetComponent<TxtEventHandler>();
        txtEventHandler.onCharacterSelection.AddListener(Translate);
      }

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

  void Translate(char c, int index)
  {
    contentText.text = content.Substring(0, index) + "<color=#ff00ff>" + content.Substring(index, 1) + "</color>" + content.Substring(index + 1);
    translator.Translate(c).ConfigureAwait(false);
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
