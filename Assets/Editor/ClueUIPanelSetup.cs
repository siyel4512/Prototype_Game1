using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

public static class ClueUIPanelSetup
{
    [MenuItem("Tools/Setup Clue UI Panel")]
    public static void SetupClueUIPanel()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/AppleGothic_Dynamic.asset");

        // UICanvas 찾기
        var canvasObjs = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Canvas uiCanvas = null;
        foreach (var c in canvasObjs)
            if (c.gameObject.name == "UICanvas") { uiCanvas = c; break; }

        if (uiCanvas == null) { Debug.LogError("UICanvas 없음"); return; }

        // 기존 CluePanel 삭제
        var existing = uiCanvas.transform.Find("CluePanel");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        // CluePanel root (820x580)
        var panelGO = CreateUIGO("CluePanel", uiCanvas.transform);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = panelRT.anchorMax = panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(820, 580);
        panelGO.AddComponent<Image>().color = new Color(0.06f, 0.06f, 0.10f, 0.96f);
        panelGO.SetActive(false);

        // Header (top 48px)
        var headerGO = CreateUIGO("Header", panelGO.transform);
        var headerRT = headerGO.GetComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0, 1); headerRT.anchorMax = new Vector2(1, 1);
        headerRT.pivot = new Vector2(0.5f, 1f);
        headerRT.anchoredPosition = Vector2.zero; headerRT.sizeDelta = new Vector2(0, 48);
        headerGO.AddComponent<Image>().color = new Color(0.10f, 0.10f, 0.18f, 1f);

        var headerTextGO = CreateUIGO("Text", headerGO.transform);
        StretchFull(headerTextGO.GetComponent<RectTransform>());
        var headerTMP = headerTextGO.AddComponent<TextMeshProUGUI>();
        headerTMP.text = "수집한 단서   [TAB: 닫기]";
        headerTMP.fontSize = 16;
        headerTMP.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        headerTMP.alignment = TextAlignmentOptions.MidlineLeft;
        headerTMP.margin = new Vector4(16, 0, 0, 0);
        if (font) headerTMP.font = font;

        // Body (아래 영역 전체)
        var bodyGO = CreateUIGO("Body", panelGO.transform);
        var bodyRT = bodyGO.GetComponent<RectTransform>();
        bodyRT.anchorMin = Vector2.zero; bodyRT.anchorMax = Vector2.one;
        bodyRT.offsetMin = Vector2.zero; bodyRT.offsetMax = new Vector2(0, -48);
        var hlg = bodyGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;
        hlg.spacing = 0;

        // LeftPanel (240px)
        var leftGO = CreateUIGO("LeftPanel", bodyGO.transform);
        leftGO.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.14f, 1f);
        var leftLE = leftGO.AddComponent<LayoutElement>();
        leftLE.preferredWidth = 240; leftLE.flexibleWidth = 0;

        // ScrollView
        var svGO = CreateUIGO("ScrollView", leftGO.transform);
        StretchFull(svGO.GetComponent<RectTransform>());
        var sv = svGO.AddComponent<ScrollRect>();
        sv.horizontal = false; sv.vertical = true;

        var vpGO = CreateUIGO("Viewport", svGO.transform);
        StretchFull(vpGO.GetComponent<RectTransform>());
        vpGO.AddComponent<RectMask2D>();
        sv.viewport = vpGO.GetComponent<RectTransform>();

        var contentGO = CreateUIGO("Content", vpGO.transform);
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.anchoredPosition = Vector2.zero; contentRT.sizeDelta = Vector2.zero;
        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(4, 4, 4, 4); vlg.spacing = 2;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sv.content = contentRT;

        // Divider (1px)
        var divGO = CreateUIGO("Divider", bodyGO.transform);
        divGO.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.35f, 1f);
        var divLE = divGO.AddComponent<LayoutElement>();
        divLE.preferredWidth = 1; divLE.flexibleWidth = 0;

        // DetailPanel
        var rightGO = CreateUIGO("DetailPanel", bodyGO.transform);
        rightGO.AddComponent<Image>().color = new Color(0.07f, 0.07f, 0.12f, 1f);
        rightGO.AddComponent<LayoutElement>().flexibleWidth = 1;

        // EmptyHint
        var emptyGO = CreateUIGO("EmptyHint", rightGO.transform);
        var emptyRT = emptyGO.GetComponent<RectTransform>();
        emptyRT.anchorMin = emptyRT.anchorMax = emptyRT.pivot = new Vector2(0.5f, 0.5f);
        emptyRT.anchoredPosition = Vector2.zero; emptyRT.sizeDelta = new Vector2(300, 40);
        var emptyTMP = emptyGO.AddComponent<TextMeshProUGUI>();
        emptyTMP.text = "목록에서 단서를 선택하세요.";
        emptyTMP.fontSize = 15;
        emptyTMP.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        emptyTMP.alignment = TextAlignmentOptions.Center;
        if (font) emptyTMP.font = font;
        emptyGO.SetActive(false);

        // DetailRoot
        var detailRootGO = CreateUIGO("DetailRoot", rightGO.transform);
        StretchFull(detailRootGO.GetComponent<RectTransform>());
        var drVLG = detailRootGO.AddComponent<VerticalLayoutGroup>();
        drVLG.padding = new RectOffset(20, 20, 20, 20); drVLG.spacing = 12;
        drVLG.childControlWidth = true; drVLG.childControlHeight = true;
        drVLG.childForceExpandWidth = true; drVLG.childForceExpandHeight = false;
        detailRootGO.SetActive(false);

        // TitleArea
        var titleAreaGO = CreateUIGO("TitleArea", detailRootGO.transform);
        titleAreaGO.AddComponent<LayoutElement>().preferredHeight = 32;
        var titleTextGO = CreateUIGO("Text", titleAreaGO.transform);
        StretchFull(titleTextGO.GetComponent<RectTransform>());
        var titleTMP = titleTextGO.AddComponent<TextMeshProUGUI>();
        titleTMP.fontSize = 18;
        titleTMP.color = new Color(0.95f, 0.90f, 0.6f, 1f);
        if (font) titleTMP.font = font;

        // TitleDivider
        var titleDivGO = CreateUIGO("TitleDivider", detailRootGO.transform);
        titleDivGO.AddComponent<LayoutElement>().preferredHeight = 1;
        titleDivGO.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.4f, 1f);

        // ContentArea
        var contentAreaGO = CreateUIGO("ContentArea", detailRootGO.transform);
        contentAreaGO.AddComponent<LayoutElement>().flexibleHeight = 1;
        var contentTextGO = CreateUIGO("Text", contentAreaGO.transform);
        StretchFull(contentTextGO.GetComponent<RectTransform>());
        var contentTMP = contentTextGO.AddComponent<TextMeshProUGUI>();
        contentTMP.fontSize = 15;
        contentTMP.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        contentTMP.textWrappingMode = TextWrappingModes.Normal;
        if (font) contentTMP.font = font;

        // ClueUIPanel 연결
        var clueUIPanel = uiCanvas.GetComponent<ClueUIPanel>();
        if (clueUIPanel == null) clueUIPanel = uiCanvas.gameObject.AddComponent<ClueUIPanel>();

        var so = new SerializedObject(clueUIPanel);
        so.FindProperty("panel").objectReferenceValue = panelGO;
        so.FindProperty("clueListContainer").objectReferenceValue = contentRT;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/05. Data/ClueEntryPrefab.prefab");
        so.FindProperty("clueEntryPrefab").objectReferenceValue = prefab;
        so.FindProperty("detailRoot").objectReferenceValue = detailRootGO;
        so.FindProperty("detailTitle").objectReferenceValue = titleTMP;
        so.FindProperty("detailContent").objectReferenceValue = contentTMP;
        so.FindProperty("emptyHint").objectReferenceValue = emptyGO;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[ClueUIPanelSetup] 완료! CluePanel UI 구성 성공.");
    }

    private static GameObject CreateUIGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }
}
