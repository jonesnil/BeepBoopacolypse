using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{

    Text prompt;
    InputField colonyNameInput;
    [SerializeField] ScriptableBoard board;

    // Start is called before the first frame update
    void Start()
    {
        prompt = this.transform.GetChild(0).GetComponent<Text>();
        colonyNameInput = this.transform.GetChild(1).GetComponent<InputField>();
        colonyNameInput.ActivateInputField();

        board.entries[0].SetEntry("test", 69);
        board.entries[1].SetEntry("test", 420);
        board.entries[2].SetEntry("test", 360);
        board.entries[3].SetEntry("test", 3);
        board.entries[4].SetEntry("test", 42);
        board.entries[5].SetEntry("test", 80082);
        board.entries[6].SetEntry("test", 99);

        board.entries.Sort();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnNameSelected()
    {
        board.entries[6].SetEntry("newScore", 1);
        board.entries.Sort();
    }
}
