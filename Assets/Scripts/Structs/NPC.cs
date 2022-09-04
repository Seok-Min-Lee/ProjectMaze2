using System.Collections.Generic;

public struct Npc
{
    public Npc(int id, string name)
    {
        this.id = id;
        this.name = name;
    }

    public int id { get; private set; }
    public string name { get; private set; }
}

public class NpcDataCollection : List<Npc>
{
    public NpcDataCollection() : base()
    {

    }

    public NpcDataCollection(IEnumerable<Npc> raws)
    {
        this.AddRange(raws);
    }
}
