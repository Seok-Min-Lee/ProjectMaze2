using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapFadeInAndOut : Trap
{
    public float waittingTime;

    Renderer renderer;
    Color updateColor;

    float delayFadeChange;  // �Ž� ��ȯ �Ϸ� �� �ݴ�� ��ȯ�ϱ���� ���ð�
    float updateCountPerSecond; // 1�ʴ� �Ž� ������Ʈ Ƚ��
    float updateIntervalTime;   // �Ž� ������Ʈ ���͹�

    private void Start()
    {
        renderer = GetComponent<Renderer>();

        delayFadeChange = ValueManager.TRAP_FADE_IN_AND_OUT_FADE_CHANGE_DELAY;
        
        updateCountPerSecond = ValueManager.TRAP_FADE_IN_AND_OUT_UPDATE_COUNT_PER_ONE_SECOND;
        updateIntervalTime = (float)1 / updateCountPerSecond;
    }

    public override void ActivateEvent(Player player = null)
    {
        if (!isActive)
        {
            StopCoroutine(ActiaveAfterWatting());
            StartCoroutine(ActiaveAfterWatting());

            isActive = true;
        }
    }

    public override void DeactivateEvent(Player player = null)
    {
        isActive = false;
    }

    IEnumerator ActiaveAfterWatting()
    {
        yield return new WaitForSeconds(waittingTime);
        
        StopCoroutine(FadeInAndOut());
        StartCoroutine(FadeInAndOut());
    }

    IEnumerator FadeInAndOut()
    {
        while (isActive)
        {
            // Fade Out
            for (int i = (int)updateCountPerSecond; i >= 0; i--)
            {
                UpdateMaterialAlphaValue(value: (float)(i / updateCountPerSecond));

                yield return new WaitForSeconds(updateIntervalTime);
            }
            this.GetComponent<Collider>().enabled = false;

            yield return new WaitForSeconds(delayFadeChange);

            // Fade In
            this.GetComponent<Collider>().enabled = true;
            for (int i = 0; i <= updateCountPerSecond; i++)
            {
                UpdateMaterialAlphaValue(value: (float)(i / updateCountPerSecond));

                yield return new WaitForSeconds(updateIntervalTime);
            }

            yield return new WaitForSeconds(delayFadeChange);
        }
    }

    private void UpdateMaterialAlphaValue(float value)
    {
        updateColor = renderer.material.color;
        updateColor.a = value;
        renderer.material.color = updateColor;
    }
}
