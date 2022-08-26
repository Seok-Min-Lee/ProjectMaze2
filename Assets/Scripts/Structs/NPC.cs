using System.Collections.Generic;

public struct NPC
{
    public NPC(int id, string name)
    {
        this.id = id;
        this.name = name;
    }

    public int id { get; private set; }
    public string name { get; private set; }
}

public class NpcCollection : List<NPC>
{
    public NpcCollection() : base()
    {

    }

    public NpcCollection(IEnumerable<NPC> raws)
    {
        this.AddRange(raws);
    }
}
