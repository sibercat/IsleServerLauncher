# 🦖 The Isle: Evrima Server Launcher

A modern, feature-rich dedicated server manager for **The Isle: Evrima**, built with **C# WPF** and **.NET 10**. This launcher simplifies the complex process of setting up, configuring, and maintaining an Evrima server by providing a unified graphical interface for all administrative tasks.

![alt text](https://raw.githubusercontent.com/sibercat/IsleServerLauncher/refs/heads/main/docs/Dashboard.webp)

## ✨ Key Features
Executable can be found in [Releases](https://github.com/sibercat/IsleServerLauncher/releases)
### 🚀 Server Management

* **Automated Install/Update:** Built-in SteamCMD integration to install, update, and validate server files.
* **Crash Detection:** Automatically detects server crashes and restarts the process with configurable attempt limits.
* **Performance Tuning:** CPU Core Affinity/Priority management and built-in network optimization (VirtIO/Packet drop fixes).
* **Environment Fixes:** One-click fixes for common issues like SSL Certificate errors (Epic Online Services) and Firewall configuration.



### ⚙️ Easy Configuration

* **GUI Settings Editor:** Modify gameplay rules without touching `Game.ini` or `Engine.ini`.
* **Gameplay Control:** Adjust Day/Night cycles, AI Density, Growth/Decay rates, Migration zones, and more.
* **Dino Whitelist:** Easy-to-use checklist to enable/disable specific playable dinosaurs.



### 🛠️ Administrator & RCON Tools

* **Player Management:** Live player list with Kick, Ban, and Direct Message functionality.
* **Broadcast System:** Send global announcements to all players.
* **Live Controls:** Toggle Global Chat, AI, Humans, or Whitelist on the fly.
* **Chat Monitor:** Real-time server chat log viewer with colored tagging (Global/Local/Admin).



### 🤖 Automation & Integrations

* **Discord Webhooks:** Get notifications for server status (Start/Stop/Crash).
* **Chat Relay:** Forward in-game chat directly to a Discord channel.
* **Scheduled Restarts:** Set automated restart intervals with in-game warning countdowns.
* **Auto-Backups:** Schedule automated backups of your server's save data.



## 🖼️ UI/UX

* **Modern Interface:** Clean WPF design with Light and Dark mode support.
* **Toast Notifications:** Non-intrusive status updates for actions.



## 🔧 Built With

* **Language:** C#
* **Framework:** WPF (.NET 10)
* **Styling:** XAML with Custom Control Templates
* **Services:** SteamCMD, RCON

## 📦 Installation

1. Download the latest [Release](https://github.com/sibercat/IsleServerLauncher/releases).
2. Run `IsleServerLauncher.exe` as Administrator (required for network/firewall configuration).
3. Click **"Install Server"** on the dashboard to download the necessary files via SteamCMD.
4. Configure your settings in the **Server Settings** tab.
5. Click **Start Server**.

## ⚠️ Requirements

* Windows 10/11 or Windows Server 2019+
* Visual C++ Redistributable

---

*This project is not affiliated with Afterthought LLC or The Isle development team.*
