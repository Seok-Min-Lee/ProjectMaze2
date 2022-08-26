using System.Collections.Generic;

public struct Guide
{
    public Guide(
        int id,
        GuideType type,
        string title,
        string description
    )
    {
        this.id = id;
        this.type = type;
        this.title = title;
        this.description = description;
    }

    public int id { get; private set; }
    public GuideType type { get; private set; }
    public string title { get; private set; }
    public string description { get; private set; }
}

public class GuideCollection : List<Guide>
{
    public GuideCollection() : base()
    {

    }
    public GuideCollection(IEnumerable<Guide> guides)
    {
        this.AddRange(guides);
    }
}