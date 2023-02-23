using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;

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

  int _currentChapter;
  float _scrollPositionSaveTimer = 1;
  float _normalizedAutoScrollSpeed;
  bool _isAuto;
  float _speedMultiplier = 1;
  List<string> _chapterNames;
  string _bookName;
  string _bookDir;
  TMP_Text _contentText;
  TxtEventHandler _txtEventHandler;
  string _content;

  void Start()
  {
    if (!PlayerPrefs.HasKey(PlayerPrefKeys.CURRENT_BOOK_PREF))
    {
      ToMenu();
      return;
    }

    _bookName = PlayerPrefs.GetString(PlayerPrefKeys.CURRENT_BOOK_PREF);
    _bookDir = PlayerPrefs.GetString(PlayerPrefKeys.CURRENT_BOOK_DIR_PREF) ?? BooksManager.BOOK_DIRECTORY;
    var path = $"{Application.persistentDataPath}{_bookDir}{_bookName}/";
    if (!Directory.Exists(path))
    {
      ToMenu();
      return;
    }
    try
    {
      _chapterNames = BooksManager.Instance.GetChapterList(_bookDir, _bookName);
      var chapter = PlayerPrefs.GetInt($"{_bookName}{PlayerPrefKeys.CURRENT_CHAPTER_SUFFIX_PREF}", 1);
      GoToChapter(chapter);
      var scrollPos = PlayerPrefs.GetFloat(PlayerPrefKeys.CURRENT_SCROLL_PREF, 1);
      scrollRect.verticalNormalizedPosition = scrollPos;
      SetSpeedMultiplier(PlayerPrefs.GetFloat(PlayerPrefKeys.SPEED_MULTIPLIER_PREF, 1));
    }
    catch
    {
      ToMenu();
      return;
    }

    translationPart.SetActive(_bookDir == BooksManager.BOOK_CN_DIRECTORY);

    goToTopButton.sizeDelta = goToTopButton.sizeDelta.WithY(Screen.safeArea.y + 150);
    textVerticalLayoutGroup.padding.top = Mathf.RoundToInt(goToTopButton.sizeDelta.y) + 100;
    textVerticalLayoutGroup.padding.bottom = Mathf.RoundToInt((scrollRect.transform as RectTransform).rect.height * 0.5f);
  }

  private void OnDestroy()
  {
    if (_txtEventHandler != null)
    {
      _txtEventHandler.onCharacterSelection.RemoveAllListeners();
    }
  }

  private void Update()
  {
    if (_isAuto && Mathf.Abs(scrollRect.velocity.y) < 0.1f)
    {
      scrollRect.verticalNormalizedPosition -= _normalizedAutoScrollSpeed * Time.deltaTime * _speedMultiplier;
      if (scrollRect.verticalNormalizedPosition <= 0) Next();
    }

    _scrollPositionSaveTimer -= Time.deltaTime;
    if (_scrollPositionSaveTimer <= 0)
    {
      PlayerPrefs.SetFloat(PlayerPrefKeys.CURRENT_SCROLL_PREF, scrollRect.verticalNormalizedPosition);
      _scrollPositionSaveTimer += 1;
    }

    if (Input.GetKeyDown(KeyCode.Escape)) ToMenu();
  }

  public void OnValueChanged(string value)
  {
    try
    {
      var clampedVal = Mathf.Clamp(int.Parse(value), 1, _chapterNames.Count);
      chapterNumberInput.text = clampedVal.ToString();
    }
    catch { }
  }

  void GoToChapter(int chapterIndexBase1)
  {
    if (chapterIndexBase1 == _currentChapter && chapterIndexBase1 != 1) return;

    try
    {
      _content = BooksManager.Instance.GetChapter(_bookDir, _bookName, _chapterNames[chapterIndexBase1 - 1]);
      for (var i = 0; i < container.childCount; i++) Destroy(container.GetChild(i).gameObject);
      var paragraphText = Instantiate(textPrefab, container);
      _contentText = paragraphText.GetComponent<TMP_Text>();

      if (_bookDir == BooksManager.BOOK_CN_DIRECTORY)
      {
        _txtEventHandler = paragraphText.GetComponent<TxtEventHandler>();
        _txtEventHandler.onCharacterSelection.AddListener(Translate);

        var sb = new StringBuilder();
        var dict = Services<IWordDatabaseService>.Get().WordDatabase;
        foreach (var c in _content)
        {
          if (dict.ContainsKey(c))
          {
            sb.Append($"<color=#ffffbb>{c}</color>");
          }
          else sb.Append(c);
        }

        _content = sb.ToString();
      }
      
      _contentText.text = _content;


      chapterNumberInput.text = chapterIndexBase1.ToString();
      _currentChapter = chapterIndexBase1;
      PlayerPrefs.SetInt($"{_bookName}{PlayerPrefKeys.CURRENT_CHAPTER_SUFFIX_PREF}", _currentChapter);
      GoToTop();
      StartCoroutine(UpdateAutoSpeed());
    }
    catch
    {
      if (_currentChapter != 1) GoToChapter(1);
    }
  }
  IEnumerator UpdateAutoSpeed()
  {
    _normalizedAutoScrollSpeed = 0;
    yield return null;
    _normalizedAutoScrollSpeed = SCROLL_SPEED / container.rect.height;
  }

  void Translate(char c, int index)
  {
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
    if (_currentChapter < _chapterNames.Count) GoToChapter(_currentChapter + 1);
  }

  public void Prev()
  {
    if (_currentChapter > 1) GoToChapter(_currentChapter - 1);
  }

  public void ToggleAutoScroll()
  {
    _isAuto = !_isAuto;
    autoText.text = _isAuto ? "Stop" : "Auto";
    Screen.sleepTimeout = _isAuto ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
    speedMultiplierButton.gameObject.SetActive(_isAuto);
    chapterNumberInput.gameObject.SetActive(!_isAuto);
    SetSpeedSliderVisibility(_isAuto);
  }

  public void SetSpeedMultiplier(float value)
  {
    _speedMultiplier = value;
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
