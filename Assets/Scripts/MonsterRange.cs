using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterRange : Monster
{
    public GameObject projectile;
    public float attackTime;

    Animator animator;

    Vector3 instantModifyVec = Vector3.up * 2;
    float attackDelay;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        Attack();
    }

    private void TurnBackward()
    {
    }

    private void Attack()
    {
        if(attackDelay > attackTime)
        {
            animator.SetTrigger(name: NameManager.ANIMATION_PARAMETER_DO_ATTACK);

            GameObject instantProjectile = Instantiate(original: projectile, 
                                                       position: transform.position + instantModifyVec, 
                                                       rotation: transform.rotation);

            MonsterMissile monsterMissile = instantProjectile.GetComponent<MonsterMissile>();
            instantProjectile.GetComponent<Rigidbody>().AddForce(force: Vector3.right * monsterMissile.speed, mode: ForceMode.Impulse);
            monsterMissile.damage = this.damage;

            attackDelay = 0;
        }
        else
        {
            attackDelay += Time.deltaTime;
        }
    }
}
