using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameResult : MonoBehaviour
{
    public Canvas gameCanvas;

    public GameObject spawnPoint;
    public GameObject renderText;

    private string playerWinner;

    public void Awake() {
    }

    public void LoadResult(GameObject[] cardObjects, List<GameObject> playerObjects) {
        GameObject panelResult = gameCanvas.transform.Find("GameResult/PlayerPoints/Viewport/Content").gameObject;

        // # LOAD RESULTS DATA
        // Load player headers
        GameObject playerHeaders = LoadPlayerHeaders(panelResult);
        // Load card player points
        Dictionary<string, GameObject> cardPlayerPoints = LoadCardPlayerPoints(panelResult, cardObjects);
        // Load gold results
        GameObject goldResults = LoadGoldResults(panelResult);

        // # CALC RESULTS DATA
        playerWinner = CalcResult(playerHeaders, cardPlayerPoints, goldResults, playerObjects);

        // Refresh panel result layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelResult.GetComponent<RectTransform>());
    }

    private GameObject LoadPlayerHeaders(GameObject panelResult) {
        // Create a spawn point for getting all player headers
        GameObject playerHeaders = Instantiate(spawnPoint, new Vector3(0, 0, 0), Quaternion.identity);
        playerHeaders.transform.SetParent(panelResult.transform, false);
        // Add initial "#" text to player headers
        AddTextToSpawnPoint("#", playerHeaders);

        return playerHeaders;
    }

    private Dictionary<string, GameObject> LoadCardPlayerPoints(GameObject panelResult, GameObject[] cardObjects) {
        Dictionary<string, GameObject> cardDictionary = new Dictionary<string, GameObject>();
        foreach (GameObject cardObject in cardObjects)
        {
            Card card = cardObject.GetComponent<Card>();
            string cardName = card.GetTagName();

            // Create a spawn point for getting all player points
            GameObject playerPoints = Instantiate(spawnPoint, new Vector3(0, 0, 0), Quaternion.identity);
            playerPoints.transform.SetParent(panelResult.transform, false);
            cardDictionary.Add(cardName, playerPoints);
            // Add card name text to player points
            AddTextToSpawnPoint(cardName, playerPoints);
        }
        return cardDictionary;
    }

    private GameObject LoadGoldResults(GameObject panelResult) {
        // Create a spawn for getting all gold results
        GameObject goldResults = Instantiate(spawnPoint, new Vector3(0, 0, 0), Quaternion.identity);
        goldResults.transform.SetParent(panelResult.transform, false);
        // Add initial "Results" text to gold results
        AddTextToSpawnPoint("Results", goldResults);

        return goldResults;
    }

    private string CalcResult(GameObject playerHeaders, Dictionary<string, GameObject> cardPlayerPoints, GameObject goldResults, List<GameObject> playerObjects) {
        int maxGolds = 0;
        string playerWinner = "";
        foreach (GameObject playerObject in playerObjects)
        {
            Player player = playerObject.GetComponent<Player>();

            int playerGolds = 0;
            foreach (PlayerEffect playerEffect in player.playerEffects)
            {
                if (playerEffect is Card)
                {
                    Card card = (Card)playerEffect;
                    string tagName = card.GetTagName();
                    if (cardPlayerPoints.ContainsKey(tagName))
                    {
                        GameObject playerPoints = cardPlayerPoints[tagName];
                        // Get gold point from the player
                        int goldPoint = card.GetGold(player);
                        // Adding to gold results
                        playerGolds += goldPoint;
                        // Add gold point text to player points
                        AddTextToSpawnPoint(string.Format("{0} gold", goldPoint), playerPoints);
                    }
                }
            }
            // Find player winner
            if (playerGolds > maxGolds)
            {
                maxGolds = playerGolds;
                playerWinner = player.playerName;
            }

            // Add player name  text to player headers
            AddTextToSpawnPoint(player.playerName, playerHeaders);
            // Add player golds text to gold results
            AddTextToSpawnPoint(string.Format("{0} gold", playerGolds), goldResults);
        }

        return playerWinner;
    }

    private void ShowMessageWinner(string playerName)
    {
        MessageDialog message = MessageDialog.Instance();
        message.SetMessage(string.Format("{0} is the WINNER!", playerName));

        message.Show();
    }

    public void Show()
    {
        gameCanvas.gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameCanvas.gameObject.SetActive(false);
    }

    private void AddTextToSpawnPoint(string textString, GameObject spawnObject)
    {
        // Add text into spawn point
        GameObject spawnText = Instantiate(renderText, new Vector3(0, 0, 0), Quaternion.identity);
        Text newText = spawnText.GetComponent<Text>();
        newText.text = textString;
        spawnText.transform.SetParent(spawnObject.transform, false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        if (playerWinner.Trim().Length > 0) {
            ShowMessageWinner(playerWinner);
        }
    }


}
