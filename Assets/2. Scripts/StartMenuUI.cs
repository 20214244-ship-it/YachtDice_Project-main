using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[ExecuteAlways]
public class StartMenuUI : MonoBehaviour
{
    public TMP_FontAsset koreanFont;

    void OnEnable()
    {
        Rebuild();
    }

    // OnValidate 완전 제거 - 폰트 연결 후 씬 저장하면 OnEnable이 다시 불림

    void Rebuild()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name == "UICanvas")
            {
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }
        }
        Build();
    }

    void Build()
    {
        var canvasObj = new GameObject("UICanvas");
        canvasObj.transform.SetParent(transform, false);

        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        var bg = CreatePanel(canvasObj, "Background",
            new Color(0.05f, 0.15f, 0.05f, 1f), Vector2.zero, new Vector2(1920, 1080));

        CreateText(bg, "TitleText", "요트 다이스",
            new Vector2(0, 200), new Vector2(800, 150), 90, Color.white, true);

        var startBtn = CreateButton(bg, "StartButton", "게임 시작",
            new Vector2(0, 30), new Vector2(350, 80),
            new Color(0.15f, 0.55f, 0.15f), Color.white, 36);

        var howBtn = CreateButton(bg, "HowToPlayButton", "게임 방법",
            new Vector2(0, -80), new Vector2(350, 80),
            new Color(0.2f, 0.2f, 0.4f), Color.white, 36);

        var howToPlayPanel = CreatePanel(bg, "HowToPlayPanel",
            new Color(0.05f, 0.05f, 0.1f, 0.97f), Vector2.zero, new Vector2(1920, 1080));
        howToPlayPanel.SetActive(false);

        string rules =
            "【 게임 방법 】\n\n" +
            "• 주사위 5개를 최대 3번 굴릴 수 있습니다.\n" +
            "• 굴린 후 원하는 주사위를 클릭하면 고정됩니다.\n" +
            "• 고정된 주사위는 다시 클릭하면 해제됩니다.\n" +
            "• 3번 굴리면 반드시 점수표에 점수를 기록해야 합니다.\n" +
            "• 한 번 기록한 항목은 변경할 수 없습니다.\n" +
            "• 13개 항목을 모두 채우면 게임이 끝납니다.\n\n" +
            "【 족보 】\n" +
            "에이스~헥사: 해당 숫자 합산\n" +
            "쓰리카인드: 같은 숫자 3개 이상 → 전체 합\n" +
            "포카인드: 같은 숫자 4개 이상 → 전체 합\n" +
            "풀하우스: 3+2 조합 → 전체 합\n" +
            "스몰스트레이트: 4연속 → 15점\n" +
            "라지스트레이트: 5연속 → 30점\n" +
            "요트: 5개 모두 같은 숫자 → 50점\n" +
            "찬스: 전체 합산";

        CreateText(howToPlayPanel, "RulesText", rules,
            new Vector2(0, 20), new Vector2(900, 700), 24, Color.white, false);

        var backBtn = CreateButton(howToPlayPanel, "BackButton", "← 돌아가기",
            new Vector2(0, -430), new Vector2(280, 65),
            new Color(0.4f, 0.1f, 0.1f), Color.white, 28);

        if (Application.isPlaying)
        {
            startBtn.GetComponent<Button>().onClick.AddListener(() =>
                SceneManager.LoadScene("SampleScene"));
            howBtn.GetComponent<Button>().onClick.AddListener(() =>
                howToPlayPanel.SetActive(true));
            backBtn.GetComponent<Button>().onClick.AddListener(() =>
                howToPlayPanel.SetActive(false));
        }
    }

    GameObject CreatePanel(GameObject parent, string name, Color color, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.AddComponent<Image>().color = color;
        return go;
    }

    GameObject CreateText(GameObject parent, string name, string content,
        Vector2 pos, Vector2 size, float fontSize, Color color, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        if (koreanFont != null) tmp.font = koreanFont;
        return go;
    }

    GameObject CreateButton(GameObject parent, string name, string label,
        Vector2 pos, Vector2 size, Color btnColor, Color textColor, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.AddComponent<Image>().color = btnColor;
        go.AddComponent<Button>();

        var txtGo = new GameObject("Label");
        txtGo.transform.SetParent(go.transform, false);
        var trt = txtGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.anchoredPosition = Vector2.zero;
        trt.sizeDelta = Vector2.zero;   // stretch 방식 - offsetMin/Max 안 건드림
        var tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        if (koreanFont != null) tmp.font = koreanFont;

        return go;
    }
}