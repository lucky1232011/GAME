using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class QuietHoursGame : MonoBehaviour
{
    const float RoomWidth = 16f;
    const float FloorY = -2.79f;
    const string SaveKey = "QuietHours.Save.v1";
    static readonly string[] RoomNames = { "North Corridor", "Records Wing", "Signal Lab", "Calibration Room" };
    static readonly string[] RoomSprites = { "Room_Corridor", "Room_Records", "Room_SignalLab", "Room_Calibration" };
    static readonly string[] AmbienceNames = { "Ambience_Corridor", "Ambience_Records", "Ambience_SignalLab", "Ambience_Calibration" };

    sealed class Point
    {
        public string id;
        public float x;
        public string label;
        public bool puzzle;
        public bool gate;
        public bool fileCorrection;
    }

    QuietHoursStory story;
    QuietHoursSave save;
    QuietHoursUI ui;
    QuietHoursPlayer2D player;
    Rigidbody2D playerBody;
    Camera gameCamera;
    BoxCollider2D sealedDoor;
    GameObject sealedDoorVisual;
    TextMesh doorSign;
    AudioSource interactionAudio;
    AudioSource[] roomAudio;
    AudioClip paperClip;
    AudioClip terminalClip;
    AudioClip chimeClip;
    readonly List<Point> points = new List<Point>();
    Point currentTarget;
    string activeDocumentId;
    string activePointId;
    int hintIndex;
    int currentRoom = -1;
    float saveClock;
    bool gameStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindAnyObjectByType<QuietHoursGame>() != null) return;
        new GameObject("Quiet Hours Runtime").AddComponent<QuietHoursGame>();
    }

    void Start()
    {
        story = LoadStory();
        save = LoadSave();
        BuildWorld();
        BuildPoints();
        ui = gameObject.AddComponent<QuietHoursUI>();
        ui.Configure(Begin, CloseModal, FileActiveEvidence, SubmitAnswer, RequestHint, Resume, OpenEvidence);
        ui.ShowStart(story);
        playerBody.position = new Vector2(Mathf.Clamp(save.playerX, 0.8f, 63.2f), FloorY);
        UpdateDoor();
    }

    void Update()
    {
        if (player == null || ui == null) return;
        bool canPlay = gameStarted && !ui.ModalOpen && !save.correctionFiled;
        player.controlsEnabled = canPlay;
        if (!canPlay)
        {
            playerBody.linearVelocity = Vector2.zero;
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && gameStarted && ui.ModalOpen && !save.correctionFiled)
                CloseModal();
            if (save.correctionFiled && gameStarted && !ui.ModalOpen) ui.ShowEnding(story, Restart);
            return;
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame) { ui.ShowPause(Restart); return; }
            if (Keyboard.current.tabKey.wasPressedThisFrame) { OpenEvidence(); return; }
            if (Keyboard.current.eKey.wasPressedThisFrame) Interact();
        }

        FollowPlayer();
        UpdateRoomAndAudio();
        currentTarget = FindNearbyPoint();
        ui.SetHud(RoomNames[currentRoom], CurrentLead(), DiscoveredDocuments().Count,
            currentTarget != null ? InteractionPrompt(currentTarget) : "");

        saveClock += Time.deltaTime;
        if (saveClock >= 2f) { SaveProgress(); saveClock = 0f; }
    }

    void BuildWorld()
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            if (root != gameObject)
            {
                root.SetActive(false);
                Destroy(root);
            }

        GameObject cameraObject = new GameObject("Quiet Hours 2D Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.transform.SetParent(transform, false);
        cameraObject.transform.position = new Vector3(8f, 0f, -10f);
        gameCamera = cameraObject.GetComponent<Camera>();
        gameCamera.orthographic = true;
        gameCamera.orthographicSize = 4.5f;
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCamera.backgroundColor = new Color(0.025f, 0.04f, 0.04f);

        for (int i = 0; i < RoomSprites.Length; i++)
        {
            Texture2D texture = Resources.Load<Texture2D>("QuietHours/Rooms/" + RoomSprites[i]);
            if (texture == null) { Debug.LogError("Missing room plate: " + RoomSprites[i]); continue; }
            Sprite art = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 120f);
            GameObject room = new GameObject(RoomNames[i] + " side-view", typeof(SpriteRenderer));
            room.transform.SetParent(transform, false);
            room.transform.position = new Vector3(i * RoomWidth + RoomWidth * 0.5f, 0f, 0f);
            SpriteRenderer renderer = room.GetComponent<SpriteRenderer>();
            renderer.sprite = art;
            renderer.sortingOrder = -20;
        }

        MakeBox("Continuous floor", new Vector2(32f, FloorY - 0.48f), new Vector2(64f, 1f));
        MakeBox("West end wall", new Vector2(-0.25f, 0f), new Vector2(0.5f, 9f));
        MakeBox("East end wall", new Vector2(64.25f, 0f), new Vector2(0.5f, 9f));

        GameObject actor = new GameObject("Ira Sen / protagonist", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(QuietHoursPlayer2D));
        actor.transform.SetParent(transform, false);
        actor.transform.position = new Vector3(3f, FloorY, -0.2f);
        Texture2D idle = Resources.Load<Texture2D>("QuietHours/Character/Ira_Idle");
        Texture2D stepA = Resources.Load<Texture2D>("QuietHours/Character/Ira_StepA");
        Texture2D stepB = Resources.Load<Texture2D>("QuietHours/Character/Ira_StepB");
        SpriteRenderer sprite = actor.GetComponent<SpriteRenderer>();
        if (idle != null) sprite.sprite = Sprite.Create(idle, new Rect(0, 0, idle.width, idle.height), new Vector2(0.5f, 0f), 384f);
        sprite.sortingOrder = 5;
        Rigidbody2D body = actor.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        BoxCollider2D hitbox = actor.GetComponent<BoxCollider2D>();
        hitbox.size = new Vector2(0.4f, 0.72f);
        hitbox.offset = new Vector2(0f, 0.36f);
        playerBody = body;
        player = actor.GetComponent<QuietHoursPlayer2D>();
        player.characterSprite = sprite;
        player.controlsEnabled = false;
        player.animationFrames = new Sprite[3];
        player.animationFrames[0] = sprite.sprite;
        if (stepA != null) player.animationFrames[1] = Sprite.Create(stepA, new Rect(0, 0, stepA.width, stepA.height), new Vector2(0.5f, 0f), 384f);
        if (stepB != null) player.animationFrames[2] = Sprite.Create(stepB, new Rect(0, 0, stepB.width, stepB.height), new Vector2(0.5f, 0f), 384f);
        GameObject steps = new GameObject("Footsteps");
        steps.transform.SetParent(actor.transform, false);
        player.footsteps = steps.AddComponent<AudioSource>();
        player.footsteps.playOnAwake = false;
        player.stepClip = Resources.Load<AudioClip>("QuietHours/Audio/Step");

        BuildDoor();
        CreateAudio();
    }

    void BuildDoor()
    {
        sealedDoorVisual = MakeSprite("Calibration seal", new Vector2(49.6f, 0f), new Vector2(0.5f, 5.8f), new Color(0.09f, 0.13f, 0.125f), 2);
        sealedDoor = sealedDoorVisual.AddComponent<BoxCollider2D>();
        sealedDoor.size = Vector2.one;
        MakeSprite("Seal brass edge left", new Vector2(49.35f, 0f), new Vector2(0.06f, 5.85f), new Color(0.68f, 0.55f, 0.35f), 3);
        MakeSprite("Seal brass edge right", new Vector2(49.85f, 0f), new Vector2(0.06f, 5.85f), new Color(0.68f, 0.55f, 0.35f), 3);
        doorSign = MakeTextMesh("Seal sign", new Vector3(49.6f, 2.15f, -0.35f), "SEALED", 0.09f, new Color(0.85f, 0.72f, 0.48f));
        doorSign.GetComponent<MeshRenderer>().sortingOrder = 4;
    }

    void BuildPoints()
    {
        AddDoc("transfer_notice", 5.2f, "Read the transfer notice");
        AddDoc("public_report", 22.4f, "Inspect the release copy");
        AddDoc("carbon_slip", 24.1f, "Inspect the carbon slip");
        AddDoc("alignment_card", 26.3f, "Read the receiver alignment card");
        AddDoc("review_note", 35.4f, "Read the preliminary review note");
        AddDoc("temperature_strip", 43.25f, "Read the room-temperature strip");
        points.Add(new Point { id = "math_terminal", x = 40.1f, label = "Use the receiver terminal", puzzle = true });
        points.Add(new Point { id = "sealed_gate", x = 48.1f, label = "Examine the sealed passage", gate = true });
        AddDoc("maintenance_memo", 58.4f, "Inspect the service-line memo");
        AddDoc("supervisor_addendum", 61.0f, "Read the supervisor's draft");
        points.Add(new Point { id = "file_correction", x = 62.6f, label = "Attach a linked correction", fileCorrection = true });
    }

    void AddDoc(string id, float x, string label) => points.Add(new Point { id = id, x = x, label = label });

    void FollowPlayer()
    {
        Vector3 target = new Vector3(Mathf.Clamp(player.transform.position.x, 8f, 56f), 0f, -10f);
        gameCamera.transform.position = Vector3.Lerp(gameCamera.transform.position, target, 1f - Mathf.Exp(-5f * Time.deltaTime));
    }

    void UpdateRoomAndAudio()
    {
        int room = Mathf.Clamp(Mathf.FloorToInt(player.transform.position.x / RoomWidth), 0, RoomNames.Length - 1);
        if (room != currentRoom)
        {
            currentRoom = room;
            if (roomAudio[room].clip != null && !roomAudio[room].isPlaying) roomAudio[room].Play();
            Play(paperClip, 0.16f);
        }
        for (int i = 0; i < roomAudio.Length; i++)
        {
            float targetVolume = gameStarted && i == currentRoom ? 0.24f : 0f;
            roomAudio[i].volume = Mathf.MoveTowards(roomAudio[i].volume, targetVolume, Time.deltaTime * 0.12f);
        }
    }

    Point FindNearbyPoint()
    {
        Point selected = null;
        float best = 1.45f;
        float x = player.transform.position.x;
        foreach (Point point in points)
        {
            float distance = Mathf.Abs(x - point.x);
            if (distance < best) { selected = point; best = distance; }
        }
        return selected;
    }

    string InteractionPrompt(Point point)
    {
        if (point.gate && !save.puzzleSolved) return "E   EXAMINE SEALED DOOR     •     Resolve the receiver readings to open it";
        if (point.id == "maintenance_memo" && !save.puzzleSolved) return "The calibration room remains sealed";
        if (point.id == "supervisor_addendum" && !HasEvidence("maintenance_memo")) return "The binder is clipped shut • find the service-line memo";
        if (point.puzzle && !HasPuzzleEvidence()) return "Terminal needs the carbon slip, alignment card, and temperature strip";
        if (point.fileCorrection && !HasEvidence("supervisor_addendum")) return "A linked correction needs the supervisor's draft";
        return "E   " + point.label.ToUpperInvariant() + "     •     TAB   CASE NOTES";
    }

    void Interact()
    {
        if (currentTarget == null) return;
        Point point = currentTarget;
        if (point.gate && !save.puzzleSolved)
        {
            ui.ShowMessage("The brass seal holds. The receiver terminal may establish what produced the later return.");
            return;
        }
        if (point.id == "maintenance_memo" && !save.puzzleSolved)
        {
            ui.ShowMessage("The calibration room is sealed. Resolve the receiver location to release it.");
            return;
        }
        if (point.id == "supervisor_addendum" && !HasEvidence("maintenance_memo"))
        {
            ui.ShowMessage("The draft is clipped behind a note. The service-line memo may explain why.");
            return;
        }
        if (point.puzzle)
        {
            if (!HasPuzzleEvidence()) { ui.ShowMessage("The terminal asks for the carbon mark, clock alignment, and room temperature."); return; }
            activePointId = point.id;
            hintIndex = 0;
            Play(terminalClip, 0.6f);
            ui.ShowPuzzle(story.puzzle, hintIndex);
            return;
        }
        if (point.fileCorrection)
        {
            if (!HasEvidence("supervisor_addendum")) { ui.ShowMessage("The accession terminal needs the supervisor's draft first."); return; }
            activePointId = point.id;
            ui.ShowDocument(new QuietHoursDocument
            {
                id = "correction_station", title = "Linked correction / accession 17", location = "Calibration room",
                body = "Attach a linked note to the signed release copy. The original remains unchanged.\n\nThe correction will preserve the measured result and the relevant records together."
            }, save.correctionFiled);
            return;
        }
        QuietHoursDocument document = FindDocument(point.id);
        if (document == null) { Debug.LogWarning("No story document registered for " + point.id); return; }
        activeDocumentId = document.id;
        activePointId = point.id;
        Play(paperClip, 0.5f);
        ui.ShowDocument(document, HasEvidence(document.id));
    }

    void FileActiveEvidence()
    {
        if (activePointId == "file_correction")
        {
            save.correctionFiled = true;
            SaveProgress();
            UpdateDoor();
            Play(chimeClip, 0.7f);
            ui.ShowEnding(story, Restart);
            return;
        }

        QuietHoursDocument document = FindDocument(activeDocumentId);
        if (document == null) return;
        if (!HasEvidence(document.id))
        {
            List<string> ids = new List<string>(save.discovered ?? Array.Empty<string>());
            ids.Add(document.id);
            save.discovered = ids.ToArray();
            SaveProgress();
        }
        Play(paperClip, 0.72f);
        ui.CloseModal();
        ui.ShowMessage("Filed: " + document.evidenceTitle, true);
    }

    void SubmitAnswer(string raw)
    {
        if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
        {
            ui.ShowMessage("Enter a distance in metres, such as 3.28.", true);
            return;
        }
        if (Mathf.Abs(value - story.puzzle.answer) <= story.puzzle.tolerance + 0.00001f)
        {
            save.puzzleSolved = true;
            SaveProgress();
            UpdateDoor();
            Play(chimeClip, 0.62f);
            ui.CloseModal();
            ui.ShowMessage(story.puzzle.successText);
        }
        else ui.ShowMessage("That value does not fit both arrival times yet. Recheck the sign and the distance sum.", true);
    }

    void RequestHint()
    {
        int maximum = story.puzzle.hints != null ? story.puzzle.hints.Length : 0;
        hintIndex = Mathf.Clamp(hintIndex + 1, 0, maximum);
        ui.ShowPuzzle(story.puzzle, hintIndex);
    }

    void OpenEvidence() => ui.ShowEvidence(DiscoveredDocuments());
    void Resume() => ui.CloseModal();
    void CloseModal() => ui.CloseModal();

    void Begin()
    {
        gameStarted = true;
        player.controlsEnabled = true;
        if (save.correctionFiled) ui.ShowEnding(story, Restart);
    }

    void Restart()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
        save = new QuietHoursSave();
        gameStarted = false;
        currentRoom = -1;
        activeDocumentId = null;
        activePointId = null;
        hintIndex = 0;
        player.controlsEnabled = false;
        playerBody.position = new Vector2(3f, FloorY);
        playerBody.linearVelocity = Vector2.zero;
        UpdateDoor();
        ui.ShowStart(story);
    }

    void UpdateDoor()
    {
        bool locked = !save.puzzleSolved;
        if (sealedDoor != null) sealedDoor.enabled = locked;
        if (sealedDoorVisual != null) sealedDoorVisual.SetActive(locked);
        if (doorSign != null) doorSign.text = locked ? "SEALED" : "OPEN";
    }

    BoxCollider2D MakeBox(string name, Vector2 center, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(BoxCollider2D));
        go.transform.SetParent(transform, false);
        go.transform.position = center;
        BoxCollider2D collider = go.GetComponent<BoxCollider2D>();
        collider.size = size;
        return collider;
    }

    GameObject MakeSprite(string name, Vector2 position, Vector2 scale, Color color, int order)
    {
        Texture2D pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        pixel.SetPixel(0, 0, Color.white);
        pixel.Apply();
        Sprite whiteSprite = Sprite.Create(pixel, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        GameObject go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.SetParent(transform, false);
        go.transform.position = new Vector3(position.x, position.y, -0.3f);
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = whiteSprite;
        renderer.color = color;
        renderer.sortingOrder = order;
        return go;
    }

    TextMesh MakeTextMesh(string name, Vector3 position, string value, float characterSize, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(TextMesh));
        textObject.transform.SetParent(transform, false);
        textObject.transform.position = position;
        TextMesh text = textObject.GetComponent<TextMesh>();
        text.text = value;
        text.fontSize = 42;
        text.characterSize = characterSize;
        text.alignment = TextAlignment.Center;
        text.anchor = TextAnchor.MiddleCenter;
        text.color = color;
        Font builtin = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (builtin != null) text.font = builtin;
        return text;
    }

    void CreateAudio()
    {
        roomAudio = new AudioSource[RoomNames.Length];
        for (int i = 0; i < RoomNames.Length; i++)
        {
            GameObject sourceObject = new GameObject("Ambience / " + RoomNames[i]);
            sourceObject.transform.SetParent(transform, false);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f;
            source.clip = Resources.Load<AudioClip>("QuietHours/Audio/" + AmbienceNames[i]);
            roomAudio[i] = source;
        }
        GameObject interactionObject = new GameObject("Evidence sounds");
        interactionObject.transform.SetParent(transform, false);
        interactionAudio = interactionObject.AddComponent<AudioSource>();
        interactionAudio.playOnAwake = false;
        paperClip = Resources.Load<AudioClip>("QuietHours/Audio/Paper");
        terminalClip = Resources.Load<AudioClip>("QuietHours/Audio/Terminal");
        chimeClip = Resources.Load<AudioClip>("QuietHours/Audio/CorrectionChime");
    }

    QuietHoursStory LoadStory()
    {
        TextAsset json = Resources.Load<TextAsset>("QuietHours/Data/story");
        if (json == null)
        {
            Debug.LogError("Missing story.json under Assets/Resources/QuietHours/Data.");
            return new QuietHoursStory { title = "The Quiet Interval", institution = "Morrow Institute / Records Annex", chapter = "Last accession, 05:17", protagonist = "Ira Sen", opening = "A signed transcript and its carbon copy disagree.", objective = "Reconcile the missing line before the transfer.", question = "What produced the second return?", documents = Array.Empty<QuietHoursDocument>(), puzzle = new QuietHoursPuzzle { answer = 3.28f, tolerance = 0.01f, hints = Array.Empty<string>() }, ending = "The file is amended.", endingConsequence = "The record is preserved." };
        }
        return JsonUtility.FromJson<QuietHoursStory>(json.text);
    }

    QuietHoursSave LoadSave()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new QuietHoursSave();
        try
        {
            QuietHoursSave loaded = JsonUtility.FromJson<QuietHoursSave>(PlayerPrefs.GetString(SaveKey));
            return loaded ?? new QuietHoursSave();
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not read Quiet Hours save; starting a fresh file. " + exception.Message);
            return new QuietHoursSave();
        }
    }

    void SaveProgress()
    {
        if (save == null) return;
        if (player != null) save.playerX = player.transform.position.x;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
        PlayerPrefs.Save();
    }

    QuietHoursDocument FindDocument(string id)
    {
        if (story.documents == null) return null;
        foreach (QuietHoursDocument document in story.documents) if (document.id == id) return document;
        return null;
    }

    bool HasEvidence(string id)
    {
        if (save.discovered == null) return false;
        foreach (string found in save.discovered) if (found == id) return true;
        return false;
    }

    bool HasPuzzleEvidence() => HasEvidence("carbon_slip") && HasEvidence("alignment_card") && HasEvidence("temperature_strip");

    List<QuietHoursDocument> DiscoveredDocuments()
    {
        List<QuietHoursDocument> result = new List<QuietHoursDocument>();
        if (story.documents != null)
            foreach (QuietHoursDocument document in story.documents)
                if (HasEvidence(document.id)) result.Add(document);
        return result;
    }

    string CurrentLead()
    {
        if (save.correctionFiled) return "Accession amended • signed original preserved";
        if (HasEvidence("supervisor_addendum")) return "Attach the correction beside the signed copy";
        if (HasEvidence("maintenance_memo")) return "The service-line record changes the reading";
        if (save.puzzleSolved) return "The calibration room is open";
        if (HasPuzzleEvidence()) return "Resolve the receiver location from the timing difference";
        if (HasEvidence("carbon_slip")) return "Check the receiver alignment and room temperature";
        if (HasEvidence("public_report")) return "Find the original carbon entry in Records";
        if (HasEvidence("transfer_notice")) return "Compare the release copy with the carbon slip";
        return story.objective;
    }

    void Play(AudioClip clip, float volume)
    {
        if (interactionAudio != null && clip != null) interactionAudio.PlayOneShot(clip, volume);
    }
}
