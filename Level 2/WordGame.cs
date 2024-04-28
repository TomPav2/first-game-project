using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SceneLoader;

public class WordGame : MonoBehaviour
{
    [SerializeField] private GameObject letterPanelPrefab;
    [SerializeField] private TextAsset wordList;
    [SerializeField] private TextHudController hudController;
    [SerializeField] private MainCharacterSheet mainCharacterSheet;
    [SerializeField] private GameObject letterContainer;
    [SerializeField] private WordGameSensor sensor;
    [SerializeField] private Level2Manager levelManager;
    [SerializeField] private GameObject prompt;
    [SerializeField] private GameObject barrier;

    private static readonly short OFFSET_X = -180;
    private static readonly short OFFSET_Y = -300;

    private static readonly Color COLOR_CORRECT = new Color(0, 0.8f, 0);
    private static readonly Color COLOR_CLOSE = new Color(1f, 0.5f, 0);

    protected static GameObject[,] letterArray = new GameObject[6, 5];
    protected static GameObject[] inputArray = new GameObject[5];
    private string[] possibleWords;

    protected static byte attempt = 0;
    private bool needSetup = true;
    private bool playerInRange = false;

    public void playerApproached()
    {
        playerInRange = true;
    }

    public void playerLeft()
    {
        playerInRange = false;
    }

