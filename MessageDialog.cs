using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class MessageDialog : MonoBehaviour
{
    public Text message;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public MessageDialog SetMessage(string message)
    {
        this.message.text = message;
        return this;
    }

    public void Show()
    {
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0f;
    }

    private static MessageDialog instance;
    public static MessageDialog Instance()
    {
        if (!instance)
        {
            instance = FindObjectOfType(typeof(MessageDialog)) as MessageDialog;
            if (!instance)
                Debug.Log("There need to be at least one active MessageDialog on the scene");
        }

        return instance;
    }

}
