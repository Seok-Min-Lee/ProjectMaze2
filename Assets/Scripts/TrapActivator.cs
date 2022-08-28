using UnityEngine;

public class TrapActivator : MonoBehaviour
{
    public GameObject effect;
    public GameObject[] traps;
    public TrapDeactivator deactivator;
    public bool isVolatility;   // 1ȸ���̸� true

    private void Start()
    {
        if (this.isVolatility && this.effect != null)
        {
            this.effect.SetActive(true);
        }
    }

    public void ActivateTrap(Player player = null)
    {
        // Ʈ�� �̺�Ʈ Ȱ��ȭ
        foreach(GameObject trap in traps)
        {
            trap.GetComponent<Trap>().ActivateEvent(player: player);
        }
        
        // 1ȸ���� ��� Ȱ���⸦ ��Ȱ��ȭ
        if (this.isVolatility)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            // 1ȸ���� �� ��Ȱ���Ⱑ ������ �� ����.
            // ����� ��Ȱ���Ⱑ ������ �װ��� Ȱ��ȭ.
            if (this.deactivator != null)
            {
                this.deactivator.gameObject.SetActive(true);
            }
        }
    }

    public void DeactivateTrap(Player player = null)
    {
        // Ʈ�� �̺�Ʈ ��Ȱ��ȭ
        foreach (GameObject trap in traps)
        {
            trap.GetComponent<Trap>().DeactivateEvent(player: player);
        }
    }
}
