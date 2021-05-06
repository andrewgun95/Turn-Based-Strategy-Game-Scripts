using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class GenericDialog : MonoBehaviour
{
    public Text title;
    public Text message;
    public Text acceptTxt, declineTxt;
    public Button acceptBtn, declineBtn;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public GenericDialog SetOnAccept(string text, UnityAction action)
    {
        acceptTxt.text = text;
        acceptBtn.onClick.RemoveAllListeners();
        acceptBtn.onClick.AddListener(action);
        return this;
    }

    public GenericDialog SetOnDecline(string text, UnityAction action)
    {
        declineTxt.text = text;
        declineBtn.onClick.RemoveAllListeners();
        declineBtn.onClick.AddListener(action);
        return this;
    }

    public GenericDialog SetTitle(string title)
    {
        this.title.text = title;
        return this;
    }

    public GenericDialog SetMessage(string message)
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

    private static GenericDialog instance;
    public static GenericDialog Instance()
    {
        if (!instance)
        {
            instance = FindObjectOfType(typeof(GenericDialog)) as GenericDialog;
            if (!instance)
                Debug.Log("There need to be at least one active GenericDialog on the scene");
        }

        return instance;
    }

}