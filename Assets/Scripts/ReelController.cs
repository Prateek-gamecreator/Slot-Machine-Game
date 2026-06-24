using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ReelController - Controls a single spinning reel.
/// Uses RNG to pick a final symbol, then animates the reel
/// scrolling downward and snapping to the result.
/// </summary>
public class ReelController : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Inspector References
    // ─────────────────────────────────────────

    [Header("Symbol Sprites")]
    [Tooltip("Assign sprites in order: 0=Seven, 1=Cherry, 2=Bell, 3=Bar")]
    [SerializeField] private Sprite[] symbolSprites;

    [Header("Reel Images")]
    [Tooltip("3 Image components stacked vertically inside this reel")]
    [SerializeField] private Image topSlot;
    [SerializeField] private Image middleSlot;    // The 'result' row
    [SerializeField] private Image bottomSlot;

    [Header("Reel Container")]
    [SerializeField] private RectTransform reelStrip;   // The scrolling container

    [Header("Spin Settings")]
    [SerializeField] private float spinSpeed         = 1800f;  // Pixels per second
    [SerializeField] private float symbolHeight      = 120f;   // Height of one symbol cell
    [SerializeField] private float decelerationTime  = 0.4f;   // Seconds to slow down

    // ─────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────

    private int    currentSymbolID = 0;        // Final landed symbol
    private bool   spinning        = false;
    private float  currentSpeed    = 0f;
    private float  scrollOffset    = 0f;       // How far the strip has scrolled

    // Weight table for RNG — Seven is rare, Cherry is common
    // Index matches symbolSprites array: 0=Seven, 1=Cherry, 2=Bell, 3=Bar
    private readonly int[] symbolWeights = { 1, 5, 3, 4 };   // out of 13 total

    // ─────────────────────────────────────────
    //  Public API (called by SlotMachineManager)
    // ─────────────────────────────────────────
    void Start()
    {
        // Initial symbol set karo taki white na dikhey
        int startID = Random.Range(0, symbolSprites.Length);
        topSlot.sprite = symbolSprites[GetWrappedIndex(startID - 1)];
        middleSlot.sprite = symbolSprites[startID];
        bottomSlot.sprite = symbolSprites[GetWrappedIndex(startID + 1)];
        currentSymbolID = startID;
    }
    /// <summary>Begin continuous scrolling animation.</summary>
    public void StartSpin()
    {
        spinning      = true;
        currentSpeed  = spinSpeed;
        StartCoroutine(SpinLoop());
    }

    /// <summary>
    /// Pick a random final symbol and animate the reel snapping to it.
    /// </summary>
    public void StopSpin()
    {
        currentSymbolID = PickWeightedSymbol();
        StartCoroutine(DecelerateAndSnap());
    }

    /// <summary>Returns the symbol ID currently showing in the middle slot.</summary>
    public int GetCurrentSymbolID()
    {
        return currentSymbolID;
    }

    // ─────────────────────────────────────────
    //  Spin Animation
    // ─────────────────────────────────────────

    /// <summary>Continuously scrolls reel downward while spinning == true.</summary>
    private IEnumerator SpinLoop()
    {
        while (spinning)
        {
            // Move strip downward
            scrollOffset += currentSpeed * Time.deltaTime;

            // Wrap around when one full symbol height is passed
            if (scrollOffset >= symbolHeight)
            {
                scrollOffset -= symbolHeight;
                CycleSymbols();
            }

            ApplyScroll(scrollOffset);
            yield return null;
        }
    }

    /// <summary>Gradually reduces speed and snaps to final symbol.</summary>
    private IEnumerator DecelerateAndSnap()
    {
        spinning = false;

        float elapsed      = 0f;
        float startSpeed   = currentSpeed;

        // Decelerate smoothly
        while (elapsed < decelerationTime)
        {
            elapsed      += Time.deltaTime;
            currentSpeed  = Mathf.Lerp(startSpeed, 0f, elapsed / decelerationTime);
            scrollOffset += currentSpeed * Time.deltaTime;

            if (scrollOffset >= symbolHeight)
            {
                scrollOffset -= symbolHeight;
                CycleSymbols();
            }

            ApplyScroll(scrollOffset);
            yield return null;
        }

        // Snap: set middle slot to chosen symbol, clear offset
        middleSlot.sprite = symbolSprites[currentSymbolID];
        topSlot.sprite    = symbolSprites[GetWrappedIndex(currentSymbolID - 1)];
        bottomSlot.sprite = symbolSprites[GetWrappedIndex(currentSymbolID + 1)];

        // Animate snap to zero with a short bounce
        yield return StartCoroutine(SnapToZero());
    }

    /// <summary>Tweens the remaining scroll offset back to zero (bounce feel).</summary>
    private IEnumerator SnapToZero()
    {
        float snapTime    = 0.15f;
        float elapsed     = 0f;
        float startOffset = scrollOffset;

        while (elapsed < snapTime)
        {
            elapsed      += Time.deltaTime;
            scrollOffset  = Mathf.Lerp(startOffset, 0f, elapsed / snapTime);
            ApplyScroll(scrollOffset);
            yield return null;
        }

        ApplyScroll(0f);
        scrollOffset = 0f;
        currentSpeed  = 0f;
    }

    // ─────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────

    /// <summary>Move reelStrip by offsetY to simulate scrolling.</summary>
    private void ApplyScroll(float offsetY)
    {
        Vector2 pos = reelStrip.anchoredPosition;
        pos.y = offsetY;
        reelStrip.anchoredPosition = pos;
    }

    /// <summary>
    /// Shift symbol images one step — top becomes middle, etc.
    /// A new random symbol appears at top to fill the gap.
    /// </summary>
    private void CycleSymbols()
    {
        bottomSlot.sprite = middleSlot.sprite;
        middleSlot.sprite = topSlot.sprite;
        // Assign a random sprite to the new top (not final result yet)
        topSlot.sprite    = symbolSprites[Random.Range(0, symbolSprites.Length)];
    }

    /// <summary>
    /// Weighted random symbol selection.
    /// Returns index into symbolSprites[] array.
    /// </summary>
    private int PickWeightedSymbol()
    {
        int total  = 0;
        foreach (int w in symbolWeights) total += w;

        int roll   = Random.Range(0, total);
        int cumulative = 0;

        for (int i = 0; i < symbolWeights.Length; i++)
        {
            cumulative += symbolWeights[i];
            if (roll < cumulative)
                return i;
        }
        return 0;
    }

    /// <summary>Wraps an index to stay within 0..symbolSprites.Length-1</summary>
    private int GetWrappedIndex(int index)
    {
        int len = symbolSprites.Length;
        return ((index % len) + len) % len;
    }
}
