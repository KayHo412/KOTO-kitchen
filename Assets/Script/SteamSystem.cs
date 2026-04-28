using UnityEngine;
using System.Collections;

public class SteamSystem : MonoBehaviour
{
    public ParticleSystem steamEffect;
    public PotManager potManager;
    public bool stoveIsOn = false;
    public bool potIsOnStove = false;

    private Coroutine steamTimer;

    public void ToggleStove()
    {
        stoveIsOn = !stoveIsOn;
        CheckConditions();
    }

    public void UpdatePotStatus(bool placed)
    {
        potIsOnStove = placed;
        CheckConditions();
    }

    // Call this from PotManager.SetWaterStatus to trigger the boil if stove was already on
    public void NotifyWaterAdded() => CheckConditions();

    private void CheckConditions()
    {
        // Must have: Stove ON + Pot PLACED + Pot HAS WATER
        if (stoveIsOn && potIsOnStove && potManager != null && potManager.GetWaterStatus())
        {
            if (steamTimer == null)
            {
                steamTimer = StartCoroutine(StartBoiling());
            }
        }
        else
        {
            StopSteam();
            if (potManager != null) potManager.SetWaterReady(false);
        }
    }

    IEnumerator StartBoiling()
    {
        yield return new WaitForSeconds(4f);
        if (steamEffect != null) steamEffect.Play();
        if (potManager != null) potManager.SetWaterReady(true);
        if (potManager != null && potManager.hud != null) potManager.hud.ShowOrderTicket();

        TutorialController tutorial = Object.FindFirstObjectByType<TutorialController>();
        if (tutorial != null) tutorial.ProceedNextStep(this.gameObject);
    }

    private void StopSteam()
    {
        if (steamTimer != null)
        {
            StopCoroutine(steamTimer);
            steamTimer = null;
        }
        if (steamEffect != null) steamEffect.Stop();
    }
}