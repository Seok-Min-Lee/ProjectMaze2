using System.Collections.Generic;

public struct Dialogue
{
    public Dialogue(
        int id,
        int npcId,
        int situationNo,
        int sequenceNo,
        int sequenceSubNo,
        DialogueType type,
        string text
    )
    {
        this.id = id;
        this.npcId = npcId;
        this.situationNo = situationNo;
        this.sequenceNo = sequenceNo;
        this.sequenceSubNo = sequenceSubNo;
        this.type = type;
        this.text = text;
    }

    public int id { get; private set; }
    public int npcId { get; private set; }
    public int situationNo { get; private set; }
    public int sequenceNo { get; private set; }
    public int sequenceSubNo { get; private set; }
    public DialogueType type { get; private set; }
    public string text { get; private set; }
}

public class DialogueCollection : List<Dialogue>
{
    public DialogueCollection() : base()
    {

    }

    public DialogueCollection(IEnumerable<Dialogue> dialogues)
    {
        this.AddRange(dialogues);
    }
}