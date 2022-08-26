using System.Collections;
using UnityEngine;

public class MonsterRange : Monster
{
    public GameObject projectile;
    public Transform projectilePosition;
    public float attackTime;

    public bool isReverse;  // 현재 방향전환을 앞,뒤로만 하고 있기 때문에 Bool 타입으로 처리 (처리 로직은 Player 스크립트 참조)
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

            // 투사체 오브젝트 생성
            GameObject instantProjectile = Instantiate(
                original: projectile,
                position: transform.position + instantModifyVec,
                rotation: transform.rotation
            );

            // 몬스터 타입에 따라 투사체 세팅
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
