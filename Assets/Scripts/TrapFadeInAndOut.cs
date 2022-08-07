using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapFadeInAndOut : Trap
{
    public float waittingTime;

    bool isActive;

    Renderer renderer;
    Color updateColor;

    float updateCount = 10.0f;
    float delayFadeChange = 2.0f, durationFade = 1.0f;
    float updateTime;

    private void Start()
    {
        renderer = GetComponent<Renderer>();

        updateTime = (float)1 / (updateCount * durationFade);
    }

    public override void ActivateEvent(Player player = null)
    {
        StopCoroutine(ActiaveAfterWatting());
        StartCoroutine(ActiaveAfterWatting());
    }

    public override void DeactivateEvent(Player player = null)
    {
        isActive = false;
    }

    IEnumerator ActiaveAfterWatting()
    {
        yield return new WaitForSeconds(waittingTime);
        
        isActive = true;
        StopCoroutine(FadeInAndOut());
        StartCoroutine(FadeInAndOut());
    }

    IEnumerator FadeInAndOut()
    {
        while (isActive)
        {
            // Fade Out
            for (int i = (int)updateCount; i >= 0; i--)
            {
                UpdateMaterialAlphaValue(value: (float)(i / updateCount));

                yield return new WaitForSeconds(updateTime);
            }
            this.GetComponent<Collider>().enabled = false;

            yield return new WaitForSeconds(delayFadeChange);

            // Fade In
            this.GetComponent<Collider>().enabled = true;
            for (int i = 0; i <= updateCount; i++)
            {
                UpdateMaterialAlphaValue(value: (float)(i / updateCount));

                yield return new WaitForSeconds(updateTime);
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
