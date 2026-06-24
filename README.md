# Slot-Machine-Game
Unity Slot Game Assignment 

A classic 3-reel slot machine game built in Unity as part of an internship assignment.

---

## 🕹️ How to Run

### WebGL (Browser)
1. Open the `/Build/WebGL` folder in this repo.
2. Start a local server (e.g., `python -m http.server 8080`).
3. Open `http://localhost:8080` in Chrome or Firefox.
4. Press **Spin** and select your bet to play!

### Unity Editor
1. Open the project in **Unity 2022.3 LTS** (or newer).
2. Open `Assets/Scenes/SlotGame.unity`.
3. Press **Play** in the editor.

---

## 🎮 Game Overview

| Feature | Detail |
|---------|--------|
| Starting Gold | 50G |
| Reels | 3 spinning reels |
| Symbols | Seven 🎯, Cherry 🍒, Bell 🔔, Bar |
| Win Condition | All 3 reels show the same symbol |
| Bet Options | 10G / 50G / 100G per spin |

### Payout Table
| Symbol | Multiplier |
|--------|-----------|
| 🎯 Seven (Jackpot) | 50× bet |
| 🔔 Bell | 10× bet |
| 🍒 Cherry | 5× bet |
| BAR | 3× bet |

---

## ✨ Bonus Features

- **Weighted RNG** — Sevens are rare (1 in 13 chance), making jackpots exciting and fair.
- **Staggered reel stops** — Reels stop one-by-one for dramatic tension.
- **Bounce-in popup animation** — Win popup uses EaseOutBack for a game-feel snap.
- **Gold count-up animation** — Gold display counts up to reward amount smoothly.
- **Jackpot flash effect** — Alternating colors on jackpot text for visual excitement.
- **Button press feedback** — Subtle scale squeeze on button clicks.
---

## 💭 Thought Process / Approach

### Architecture
I used a **Manager + Component** pattern:
- `SlotMachineManager` owns the game state (gold, bet, win/lose logic). It's the single source of truth.
- `ReelController` is self-contained — it only knows how to spin and report which symbol landed.
- `UIAnimator` is a utility — nothing in the game logic knows or cares about animations.

This separation makes each script testable independently and keeps the code clean.

### RNG Design
I used a **weighted random** system rather than pure uniform random. This gives control over how often each symbol appears:
- Seven: 1 weight (rare — ~7.7%)
- Bell: 3 weights (~23%)
- Bar: 4 weights (~31%)
- Cherry: 5 weights (~38%)

This mirrors real slot machines where payouts are inversely proportional to probability.

### Animation Approach
Rather than Unity's Animator component (which requires .anim files and state machines), I used **pure C# coroutines** for all animations. This keeps everything readable in code with no hidden state in the Animator window.

---

## 🛠️ Built With
- Unity 2022.3 LTS
- TextMeshPro
- Unity UI (uGUI)
- C# coroutines for animation

---

*Built with ❤️ for internship evaluation.*
