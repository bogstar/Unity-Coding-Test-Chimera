using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manager used for controlling notifications.
/// </summary>
public class NotificationManager : Manager<NotificationManager>
{
    #region Editor parameters
    [Header("References")]
    [SerializeField] private TextMeshProUGUI notificationLabel;
    [SerializeField] private ScrollRect scrollRect;
    [Header("Values")]
    [SerializeField] private int maximumMessages = 50;
    #endregion

    #region Unity methods
    private void Start()
    {
        notificationLabel.text = "";
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Displays a message on the notification tracker.
    /// </summary>
    /// <param name="message"></param>
    public void PublishNotification(string message)
    {
        notificationLabel.text += message + "\n";

        var messages = notificationLabel.text.Split('\n');

        // If there are too many messages, remove the first message.
        if (messages.Length > maximumMessages)
        {
            int firstNewLine = notificationLabel.text.IndexOf('\n');

            notificationLabel.text = notificationLabel.text.Substring(firstNewLine + 1);
        }

        StartCoroutine(PublishNotification());
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Used internally for making sure the scroll rect scrolls down on time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PublishNotification()
    {
        yield return new WaitForSeconds(0.2f);

        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
    #endregion
}