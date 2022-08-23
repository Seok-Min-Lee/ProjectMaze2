using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueType
{
    None = 0,
    Normal = 1, // 일반적인 스크립트
    Question = 2,   // 선택지가 있는 스크립트
    Option = 3, // 선택지
    Event = 4, // 이벤트가 발생하는 스크립트
}
