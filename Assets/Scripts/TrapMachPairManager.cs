using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapMachPairManager : Trap
{
    public TrapMachPairColumn[] leftColumns;
    public TrapMachPairColumn[] rightColumns;
    public GameObject reward;

    Dictionary<TrapMachPairType, int> columnValueCountDictionary;

    Player player;
    TrapMachPairType machTypeFirst, machTypeSecond;
    bool isMaching;
    int machCount, maxMachCount;

    public override void ActivateEvent(Player player = null)
    {
        if (!isActive)
        {
            isActive = true;
            this.player = player;

            //짝 분배
            SetPair();

            //기둥 활성화
            ActivateTrapMachPairColumns(columns: leftColumns);
            ActivateTrapMachPairColumns(columns: rightColumns);
        }
    }

    private void Start()
    {
        columnValueCountDictionary = new Dictionary<TrapMachPairType, int>();
        machCount = 0;
        maxMachCount = leftColumns.Length;

        reward.SetActive(false);
    }

    private void Update()
    {
        MachEnd();
    }

    public void MachPair(TrapMachPairType type)
    {
        if (isMaching)
        {
            machTypeSecond = type;

            if(machTypeFirst == machTypeSecond)
            {
                MachPairSuccess();
            }
            else
            {
                MachPairFail();
            }

            machCount++;
            isMaching = false;
        }
        else
        {
            machTypeFirst = type;
            machTypeSecond = TrapMachPairType.None;

            isMaching = true;
        }
    }

    private void MachPairSuccess()
    {
    }

    private void MachPairFail()
    {
        player.OnDamage(value: ValueManager.TRAP_MACH_PAIR_FAIL_DAMAGE, isAvoidable: true);
    }

    private void MachEnd()
    {
        if (machCount >= maxMachCount)
        {
            reward.SetActive(true);
        }
    }

    private void SetPair()
    {
        // 짝을 맞출 두 그룹을 나누어 타입을 분배한다.
        TrapMachPairType type;

        foreach (TrapMachPairColumn column in leftColumns)
        {
            type = SetLeftColumnValue();

            column.machType = type;
            columnValueCountDictionary.Add(key: type, value: 1);
        }

        foreach (TrapMachPairColumn column in rightColumns)
        {
            type = SetRightColumnValue();

            column.machType = type;
            columnValueCountDictionary[type]++;
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
            type = (TrapMachPairType)(Random.Range(minInclusive: 0, maxExclusive: leftColumns.Length) + 1);

            // 딕셔너리에 포함된 타입은 중복이기 때문에 사용하지 않는다.
            if (!columnValueCountDictionary.ContainsKey(key: type))
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
            if ((columnValueCountDictionary.TryGetValue(key: type, value: out int value) && value < 2))
            {
                break;
            }
        }

        return type;
    }
}
