using System.Collections;
using UnityEngine;

public class MonsterAttackRock : MonoBehaviour
{
    public GameObject mesh, explosionEffect;
    public float angularPowerMaxValue, scaleValueMaxValue; // ����� �������� �ִ밪
    public int damage;
    public float lifeTime;

    private Rigidbody rigid;
    private float angularPower, scaleValue; // ����� ������
    private float angularPowerIncrementValue, scaleValueIncrementValue; // ����� �������� ������
    private float gainPowerTime = 2f;   // ����� ������ ���� �ð�, Awake() ���� �ʱ�ȭ �ϸ� �ȵ�.
    private bool isShoot;

    void Awake()
    {
        this.rigid = GetComponent<Rigidbody>();

        //�����Ϳ����� �۵��ϳ� ������ ���α׷������� �۵����� ����
        // �ӽ÷� �ִ밪�� ���·� ó��

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
