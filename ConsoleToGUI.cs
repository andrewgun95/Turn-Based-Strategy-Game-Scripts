using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleToGUI : MonoBehaviour
{
    private string strLog = "*begin log";

    private string fileName = "";

    private bool show = false;

    int maxCharacters = 10000;
    void OnEnable() { Application.logMessageReceived += Log; }
    void OnDisable() { Application.logMessageReceived -= Log; }
    void Update() { if (Input.GetKeyDown(KeyCode.Space)) { show = !show; } }
    public void Log(string logString)
    {
        Log(logString, "", LogType.Log);
    }

    public void Log(string logString, LogType logType) {
        Log(logString, "", logType);
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        // for onscreen...
        strLog = strLog + "\n" + logString;
        if (strLog.Length > maxCharacters) { strLog = strLog.Substring(strLog.Length - maxCharacters); }

        // for the file ...
        if (fileName == "")
        {
            string d = System.Environment.GetFolderPath(
               System.Environment.SpecialFolder.Desktop) + "/LOGS";
            System.IO.Directory.CreateDirectory(d);
            string r = Random.Range(1000, 9999).ToString();
            fileName = d + "/log-" + r + ".txt";
        }
        try { System.IO.File.AppendAllText(fileName, logString + "\n"); }
        catch { }
    }

    void OnGUI()
    {
        if (!show) { return; }
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
           new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));

        GUI.TextArea(new Rect(10, 10, 540, 370), strLog);
    }
}
