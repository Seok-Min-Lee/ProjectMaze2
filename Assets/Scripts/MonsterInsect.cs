using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MonsterInsect : Monster
{
    public GameObject meshObject, effectObject;
    public Transform target;
    public BoxCollider AttackArea;
    public float senseRadius, senseDistance, lockOnRadius, lockOnDistance;

    private NavMeshAgent agent;
    private Animator animator;

    bool isAttack, isSuicide;

    private void Start()
    {
        this.agent = GetComponent<NavMeshAgent>();
        this.animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Debug.DrawRay(start: this.transform.position + (Vector3.up * 2), dir: Vector3.right * this.senseDistance, color: Color.red);

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
        //RaycastHit hit;

        //if(Physics.Raycast(origin: transform.position + (Vector3.up * 2), 
        //                   direction: Vector3.right,
        //                   maxDistance: maxDistance, 
        //                   layerMask: LayerMask.GetMask(NameManager.LAYER_PLAYER),
        //                   hitInfo: out hit))
        //{
        //    target = hit.transform;

        //    animator.SetBool(name: NameManager.ANIMATION_PARAMETER_RUN_FORWARD, value: true);
        //}
        RaycastHit[] hits = Physics.SphereCastAll(
            origin: transform.position + (Vector3.up * 2),
            direction: Vector3.right,
            radius: senseRadius,
            maxDistance: senseDistance,
            layerMask: LayerMask.GetMask(NameManager.LAYER_PLAYER)
        );

        if(hits.Length > 0)
        {
            this.target = hits[0].transform;
            this.animator.SetBool(name: NameManager.ANIMATION_PARAMETER_RUN_FORWARD, value: true);
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
            if (!this.isSuicide)
            {
                this.isSuicide = true;

                StartCoroutine(routine: SuiCide());
            }

            this.isAttack = true;

            this.agent.isStopped = false;
            this.animator.SetBool(name: NameManager.ANIMATION_PARAMETER_RUN_FORWARD, value: false);
            this.animator.SetTrigger(name: NameManager.ANIMATION_PARAMETER_STAB_ATTACK);

            StartCoroutine(routine: Attack());
        }
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.2f);
        this.AttackArea.enabled = true;

        yield return new WaitForSeconds(0.2f);
        this.AttackArea.enabled = false;

        yield return new WaitForSeconds(0.6f);
        this.animator.SetBool(name: NameManager.ANIMATION_PARAMETER_RUN_FORWARD, value: true);
        this.isAttack = false;
    }

    IEnumerator SuiCide()
    {
        yield return new WaitForSeconds(3f);
        ExplosionDestroy();
    }

    public void ExplosionDestroy()
    {
        this.meshObject.SetActive(false);
        this.effectObject.SetActive(true);

        Destroy(obj: this.gameObject, t: 1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(center: this.transform.position + this.transform.forward * this.lockOnDistance, radius: this.lockOnRadius);
        Gizmos.DrawWireSphere(center: this.transform.position + (Vector3.up * 2) + this.transform.forward * this.senseDistance, radius: this.senseRadius);
    }
}
