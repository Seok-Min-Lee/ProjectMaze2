using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapMachPairManager : Trap
{
    public TrapMachPairColumn[] leftColumns;
    public TrapMachPairColumn[] rightColumns;

    Dictionary<TrapMachPairType, int> columnValueCountDictionary;

    Player player;
    bool isActive, isMaching;
    TrapMachPairType machTypeFirst, machTypeSecond;

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

    public override void DeactivateEvent(Player player = null)
    {

    }

    private void Start()
    {
        columnValueCountDictionary = new Dictionary<TrapMachPairType, int>();
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
        Debug.Log("Success");
    }

    private void MachPairFail()
    {
        Debug.Log("Fail");
        player.currentHp -= 33;
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
            type = (TrapMachPairType)(int)(Random.RandomRange(min: 0, max: leftColumns.Length) + 1);

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
            type = (TrapMachPairType)(int)(Random.RandomRange(min: 0, max: leftColumns.Length) + 1);

            // 딕셔너리에 추가된 타입 중 2번 이상 사용되지 않은 것을 사용한다.
            if ((columnValueCountDictionary.TryGetValue(key: type, value: out int value) && value < 2))
            {
                break;
            }
        }

        return type;
    }
}
