using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapMachPairManager : Trap
{
    public GameManager manager;
    public TrapMachPairColumn[] leftColumns;
    public TrapMachPairColumn[] rightColumns;
    public GameObject reward;

    private Dictionary<TrapMachPairType, int> columnValueCountDictionary;

    private Player player;
    private TrapMachPairType machTypeFirst, machTypeSecond;
    private bool isMaching;
    private int machCount, maxMachCount;

    public override void ActivateEvent(Player player = null)
    {
        if (!this.isActive)
        {
            this.isActive = true;
            this.player = player;

            //짝 분배
            SetPair();

            //기둥 활성화
            ActivateTrapMachPairColumns(columns: this.leftColumns);
            ActivateTrapMachPairColumns(columns: this.rightColumns);
        }
    }

    private void Start()
    {
        this.columnValueCountDictionary = new Dictionary<TrapMachPairType, int>();
        this.machCount = 0;
        this.maxMachCount = this.leftColumns.Length;

        this.reward.SetActive(false);
    }

    private void Update()
    {
        MachEnd();
    }

    public void MachPair(TrapMachPairType type)
    {
        if (this.isMaching)
        {
            this.machTypeSecond = type;

            if(this.machTypeFirst == this.machTypeSecond)
            {
                MachPairSuccess();
            }
            else
            {
                MachPairFailure();
            }

            this.machCount++;
            this.isMaching = false;
        }
        else
        {
            this.machTypeFirst = type;
            this.machTypeSecond = TrapMachPairType.None;

            this.isMaching = true;
        }
    }

    private void MachPairSuccess()
    {
        manager.DisplayConfirmMessage(text: ValueManager.MESSAGE_TRAP_MACH_PAIR_SUCCESS, type: EventMessageType.TrapSuccess);
    }

    private void MachPairFailure()
    {
        player.OnDamage(value: ValueManager.TRAP_MACH_PAIR_FAIL_DAMAGE, isAvoidable: true);
        manager.DisplayConfirmMessage(text: ValueManager.MESSAGE_TRAP_MACH_PAIR_FAILURE, type: EventMessageType.TrapFailure);
    }

    private void MachEnd()
    {
        if (this.machCount >= this.maxMachCount)
        {
            this.reward.SetActive(true);
        }
    }

    private void SetPair()
    {
        // 짝을 맞출 두 그룹을 나누어 타입을 분배한다.
        TrapMachPairType type;

        foreach (TrapMachPairColumn column in this.leftColumns)
        {
            type = SetLeftColumnValue();

            column.machType = type;
            this.columnValueCountDictionary.Add(key: type, value: 1);
        }

        foreach (TrapMachPairColumn column in this.rightColumns)
        {
            type = SetRightColumnValue();

            column.machType = type;
            this.columnValueCountDictionary[type]++;
        }
    }

    private void ActivateTrapMachPairColumns(IEnumerable<TrapMachPairColumn> columns)
    {
        foreach(TrapMachPairColumn column in columns)
        {
            column.ActivateEvent();
        }
    }

    private TrapMachPairType SetLeftColumnValue()
    {
        TrapMachPairType type = TrapMachPairType.None;

        while (true)
        {
            type = (TrapMachPairType)(Random.Range(minInclusive: 0, maxExclusive: this.leftColumns.Length) + 1);

            // 딕셔너리에 포함된 타입은 중복이기 때문에 사용하지 않는다.
            if (!this.columnValueCountDictionary.ContainsKey(key: type))
            {
                break;
            }
        }

        return type;
    }

    private TrapMachPairType SetRightColumnValue()
    {
        TrapMachPairType type = TrapMachPairType.None;

        while (true)
        {
            type = (TrapMachPairType)(Random.Range(minInclusive: 0, maxExclusive: leftColumns.Length) + 1);

            // 딕셔너리에 추가된 타입 중 2번 이상 사용되지 않은 것을 사용한다.
            if (this.columnValueCountDictionary.TryGetValue(key: type, value: out int value) && value < 2)
            {
                break;
            }
        }

        return type;
    }
}
