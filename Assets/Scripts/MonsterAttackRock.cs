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
    private float gainPowerTime = ValueManager.MONSTER_CATAPULT_PROJECTILE_GAIN_POWER_TIME;   // ����� ������ ���� �ð�, Awake() ���� �ʱ�ȭ �ϸ� �ȵ�.
    private bool isShoot;

    void Awake()
    {
        this.rigid = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // � �̺�Ʈ�� �߻����� �ʴ��� �ð��� ������ �ı��ǵ��� ��.
        Destroy(obj: this.gameObject, t: this.lifeTime);

        // ������ �Բ� ���� ����
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
        // ���� ��ġ�� ����
        this.rigid.angularVelocity = Vector3.zero;
        this.rigid.velocity = Vector3.zero;

        // �Ž��� ��Ȱ��ȭ �� ����Ʈ Ȱ��ȭ
        this.mesh.SetActive(false);
        this.explosionEffect.SetActive(true);

        // ������ �ð��� ������ �ı�
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
            // ����� ������ ����
            if (this.angularPower < this.angularPowerMaxValue)
            {
                this.angularPower += this.angularPowerIncrementValue;
            }

            if (this.scaleValue < this.scaleValueMaxValue)
            {
                this.scaleValue += this.scaleValueIncrementValue;
            }

            // ������ ����� ������ ����
            this.transform.localScale = Vector3.one * this.scaleValue;
            this.rigid.AddTorque(torque: transform.right * this.angularPower, mode: ForceMode.Acceleration);

            yield return null;
        }
    }
}
