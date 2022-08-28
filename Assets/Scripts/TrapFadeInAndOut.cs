using System.Collections;
using UnityEngine;

public class TrapFadeInAndOut : Trap
{
    public float waittingTime;

    private Renderer renderer;
    private Color updateColor;

    private float delayFadeChange;  // �Ž� ��ȯ �Ϸ� �� �ݴ�� ��ȯ�ϱ���� ���ð�
    private float updateCountPerSecond; // 1�ʴ� �Ž� ������Ʈ Ƚ��
    private float updateIntervalTime;   // �Ž� ������Ʈ ���͹�

    private void Start()
    {
        this.renderer = GetComponent<Renderer>();

        this.delayFadeChange = ValueManager.TRAP_FADE_IN_AND_OUT_FADE_CHANGE_DELAY;
        
        this.updateCountPerSecond = ValueManager.TRAP_FADE_IN_AND_OUT_UPDATE_COUNT_PER_ONE_SECOND;
        this.updateIntervalTime = (float)1 / this.updateCountPerSecond;
    }

    public override void ActivateEvent(Player player = null)
    {
        if (!this.isActive)
        {
            StopCoroutine(ActiaveAfterWatting());
            StartCoroutine(ActiaveAfterWatting());

            this.isActive = true;
        }
    }

    public override void DeactivateEvent(Player player = null)
    {
        this.isActive = false;
    }

    IEnumerator ActiaveAfterWatting()
    {
        yield return new WaitForSeconds(this.waittingTime);
        
        StopCoroutine(FadeInAndOut());
        StartCoroutine(FadeInAndOut());
    }

    IEnumerator FadeInAndOut()
    {
        while (this.isActive)
        {
            // Fade Out
            for (int i = (int)this.updateCountPerSecond; i >= 0; i--)
            {
                UpdateMaterialAlphaValue(value: (float)(i / this.updateCountPerSecond));

                yield return new WaitForSeconds(this.updateIntervalTime);
            }
            this.GetComponent<Collider>().enabled = false;

            yield return new WaitForSeconds(this.delayFadeChange);

            // Fade In
            this.GetComponent<Collider>().enabled = true;
            for (int i = 0; i <= this.updateCountPerSecond; i++)
            {
                UpdateMaterialAlphaValue(value: (float)(i / this.updateCountPerSecond));

                yield return new WaitForSeconds(this.updateIntervalTime);
            }

            yield return new WaitForSeconds(this.delayFadeChange);
        }
    }

    private void UpdateMaterialAlphaValue(float value)
    {
        this.updateColor = renderer.material.color;
        this.updateColor.a = value;
        this.renderer.material.color = updateColor;
    }
}
