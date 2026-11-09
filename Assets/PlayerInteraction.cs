using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    public static bool inDialogue = false;

    public float interactDistance = 3f;
    public int totalEvidence = 3;

    Camera cam;
    Evidence lookedAtEvidence;
    NPC lookedAtNPC;
    NPC currentNPC;
    int lineIndex = 0;
    bool talkedToManager = false;

    List<string> notebook = new List<string>();
    bool showNotebook = false;
    Texture2D bgTex;

    void Start()
    {
        cam = Camera.main;
        inDialogue = false;
    }

    void Update()
    {
        
        if (inDialogue)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                lineIndex++;
                if (lineIndex >= currentNPC.lines.Length)
                {
                    inDialogue = false;
                    talkedToManager = true;
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
                notebook.Add(lookedAtEvidence.evidenceName + ": " + lookedAtEvidence.description);
                Destroy(lookedAtEvidence.gameObject);
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

    void OnGUI()
    {
        if (bgTex == null)
        {
            bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0.9f, 0.9f, 0.9f));
            bgTex.Apply();
        }

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 22;
        style.wordWrap = true;
        style.normal.background = bgTex;
        style.normal.textColor = Color.black;

        string objective = talkedToManager
            ? "Objective: Collect evidence (" + notebook.Count + "/" + totalEvidence + ")"
            : "Objective: Talk to the Manager";
        GUI.Box(new Rect(Screen.width - 520, 20, 500, 45), objective, style);

       
        if (!inDialogue)
        {
            if (lookedAtEvidence != null)
                GUI.Box(new Rect(Screen.width / 2 - 200, Screen.height / 2 + 40, 400, 45),
                        "Press E to collect: " + lookedAtEvidence.evidenceName, style);
            else if (lookedAtNPC != null)
                GUI.Box(new Rect(Screen.width / 2 - 200, Screen.height / 2 + 40, 400, 45),
                        "Press E to talk to " + lookedAtNPC.npcName, style);
        }
        if (inDialogue)
        {
            GUI.Box(new Rect(Screen.width / 2 - 400, Screen.height - 180, 800, 150),
                    currentNPC.npcName + ":\n" + currentNPC.lines[lineIndex] + "\n\n(Press E to continue)", style);
        }

        
        if (showNotebook)
        {
            GUI.Box(new Rect(20, 20, 600, 50 + notebook.Count * 30), "Notebook", style);
            for (int i = 0; i < notebook.Count; i++)
                GUI.Label(new Rect(30, 55 + i * 30, 580, 30), "- " + notebook[i], style);
        }

     
        if (talkedToManager && notebook.Count >= totalEvidence)
            GUI.Box(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 60, 400, 60),
                    "Level 1 Complete!", style);
    }
}