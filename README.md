# Scratch-Remix 🚀

> A lightweight, performance-focused reimagining of Scratch —  
> built with Unity, designed for clarity, consistency, and future growth.

**Scratch-Remix** is an experimental project aiming to re-create a visual programming editor inspired by Scratch, while addressing some of its long-standing limitations.  
It’s not a full replacement (yet), but rather a playground to explore **frame-rate independent logic**, **modern UI design**, and **better extensibility**.

---

## ✨ Motivation

- **🧊 Frame-locked movement?** Original Scratch ties logic to frames, leading to inconsistent behavior.  
  → Scratch-Remix uses Unity’s `Time.deltaTime` to keep logic smooth and predictable across devices.

- **💔 Platform inconsistency?** Scratch projects sometimes feel different depending on the editor or hardware.  
  → Scratch-Remix aims for consistent cross-platform behavior (desktop & mobile).

- **🎨 Basic visuals?** Scratch’s UI can feel dated and rigid.  
  → Scratch-Remix experiments with a **modern macOS-inspired UI**, including a transparent auto-hiding toolbar and custom rounded-rectangle shaders.

---

## 🚀 Current Progress

### ✅ Implemented (Prototype Stage)
- Unity-based 2D rendering pipeline.  
- Transparent, auto-hiding toolbar (macOS-style).  
- Basic adaptive quality system (toggle high/medium/low).  
- Custom rounded-rectangle shader for UI elements.  

### 🛠️ Work in Progress
- Visual scripting node system (early experiments).  
- DeltaTime-based motion nodes.  

### 🔮 Planned (Not Yet Implemented)
- Node connection & data flow system.  
- Debugger with breakpoints & step execution.  
- Extensible node API for C# scripts.  
- Community project sharing.  

---

## 🛠️ Getting Started (For Developers)

### Requirements
- Unity **6000.1.12f2** or later.

### Setup
```bash
git clone https://github.com/onduclair-star/Scratch-Remix
````

Open the project in Unity, load the sample scene, and hit **Play**.

---

## ⚠️ Project Status

This project is in a **very early prototype stage**.
Features may be incomplete, unstable, or subject to major changes. Contributions and feedback are welcome!

---
