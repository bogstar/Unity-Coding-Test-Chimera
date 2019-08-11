using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// UI panel to show current turn.
/// </summary>
public class TurnPanel : MonoBehaviour
{
    #region Editor properties
    [SerializeField] private TextMeshProUGUI turnLabel;
    #endregion

    #region Public methods
    /// <summary>
    /// Display panel.
    /// </summary>
    /// <param name="display"></param>
    public void Display(bool display)
    {
        gameObject.SetActive(display);
    }

    /// <summary>
    /// Set panel content.
    /// </summary>
    /// <param name="phase"></param>
    public void SetContent(string turn)
    {
        turnLabel.text = turn;
    }
    #endregion
}
