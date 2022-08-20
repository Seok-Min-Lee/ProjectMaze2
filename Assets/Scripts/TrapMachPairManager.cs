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
    bool isActive, isMaching;
    int machCount, maxMachCount;

    public override void ActivateEvent(Player player = null)
    {
        if (!isActive)
        {
            isActive = true;
            this.player = player;

            //¦ �й�
            SetPair();

            //��� Ȱ��ȭ
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
        Debug.Log("Mach Pair Success");
    }

    private void MachPairFail()
    {
        Debug.Log("Mach Pair Fail");
        player.currentHp -= 33;
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
        // ¦�� ���� �� �׷��� ������ Ÿ���� �й��Ѵ�.
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

            // ��ųʸ��� ���Ե� Ÿ���� �ߺ��̱� ������ ������� �ʴ´�.
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

            // ��ųʸ��� �߰��� Ÿ�� �� 2�� �̻� ������ ���� ���� ����Ѵ�.
            if ((columnValueCountDictionary.TryGetValue(key: type, value: out int value) && value < 2))
            {
                break;
            }
        }

        return type;
    }
}
