public class TrapMisteryWall : Trap
{
    public override void ActivateEvent(Player player = null)
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
}
