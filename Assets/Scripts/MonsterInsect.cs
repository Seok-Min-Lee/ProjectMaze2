using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MonsterInsect : Monster
{
    public GameObject meshObject, effectObject;
    public Transform target;
    public BoxCollider AttackArea;
    public float senseRadius, senseDistance, lockOnRadius, lockOnDistance;

    NavMeshAgent agent;
    Animator animator;

    bool isAttack, isSuicide;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Debug.DrawRay(start: transform.position + (Vector3.up * 2), dir: Vector3.right * senseDistance, color: Color.red);

        if (target == null)
        {
            SetNavigationTarget();
        }
        else
        {
            LockOn();
            agent.SetDestination(target: target.position);
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
            target = hits[0].transform;
            animator.SetBool(name: NameManager.ANIMATION_PARAMETER_RUN_FORWARD, value: true);
        }
    }

    private void LockOn()
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            origin: transform.position, 
            direction: Vector3.right, 
            radius: lockOnRadius, 
            maxDistance: lockOnDistance, 
            layerMask: LayerMask.GetMask(NameManager.LAYER_PLAYER)
        );

        if(hits.Length > 0 && !isAttack)
        {
            if (!isSuicide)
            {
                isSuicide = true;

                StartCoroutine(routine: SuiCide());
            }

            isAttack = true;

            agent.isStopped = false;
            animator.SetBool(name: NameManager.ANIMATION_PARAMETER_RUN_FORWARD, value: false);
            animator.SetTrigger(name: NameManager.ANIMATION_PARAMETER_STAB_ATTACK);

            StartCoroutine(routine: Attack());
        }
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.2f);
        AttackArea.enabled = true;

        yield return new WaitForSeconds(0.2f);
        AttackArea.enabled = false;

        yield return new WaitForSeconds(0.6f);
        animator.SetBool(name: NameManager.ANIMATION_PARAMETER_RUN_FORWARD, value: true);
        isAttack = false;
    }

    IEnumerator SuiCide()
    {
        yield return new WaitForSeconds(3f);
        ExplosionDestroy();
    }

    public void ExplosionDestroy()
    {
        meshObject.SetActive(false);
        effectObject.SetActive(true);

        Destroy(obj: this.gameObject, t: 1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(center: transform.position + transform.forward * lockOnDistance, radius: lockOnRadius);
        Gizmos.DrawWireSphere(center: transform.position + (Vector3.up * 2) + transform.forward * senseDistance, radius: senseRadius);
    }
}
