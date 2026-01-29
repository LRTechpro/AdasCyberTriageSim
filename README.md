# ADAS Cyber Triage Simulator

**ADAS Cyber Triage Simulator** is a C# WinForms arcade-style lane-runner game designed to teach **automotive cybersecurity concepts** through interactive gameplay.

The project blends real-world vehicle security controls (OTA validation, CAN segmentation, UDS session protection) with fast-paced arcade mechanics to simulate **cyber triage under time pressure**.

This project serves both as a **learning tool** and a **portfolio demonstration** of applied software engineering, real-time simulation, and automotive cybersecurity domain knowledge.

---

## Gameplay Overview

The player controls a vehicle at the bottom of a **three-lane roadway** and must survive a **75-second cyber incident scenario**.

During the run:

### Security Controls (Gates)
- Scroll down the lanes
- Increase score and streak
- Grant a **temporary security token** (6 seconds)

### Threats
- Represent specific automotive cyber attacks
- Cause damage unless the **correct token** is active
- Reward skillful timing and control matching

### Spinners (CAN Flood Attacks)
- Fleet-wide attacks
- Always cause damage
- Force evasive movement and posture management

---

## Difficulty Scaling

- Object scroll speed increases every **3 seconds**
- Threat damage increases at higher speeds
- Spawn patterns become more aggressive late-game

---

## Automotive Cybersecurity Concepts Modeled

Each security token maps directly to a real automotive cybersecurity control:

| Token | Protects Against |
|------|------------------|
| **Validate OTA Signature** | OTA downgrade / hash mismatch attacks |
| **Segment CAN Gateway** | ECU lateral movement / gateway pivoting |
| **Lock UDS Session** | Diagnostic brute force (UDS 0x27 seed/key) |
| **Rotate ECU Keys** | Key reuse and replay attacks |

Threat objects explicitly reference these attack types to reinforce **cause-and-effect learning**.

---

## Win / Lose Conditions

- **Win**: Survive the full 75-second incident window  
- **Lose**: Posture (vehicle security integrity) reaches zero  

At the end of each run, the player receives a **grade (S–D)** based on:
- Total score
- Remaining posture
- Streak performance

---

## Controls

- **Mouse drag** inside the game panel  
  - Vehicle smoothly follows horizontal mouse movement  
  - Movement is constrained to lane boundaries  

---

## Technical Highlights

- **Language / Framework**: C# (.NET) WinForms
- **Rendering**: GDI+ with anti-aliasing and rounded geometry
- **Game Loop**: Timer-driven (16ms ticks ≈ 60 FPS)
- **Collision System**: Rectangle-based detection with conditional logic
- **Difficulty Scaling**: Time-based speed and damage ramp
- **Domain Modeling**: Realistic automotive security abstractions

Core gameplay logic is implemented in `FormMain.cs`.

---

## Running the Project

### Requirements
- Windows
- Visual Studio
- .NET version specified by the project

### Steps
1. Clone the repository
2. Open `AdasCyberTriageSim.sln` in Visual Studio
3. Build and Run

---

## Assets

The project includes a background image:


Ensure the image file properties are set to:
- **Build Action**: Content
- **Copy to Output Directory**: Copy if newer

---

## Intended Use

This project is intended for:
- Software engineering portfolios
- Automotive cybersecurity demonstrations
- Game-based learning experiments
- Interviews and technical walkthroughs

This is **not** a production game. It is a focused demonstration of applied engineering and security reasoning.

---

## Future Enhancements

- Keyboard control support
- Persistent high-score tracking
- Additional attack types (DoIP, SOME/IP spoofing, ECU reflashing)
- Difficulty selection modes
- Audio and music toggle
- Post-run analytics dashboard

---

## License

MIT License  
© 2026 Harold L. R. Watkins


