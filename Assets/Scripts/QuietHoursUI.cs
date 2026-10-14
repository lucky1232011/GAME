using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class QuietHoursUI : MonoBehaviour
{
    const string Ink = "#11191A";
    const string Panel = "#182223";
    const string Cream = "#E7DDC5";
    const string Muted = "#9BAAA4";
    const string Brass = "#C8AC72";

    Canvas canvas;
    Font bodyFont;
    Font displayFont;
    Text roomText;
    Text leadText;
    Text evidenceText;
    Text promptText;
    Text hintToast;
    GameObject modal;
    Action onBegin;
    Action onClose;
    Action onFile;
    Action<string> onSubmit;
    Action onHint;
    Action onResume;
    Action onEvidence;
    InputField answerField;
    float toastTimer;

    public bool ModalOpen { get; private set; }

    public void Configure(Action begin, Action close, Action file, Action<string> submit,
        Action hint, Action resume, Action evidence)
    {
        onBegin = begin;
        onClose = close;
        onFile = file;
        onSubmit = submit;
        onHint = hint;
        onResume = resume;
        onEvidence = evidence;
        BuildCanvas();
        ShowStart(null);
    }

    void Update()
    {
        if (toastTimer > 0f)
        {
            toastTimer -= Time.unscaledDeltaTime;
            if (toastTimer <= 0f && hintToast != null) hintToast.text = "";
        }
    }

    void BuildCanvas()
    {
        Font fallback = Resources.GetBuiltinResource<Font>("Arial.ttf");
        bodyFont = Font.CreateDynamicFontFromOSFont(new[] { "Aptos", "Segoe UI", "Arial" }, 24);
        displayFont = Font.CreateDynamicFontFromOSFont(new[] { "Georgia", "Times New Roman" }, 42);
        if (bodyFont == null) bodyFont = fallback;
        if (displayFont == null) displayFont = bodyFont;

        GameObject canvasObject = new GameObject("Quiet Hours UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventObject.transform.SetParent(transform, false);
        eventObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

        roomText = MakeText(canvas.transform, "Room", 27, bodyFont, Cream, TextAnchor.UpperLeft,
            new Vector2(0.03f, 0.965f), new Vector2(0, -34), new Vector2(700, 44));
        leadText = MakeText(canvas.transform, "Current lead", 19, bodyFont, Muted, TextAnchor.UpperLeft,
            new Vector2(0.03f, 0.925f), new Vector2(0, -16), new Vector2(890, 42));
        evidenceText = MakeText(canvas.transform, "Evidence count", 18, bodyFont, Brass, TextAnchor.UpperRight,
            new Vector2(0.97f, 0.965f), new Vector2(0, -34), new Vector2(280, 42));
        promptText = MakeText(canvas.transform, "Interaction prompt", 20, bodyFont, Cream, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.065f), Vector2.zero, new Vector2(1000, 52));
        hintToast = MakeText(canvas.transform, "Toast", 20, bodyFont, Brass, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.13f), Vector2.zero, new Vector2(1200, 52));
        promptText.text = "";
        hintToast.text = "";
    }

    public void SetHud(string room, string lead, int evidenceCount, string nearby)
    {
        if (ModalOpen) return;
        roomText.text = room.ToUpperInvariant();
        leadText.text = lead;
        evidenceText.text = "EVIDENCE   " + evidenceCount.ToString("00");
        promptText.text = string.IsNullOrEmpty(nearby) ? "A / D or ← / →   MOVE      TAB   CASE NOTES      ESC   PAUSE" : "E   " + nearby.ToUpperInvariant() + "     •     TAB   CASE NOTES";
    }

    public void ShowStart(QuietHoursStory story)
    {
        ClearModal();
        ModalOpen = true;
        modal = MakePanel("Opening", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1130, 760), new Color(0.055f, 0.085f, 0.09f, 0.97f));
        string title = story != null ? story.title : "THE QUIET INTERVAL";
        string subtitle = story != null ? story.institution + "   /   " + story.chapter : "MORROW INSTITUTE  /  RECORDS ANNEX";
        string copy = story != null ? story.opening : "At 05:30 the paper archive is sealed for transfer. A signed transcript and its carbon copy disagree. You have one shift to understand the missing line.";
        MakeText(modal.transform, "Eyebrow", 17, bodyFont, Brass, TextAnchor.MiddleLeft,
            new Vector2(0.09f, 0.83f), Vector2.zero, new Vector2(900, 38)).text = subtitle.ToUpperInvariant();
        MakeText(modal.transform, "Title", 62, displayFont, Cream, TextAnchor.MiddleLeft,
            new Vector2(0.09f, 0.70f), Vector2.zero, new Vector2(970, 100)).text = title;
        MakeText(modal.transform, "Protagonist", 20, bodyFont, Muted, TextAnchor.MiddleLeft,
            new Vector2(0.09f, 0.59f), Vector2.zero, new Vector2(940, 42)).text = story != null ? story.protagonist : "IRA SEN   /   TEMPORARY RECORDS ANALYST";
        MakeText(modal.transform, "Opening", 24, bodyFont, Cream, TextAnchor.UpperLeft,
            new Vector2(0.09f, 0.52f), new Vector2(0, -15), new Vector2(930, 250)).text = copy;
        MakeText(modal.transform, "Controls", 18, bodyFont, Muted, TextAnchor.MiddleLeft,
            new Vector2(0.09f, 0.24f), Vector2.zero, new Vector2(930, 42)).text = "WALK WITH A / D OR ARROW KEYS     •     E INSPECT     •     TAB CASE NOTES";
        MakeButton(modal.transform, "Enter the annex", 23, new Vector2(0.5f, 0.105f), new Vector2(330, 66), () =>
        {
            ClearModal(); ModalOpen = false; onBegin?.Invoke();
        });
    }

    public void ShowDocument(QuietHoursDocument document, bool alreadyFiled)
    {
        ClearModal(); ModalOpen = true;
        modal = MakeModalShell("DOCUMENT   /   " + document.location.ToUpperInvariant());
        MakeText(modal.transform, "Document title", 39, displayFont, Cream, TextAnchor.UpperLeft,
            new Vector2(0.075f, 0.81f), new Vector2(0, -7), new Vector2(1420, 82)).text = document.title;
        MakeText(modal.transform, "Document body", 23, bodyFont, Cream, TextAnchor.UpperLeft,
            new Vector2(0.075f, 0.72f), new Vector2(0, -10), new Vector2(1420, 480)).text = document.body;
        if (!alreadyFiled)
        {
            MakeButton(modal.transform, "File evidence", 20, new Vector2(0.71f, 0.105f), new Vector2(270, 58), () => onFile?.Invoke());
            MakeButton(modal.transform, "Close", 20, new Vector2(0.87f, 0.105f), new Vector2(190, 58), () => onClose?.Invoke());
        }
        else MakeButton(modal.transform, "Close", 20, new Vector2(0.79f, 0.105f), new Vector2(230, 58), () => onClose?.Invoke());
    }

    public void ShowEvidence(IReadOnlyList<QuietHoursDocument> documents)
    {
        ClearModal(); ModalOpen = true;
        modal = MakeModalShell("CASE NOTES   /   VERIFIED EVIDENCE");
        if (documents.Count == 0)
        {
            MakeText(modal.transform, "Empty", 24, bodyFont, Muted, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.51f), Vector2.zero, new Vector2(1200, 140)).text = "Nothing verified yet. The corridor notice is still on the wall.";
        }
        else
        {
            string combined = "";
            for (int i = 0; i < documents.Count; i++)
            {
                QuietHoursDocument doc = documents[i];
                combined += (i + 1).ToString("00") + "   /   " + doc.evidenceTitle.ToUpperInvariant() + "\n" + doc.evidenceBody + "\n\n";
            }
            MakeText(modal.transform, "Evidence list", 21, bodyFont, Cream, TextAnchor.UpperLeft,
                new Vector2(0.075f, 0.72f), new Vector2(0, -12), new Vector2(1430, 530)).text = combined;
        }
        MakeButton(modal.transform, "Return to the corridor", 20, new Vector2(0.79f, 0.105f), new Vector2(330, 58), () => onClose?.Invoke());
    }

    public void ShowPuzzle(QuietHoursPuzzle puzzle, int hintIndex)
    {
        ClearModal(); ModalOpen = true;
        modal = MakeModalShell("RESEARCH TERMINAL   /   TRIAL 17");
        MakeText(modal.transform, "Puzzle title", 39, displayFont, Cream, TextAnchor.UpperLeft,
            new Vector2(0.075f, 0.81f), new Vector2(0, -7), new Vector2(1420, 78)).text = puzzle.title;
        MakeText(modal.transform, "Puzzle prompt", 23, bodyFont, Cream, TextAnchor.UpperLeft,
            new Vector2(0.075f, 0.71f), new Vector2(0, -12), new Vector2(1420, 220)).text = puzzle.prompt + "\n\n" + puzzle.answerFormat;
        MakeText(modal.transform, "Input label", 18, bodyFont, Brass, TextAnchor.MiddleLeft,
            new Vector2(0.075f, 0.43f), Vector2.zero, new Vector2(400, 40)).text = "DISTANCE FROM RECEIVER A   /   METRES";
        answerField = MakeInput(modal.transform, new Vector2(0.075f, 0.36f), new Vector2(360, 64));
        if (hintIndex > 0 && puzzle.hints != null && puzzle.hints.Length >= hintIndex)
        {
            MakeText(modal.transform, "Hint", 21, bodyFont, Muted, TextAnchor.UpperLeft,
                new Vector2(0.075f, 0.27f), new Vector2(0, -6), new Vector2(1390, 120)).text = "HINT " + hintIndex.ToString("00") + "   /   " + puzzle.hints[hintIndex - 1];
        }
        MakeButton(modal.transform, "Try value", 20, new Vector2(0.69f, 0.105f), new Vector2(230, 58), () => onSubmit?.Invoke(answerField != null ? answerField.text : ""));
        MakeButton(modal.transform, hintIndex < (puzzle.hints != null ? puzzle.hints.Length : 0) ? "A hint" : "Close", 20,
            new Vector2(0.85f, 0.105f), new Vector2(205, 58), () =>
            {
                if (hintIndex < (puzzle.hints != null ? puzzle.hints.Length : 0)) onHint?.Invoke();
                else onClose?.Invoke();
            });
        MakeButton(modal.transform, "Back", 18, new Vector2(0.51f, 0.105f), new Vector2(150, 58), () => onClose?.Invoke());
    }

    public void ShowPause(Action restart)
    {
        ClearModal(); ModalOpen = true;
        modal = MakePanel("Pause", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 430), new Color(0.055f, 0.085f, 0.09f, 0.98f));
        MakeText(modal.transform, "Pause title", 44, displayFont, Cream, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.76f), Vector2.zero, new Vector2(600, 80)).text = "The Annex is quiet";
        MakeButton(modal.transform, "Resume", 20, new Vector2(0.5f, 0.50f), new Vector2(300, 60), () => onResume?.Invoke());
        MakeButton(modal.transform, "Case notes", 20, new Vector2(0.5f, 0.31f), new Vector2(300, 60), () => onEvidence?.Invoke());
        MakeButton(modal.transform, "Restart investigation", 17, new Vector2(0.5f, 0.13f), new Vector2(300, 54), restart);
    }

    public void ShowEnding(QuietHoursStory story, Action restart)
    {
        ClearModal(); ModalOpen = true;
        modal = MakePanel("Ending", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1190, 760), new Color(0.055f, 0.085f, 0.09f, 0.98f));
        MakeText(modal.transform, "Filed", 18, bodyFont, Brass, TextAnchor.MiddleLeft,
            new Vector2(0.09f, 0.83f), Vector2.zero, new Vector2(950, 40)).text = "ACCESSION COMPLETE   /   LINKED CORRECTION FILED";
        MakeText(modal.transform, "Ending title", 52, displayFont, Cream, TextAnchor.MiddleLeft,
            new Vector2(0.09f, 0.69f), Vector2.zero, new Vector2(980, 95)).text = "Before the transfer";
        MakeText(modal.transform, "Ending body", 23, bodyFont, Cream, TextAnchor.UpperLeft,
            new Vector2(0.09f, 0.58f), new Vector2(0, -10), new Vector2(960, 310)).text = story.ending + "\n\n" + story.endingConsequence;
        MakeButton(modal.transform, "Begin another investigation", 20, new Vector2(0.5f, 0.11f), new Vector2(380, 60), restart);
    }

    public void ShowMessage(string message, bool toast = true)
    {
        if (toast) hintToast.text = message;
        else promptText.text = message;
        toastTimer = 3.2f;
    }

    public void CloseModal()
    {
        ClearModal(); ModalOpen = false;
        promptText.text = "";
    }

    GameObject MakeModalShell(string eyebrow)
    {
        GameObject panel = MakePanel("Document modal", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1580, 920), new Color(0.055f, 0.085f, 0.09f, 0.985f));
        MakeText(panel.transform, "Modal eyebrow", 17, bodyFont, Brass, TextAnchor.MiddleLeft,
            new Vector2(0.075f, 0.92f), Vector2.zero, new Vector2(1430, 40)).text = eyebrow;
        MakeImage(panel.transform, "Divider", new Color(0.78f, 0.67f, 0.44f, 0.72f), new Vector2(0.075f, 0.875f), Vector2.zero, new Vector2(1420, 2));
        return panel;
    }

    GameObject MakePanel(string name, Vector2 anchor, Vector2 position, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = panel.GetComponent<Image>();
        image.color = color;
        return panel;
    }

    Text MakeText(Transform parent, string name, int size, Font font, string htmlColor, TextAnchor alignment,
        Vector2 anchor, Vector2 position, Vector2 dimensions)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        Text text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.color = ColorValue(htmlColor);
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.supportRichText = false;
        text.raycastTarget = false;
        return text;
    }

    Image MakeImage(Transform parent, string name, Color color, Vector2 anchor, Vector2 position, Vector2 dimensions)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    Button MakeButton(Transform parent, string label, int size, Vector2 anchor, Vector2 dimensions, Action action)
    {
        GameObject go = new GameObject(label + " button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = dimensions;
        Image image = go.GetComponent<Image>();
        image.color = new Color(0.13f, 0.18f, 0.18f, 1f);
        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => action?.Invoke());
        Text text = MakeText(go.transform, "Label", size, bodyFont, Cream, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), Vector2.zero, dimensions - new Vector2(20, 8));
        text.fontStyle = FontStyle.Bold;
        return button;
    }

    InputField MakeInput(Transform parent, Vector2 anchor, Vector2 dimensions)
    {
        GameObject go = new GameObject("Answer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = dimensions;
        go.GetComponent<Image>().color = new Color(0.11f, 0.15f, 0.15f, 1f);
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(go.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16, 7);
        textRect.offsetMax = new Vector2(-16, -7);
        Text value = textObject.GetComponent<Text>();
        value.font = bodyFont;
        value.fontSize = 26;
        value.color = CreamColor;
        value.alignment = TextAnchor.MiddleLeft;
        GameObject placeholderObject = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        placeholderObject.transform.SetParent(go.transform, false);
        RectTransform placeholderRect = placeholderObject.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(16, 7);
        placeholderRect.offsetMax = new Vector2(-16, -7);
        Text placeholder = placeholderObject.GetComponent<Text>();
        placeholder.font = bodyFont;
        placeholder.fontSize = 22;
        placeholder.fontStyle = FontStyle.Italic;
        placeholder.color = MutedColor;
        placeholder.text = "e.g. 3.28";
        InputField input = go.GetComponent<InputField>();
        input.textComponent = value;
        input.placeholder = placeholder;
        input.contentType = InputField.ContentType.DecimalNumber;
        answerField = input;
        return input;
    }

    void ClearModal()
    {
        if (modal != null) Destroy(modal);
        modal = null;
        answerField = null;
    }

    static Color ColorValue(string html) => ColorUtility.TryParseHtmlString(html, out Color c) ? c : Color.white;
    static Color CreamColor => ColorValue(Cream);
    static Color MutedColor => ColorValue(Muted);
}

