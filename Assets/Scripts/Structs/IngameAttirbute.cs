using System.Collections.Generic;

public struct IngameAttribute
{
    public IngameAttribute(
        int id,
        string attributeName,
        int value
    )
    {
        this.id = id;
        this.attributeName = attributeName;
        this.value = value;
    }

    public int id { get; private set; }
    public string attributeName { get; private set; }
    public int value { get; private set; }
}

public class IngameAttributeCollection : List<IngameAttribute>
{
    public IngameAttributeCollection() : base()
    {

    }

    public IngameAttributeCollection(IEnumerable<IngameAttribute> attributes)
    {
        this.AddRange(attributes);
    }
}
