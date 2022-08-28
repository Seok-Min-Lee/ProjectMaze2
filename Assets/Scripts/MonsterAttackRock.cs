using System.Collections;
using UnityEngine;

public class MonsterAttackRock : MonoBehaviour
{
    public GameObject mesh, explosionEffect;
    public float angularPowerMaxValue, scaleValueMaxValue; // 운동량과 스케일의 최대값
    public int damage;
    public float lifeTime;

    Rigidbody rigid;

    float angularPower, scaleValue; // 운동량과 스케일
    float angularPowerIncrementValue, scaleValueIncrementValue; // 운동량과 스케일의 증가값
    bool isShoot;

    float gainPowerTime = 2f;   // 운동량과 스케일 증가 시간, Awake() 에서 초기화 하면 안됨.

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();

        //에디터에서는 작동하나 빌드한 프로그램에서는 작동하지 않음
        // 임시로 최대값인 상태로 처리

        //StartCoroutine(GainPowerTimer());
        //StartCoroutine(GainPower());

        //angularPower = 1f;
        //scaleValue = 0.05f;
        //angularPowerIncrementValue = 0.05f;
        //scaleValueIncrementValue = 0.01f;

        transform.localScale = Vector3.one * scaleValueMaxValue;
        rigid.AddTorque(torque: transform.right * angularPowerMaxValue, mode: ForceMode.Acceleration);
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

    public void ExplosionDestroy()
    {
        rigid.angularVelocity = Vector3.zero;
        rigid.velocity = Vector3.zero;

        mesh.SetActive(false);
        explosionEffect.SetActive(true);

        Destroy(obj: this.gameObject, t: 1f);
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
