using System;
using TMPro;
using UnityEngine;

public class DeleteCanvasController : MonoBehaviour
{
  public TMP_Text bookNameText;
  private string bookName;
  private Action<bool> OnComplete;
  public void ShowConfirmDelete(string bookName, Action<bool> OnComplete)
  {
    this.bookName = bookName;
    bookNameText.text = bookName;
    gameObject.SetActive(true);
    this.OnComplete = OnComplete;
  }

  public void SendResult(bool result)
  {
    gameObject.SetActive(false);
    OnComplete?.Invoke(result);
  }
}