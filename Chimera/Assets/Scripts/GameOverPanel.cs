using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI gameOverLabel;

    public void DisplayPanel(string text)
    {
        gameObject.SetActive(true);
        gameOverLabel.text = text;
    }
}
