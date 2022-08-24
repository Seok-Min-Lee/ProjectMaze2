using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ValueManager
{
    public const float PLAYER_MOVE_SPEED_DEFAULT = 4f;  // �÷��̾� �̵� �ӵ� �⺻��
    public const float PLAYER_SPRINT_SPEED_DEFAULT = 10f;   // �÷��̾� �޸��� �ӵ� �⺻��
    public const float PLAYER_DETOX_ACTIVATE_TIME = 5.0f;     //'�ߵ�' ���� ������ ���� ���Է� �ð�
    public const float PLAYER_CONFUSION_STACK_UPDATE_TIME = 1.0f;   //'����' ���� ������Ʈ �ð�
    public const float PLAYER_CONFUSION_DURATION = 5.0f;  // '����' ���� �� ���� �ð�

    public const float PLAYER_MAGIC_SPEED_RATIO = 1.5f; // �÷��̾� �̵��ӵ� ���� ����

    public const int PLAYER_MAGIC_GIANT_STACK_MAX = 5;  // �÷��̾� ��� �ִ� ����
    public const int PLAYER_POISON_STACK_MAX = 5;   // �� �ִ� ����
    public const int PLAYER_POISON_TIC_DAMAGE = 2;    // �� ���ô� ��Ʈ ������
    public const int PLAYER_CONFUSION_STACK_MAX = 100;  // '����' ���� �ִ밪
}
