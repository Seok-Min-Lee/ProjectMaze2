using System.Collections.Generic;

public struct IngamePreference 
{
    public IngamePreference(
        int id,
        string name,
        string value
    )
    {
        this.id = id;
        this.name = name;
        this.value = value;
    }

    public int id { get; private set; }
    public string name { get; private set; }
    public string value { get; private set; }
}

public class IngamePreferenceCollection : List<IngamePreference>
{
    public IngamePreferenceCollection() : base()
    {

    }
    public IngamePreferenceCollection(IEnumerable<IngamePreference> ingamePreferences)
    {
        this.AddRange(ingamePreferences);
    }
}
