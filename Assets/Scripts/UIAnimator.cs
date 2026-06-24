using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UIAnimator - Handles all UI visual effects:
/// - Win popup scale-in animation
/// - Jackpot text flashing
/// - Gold counter count-up animation
/// - Button press feedback
/// </summary>
public class UIAnimator : MonoBehaviour
{
    [Header("Win Popup")]
    [SerializeField] private RectTransform jackpotPanel;
    [SerializeField] private float popupDuration = 0.3f;

    [Header("Jackpot Flash")]
    [SerializeField] private TextMeshProUGUI jackpotText;
    [SerializeField] private Color flashColor1 = Color.yellow;
    [SerializeField] private Color flashColor2 = Color.red;
    [SerializeField] private float flashSpeed  = 0.2f;

    [Header("Gold Counter")]
    [SerializeField] private TextMeshProUGUI goldDisplay;

    private Coroutine jackpotFlashCoroutine;

    // ─────────────────────────────────────────
    //  Popup Animation
    // ─────────────────────────────────────────

    /// <summary>Scale popup from zero to full size (punch-in effect).</summary>
    public void ShowPopup(GameObject panel)
    {
        panel.SetActive(true);
        RectTransform rt = panel.GetComponent<RectTransform>();
        StartCoroutine(ScaleIn(rt));
    }

    private IEnumerator ScaleIn(RectTransform rt)
    {
        float elapsed = 0f;
        rt.localScale = Vector3.zero;

        while (elapsed < popupDuration)
        {
            elapsed       += Time.deltaTime;
            float t        = elapsed / popupDuration;
            // Overshoot slightly for a "bouncy" feel
            float scale    = Mathf.LerpUnclamped(0f, 1f, EaseOutBack(t));
            rt.localScale  = Vector3.one * scale;
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    // Easing: shoots past 1 then settles back (classic Unity-feel)
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // ─────────────────────────────────────────
    //  Jackpot Flash
    // ─────────────────────────────────────────

    public void StartJackpotFlash()
    {
        if (jackpotFlashCoroutine != null)
            StopCoroutine(jackpotFlashCoroutine);
        jackpotFlashCoroutine = StartCoroutine(FlashJackpot());
    }

    public void StopJackpotFlash()
    {
        if (jackpotFlashCoroutine != null)
        {
            StopCoroutine(jackpotFlashCoroutine);
            jackpotFlashCoroutine = null;
        }
    }

    private IEnumerator FlashJackpot()
    {
        bool toggle = false;
        while (true)
        {
            jackpotText.color = toggle ? flashColor1 : flashColor2;
            toggle = !toggle;
            yield return new WaitForSeconds(flashSpeed);
        }
    }

    // ─────────────────────────────────────────
    //  Gold Count-Up
    // ─────────────────────────────────────────

    /// <summary>Animate gold display counting up from old to new value.</summary>
    public void AnimateGoldChange(int fromValue, int toValue, float duration = 0.8f)
    {
        StartCoroutine(CountUp(fromValue, toValue, duration));
    }

    private IEnumerator CountUp(int from, int to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed      += Time.deltaTime;
            float t       = elapsed / duration;
            int   current = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            goldDisplay.text = current.ToString();
            yield return null;
        }
        goldDisplay.text = to.ToString();
    }

    // ─────────────────────────────────────────
    //  Button Press Feedback
    // ─────────────────────────────────────────

    /// <summary>Squeeze a button briefly on click for tactile feel.</summary>
    public void PressButton(RectTransform btn)
    {
        StartCoroutine(SqueezeButton(btn));
    }

    private IEnumerator SqueezeButton(RectTransform btn)
    {
        float duration = 0.1f;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed      += Time.deltaTime;
            float t       = elapsed / duration;
            float scale   = Mathf.Lerp(1f, 0.9f, t);
            btn.localScale = Vector3.one * scale;
            yield return null;
        }

        // Bounce back
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed      += Time.deltaTime;
            float t       = elapsed / duration;
            float scale   = Mathf.Lerp(0.9f, 1f, t);
            btn.localScale = Vector3.one * scale;
            yield return null;
        }

        btn.localScale = Vector3.one;
    }
}
