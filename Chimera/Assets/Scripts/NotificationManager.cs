using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : Manager<NotificationManager>
{
    [SerializeField]
    private TextMeshProUGUI notificationLabel;
    [SerializeField]
    private ScrollRect scrollRect;

    private void Start()
    {
        notificationLabel.text = "";
    }

    public void PublishNotification(string message)
    {
        notificationLabel.text += message + "\n";

        var messages = notificationLabel.text.Split('\n');

        if (messages.Length > 50)
        {
            int firstNewLine = notificationLabel.text.IndexOf('\n');

            notificationLabel.text = notificationLabel.text.Substring(firstNewLine + 1);
        }

        StartCoroutine(PublishNotification());
    }

    private IEnumerator PublishNotification()
    {
        yield return new WaitForSeconds(0.2f);

        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
}