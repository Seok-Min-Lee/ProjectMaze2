using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTrafficLight : Trap
{
    public GameManager manager;
    public TrapTrafficLightType trafficLightType { get; private set; }
    public int damage;

    TrapTrafficLightType latestType;
    Player player;

    public override void ActivateEvent(Player player = null)
    {
        if (!isActive)
        {
            this.gameObject.SetActive(true);
            isActive = true;

            latestType = TrapTrafficLightType.Green;
            trafficLightType = TrapTrafficLightType.Green;
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
        isActive = false;
        StopAllCoroutines();
        manager.ActivateTrafficLight(isActivate: false);
    }

    private void Update()
    {
        if(trafficLightType != latestType)
        {
            latestType = trafficLightType;
            manager.UpdateTrafficLightByType(trafficLightType);
        }
    }

    public void Jaywalk(Player player)
    {
        if (trafficLightType == TrapTrafficLightType.Red &&
            player.IsMoving())
        {
            player.OnDamage(value: player.maxHp, isAvoidable: false);
            this.DeactivateEvent(player: this.player);
        }
    }

    IEnumerator Timer()
    {
        if(player != null)
        {
            trafficLightType = TrapTrafficLightType.Green;
            yield return new WaitForSeconds(2f);

            trafficLightType = TrapTrafficLightType.Orange;
            yield return new WaitForSeconds(0.5f);

            trafficLightType = TrapTrafficLightType.Red;
            yield return new WaitForSeconds(2f);

            trafficLightType = TrapTrafficLightType.Orange;
            yield return new WaitForSeconds(0.5f);

            StartCoroutine(Timer());
        }
    }
}
