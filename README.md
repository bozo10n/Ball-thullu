# Ball-Thullu  
*A Survival Game Prototype With Cosmic Swarm Horror (Unity 3D)*  

> **This is a messy, mid, buggy proof of concept (POC)** for a larger commercial project being remade from scratch. But the boss mechanic is pretty interesting. Worth a peek.

---

## What is This?

This is a short open-source **proof-of-concept** for a survival-style boss fight featuring a **swarming AI "chakra" system** that orbits the player, executes multiple attack patterns, and ramps up intensity. It’s not polished, not optimized, and definitely not balanced but it *does* experiment with a pretty wild boss fight system that I wanted to throw out into the world.

There’s:
- A boss (central controller: `ChakraController.cs`)
- Simple third-person player movement with a dodge mechanic
- No win condition just **survive as long as possible**

---

## Core Mechanic: Swarming Chakra Boss

The boss is made up of floating swarm "balls" that:
- Orbit a formation center hovering over the player
- Maintain spacing and separation from each other
- Follow the player with smooth motion
- Randomly switch between **Single, Burst, Volley,** and **Spread** attack patterns
- Optionally enter **rage mode** to increase aggression and projectile strength
- Fire **homing** or **non-homing** projectiles with acceleration curves and predicted targeting

> TL;DR: They form a dynamic formation, break off to shoot you, and reform. 
---

## Key Files

- `ChakraController.cs`: The brains of the boss fight. Manages formation logic, attack patterns, projectile behavior, rage mode, and target prediction.
- `PlayerController.cs`: Basic third-person movement + dodge roll (just enough to test the fight).
- `SwarmAgent.cs`: A basic NavMesh agent script that allows individual agents to gather at the swarm-formation spot.
- `HomingProjectile.cs`: Logic for swarm balls that track the player after being shot.
- `DamageOnCollision.cs`: Simple damage logic for projectiles hitting the player.
- **Animations** and some placeholder **VFX/trail renderers** for the swarm balls.

---

## How to Play

1. Open the Unity project.
2. Load the test scene (usually called `SwarmBoss` or similar).
3. Press play.
4. Try not to get wrecked.

> There’s no victory screen. If you're not dead yet, you’re winning.

---

## Known Issues

- Boss logic is messy and overgrown (because I kept prototyping on top of it).
- Projectiles can sometimes derp out or miss their timing.
- Rage mode isn't always consistent across patterns.
- Swarm sometimes clips through terrain or misbehaves at high speed.
- Animation syncing between player and logic is meh at best.

---

## Why Open Source This?

This POC is being **rebuilt from the ground up** for a commercial version with an entirely new structure and style. But I thought this early iteration might help or inspire someone who's:

- Building AI that orbits/encircles a player
- Experimenting with attack pattern systems in Unity
- Looking into swarm movement mechanics with separation logic
- Just curious how a weird boss fight can come together

---

## 📜 License

This project uses the MIT license.
