using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    public static bool inDialogue = false;

    public float interactDistance = 3f;
    public int totalEvidence = 3;
    public int maxHealth = 3;

    int currentHealth;
    bool isGameOver = false;
    bool isWin = false;

    enum MenuState { MainMenu, LevelSelect, Playing }
    MenuState menuState = MenuState.MainMenu;

    Camera cam;
    Evidence lookedAtEvidence;
    Evidence activePuzzleEvidence;
    NPC lookedAtNPC;
    NPC currentNPC;
    int lineIndex = 0;

    List<string> notebook = new List<string>();
    bool showNotebook = false;
    Texture2D bgTex;
    Texture2D darkBgTex;

    string feedbackMessage = "";
    float feedbackTimer = 0f;
    Color feedbackColor = Color.green;

    void Start()
    {
        cam = Camera.main;
        currentHealth = maxHealth;
        inDialogue = true;
        menuState = MenuState.MainMenu;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (menuState != MenuState.Playing)
            return;

        if (feedbackTimer > 0)
            feedbackTimer -= Time.deltaTime;

        if ((isWin || isGameOver) && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (isWin || isGameOver)
            return;

        if (activePuzzleEvidence != null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetKeyDown(KeyCode.Escape))
                ClosePuzzle();
            return;
        }

        if (inDialogue)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                lineIndex++;
                if (lineIndex >= currentNPC.lines.Length)
                {
                    inDialogue = false;
                    isWin = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            return;
        }

        lookedAtEvidence = null;
        lookedAtNPC = null;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            lookedAtEvidence = hit.collider.GetComponent<Evidence>();
            lookedAtNPC = hit.collider.GetComponent<NPC>();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (lookedAtEvidence != null)
            {
                activePuzzleEvidence = lookedAtEvidence;
                inDialogue = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (lookedAtNPC != null)
            {
                currentNPC = lookedAtNPC;
                lineIndex = 0;
                inDialogue = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
            showNotebook = !showNotebook;
    }

    void StartPlaying()
    {
        menuState = MenuState.Playing;
        inDialogue = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ClosePuzzle()
    {
        activePuzzleEvidence = null;
        inDialogue = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void HandleWrongAnswer(string customHint)
    {
        currentHealth--;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            activePuzzleEvidence = null;
            inDialogue = true;
            isGameOver = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            feedbackMessage = customHint + "  (-1 Health! Remaining: " + currentHealth + "/" + maxHealth + ")";
            feedbackColor = new Color(1f, 0.35f, 0.35f);
            feedbackTimer = 3.5f;
        }
    }

    void SolveCurrentPuzzle()
    {
        notebook.Add(activePuzzleEvidence.evidenceName + ": " + activePuzzleEvidence.description);

        if (activePuzzleEvidence.linkedLock != null)
        {
            activePuzzleEvidence.linkedLock.SetActive(false);
        }

        int unlockedCount = notebook.Count;
        if (unlockedCount < totalEvidence)
            feedbackMessage = "Lock " + unlockedCount + " Unlocked! (" + unlockedCount + "/" + totalEvidence + " Locks Broken)";
        else
            feedbackMessage = "ALL LOCKS BROKEN! Enter the room and talk to the Manager!";

        feedbackColor = Color.green;
        feedbackTimer = 4f;

        Destroy(activePuzzleEvidence.gameObject);
        ClosePuzzle();
    }

    void OnGUI()
    {
        if (bgTex == null)
        {
            bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0.92f, 0.92f, 0.92f));
            bgTex.Apply();
        }
        if (darkBgTex == null)
        {
            darkBgTex = new Texture2D(1, 1);
            darkBgTex.SetPixel(0, 0, new Color(0.12f, 0.14f, 0.19f, 0.97f));
            darkBgTex.Apply();
        }

        if (menuState == MenuState.MainMenu)
        {
            DrawMainMenu();
            return;
        }
        if (menuState == MenuState.LevelSelect)
        {
            DrawLevelSelectMenu();
            return;
        }

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 19;
        style.wordWrap = true;
        style.normal.background = bgTex;
        style.normal.textColor = Color.black;

        // Health HUD (Top Left)
        GUIStyle healthStyle = new GUIStyle(GUI.skin.box);
        healthStyle.fontSize = 20;
        healthStyle.fontStyle = FontStyle.Bold;
        healthStyle.normal.background = darkBgTex;
        healthStyle.normal.textColor = currentHealth > 1 ? new Color(1f, 0.4f, 0.4f) : Color.red;

        string hearts = "";
        for (int i = 0; i < maxHealth; i++)
            hearts += (i < currentHealth) ? " ♥" : " ♡";
        GUI.Box(new Rect(20, 20, 230, 45), "Health:" + hearts, healthStyle);

        // Objective HUD (Top Right)
        string objective;
        if (notebook.Count < totalEvidence)
            objective = "Objective: Solve 3 puzzles to free the Manager (" + notebook.Count + "/" + totalEvidence + ")";
        else if (!isWin)
            objective = "Objective: Enter the room and talk to the Manager";
        else
            objective = "Objective: Completed! Manager Freed.";

        GUI.Box(new Rect(Screen.width - 580, 20, 560, 45), objective, style);

        if (feedbackTimer > 0 && !isGameOver && !isWin)
        {
            GUIStyle alertStyle = new GUIStyle(GUI.skin.box);
            alertStyle.fontSize = 20;
            alertStyle.fontStyle = FontStyle.Bold;
            alertStyle.normal.background = darkBgTex;
            alertStyle.normal.textColor = feedbackColor;
            GUI.Box(new Rect(Screen.width / 2 - 340, 80, 680, 48), feedbackMessage, alertStyle);
        }

        if (isGameOver)
        {
            DrawEndScreen("GAME OVER", "You ran out of health! The security system locked down.", new Color(1f, 0.3f, 0.3f));
            return;
        }

        if (isWin)
        {
            DrawEndScreen("YOU WIN!", "Level 1 Complete! You freed the Manager & secured the evidence.", new Color(0.3f, 1f, 0.45f));
            return;
        }

        if (activePuzzleEvidence != null)
        {
            DrawPuzzleWindow();
            return;
        }

        if (!inDialogue)
        {
            GUIStyle crossStyle = new GUIStyle(GUI.skin.label);
            crossStyle.fontSize = 26;
            crossStyle.fontStyle = FontStyle.Bold;
            crossStyle.alignment = TextAnchor.MiddleCenter;
            crossStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(Screen.width / 2 - 15, Screen.height / 2 - 15, 30, 30), "+", crossStyle);

            if (lookedAtEvidence != null)
                GUI.Box(new Rect(Screen.width / 2 - 220, Screen.height / 2 + 40, 440, 45),
                        "Press E to inspect & solve: " + lookedAtEvidence.evidenceName, style);
            else if (lookedAtNPC != null)
                GUI.Box(new Rect(Screen.width / 2 - 220, Screen.height / 2 + 40, 440, 45),
                        "Press E to talk to " + lookedAtNPC.npcName, style);
        }

        if (inDialogue && currentNPC != null)
        {
            GUI.Box(new Rect(Screen.width / 2 - 400, Screen.height - 180, 800, 150),
                    currentNPC.npcName + ":\n" + currentNPC.lines[lineIndex] + "\n\n(Press E to continue)", style);
        }

        if (showNotebook)
        {
            GUI.Box(new Rect(20, 80, 600, 60 + notebook.Count * 35), "Detective Notebook (Press Tab to close)", style);
            for (int i = 0; i < notebook.Count; i++)
                GUI.Label(new Rect(30, 120 + i * 35, 580, 35), "- " + notebook[i], style);
        }
    }

    void DrawMainMenu()
    {
        float w = 520;
        float h = 420;
        float x = (Screen.width - w) / 2;
        float y = (Screen.height - h) / 2;

        GUIStyle panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.fontSize = 22;
        panelStyle.fontStyle = FontStyle.Bold;
        panelStyle.wordWrap = true;
        panelStyle.normal.background = darkBgTex;
        panelStyle.normal.textColor = Color.white;
        panelStyle.padding = new RectOffset(25, 25, 30, 25);

        GUI.Box(new Rect(x, y, w, h),
            "CYBER DETECTIVE\n" +
            "Security Breach Investigation\n\n" +
            "Controls: [WASD] Move | [E] Interact | [Tab] Notebook", panelStyle);

        GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
        btnStyle.fontSize = 20;
        btnStyle.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(x + 110, y + 175, 300, 50), "Start", btnStyle))
            StartPlaying();

        if (GUI.Button(new Rect(x + 110, y + 240, 300, 50), "Levels", btnStyle))
            menuState = MenuState.LevelSelect;

        if (GUI.Button(new Rect(x + 110, y + 305, 300, 50), "Quit", btnStyle))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    void DrawLevelSelectMenu()
    {
        float w = 540;
        float h = 430;
        float x = (Screen.width - w) / 2;
        float y = (Screen.height - h) / 2;

        GUIStyle panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.fontSize = 22;
        panelStyle.fontStyle = FontStyle.Bold;
        panelStyle.normal.background = darkBgTex;
        panelStyle.normal.textColor = Color.white;
        panelStyle.padding = new RectOffset(25, 25, 25, 25);

        GUI.Box(new Rect(x, y, w, h), "SELECT LEVEL", panelStyle);

        GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
        btnStyle.fontSize = 18;
        btnStyle.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(x + 70, y + 90, 400, 52), "Level 1: The Locked Glass Room (Play)", btnStyle))
            StartPlaying();

        GUI.enabled = false;
        GUI.Button(new Rect(x + 70, y + 160, 400, 52), "Level 2: Server Room Infiltration (Locked)", btnStyle);
        GUI.Button(new Rect(x + 70, y + 230, 400, 52), "Level 3: Exposing the Traitor (Locked)", btnStyle);
        GUI.enabled = true;

        if (GUI.Button(new Rect(x + 170, y + 320, 200, 45), "Back to Menu", btnStyle))
            menuState = MenuState.MainMenu;
    }

    void DrawEndScreen(string title, string subtitle, Color titleColor)
    {
        float w = 600;
        float h = 290;
        float x = (Screen.width - w) / 2;
        float y = (Screen.height - h) / 2;

        GUIStyle panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.fontSize = 20;
        panelStyle.fontStyle = FontStyle.Bold;
        panelStyle.wordWrap = true;
        panelStyle.normal.background = darkBgTex;
        panelStyle.normal.textColor = Color.white;
        panelStyle.padding = new RectOffset(25, 25, 85, 25);

        GUI.Box(new Rect(x, y, w, h), subtitle, panelStyle);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 36;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = titleColor;
        GUI.Label(new Rect(x, y + 20, w, 55), title, titleStyle);

        GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
        btnStyle.fontSize = 18;
        btnStyle.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(x + 60, y + h - 75, 220, 48), "Play Again [R]", btnStyle))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if (GUI.Button(new Rect(x + w - 280, y + h - 75, 220, 48), "Main Menu", btnStyle))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void DrawPuzzleWindow()
    {
        float w = Mathf.Min(760, Screen.width - 40);
        float h = Mathf.Min(410, Screen.height - 40);
        float x = (Screen.width - w) / 2;
        float y = (Screen.height - h) / 2;

        GUIStyle panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.fontSize = 16;
        panelStyle.fontStyle = FontStyle.Bold;
        panelStyle.wordWrap = true;
        panelStyle.normal.background = darkBgTex;
        panelStyle.normal.textColor = Color.white;
        panelStyle.padding = new RectOffset(20, 20, 15, 15);

        GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
        btnStyle.fontSize = 16;
        btnStyle.wordWrap = true;

        int type = activePuzzleEvidence.puzzleType;
        float btnH = 38;
        float btnStartY = y + h - 135;

        if (type == 1)
        {
            GUI.Box(new Rect(x, y, w, h),
                "PUZZLE 1: CODE COMPARISON (Lock 1)\n\n" +
                "Which signature proves the rival copied our source code?\n\n" +
                "Original Code:   hash = (key * 31) ^ 0x5F3759DF;\n" +
                "Suspect File A:  hash = (key + 12) & 0xFF;\n" +
                "Suspect File B:  hash = key % 1024;\n" +
                "Suspect File C:  hash = (key * 31) ^ 0x5F3759DF;", panelStyle);

            if (GUI.Button(new Rect(x + 30, btnStartY, w - 60, btnH), "1) Suspect File A (Uses bitwise AND)", btnStyle))
                HandleWrongAnswer("Wrong! That is a standard bitmask.");
            if (GUI.Button(new Rect(x + 30, btnStartY + 43, w - 60, btnH), "2) Suspect File B (Simple modulo remainder)", btnStyle))
                HandleWrongAnswer("Wrong! Look closely at the hex magic constant.");
            if (GUI.Button(new Rect(x + 30, btnStartY + 86, w - 60, btnH), "3) Suspect File C (Identical 0x5F3759DF constant)", btnStyle))
                SolveCurrentPuzzle();
        }
        else if (type == 2)
        {
            GUI.Box(new Rect(x, y, w, h),
                "PUZZLE 2: CRYPTOGRAPHY ANALYSIS (Lock 2)\n\n" +
                "Cipher Rule: Caesar Cipher with Shift = +3.\n" +
                "Encrypted Word:   W U D L W R U\n" +
                "(Hint: W - 3 = T,  U - 3 = R,  D - 3 = A ...)\n\n" +
                "What is the decrypted word?", panelStyle);

            if (GUI.Button(new Rect(x + 30, btnStartY, w - 60, btnH), "1) TARGET", btnStyle))
                HandleWrongAnswer("Incorrect decryption! Check the shift (-3).");
            if (GUI.Button(new Rect(x + 30, btnStartY + 43, w - 60, btnH), "2) TRAITOR", btnStyle))
                SolveCurrentPuzzle();
            if (GUI.Button(new Rect(x + 30, btnStartY + 86, w - 60, btnH), "3) TRACKER", btnStyle))
                HandleWrongAnswer("Incorrect! Count 3 letters back from L.");
        }
        else
        {
            GUI.Box(new Rect(x, y, w, h),
                "PUZZLE 3: SECURITY LOGIC GATE (Lock 3)\n\n" +
                "Circuit Formula:   OUTPUT = (A AND B) OR (NOT C)\n" +
                "Given Inputs:      A = 1 ,  B = 0 ,  C = 1\n\n" +
                "Which single input change makes OUTPUT = 1?", panelStyle);

            if (GUI.Button(new Rect(x + 30, btnStartY, w - 60, btnH), "1) Set C = 0  ->  (1 AND 0) OR (NOT 0)", btnStyle))
                SolveCurrentPuzzle();
            if (GUI.Button(new Rect(x + 30, btnStartY + 43, w - 60, btnH), "2) Set A = 0  ->  (0 AND 0) OR (NOT 1)", btnStyle))
                HandleWrongAnswer("Result is 0! The lock stays closed.");
            if (GUI.Button(new Rect(x + 30, btnStartY + 86, w - 60, btnH), "3) Keep all as is  ->  (1 AND 0) OR (NOT 1)", btnStyle))
                HandleWrongAnswer("Result is 0! Try inverting C.");
        }

        if (GUI.Button(new Rect(x + w - 125, y + 12, 110, 32), "Close [ESC]", btnStyle))
            ClosePuzzle();
    }
}