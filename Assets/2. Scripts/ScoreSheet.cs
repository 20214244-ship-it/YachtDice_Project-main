using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class ScoreSheet : MonoBehaviour
{
    public static ScoreSheet Instance;

    [Header("ScorePanel 연결 (필수)")]
    public RectTransform scorePanel;

    [Header("한글 폰트 연결 (필수)")]
    public TMP_FontAsset koreanFont;

    [Header("합계 텍스트 (선택)")]
    public TMP_Text totalText;

    private class ScoreEntry
    {
        public Category category;
        public TMP_Text nameText;
        public TMP_Text scoreText;
        public Button   button;
        public Image    background;
        public bool     isLocked    = false;
        public int      lockedScore = 0;
    }

    private List<ScoreEntry> entries = new List<ScoreEntry>();

    private Color bgNormal     = new Color(0.95f, 0.93f, 0.88f, 1f);
    private Color bgAlt        = new Color(0.90f, 0.88f, 0.83f, 1f);
    private Color bgLocked     = new Color(0.75f, 0.85f, 0.75f, 1f);
    private Color textNormal   = new Color(0f,    0f,    0f,    1f);
    private Color scorePreview = new Color(0f,    0.5f,  0f,    1f);
    private Color scoreLocked  = new Color(0f,    0.35f, 0f,    1f);

    private const int BonusThreshold = 63;
    private const int BonusScore     = 35;
    private float rowHeight = 52f;

    void Awake()
    {
        Instance = this;
        BuildScoreSheet();
    }

    void Start() { }

    private void BuildScoreSheet()
    {
        if (scorePanel == null) { Debug.LogError("[ScoreSheet] scorePanel 미연결!"); return; }

        foreach (Transform child in scorePanel)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        entries.Clear();

        var cats = (Category[])System.Enum.GetValues(typeof(Category));
        scorePanel.sizeDelta = new Vector2(scorePanel.sizeDelta.x, rowHeight * cats.Length);

        for (int i = 0; i < cats.Length; i++)
        {
            Category cat = cats[i];
            float yPos = -(i * rowHeight) - rowHeight * 0.5f;

            // Row 배경
            GameObject rowObj = new GameObject("Row_" + cat, typeof(RectTransform), typeof(Image));
            rowObj.transform.SetParent(scorePanel, false);
            RectTransform rowRT = rowObj.GetComponent<RectTransform>();
            rowRT.anchorMin        = new Vector2(0, 1);
            rowRT.anchorMax        = new Vector2(1, 1);
            rowRT.pivot            = new Vector2(0.5f, 0.5f);
            rowRT.sizeDelta        = new Vector2(0, rowHeight - 2f);
            rowRT.anchoredPosition = new Vector2(0, yPos);

            Image rowBg  = rowObj.GetComponent<Image>();
            rowBg.color  = i % 2 == 0 ? bgNormal : bgAlt;

            // 족보 이름
            TMP_Text nameText = MakeText(rowObj.transform, "NameText",
                ScoreCalculator.GetName(cat), 17, HorizontalAlignmentOptions.Left);
            RectTransform nameRT = nameText.rectTransform;
            nameRT.anchorMin = new Vector2(0f,    0f);
            nameRT.anchorMax = new Vector2(0.62f, 1f);
            nameRT.offsetMin = new Vector2(10f, 0f);
            nameRT.offsetMax = new Vector2(0f,  0f);

            // 점수
            TMP_Text scoreText = MakeText(rowObj.transform, "ScoreText",
                "", 17, HorizontalAlignmentOptions.Center);
            RectTransform scoreRT = scoreText.rectTransform;
            scoreRT.anchorMin = new Vector2(0.62f, 0f);
            scoreRT.anchorMax = new Vector2(1f,    1f);
            scoreRT.offsetMin = new Vector2(0f,  0f);
            scoreRT.offsetMax = new Vector2(-5f, 0f);

            // 투명 버튼
            GameObject btnObj = new GameObject("Btn",
                typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(rowObj.transform, false);
            RectTransform btnRT = btnObj.GetComponent<RectTransform>();
            btnRT.anchorMin = Vector2.zero;
            btnRT.anchorMax = Vector2.one;
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;
            btnObj.GetComponent<Image>().color = Color.clear;

            Button btn = btnObj.GetComponent<Button>();
            btn.interactable = false;
            ColorBlock cb = btn.colors;
            cb.normalColor      = Color.clear;
            cb.highlightedColor = new Color(0.4f, 0.9f, 0.4f, 0.25f);
            cb.pressedColor     = new Color(0.2f, 0.7f, 0.2f, 0.40f);
            btn.colors = cb;
            var c = cat;
            btn.onClick.AddListener(() => OnRowClicked(c));

            entries.Add(new ScoreEntry
            {
                category   = cat,
                nameText   = nameText,
                scoreText  = scoreText,
                button     = btn,
                background = rowBg
            });
        }
    }

    private TMP_Text MakeText(Transform parent, string name, string text,
                               float size, HorizontalAlignmentOptions hAlign)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text                = text;
        tmp.fontSize            = size;
        tmp.fontStyle           = name == "NameText" ? FontStyles.Bold : FontStyles.Bold;
        tmp.horizontalAlignment = hAlign;
        tmp.verticalAlignment   = VerticalAlignmentOptions.Middle;
        tmp.color               = textNormal;
        if (koreanFont != null) tmp.font = koreanFont;
        return tmp;
    }

    public void ShowPreviews(int[] dice)
    {
        foreach (var e in entries)
        {
            if (e.isLocked) continue;
            int score = ScoreCalculator.Calculate(e.category, dice);
            if (e.scoreText != null)
            {
                e.scoreText.text  = score > 0 ? score.ToString() : "-";
                e.scoreText.color = scorePreview;
            }
            if (e.button != null) e.button.interactable = true;
        }
    }

    public void ClearPreviews()
    {
        foreach (var e in entries)
        {
            if (e.isLocked) continue;
            if (e.scoreText != null) { e.scoreText.text = ""; e.scoreText.color = textNormal; }
            if (e.button    != null) e.button.interactable = false;
        }
    }

    public void EnableSelection(bool on)
    {
        foreach (var e in entries)
            if (!e.isLocked && e.button != null)
                e.button.interactable = on;
    }

    private void OnRowClicked(Category cat)
    {
        foreach (var e in entries)
        {
            if (e.category != cat || e.isLocked) continue;
            int[] dice    = GameManager.Instance.GetDiceValues();
            e.lockedScore = ScoreCalculator.Calculate(cat, dice);
            e.isLocked    = true;
            if (e.scoreText  != null) { e.scoreText.text = e.lockedScore.ToString(); e.scoreText.color = scoreLocked; }
            if (e.background != null) e.background.color = bgLocked;
            if (e.button     != null) e.button.interactable = false;
            EnableSelection(false);
            UpdateTotal();
            GameManager.Instance.OnCategorySelected();
            return;
        }
    }

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
        if (totalText != null) totalText.text = $"합계: {upper + lower + bonus}";
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
        return upper + lower + (upper >= BonusThreshold ? BonusScore : 0);
    }

    public bool AllLocked()
    {
        foreach (var e in entries) if (!e.isLocked) return false;
        return true;
    }
}
