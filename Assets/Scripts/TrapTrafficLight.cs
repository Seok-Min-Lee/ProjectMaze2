using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTrafficLight : Trap
{
    public GameManager manager;
    public TrapTrafficLightType type { get; private set; }
    public int damage;

    TrapTrafficLightType latestType;
    Player player;
    bool isActive;

    public override void ActivateEvent(Player player = null)
    {
        if (!isActive)
        {
            this.gameObject.SetActive(true);
            isActive = true;

            latestType = TrapTrafficLightType.Green;
            type = TrapTrafficLightType.Green;
            this.player = player;

            // UI 업데이트
            manager.ActivateTrafficLight(isActivate: true);
            manager.UpdateTrafficLightByType(type: this.type);

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
        if(type != latestType)
        {
            latestType = type;
            manager.UpdateTrafficLightByType(type);
        }
    }

    public void Jaywalk(Player player)
    {
        //if(type == TrapTrafficLightType.Red &&
        //   player.IsMoving())
        //{
        //    player.ChangeCurrentHp(value: -damage);
        //}
    }

    IEnumerator Timer()
    {
        if(player != null)
        {
            type = TrapTrafficLightType.Green;
            yield return new WaitForSeconds(2f);

            type = TrapTrafficLightType.Orange;
            yield return new WaitForSeconds(0.5f);

            type = TrapTrafficLightType.Red;
            yield return new WaitForSeconds(2f);

            type = TrapTrafficLightType.Orange;
            yield return new WaitForSeconds(0.5f);

            StartCoroutine(Timer());
        }
    }
}
