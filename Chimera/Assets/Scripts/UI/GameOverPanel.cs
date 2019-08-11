using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// UI panel for displaying Game Over screen.
/// </summary>
public class GameOverPanel : MonoBehaviour
{
    #region Editor properties
    [SerializeField] private TextMeshProUGUI gameOverLabel;
    #endregion

    #region Public methods
    /// <summary>
    /// Display game over panel with custom text.
    /// </summary>
    /// <param name="text">Custom text.</param>
    public void DisplayPanel(string text)
    {
        gameObject.SetActive(true);
        gameOverLabel.text = text;
    }
    #endregion
}
