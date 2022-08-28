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
            this.manager.ActivateTrafficLight(isActivate: true);
            this.manager.UpdateTrafficLightByType(type: this.trafficLightType);

            StopCoroutine(Timer());
            StartCoroutine(Timer());
        }
    }

    public override void DeactivateEvent(Player player = null)
    {
        this.isActive = false;
        StopAllCoroutines();
        this.manager.ActivateTrafficLight(isActivate: false);
    }

    private void Update()
    {
        if(this.trafficLightType != this.latestType)
        {
            this.latestType = this.trafficLightType;
            this.manager.UpdateTrafficLightByType(trafficLightType);
        }
    }

    public void Jaywalk(Player player)
    {
        if (this.trafficLightType == TrapTrafficLightType.Red &&
            player.IsMoving())
        {
            int _damage = this.damage > 0 ? this.damage : player.maxHp;

            player.OnDamage(value: _damage, isAvoidable: false);
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

            this.trafficLightType = TrapTrafficLightType.Orange;
            yield return new WaitForSeconds(durationOrange);

            StartCoroutine(Timer());
        }
    }
}
