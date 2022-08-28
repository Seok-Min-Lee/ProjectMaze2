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
    private float gainPowerTime = 2f;   // 운동량과 스케일 증가 시간, Awake() 에서 초기화 하면 안됨.
    private bool isShoot;

    void Awake()
    {
        this.rigid = GetComponent<Rigidbody>();

        //에디터에서는 작동하나 빌드한 프로그램에서는 작동하지 않음
        // 임시로 최대값인 상태로 처리

        //StartCoroutine(GainPowerTimer());
        //StartCoroutine(GainPower());

        //angularPower = 1f;
        //scaleValue = 0.05f;
        //angularPowerIncrementValue = 0.05f;
        //scaleValueIncrementValue = 0.01f;

        this.transform.localScale = Vector3.one * this.scaleValueMaxValue;
        this.rigid.AddTorque(torque: this.transform.right * this.angularPowerMaxValue, mode: ForceMode.Acceleration);
    }

    private void Start()
    {
        Destroy(obj: this.gameObject, t: this.lifeTime);
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
        this.rigid.angularVelocity = Vector3.zero;
        this.rigid.velocity = Vector3.zero;

        this.mesh.SetActive(false);
        this.explosionEffect.SetActive(true);

        Destroy(obj: this.gameObject, t: 1f);
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
            if (this.angularPower < this.angularPowerMaxValue)
            {
                this.angularPower += this.angularPowerIncrementValue;
            }

            if (this.scaleValue < this.scaleValueMaxValue)
            {
                this.scaleValue += this.scaleValueIncrementValue;
            }

            this.transform.localScale = Vector3.one * this.scaleValue;
            this.rigid.AddTorque(torque: transform.right * this.angularPower, mode: ForceMode.Acceleration);

            yield return null;
        }
    }
}