    private void Update()
    {
        if (inMinigame)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                closeGame();
                return;
            }
            foreach (char c in Input.inputString)
            {
                if (c == '\b') // has backspace/delete been pressed?
                {
                    InputHandler.remove();
                }
                else if ((c == '\n') || (c == '\r')) // enter/return
                {
                    char[] wordToGuess = InputHandler.send();
                    if (wordToGuess != null)
                    {
                        bool result = Guesser.guessWord(wordToGuess);
                        attempt++;
                        if (result) success();
                        else if (attempt == 6) failure();
                    }
                }
                else
                {
                    InputHandler.add(c);
                }
            }
        }
        else if (playerInRange && Input.GetKeyDown(KeyCode.Space))
        {
            prompt.SetActive(false);
            openGame();
        }
    }

    private void openGame()
    {
        lockControls = true;
        inMinigame = true;
        GetComponent<Image>().enabled = true;
        letterContainer.SetActive(true);
        setupWordGame();
    }

    private void closeGame()
    {
        lockControls = false;
        inMinigame = false;
        GetComponent<Image>().enabled = false;
        letterContainer.SetActive(false);
        StartCoroutine(delayedToggle());

        if (attempt > 0)
        {
            hudController.popUp("Never give up!", "Or else...", null);
            mainCharacterSheet.damage(1);
        }
    }

    private void success()
    {
        attempt = 0;
        closeGame();
        hudController.popUp("Correct!", "The word was " + Guesser.TARGET_WORD, null);
        GameObject.Destroy(sensor.gameObject);
        StartCoroutine(endThis());
    }

    private void failure()
    {
        attempt = 0;
        closeGame();
        hudController.popUp("Wrong!", "The word was " + Guesser.TARGET_WORD, null);
        mainCharacterSheet.damage(1);
    }

    private void setupWordGame()
    {
        Guesser.TARGET_WORD = getRandomWord();
        for (byte word = 0; word < letterArray.GetLength(0); word++)
        {
            for (byte letter = 0; letter < letterArray.GetLength(1); letter++)
            {
                if (needSetup) letterArray[word, letter] = makeLP(letter, word);
                else clear(word, letter);
            }
        }

        for (byte i = 0; i < 5; i++)
        {
            inputArray[i] = makeLP(i, 7);
        }

        attempt = 0;
        needSetup = false;
        InputHandler.clearInput();
    }

    private string getRandomWord()
    {
        //if (wordArray == null) possibleWords = wordList.text.ToUpper().Split(',');
        //return possibleWords[UnityEngine.Random.Range(0, possibleWords.Count())]; // TODO temporary

        return "APPLE";
    }

    private GameObject makeLP(byte x, byte y)
    {
        GameObject newLetterPanel = Instantiate(letterPanelPrefab);
        newLetterPanel.transform.SetParent(letterContainer.transform);
        newLetterPanel.transform.localScale = Vector3.one;
        newLetterPanel.transform.localPosition = new Vector3(x * 90 + OFFSET_X, -(y * 90 + OFFSET_Y), 0);
        return newLetterPanel;
    }

    private void clear(byte word, byte letter)
    {
        letterArray[word, letter].GetComponentInChildren<TextMeshProUGUI>().enabled = false;
        letterArray[word, letter].GetComponent<Image>().color = Color.white;
    }
    
    private IEnumerator endThis()
    {
        yield return new WaitForSeconds(3);
        levelManager.givePlayerHeart(barrier);
        hudController.popUp("You got a heart!", "It's quite heavy...", "Hand it in before continuing.");
        GameObject.Destroy(gameObject);
        yield break;
    }

    // TODO rework this so the minigame is closed by universal controller
    private IEnumerator delayedToggle()
    {
        bool toggle = true;
        while (toggle)
        {
            toggle = false;
            yield return null;
        }
        inMinigame = false;
        yield break;
    }

    // ----------- INPUT ------------
    private static class InputHandler
    {
        private static byte index = 0;
        private static char[] letters = new char[5];

        public static void add(char c)
        {
            if (index > 4 || !Char.IsLetter(c)) return;
            char letter = Char.ToUpper(c);
            letters[index] = letter;
            showOnInput(letter.ToString(), index);
            index++;
        }

        public static void remove()
        {
            if (index == 0) return;
            index--;
            showOnInput("", index);
        }

        public static char[] send()
        {
            if (index < 5) return null;
            clearInput();
            return letters;
        }

        public static void clearInput()
        {
            index = 0;
            for (byte i = 0; i < 5; i++)
            {
                showOnInput("", i);
            }
        }

        private static void showOnInput(string letter, byte index)
        {
            inputArray[index].GetComponentInChildren<TextMeshProUGUI>().SetText(letter);
        }
    }

    // ----------- GUESS ------------
    private static class Guesser
    {
        // GUESS VALUE
        public static string TARGET_WORD { get; set; }

        // GUESSING MECHANICS
        public static bool guessWord(char[] guess)
        {
            bool[] primaryResult = primaryGuess(guess);

            if (checkResult(primaryResult)) return true;

            fillCloseGuesses(guess, primaryResult);

            return false;
        }

        private static bool[] primaryGuess(char[] guess)
        {
            bool[] correctGuesses = new bool[5];

            for (int i = 0; i < 5; i++)
            {
                if (guess[i] == TARGET_WORD[i])
                {
                    letterFill(letterArray[attempt, i], guess[i], COLOR_CORRECT);
                    correctGuesses[i] = true;
                }
                else letterFill(letterArray[attempt, i], guess[i], Color.white);
            }
            return correctGuesses;
        }

        private static bool checkResult(bool[] correctGuesses)
        {
            for (int i = 0; i < 5; i++)
            {
                if (!correctGuesses[i]) return false;
            }
            return true;
        }

        private static void fillCloseGuesses(char[] guess, bool[] filledGuesses)
        {
            bool[] guessedLetters = (bool[]) filledGuesses.Clone();

            for (int i = 0; i < 5; i++) // outer loop: iterating remaining guessed letters
            {
                if (filledGuesses[i]) continue;
                for (int j = 0; j < 5; j++) // inner loop: testing them against remaining correct letters
                {
                    if (guessedLetters[j]) continue;
                    if (guess[i] == TARGET_WORD[j])
                    {
                        letterColor(letterArray[attempt, i]);
                        guessedLetters[j] = true;
                        break;
                    }
                }
            }
        }

        // GUI CONTROL
        private static void letterFill(GameObject letterPanel, char letter, Color color)
        {
            letterPanel.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
            letterPanel.GetComponentInChildren<TextMeshProUGUI>().SetText(letter.ToString());
            letterPanel.GetComponent<Image>().color = color;
        }

        private static void letterColor(GameObject letterPanel)
        {
            letterPanel.GetComponent<Image>().color = COLOR_CLOSE;
        }
    }
}