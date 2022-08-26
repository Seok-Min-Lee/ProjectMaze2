using System.Collections;
using UnityEngine;

public class MonsterRange : Monster
{
    public GameObject projectile;
    public Transform projectilePosition;
    public float attackTime;

    public bool isReverse;  // ���� ������ȯ�� ��,�ڷθ� �ϰ� �ֱ� ������ Bool Ÿ������ ó�� (ó�� ������ Player ��ũ��Ʈ ����)
    Animator animator;

    Vector3 instantModifyVec;
    float attackDelay;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();

        instantModifyVec = projectilePosition.position - this.transform.position;
    }

    private void Update()
    {
        StartCoroutine(Attack());
    }

    public void TurnBack()
    {
        this.isReverse = !this.isReverse;

        this.gameObject.transform.rotation = Quaternion.Euler(0, transform.localEulerAngles.y + ValueManager.MONSTSER_TURN_BACK_ANGLE, 0);
    }

    IEnumerator Attack()
    {
        if(attackDelay > attackTime)
        {
            animator.SetTrigger(name: NameManager.ANIMATION_PARAMETER_DO_ATTACK);

            // ����ü ������Ʈ ����
            GameObject instantProjectile = Instantiate(
                original: projectile,
                position: transform.position + instantModifyVec,
                rotation: transform.rotation
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
            
            attackDelay = 0;
        }
        else
        {
            attackDelay += Time.deltaTime;
        }

        yield return null;
    }

    private void AttackTypeRange(GameObject projectile)
    {
        MonsterMissile monsterMissile = projectile.GetComponent<MonsterMissile>();

        projectile.GetComponent<Rigidbody>().velocity = transform.forward * monsterMissile.speed;
        monsterMissile.damage = this.damage;
    }

    private void AttackTypeCatapult(GameObject projectile)
    {
        MonsterAttackRock monsterRock = projectile.GetComponent<MonsterAttackRock>();
        monsterRock.damage = this.damage;
    }
}
