using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class TxtEventHandler : MonoBehaviour, IPointerClickHandler
{
  [Serializable]
  public class CharacterSelectionEvent : UnityEvent<char, int> { }

  /// <summary>
  /// Event delegate triggered when pointer is over a character.
  /// </summary>
  public CharacterSelectionEvent onCharacterSelection
  {
    get { return m_OnCharacterSelection; }
    set { m_OnCharacterSelection = value; }
  }
  [SerializeField]
  private CharacterSelectionEvent m_OnCharacterSelection = new CharacterSelectionEvent();

  private TMP_Text m_TextComponent;

  private Camera m_Camera;
  private Canvas m_Canvas;

  private int m_lastCharIndex = -1;

  void Awake()
  {
    // Get a reference to the text component.
    m_TextComponent = gameObject.GetComponent<TMP_Text>();

    // Get a reference to the camera rendering the text taking into consideration the text component type.
    if (m_TextComponent.GetType() == typeof(TextMeshProUGUI))
    {
      m_Canvas = gameObject.GetComponentInParent<Canvas>();
      if (m_Canvas != null)
      {
        if (m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
          m_Camera = null;
        else
          m_Camera = m_Canvas.worldCamera;
      }
    }
    else
    {
      m_Camera = Camera.main;
    }
  }


  private void SendOnCharacterSelection(char character, int characterIndex)
  {
    onCharacterSelection?.Invoke(character, characterIndex);
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (TMP_TextUtilities.IsIntersectingRectTransform(m_TextComponent.rectTransform, Input.mousePosition, m_Camera))
    {
      #region Example of Character or Sprite Selection
      int charIndex = TMP_TextUtilities.FindIntersectingCharacter(m_TextComponent, Input.mousePosition, m_Camera, true);
      if (charIndex != -1 && charIndex != m_lastCharIndex)
      {
        m_lastCharIndex = charIndex;

        TMP_TextElementType elementType = m_TextComponent.textInfo.characterInfo[charIndex].elementType;

        // Send event to any event listeners depending on whether it is a character or sprite.
        if (elementType == TMP_TextElementType.Character)
          SendOnCharacterSelection(m_TextComponent.textInfo.characterInfo[charIndex].character, charIndex);
      }
      #endregion
    }
  }
}
