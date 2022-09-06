using System.Collections;
using UnityEngine;

public class MonsterAttackRock : MonoBehaviour
{
    public GameObject mesh, explosionEffect;
    public float angularPowerMaxValue, scaleValueMaxValue; // 운동량과 스케일의 최대값
    public int damage;
    public float lifeTime;

    private Rigidbody rigid;
    private float angularPower, scaleValue; // 운동량과 스케일
    private float angularPowerIncrementValue, scaleValueIncrementValue; // 운동량과 스케일의 증가값
    private float gainPowerTime = ValueManager.MONSTER_CATAPULT_PROJECTILE_GAIN_POWER_TIME;   // 운동량과 스케일 증가 시간, Awake() 에서 초기화 하면 안됨.
    private bool isShoot;

    void Awake()
    {
        this.rigid = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // 어떤 이벤트도 발생하지 않더라도 시간이 지나면 파괴되도록 함.
        Destroy(obj: this.gameObject, t: this.lifeTime);

        // 생성과 함께 성장 시작
        StartCoroutine(GainPowerTimer());
        StartCoroutine(GainPower());

        this.angularPower = ValueManager.MONSTER_CATAPULT_PROJECTILE_ANGULAR_POWER;
        this.scaleValue = ValueManager.MONSTER_CATAPULT_PROJECTILE_SCALE_VALUE;
        this.angularPowerIncrementValue = ValueManager.MONSTER_CATAPULT_PROJECTILE_ANGULAR_POWER_INCREMENT_VALUE;
        this.scaleValueIncrementValue = ValueManager.MONSTER_CATAPULT_PROJECTILE_SCALE_VALUE_INCREMENT_VALUE;
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
        // 폭발 위치에 정지
        this.rigid.angularVelocity = Vector3.zero;
        this.rigid.velocity = Vector3.zero;

        // 매쉬를 비활성화 및 이펙트 활성화
        this.mesh.SetActive(false);
        this.explosionEffect.SetActive(true);

        // 임의의 시간이 지나면 파괴
        Destroy(obj: this.gameObject, t: ValueManager.MONSTER_DESTORY_DELAY);
    }

    IEnumerator GainPowerTimer()
    {
        yield return new WaitForSeconds(seconds: gainPowerTime);
        this.isShoot = true;
    }

    IEnumerator GainPower()
    {
        while (!isShoot)
        {
            // 운동량과 스케일 증가
            if (this.angularPower < this.angularPowerMaxValue)
            {
                this.angularPower += this.angularPowerIncrementValue;
            }

            if (this.scaleValue < this.scaleValueMaxValue)
            {
                this.scaleValue += this.scaleValueIncrementValue;
            }

            // 증가한 운동량과 스케일 적용
            this.transform.localScale = Vector3.one * this.scaleValue;
            this.rigid.AddTorque(torque: transform.right * this.angularPower, mode: ForceMode.Acceleration);

            yield return null;
        }
    }
}
