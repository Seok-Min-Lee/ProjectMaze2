using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackRock : MonoBehaviour
{
    public float angularPowerMaxValue, scaleValueMaxValue; // ����� �������� �ִ밪
    public int damage;
    public float lifeTime;

    Rigidbody rigid;

    float angularPower, scaleValue; // ����� ������
    float angularPowerIncrementValue, scaleValueIncrementValue; // ����� �������� ������
    bool isShoot;

    float gainPowerTime = 2f;   // ����� ������ ���� �ð�, Awake() ���� �ʱ�ȭ �ϸ� �ȵ�.

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        StartCoroutine(GainPowerTimer());
        StartCoroutine(GainPower());

        angularPower = 1f;
        scaleValue = 0.05f;
        angularPowerIncrementValue = 0.05f;
        scaleValueIncrementValue = 0.01f;
    }

    private void Start()
    {
        Destroy(obj: this.gameObject, t: lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == NameManager.TAG_WALL)
        {
            Destroy(this.gameObject);
        }
    }

    IEnumerator GainPowerTimer()
    {
        yield return new WaitForSeconds(seconds: gainPowerTime);
        isShoot = true;
    }

    IEnumerator GainPower()
    {
        while (!isShoot)
        {
            if (angularPower < angularPowerMaxValue)
            {
                angularPower += angularPowerIncrementValue;
            }

            if (scaleValue < scaleValueMaxValue)
            {
                scaleValue += scaleValueIncrementValue;
            }

            transform.localScale = Vector3.one * scaleValue;
            rigid.AddTorque(torque: transform.right * angularPower, mode: ForceMode.Acceleration);

            yield return null;
        }
    }
}
