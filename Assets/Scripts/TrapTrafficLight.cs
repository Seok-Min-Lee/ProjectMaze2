using System.Collections;
using UnityEngine;

public class TrapTrafficLight : Trap
{
    public GameManager manager;
    public TrapTrafficLightType trafficLightType { get; private set; }
    public int damage;
    public float durationRed, durationGreen, durationOrange;

    private TrapTrafficLightType latestType;
    private Player player;

    public override void ActivateEvent(Player player = null)
    {
        if (!this.isActive)
        {
            this.gameObject.SetActive(true);
            this.isActive = true;

            this.latestType = TrapTrafficLightType.Green;
            this.trafficLightType = TrapTrafficLightType.Green;
            this.player = player;

            // UI 업데이트
            manager.ActivateTrafficLight(isActivate: true);
            manager.UpdateTrafficLightByType(type: this.trafficLightType);

            StopCoroutine(Timer());
            StartCoroutine(Timer());
        }
    }

    public override void DeactivateEvent(Player player = null)
    {
        this.isActive = false;
        StopAllCoroutines();
        manager.ActivateTrafficLight(isActivate: false);
    }

    private void Update()
    {
        if(this.trafficLightType != this.latestType)
        {
            this.latestType = this.trafficLightType;
            manager.UpdateTrafficLightByType(trafficLightType);
        }
    }

    public void Jaywalk(Player player)
    {
        if (this.trafficLightType == TrapTrafficLightType.Red &&
            player.IsMoving())
        {
            int _damage = this.damage > 0 ? this.damage : player.maxHp;

            player.OnDamage(value: _damage, isAvoidable: false);
            manager.DisplayConfirmMessage(text: ValueManager.MESSAGE_TRAP_TRAFFIC_LIGHT_FAILURE, type: EventMessageType.TrapFailure);

            this.DeactivateEvent(player: this.player);
        }
    }

    IEnumerator Timer()
    {
        if(this.player != null)
        {
            this.trafficLightType = TrapTrafficLightType.Green;
            yield return new WaitForSeconds(durationGreen);

            this.trafficLightType = TrapTrafficLightType.Orange;
            yield return new WaitForSeconds(durationOrange);

            this.trafficLightType = TrapTrafficLightType.Red;
            yield return new WaitForSeconds(durationRed);

            StartCoroutine(Timer());
        }
    }
}
