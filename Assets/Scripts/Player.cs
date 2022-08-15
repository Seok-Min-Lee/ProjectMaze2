using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class Player : MonoBehaviour
{
    public StarterAssetsInputs _input { get; private set; }

    public GameManager manager;
    public GameObject addictEffect, detoxEffect, confusionEffect, confusionChargeEffect;
    public GameObject[] beads;

    public int currentHp, maxHp;
    public int currentLife, maxLife;
    public int currentConfusion = 0, maxConfusion = 100;
    public bool[] enableBeads;

    public bool isPoison;
    public bool isConfusion;
    public bool isInteract, wasInteractPreprocess, isInteractPreprocessReady;

    CharacterController controller;
    Vector3 respawnPoint, interactPoint;
    NPCInteractionZone interactNpc;

    int poisonStack = 0;
    float countTimeDetox = 0f, countTimeConfusion = 0f;

    int poisonStackMax, poisonTicDamage;
    float activateTimeDetox, activateTimeConfusion, durationConfusion;

    #region ##### 유니티 내장 함수 #####
    
    private void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
        enableBeads = new bool[3];

        manager.InitializePlayer(
            playerPoisonStackMax: out poisonStackMax,
            playerPoisonTicDamage: out poisonTicDamage,
            playerActivateTimeDetox: out activateTimeDetox,
            playerActivateTimeConfusion: out activateTimeConfusion,
            playerDurationConfusion: out durationConfusion
        );
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        //OnDamage();
        Detoxify();
        InteractPreprocess();
        Interact();
        InteractPostProcess();
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject gameObject = other.gameObject;

        switch (other.tag)
        {
            case NameManager.TAG_ITEM:
                GetItem(gameObject: gameObject);
                break;

            case NameManager.TAG_PLAYER_RESPAWN:
                respawnPoint = other.GetComponent<Transform>().position + (Vector3.up * 10);    // 리스폰 지점 업데이트
                break;

            case NameManager.TAG_FALL:
                Respawn();
                break;

            case NameManager.TAG_MONSTER_ATTACK:
                OnDamage(monster: other.GetComponentInParent<Monster>());
                break;

            case NameManager.TAG_MONSTER_TURN_BACK_AREA:
                other.GetComponentInParent<MonsterRange>().TurnBack();
                break;

            case NameManager.TAG_TRAP_ACTIVATOR:
                other.GetComponentInParent<TrapActivator>().ActivateTrap(player: this);
                break;

            case NameManager.TAG_TRAP_DEACTIVATOR:
                other.GetComponentInParent<TrapDeactivator>().CallDeactivateTrap(player: this);
                break;

            case NameManager.TAG_PORTAL:
                Portal portal = other.GetComponentInParent<Portal>();
                manager.UpdateScene(sceneType: portal.nextSceneType);
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        switch (other.tag)
        {
            case NameManager.TAG_NEAGTIVE_EFFECT:
                OnTriggerStayInNegativeEffectZone(negativeEffect: other.GetComponent<NegativeEffectZone>());
                break;

            case NameManager.TAG_NPC_INTERACTION_ZONE:
                OnTriggerStayInNpcInteractionZone(npc: other.GetComponent<NPCInteractionZone>());
                break;

            case NameManager.TAG_TRAP:
                OnTriggerStayInTrap(trapGameObject: other.gameObject);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            case NameManager.TAG_NPC_INTERACTION_ZONE:
                OnTriggerExitFromNpcInteractionZone();
                break;
        }
    }

    #endregion

    #region ##### 실질 기능 함수 #####

    private void GetItem(GameObject gameObject)
    {
        Item item = gameObject.GetComponent<Item>();

        switch (item.type)
        {
            case ItemType.Heal:
                // hp, life 둘다 0 인 경우 GameOver 추가 필요.
                ChangeCurrentHp(value: item.value);
                break;
            case ItemType.Map:
                // 미니맵 관리를 플레이어가 아닌 외부에서 하게 될 경우 수정.
                manager.ActivateMinimap(isActive: true);
                break;
            case ItemType.Bead:
                // 구슬 관리를 플레이어가 아닌 외부에서 하게 될 경우 수정.
                ActivateBead(index: item.value);
                break;
        }

        gameObject.SetActive(false);
    }

    private void ActivateBead(int index)
    {
        if(index >= 0 && index < enableBeads.Length)
        {
            enableBeads[index] = true;
            beads[index].SetActive(true);
        }
    }

    private void OnDamage(Monster monster)
    {
        KnockBack();    // 내용 추가 필요.

        // monster 별 업데이트 할 것.
        switch (monster.type)
        {
            case MonsterType.Insect:
                OnDamageByInsect(damage: monster.damage);
                break;
            case MonsterType.Zombie:
                break;
            case MonsterType.Range:
                break;
            case MonsterType.Catapult:
                break;
            default:
                break;
        }

        StartCoroutine(routine: Addict(summaryDamage: -poisonTicDamage * poisonStack));
    }

    private void KnockBack()
    {

    }

    private void OnDamageByInsect(int damage)
    {
        currentHp -= damage;    // 기본공격 데미지

        isPoison = true;        // 중독 상태 활성화
        poisonStack = poisonStack < poisonStackMax ? poisonStack + 1 : poisonStackMax;  // 중독 스택 변경
        ChangePoisonEffect(isAddict: isPoison); // 중독 이펙트 활성화

        countTimeDetox = 0;       // 해독 타이머 초기화
    }

    private IEnumerator Addict(int summaryDamage)
    {
        if (isPoison)
        {
            ChangeCurrentHp(value: summaryDamage);
        }

        yield return new WaitForSeconds(1f);
        StartCoroutine(routine: Addict(summaryDamage: summaryDamage));
    }

    private void Detoxify()
    {
        if (isPoison)
        {
            UpdateCountTimeDetox();

            if(countTimeDetox > activateTimeDetox)
            {
                isPoison = false;
                ChangePoisonEffect(isAddict: isPoison);
            }
        }
    }

    private void OnTriggerStayInNegativeEffectZone(NegativeEffectZone negativeEffect)
    {
        if (!isConfusion)
        {
            Timer(tick: Time.deltaTime, time: ref countTimeConfusion);
            if (countTimeConfusion >= activateTimeConfusion)
            {
                StartCoroutine(routine: ActiveVolatileEffect(effect: confusionChargeEffect, duration: confusionChargeEffect.GetComponent<ParticleSystem>().duration));

                currentConfusion += negativeEffect.value;
                countTimeConfusion = 0f;

                if (currentConfusion >= maxConfusion)
                {
                    StartCoroutine(routine: Confuse());
                }
            }
        }
    }

    private void OnTriggerStayInNpcInteractionZone(NPCInteractionZone npc)
    {
        interactNpc = npc;
        isInteractPreprocessReady = true;
        interactPoint = npc.transform.position;
    }

    private void OnTriggerStayInTrap(GameObject trapGameObject)
    {
        TrapTrafficLight trap = trapGameObject.GetComponent<TrapTrafficLight>();
        if (trap != null &&
            trap.type == TrapTrafficLightType.Red &&
            IsMoving())
        {
            Respawn();
            trap.DeactivateEvent(player: this);
        }
    }

    private void OnTriggerExitFromNpcInteractionZone()
    {
        isInteractPreprocessReady = false;
        wasInteractPreprocess = false;
        isInteract = false;
    }

    private void UpdateCountTimeDetox()
    {
        // 점프, 이동 입력 없을 경우 정지 상태로 판단
        if (!_input.jump &&
            _input.move == Vector2.zero)
        {
            Timer(tick: Time.deltaTime, time: ref countTimeDetox);
        }
        else
        {
            countTimeDetox = 0;
        }
    }

    private void ChangePoisonEffect(bool isAddict)
    {
        addictEffect.SetActive(isAddict);
        detoxEffect.SetActive(!isAddict);

        if (isAddict)
        {
            addictEffect.GetComponent<ParticleSystem>().maxParticles = poisonStack;
        }
        else
        {
            detoxEffect.GetComponent<ParticleSystem>().maxParticles = poisonStack;
        }
    }

    private void InteractPreprocess()
    {
        // 상호작용 전 한 번만 실행하기 위한 조건 세팅
        // 1. 상호작용 상태가 아니다.
        // 2. 상호작용 전처리를 하지 않은 상태이다.
        // 3. 상호작용 전처리 준비 상태이다.
        // 4. 상호작용 버튼 입력이 들어왔다.
        if (!isInteract &&
            !wasInteractPreprocess &&
            isInteractPreprocessReady && 
            _input.interact)
        {
            // 플레이어 위치 NPC 앞으로 이동 및 정지
            // NPC 바라보도록 추후 보완.
            ForceToMove(point: interactPoint);
            StopPlayerMotion();

            // 카메라 위치 조정
            Vector3 cameraPosition = manager.npcInteractionCamera.gameObject.transform.position;
            cameraPosition.x = this.transform.position.x;
            manager.MoveGameObject(gameObject: manager.npcInteractionCamera, vector: cameraPosition);

            // 카메라 및 UI 업데이트.
            manager.UpdateUINormalToInteraction(npc: interactNpc);

            // 상호작용 준비 상태 업데이트.
            wasInteractPreprocess = true;
            isInteract = true;

            _input.interact = false;
        }
    }

    private void Interact()
    {
        //InteractPreprocess();

        if (wasInteractPreprocess && isInteract)
        {
            // 상호작용 과정
            if (_input.interact)
            {
                isInteract = !manager.TryUpdateInteractionUI();

                _input.interact = false;
            }
        }

        //InteractPostProcess();
    }

    private void InteractPostProcess()
    {
        // 상호작용 후 한 번만 실행하기 위한 조건 세팅
        // 1. 상호작용 상태가 아니다.
        // 2. 상호작용 전처리를 한 상태이다.
        // 3. 상호작용 버튼 입력이 들어왔다.
        if (!isInteract &&
            wasInteractPreprocess &&
            _input.interact)
        {
            // UI 업데이트.
            manager.UpdateUIWhetherInteraction(isInteract: false);

            // 상호작용 다시 할 수 있게 하기 위한 변수 초기화
            wasInteractPreprocess = false;

            // 상호작용 키 입력 초기화
            _input.interact = false;
        }
    }

    IEnumerator Confuse()
    {
        isConfusion = true;
        _input.isReverse = true;

        // 플레이어 이펙트 활성화 추가
        confusionEffect.SetActive(true);
        yield return new WaitForSeconds(durationConfusion);

        //플레이어 이펙트 비활성화 추가
        confusionEffect.SetActive(false);
        _input.isReverse = false;
        isConfusion = false;

        currentConfusion = 0;
    }
    #endregion

    #region ##### 다용도 함수 #####

    public void ForceToMove(Vector3 point)
    {
        // CharacterController 가 활성화되어 있으면 Transform.position 값을 변경해도 적용되지 않는다.
        controller.enabled = false;
        this.transform.position = point;
        controller.enabled = true;
    }

    public bool IsMoving()
    {
        if(_input.jump || _input.move != Vector2.zero)
        {
            return true;
        }

        return false;
    }

    public void ChangeCurrentHp(int value)
    {
        currentHp += value;

        if (currentHp > maxHp)
        {
            currentHp = maxHp;
        }
        else if (currentHp < 0)
        {
            currentHp = 0;

            if (currentLife > 0)
            {
                Respawn();
            }
            else
            {
                //Gameover 추가.
            }
        }
    }

    private void StopPlayerMotion()
    {
        _input.MoveInput(newMoveDirection: Vector2.zero);
        _input.JumpInput(newJumpState: false);
        _input.SprintInput(newSprintState: false);
        _input.InteractInput(newInteractState: false);
    }

    private void Respawn()
    {
        ForceToMove(point: respawnPoint);

        currentHp = maxHp;
        currentLife--;
    }

    private void Timer(float tick, ref float time)
    {
        time += tick;
    }

    IEnumerator ActiveVolatileEffect(GameObject effect, float duration)
    {
        effect.SetActive(true);
        yield return new WaitForSeconds(duration);
        effect.SetActive(false);
    }

    #endregion
}
