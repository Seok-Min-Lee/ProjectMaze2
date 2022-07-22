using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackRock : MonoBehaviour
{
    public float angularPowerMaxValue, scaleValueMaxValue; // ����� �������� �ִ밪
    public int damage;

    Rigidbody rigid;

    float angularPower, scaleValue; // ����� ������
    float angularPowerIncrementValue, scaleValueIncrementValue; // ����� �������� ������
    float delayDestroy;
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

        delayDestroy = 4f;
    }

    private void Start()
    {
        Destroy(obj: this.gameObject, t: delayDestroy);
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
            Debug.Log(transform.localScale);
            rigid.AddTorque(torque: transform.right * angularPower, mode: ForceMode.Acceleration);

            yield return null;
        }
    }
}
