using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("주사위 5개 연결")]
    public Dice[] diceArray;

    [Header("매니저 직접 연결 (Inspector)")]
    public UIManager   uiManager;
    public ScoreSheet  scoreSheet;

    public int  RollsLeft { get; private set; } = 3;
    public bool HasRolled { get; private set; } = false;
    public bool GameOver  { get; private set; } = false;
    public bool IsRolling => IsAnyDiceRolling();

    void Awake() => Instance = this;

    void Start()
    {
        // Instance 방식 대신 직접 참조로 null 방지
        if (uiManager  == null) uiManager  = FindObjectOfType<UIManager>();
        if (scoreSheet == null) scoreSheet = FindObjectOfType<ScoreSheet>();

        StartNewTurn();
    }

    public void StartNewTurn()
    {
        RollsLeft = 3;
        HasRolled = false;
        GameOver  = false;

        foreach (var d in diceArray)
            d.ResetForNewTurn();

        if (uiManager != null)
        {
            uiManager.UpdateRollButton(RollsLeft, false);
            uiManager.ClearScorePreviews();
            uiManager.ShowMessage("굴리기 버튼을 눌러 시작하세요!");
        }
    }

    public void OnRollButtonClicked()
    {
        if (GameOver || RollsLeft <= 0 || IsRolling) return;

        RollsLeft--;
        HasRolled = true;

        foreach (var d in diceArray)
            d.Roll();

        if (uiManager != null)
        {
            uiManager.UpdateRollButton(RollsLeft, true);
            uiManager.ShowMessage("굴리는 중...");
        }

        StartCoroutine(WaitForAllDiceStopped());
    }

    private IEnumerator WaitForAllDiceStopped()
    {
        yield return new WaitForSeconds(0.5f);
        while (IsRolling) yield return null;

        int[] values = GetDiceValues();

        if (uiManager != null)
            uiManager.ShowScorePreviews(values);

        if (RollsLeft > 0)
        {
            if (uiManager != null)
                uiManager.ShowMessage($"주사위를 고정하거나 다시 굴리세요! (남은 굴리기: {RollsLeft})");
        }
        else
        {
            if (uiManager != null)
            {
                uiManager.ShowMessage("족보를 선택해 점수를 기록하세요!");
                uiManager.EnableScoreSelection(true);
            }
        }
    }

    public void OnCategorySelected()
    {
        if (uiManager != null)
            uiManager.EnableScoreSelection(false);

        if (scoreSheet != null && scoreSheet.AllLocked())
        {
            GameOver = true;
            if (uiManager != null)
                uiManager.ShowGameOver(scoreSheet.GetTotal());
            return;
        }
        StartNewTurn();
    }

    public bool CanPlayerClick() => HasRolled && !IsRolling && !GameOver;

    private bool IsAnyDiceRolling()
    {
        foreach (var d in diceArray)
            if (d.IsRolling) return true;
        return false;
    }

    public int[] GetDiceValues()
    {
        int[] vals = new int[diceArray.Length];
        for (int i = 0; i < diceArray.Length; i++)
            vals[i] = diceArray[i].Value;
        return vals;
    }
}
