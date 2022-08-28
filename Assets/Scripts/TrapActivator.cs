using UnityEngine;

public class TrapActivator : MonoBehaviour
{
    public GameObject effect;
    public GameObject[] traps;
    public TrapDeactivator deactivator;
    public bool isVolatility;   // 1회성이면 true

    private void Start()
    {
        if (this.isVolatility && this.effect != null)
        {
            this.effect.SetActive(true);
        }
    }

    public void ActivateTrap(Player player = null)
    {
        // 트랩 이벤트 활성화
        foreach(GameObject trap in traps)
        {
            trap.GetComponent<Trap>().ActivateEvent(player: player);
        }
        
        // 1회성인 경우 활성기를 비활성화
        if (this.isVolatility)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            // 1회성일 때 비활성기가 존재할 수 없음.
            // 연결된 비활성기가 있으면 그것을 활성화.
            if (this.deactivator != null)
            {
                this.deactivator.gameObject.SetActive(true);
            }
        }
    }

    public void DeactivateTrap(Player player = null)
    {
        // 트랩 이벤트 비활성화
        foreach (GameObject trap in traps)
        {
            trap.GetComponent<Trap>().DeactivateEvent(player: player);
        }
    }
}
