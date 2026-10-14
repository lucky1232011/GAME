using System;

[Serializable]
public sealed class QuietHoursStory
{
    public string title;
    public string institution;
    public string chapter;
    public string protagonist;
    public string opening;
    public string objective;
    public string question;
    public QuietHoursDocument[] documents;
    public QuietHoursPuzzle puzzle;
    public string ending;
    public string endingConsequence;
}

[Serializable]
public sealed class QuietHoursDocument
{
    public string id;
    public string title;
    public string location;
    public string body;
    public string evidenceTitle;
    public string evidenceBody;
}

[Serializable]
public sealed class QuietHoursPuzzle
{
    public string id;
    public string title;
    public string prompt;
    public float answer;
    public float tolerance;
    public string answerFormat;
    public string reasoning;
    public string[] hints;
    public string successText;
}

[Serializable]
public sealed class QuietHoursSave
{
    public string[] discovered = Array.Empty<string>();
    public bool puzzleSolved;
    public bool correctionFiled;
    public float playerX = 3f;
}
