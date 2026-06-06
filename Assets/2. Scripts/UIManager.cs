using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("굴리기 버튼")]
    public Button   rollButton;
    public TMP_Text rollButtonText;

    [Header("안내 메시지")]
    public TMP_Text messageText;

    [Header("게임 오버 패널")]
    public GameObject gameOverPanel;
    public TMP_Text   gameOverText;
    public Button     restartButton;

    void Awake() => Instance = this;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        // 시작 전부터 버튼 텍스트 표시
        if (rollButtonText != null) rollButtonText.text = "굴리기";
    }

    // ── 굴리기 버튼 ────────────────────────────────────────────
    /// <summary>Button OnClick 에 이 함수 연결</summary>
    public void OnRollButtonClicked()
    {
        GameManager.Instance.OnRollButtonClicked();
    }

    public void UpdateRollButton(int rollsLeft, bool isRolling)
    {
        if (rollButton != null)
            rollButton.interactable = rollsLeft > 0 && !isRolling;

        if (rollButtonText != null)
        {
            if (rollsLeft <= 0)
                rollButtonText.text = "족보를 선택하세요";
            else if (rollsLeft == 3)
                rollButtonText.text = "굴리기";
            else
                rollButtonText.text = $"다시 굴리기 ({3 - rollsLeft}/3)";
        }
    }

    // ── 점수 미리보기 ──────────────────────────────────────────
    public void ShowScorePreviews(int[] dice) =>
        ScoreSheet.Instance.ShowPreviews(dice);

    public void ClearScorePreviews() =>
        ScoreSheet.Instance.ClearPreviews();

    public void EnableScoreSelection(bool on) =>
        ScoreSheet.Instance.EnableSelection(on);

    // ── 메시지 ─────────────────────────────────────────────────
    public void ShowMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
    }

    // ── 게임 오버 ──────────────────────────────────────────────
    public void ShowGameOver(int total)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverText  != null)
            gameOverText.text = $"게임 종료!\n최종 점수: {total}점\n\n수고했어요 🎉";
    }

    private void RestartGame()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.name);
    }
}
