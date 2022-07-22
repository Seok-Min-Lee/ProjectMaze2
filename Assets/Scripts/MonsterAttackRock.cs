using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackRock : MonoBehaviour
{
    public float angularPowerMaxValue, scaleValueMaxValue; // 운동량과 스케일의 최대값
    public int damage;

    Rigidbody rigid;

    float angularPower, scaleValue; // 운동량과 스케일
    float angularPowerIncrementValue, scaleValueIncrementValue; // 운동량과 스케일의 증가값
    float delayDestroy;
    bool isShoot;

    float gainPowerTime = 2f;   // 운동량과 스케일 증가 시간, Awake() 에서 초기화 하면 안됨.

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
