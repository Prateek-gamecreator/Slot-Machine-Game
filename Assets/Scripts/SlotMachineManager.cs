using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SlotMachineManager - Main controller for the slot machine game.
/// Handles game state, betting, spinning, and win detection.
/// </summary>
public class SlotMachineManager : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Inspector References
    // ─────────────────────────────────────────

    [Header("Reel Controllers")]
    [SerializeField] private ReelController reel1;
    [SerializeField] private ReelController reel2;
    [SerializeField] private ReelController reel3;

    [Header("UI Elements")]
    [SerializeField] private Animator spinAnimation;
    [SerializeField] private TextMeshProUGUI goldText;         // Shows current gold
    [SerializeField] private TextMeshProUGUI resultText;       // Win / Lose message
    [SerializeField] private GameObject betMenuPanel;          // Bet 10G / 50G / 100G popup
    [SerializeField] private GameObject winPopupPanel;         // Win celebration popup
    [SerializeField] private GameObject jackpotPanel;          // Jackpot special popup
    [SerializeField] private TextMeshProUGUI winAmountText;    // Shows how much player won

    [Header("Buttons")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button bet10Button;
    [SerializeField] private Button bet50Button;
    [SerializeField] private Button bet100Button;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button quitGameButton;                 // Confirm exit
    [SerializeField] private Button exitBetButton;                  // Cancel exit

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spinSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip jackpotSound;
    [SerializeField] private AudioClip loseSound;
    [SerializeField] private AudioClip buttonClickSound;

    // ─────────────────────────────────────────
    //  Game State
    // ─────────────────────────────────────────

    private int currentGold = 50;       // Starting gold (matches screenshot: 50)
    private int currentBet  = 0;
    private bool isSpinning = false;

    // ─────────────────────────────────────────
    //  Payout Table
    //  Symbol IDs: 0=Seven, 1=Cherry, 2=Bell, 3=Bar
    //  Three of a kind → payout multiplier
    // ─────────────────────────────────────────
    private readonly Dictionary<int, int> payoutTable = new Dictionary<int, int>
    {
        { 0, 50 },   // Three Sevens  → 50× bet (JACKPOT)
        { 1, 5  },   // Three Cherries → 5× bet
        { 2, 10 },   // Three Bells    → 10× bet
        { 3, 3  },   // Three Bars     → 3× bet
    };

    // ─────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────

    private void Start()
    {
        UpdateGoldUI();
        SetupButtonListeners();
        HideAllPopups();

        // Show bet menu immediately (matches the screenshot design)
        ShowBetMenu();
    }

    // ─────────────────────────────────────────
    //  Button Setup
    // ─────────────────────────────────────────

    private void SetupButtonListeners()
    {
        bet10Button.onClick.AddListener(() => PlaceBet(10));
        bet50Button.onClick.AddListener(() => PlaceBet(50));
        bet100Button.onClick.AddListener(() => PlaceBet(100));
        playAgainButton.onClick.AddListener(ShowExitConfirm);
        spinButton.onClick.AddListener(OnSpinButtonClicked);
        quitGameButton.onClick.AddListener(ExitGame);
        exitBetButton.onClick.AddListener(HideAllPopups);
    }

    // ─────────────────────────────────────────
    //  Betting Logic
    // ─────────────────────────────────────────

    /// <summary>Called when player selects a bet amount from the menu.</summary>
    public void PlaceBet(int amount)
    {
        PlaySFX(buttonClickSound);

        if (currentGold < amount)
        {
            resultText.text = "Not enough gold!";
            return;
        }

        currentBet  = amount;
        currentGold -= amount;
        UpdateGoldUI();
        HideAllPopups();

        // Auto-spin after placing bet
        StartCoroutine(SpinReels());
    }

    // ─────────────────────────────────────────
    //  Spin Logic
    // ─────────────────────────────────────────

    private void OnSpinButtonClicked()
    {
        if (isSpinning) return;
        ShowBetMenu();    // Let player pick bet before spinning
    }

    private IEnumerator SpinReels()
    {
        if (isSpinning) yield break;
        isSpinning    = true;
        resultText.text = "";

        PlaySFX(spinSound);
        spinAnimation.SetTrigger("spin");

        // Start all three reels spinning
        reel1.StartSpin();
        reel2.StartSpin();
        reel3.StartSpin();

        // Stop reels with staggered delays for dramatic effect
        yield return new WaitForSeconds(1.0f);
        reel1.StopSpin();

        yield return new WaitForSeconds(0.4f);
        reel2.StopSpin();

        yield return new WaitForSeconds(0.4f);
        reel3.StopSpin();

        // Wait for last reel animation to settle
        yield return new WaitForSeconds(0.5f);

        // Evaluate result after all reels stopped
        EvaluateResult();
        isSpinning = false;
    }

    // ─────────────────────────────────────────
    //  Win Detection
    // ─────────────────────────────────────────

    private void EvaluateResult()
    {
        int s1 = reel1.GetCurrentSymbolID();
        int s2 = reel2.GetCurrentSymbolID();
        int s3 = reel3.GetCurrentSymbolID();

        bool isWin = (s1 == s2 && s2 == s3);

        if (isWin)
        {
            int multiplier = payoutTable.ContainsKey(s1) ? payoutTable[s1] : 2;
            int winAmount  = currentBet * multiplier;
            currentGold   += winAmount;
            UpdateGoldUI();

            // Jackpot: three Sevens
            if (s1 == 0)
            {
                ShowJackpot(winAmount);
            }
            else
            {
                ShowWinPopup(winAmount);
            }
        }
        else
        {
            PlaySFX(loseSound);
            resultText.text = "Try again!";

            // If player is broke, offer restart
            if (currentGold <= 0)
            {
                StartCoroutine(ShowGameOverAfterDelay());
            }
            else
            {
                ShowBetMenu();    // Let player bet again
            }
        }
    }

    // ─────────────────────────────────────────
    //  UI Helpers
    // ─────────────────────────────────────────

    private void UpdateGoldUI()
    {
        goldText.text = currentGold.ToString();
    }

    private void HideAllPopups()
    {
        betMenuPanel.SetActive(false);
        winPopupPanel.SetActive(false);
        jackpotPanel.SetActive(false);
    }

    private void ShowBetMenu()
    {
        HideAllPopups();
        betMenuPanel.SetActive(true);
    }

    private void ShowWinPopup(int amount)
    {
        PlaySFX(winSound);
        winAmountText.text = $"You won {amount}G!";
        winPopupPanel.SetActive(true);
        resultText.text = "Winner!";
    }

    private void ShowJackpot(int amount)
    {
        PlaySFX(jackpotSound);
        winAmountText.text = $"JACKPOT!\n+{amount}G";
        jackpotPanel.SetActive(true);
        resultText.text = "JACKPOT!";
    }

    private void ShowExitConfirm()
    {
        PlaySFX(buttonClickSound);
        HideAllPopups();
        // Reuse bet menu panel for exit confirm (set text dynamically)
        betMenuPanel.SetActive(true);
    }

    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        resultText.text = "Game Over! No gold left.";
        // Optionally: reload scene or reset gold
        currentGold = 50;
        UpdateGoldUI();
        ShowBetMenu();
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ─────────────────────────────────────────
    //  Audio Helper
    // ─────────────────────────────────────────

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
