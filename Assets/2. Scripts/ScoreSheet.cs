using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ScoreSheet : MonoBehaviour
{
    public static ScoreSheet Instance;

    [Header("Row 프리팹 (Row_Ones 프리팹 연결)")]
    public GameObject rowPrefab;

    [Header("ScorePanel (Vertical Layout Group 있는 패널)")]
    public Transform scorePanel;

    [Header("합계 텍스트 (나중에 연결)")]
    public TMP_Text totalText;

    // 내부 데이터
    private class ScoreEntry
    {
        public Category  category;
        public TMP_Text  nameText;
        public TMP_Text  scoreText;
        public Button    button;
        public bool      isLocked    = false;
        public int       lockedScore = 0;
    }

    private List<ScoreEntry> entries = new List<ScoreEntry>();

    private readonly Color colorPreview = new Color(0.55f, 0.9f, 0.55f);
    private readonly Color colorLocked  = Color.white;
    private readonly Color colorEmpty   = new Color(0.7f, 0.7f, 0.7f);

    private const int BonusThreshold = 63;
    private const int BonusScore     = 35;

    // ───────────────────────────────────────────────────────────
    void Awake()
    {
        Instance = this;
        BuildScoreSheet();
    }

    private void BuildScoreSheet()
    {
        if (rowPrefab == null || scorePanel == null)
        {
            Debug.LogError("[ScoreSheet] rowPrefab 또는 scorePanel 이 연결되지 않았습니다!");
            return;
        }

        foreach (Category cat in System.Enum.GetValues(typeof(Category)))
        {
            // 프리팹 복사
            GameObject row = Instantiate(rowPrefab, scorePanel);

            // 이름 텍스트
            TMP_Text nameText  = row.transform.Find("NameText")?.GetComponent<TMP_Text>();
            TMP_Text scoreText = row.transform.Find("ScoreText")?.GetComponent<TMP_Text>();
            Button   btn       = row.transform.Find("SelectButton")?.GetComponent<Button>();

            if (nameText  != null) nameText.text  = ScoreCalculator.GetName(cat);
            if (scoreText != null)
            {
                scoreText.text  = "";
                scoreText.color = colorEmpty;
            }
            if (btn != null)
            {
                btn.interactable = false;
                var c = cat; // 람다 캡처
                btn.onClick.AddListener(() => OnRowClicked(c));
            }

            entries.Add(new ScoreEntry
            {
                category  = cat,
                nameText  = nameText,
                scoreText = scoreText,
                button    = btn
            });
        }

        // 원본 프리팹 인스턴스 제거 (Hierarchy에 남아있는 Row_Ones)
        // → 프리팹으로 만들었으면 씬에서 Row_Ones 삭제해도 됨
    }

    // ── 미리보기 ───────────────────────────────────────────────
    public void ShowPreviews(int[] dice)
    {
        foreach (var e in entries)
        {
            if (e.isLocked) continue;
            int score = ScoreCalculator.Calculate(e.category, dice);
            if (e.scoreText != null)
            {
                e.scoreText.text  = score.ToString();
                e.scoreText.color = colorPreview;
            }
            if (e.button != null)
                e.button.interactable = true;
        }
    }

    public void ClearPreviews()
    {
        foreach (var e in entries)
        {
            if (e.isLocked) continue;
            if (e.scoreText != null)
            {
                e.scoreText.text  = "";
                e.scoreText.color = colorEmpty;
            }
            if (e.button != null)
                e.button.interactable = false;
        }
    }

    public void EnableSelection(bool on)
    {
        foreach (var e in entries)
            if (!e.isLocked && e.button != null)
                e.button.interactable = on;
    }

    // ── 족보 선택 확정 ─────────────────────────────────────────
    private void OnRowClicked(Category cat)
    {
        foreach (var e in entries)
        {
            if (e.category != cat || e.isLocked) continue;

            int[] dice      = GameManager.Instance.GetDiceValues();
            e.lockedScore   = ScoreCalculator.Calculate(cat, dice);
            e.isLocked      = true;

            if (e.scoreText != null)
            {
                e.scoreText.text  = e.lockedScore.ToString();
                e.scoreText.color = colorLocked;
            }
            if (e.button != null)
                e.button.interactable = false;

            EnableSelection(false);
            UpdateTotal();
            GameManager.Instance.OnCategorySelected();
            return;
        }
    }

    // ── 합계 ───────────────────────────────────────────────────
    private void UpdateTotal()
    {
        int upper = 0, lower = 0;
        foreach (var e in entries)
        {
            if (!e.isLocked) continue;
            if (e.category <= Category.Sixes) upper += e.lockedScore;
            else                              lower += e.lockedScore;
        }
        int bonus = upper >= BonusThreshold ? BonusScore : 0;
        if (totalText != null)
            totalText.text = $"합계: {upper + lower + bonus}";
    }

    public int GetTotal()
    {
        int upper = 0, lower = 0;
        foreach (var e in entries)
        {
            if (!e.isLocked) continue;
            if (e.category <= Category.Sixes) upper += e.lockedScore;
            else                              lower += e.lockedScore;
        }
        int bonus = upper >= BonusThreshold ? BonusScore : 0;
        return upper + lower + bonus;
    }

    public bool AllLocked()
    {
        foreach (var e in entries)
            if (!e.isLocked) return false;
        return true;
    }
}
