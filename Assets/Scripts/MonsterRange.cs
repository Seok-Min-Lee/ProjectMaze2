using System.Collections;
using System.Collections.Generic;
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

    void Update()
    {
        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        if(attackDelay > attackTime)
        {
            animator.SetTrigger(name: NameManager.ANIMATION_PARAMETER_DO_ATTACK);

            GameObject instantProjectile = Instantiate(original: projectile,
                                                       position: transform.position + instantModifyVec,
                                                       rotation: transform.rotation);
            if (this.type == MonsterType.Range)
            {
                MonsterMissile monsterMissile = instantProjectile.GetComponent<MonsterMissile>();

                instantProjectile.GetComponent<Rigidbody>().velocity = transform.forward * monsterMissile.speed;
                monsterMissile.damage = this.damage;
            }
            else if(this.type == MonsterType.Catapult)
            {
                MonsterAttackRock monsterRock = instantProjectile.GetComponent<MonsterAttackRock>();
                monsterRock.damage = this.damage;
            }
            

            attackDelay = 0;
        }
        else
        {
            attackDelay += Time.deltaTime;
        }

        yield return null;
    }

    public void TurnBack()
    {
        this.isReverse = !this.isReverse;

        this.gameObject.transform.rotation = Quaternion.Euler(0, transform.localEulerAngles.y + 180, 0);
    }
}
