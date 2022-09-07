public class TrapMisteryDoor : Trap
{
    public override void ActivateEvent(Player player = null)
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);

        if(player != null)
        {
            player.manager.DisplayConfirmMessage(text: ValueManager.MESSAGE_MISTERY_DOOR_ACTIVATE, type: EventMessageType.TrapSuccess);
        }
    }
}
