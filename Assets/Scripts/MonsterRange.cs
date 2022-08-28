using System.Collections;
using UnityEngine;

public class MonsterRange : Monster
{
    public GameObject projectile;
    public Transform projectilePosition;
    public float attackTime;

    public bool isReverse;  // ���� ������ȯ�� ��,�ڷθ� �ϰ� �ֱ� ������ Bool Ÿ������ ó�� (ó�� ������ Player ��ũ��Ʈ ����)
    
    private Animator animator;
    private Vector3 instantModifyVec;
    private float attackDelay;

    private void Start()
    {
        this.animator = GetComponentInChildren<Animator>();

        this.instantModifyVec = this.projectilePosition.position - this.transform.position;
    }

    private void Update()
    {
        StartCoroutine(Attack());
    }

    public void TurnBack()
    {
        this.isReverse = !this.isReverse;

        this.gameObject.transform.rotation = Quaternion.Euler(0, this.transform.localEulerAngles.y + ValueManager.MONSTER_TURN_BACK_ANGLE, 0);
    }

    IEnumerator Attack()
    {
        if(this.attackDelay > this.attackTime)
        {
            this.animator.SetTrigger(name: NameManager.ANIMATION_PARAMETER_DO_ATTACK);

            // ����ü ������Ʈ ����
            GameObject instantProjectile = Instantiate(
                original: this.projectile,
                position: this.transform.position + this.instantModifyVec,
                rotation: this.transform.rotation
            );

            // ���� Ÿ�Կ� ���� ����ü ����
            switch (this.type)
            {
                case MonsterType.Range:
                    AttackTypeRange(projectile: instantProjectile);
                    break;
                case MonsterType.Catapult:
                    AttackTypeCatapult(projectile: instantProjectile);
                    break;
            }
            
            this.attackDelay = 0;
        }
        else
        {
            this.attackDelay += Time.deltaTime;
        }

        yield return null;
    }

    private void AttackTypeRange(GameObject projectile)
    {
        MonsterMissile monsterMissile = projectile.GetComponent<MonsterMissile>();

        projectile.GetComponent<Rigidbody>().velocity = this.transform.forward * monsterMissile.speed;
        monsterMissile.damage = this.damage;
    }

    private void AttackTypeCatapult(GameObject projectile)
    {
        MonsterAttackRock monsterRock = projectile.GetComponent<MonsterAttackRock>();
        monsterRock.damage = this.damage;
    }
}
