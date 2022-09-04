using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MonsterInsect : Monster
{
    public GameObject meshObject, effectObject;
    public Transform target, senseCenter;
    public BoxCollider AttackArea;
    public float senseRadius, senseDistance, lockOnRadius, lockOnDistance;

    public bool isAttack { get; private set; }

    private NavMeshAgent agent;
    private Animator animator;
    private bool isSuicide;

    private void Start()
    {
        this.agent = GetComponent<NavMeshAgent>();
        this.animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (target == null)
        {
            SetNavigationTarget();
        }
        else
        {
            LockOn();
            this.agent.SetDestination(target: this.target.position);
        }
    }

    private void SetNavigationTarget()
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            origin: senseCenter.position,
            direction: Vector3.right,
            radius: senseRadius,
            maxDistance: senseDistance,
            layerMask: LayerMask.GetMask(NameManager.LAYER_PLAYER)
        );

        if(hits.Length > 0)
        {
            this.target = hits[0].transform;
            this.animator.SetBool(name: NameManager.ANIMATION_PARAMETER_INSECT_RUN_FORWARD, value: true);
        }
    }

    private void LockOn()
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            origin: this.transform.position, 
            direction: Vector3.right, 
            radius: this.lockOnRadius, 
            maxDistance: this.lockOnDistance, 
            layerMask: LayerMask.GetMask(NameManager.LAYER_PLAYER)
        );

        if(hits.Length > 0 && !isAttack)
        {
            this.isAttack = true;

            this.agent.isStopped = true;
            this.animator.SetBool(name: NameManager.ANIMATION_PARAMETER_INSECT_RUN_FORWARD, value: false);

            StartCoroutine(routine: Attack());
        }
    }

    IEnumerator Attack()
    {
        if (!this.isSuicide)
        {
            this.isSuicide = true;

            StartCoroutine(routine: SuiCide());
        }
        this.animator.SetTrigger(name: NameManager.ANIMATION_PARAMETER_INSECT_STAB_ATTACK);
        yield return new WaitForSeconds(ValueManager.MONSTER_INSECT_ATTACK_AREA_ACTIVATE_DELAY);
        this.AttackArea.enabled = true;

        yield return new WaitForSeconds(ValueManager.MONSTER_INSECT_ATTACK_AREA_DEACTIVATE_DELAY);
        this.AttackArea.enabled = false;

        yield return new WaitForSeconds(ValueManager.MONSTER_INSECT_ATTACK_AREA_DEACTIVATE_AFTER_DELAY);
        this.animator.SetBool(name: NameManager.ANIMATION_PARAMETER_INSECT_RUN_FORWARD, value: true);
        this.isAttack = false;
        this.agent.isStopped = false;
    }

    IEnumerator SuiCide()
    {
        yield return new WaitForSeconds(ValueManager.MONSTER_INSECT_SUICIDE_ANIMATION_BEFORE_DELAY);
        this.animator.SetTrigger(name: NameManager.ANIMATION_PARAMETE_INSECT_DIE);
        yield return new WaitForSeconds(ValueManager.MONSTER_INSECT_SUICIDE_ANIMATION_AFTER_DELAY);
        ExplosionDestroy();
    }

    public void ExplosionDestroy()
    {
        this.meshObject.SetActive(false);
        this.effectObject.SetActive(true);

        Destroy(obj: this.gameObject, t: ValueManager.MONSTER_DESTORY_DELAY);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(center: this.transform.position + this.transform.forward * this.lockOnDistance, radius: this.lockOnRadius);
        Gizmos.DrawWireSphere(center: senseCenter.position, radius: this.senseRadius);
    }
}
