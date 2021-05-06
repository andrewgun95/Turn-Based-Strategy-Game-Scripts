using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GainExtraTurn: MonoBehaviour, PlayerEffect
{
    public int amountTurn;
    public string targetArea;

    public void Apply(Player player)
    {
        const int delayMessage = 10; // Show message in 5 s

        MessageDialog message = MessageDialog.Instance();
        message.SetMessage(string.Format("Gain extra turn \"{0}\" into \"{1}\" area", amountTurn, targetArea));

        message.Show();
        StartCountdown(delayMessage, () =>
        {
            message.Hide();
        });
    }

    #region Countdown Timer
    private float timeRemaining;
    private bool timeIsRunning;

    private Action callbackHandler;

    void Start() {
        timeRemaining = 0;
        timeIsRunning = false;
    }

    private void StartCountdown(float amount, Action over) {
        timeRemaining = amount;
        timeIsRunning = true;
        callbackHandler = over;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                callbackHandler();
                timeRemaining = 0;
                timeIsRunning = false;
            }
        }
    }
    #endregion
}
