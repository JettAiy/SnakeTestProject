using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
   
    [SerializeField] private GameObject button_Start;
    [SerializeField] private TMPro.TextMeshProUGUI text_score;

    #region INIT

    private void Awake()
    {
        button_Start.GetComponent<Button>().onClick.AddListener(OnStart);
    }

    private void Start()
    {
        PlayerController.Current.OnReset += ShowStartButton;
        PlayerController.Current.OnScoreChange += UpdateScoreText;
    }

    #endregion

    #region BUTTON
    private void OnStart()
    {
        PlayerController.Current.ResetControl();
        button_Start.SetActive(false);
    }
    #endregion


    #region EVENTS
    private void ShowStartButton()
    {
        button_Start.SetActive(true);
    }

    private void UpdateScoreText(int score)
    {
        text_score.SetText("SCORE: " + score);
    }
    #endregion

}
