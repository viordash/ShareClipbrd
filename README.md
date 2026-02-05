# ShareClipbrd

[![.NET](https://github.com/viordash/ShareClipbrd/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/viordash/ShareClipbrd/actions/workflows/dotnet.yml)
[![Russian](https://img.shields.io/badge/lang-Russian-blue.svg)](README.ru.md)

ShareClipbrd is a cross-platform (Windows, Linux) utility for controlled clipboard sharing between computers in a local network.

![Main app window](https://github.com/viordash/ShareClipbrd/blob/main/assets/Open%20settings.png)

The project was designed as a secure and functional alternative to the automatic clipboard synchronization commonly found in virtualization software (VMware, VirtualBox, Hyper-V) and RDP sessions.

Unlike automatic syncing, which creates a significant risk of accidentally leaking sensitive data (such as passwords, private keys, or personal messages) between the host and guest environments, ShareClipbrd transmits data only upon explicit user action. Additionally, it provides a clipboard sharing solution for isolated systems or environments where standard "Guest Additions/Tools" are not installed or cannot be used.

## Key Features

*   **Cross-platform:** Supports Windows and Linux (X11/Wayland) based on .NET 10 and Avalonia UI.
*   **Data types:** Transfer text, file lists/directories, and images.
*   **Multi-profile:** 3 pre-configured profiles for quick switching between different recipients.
*   **Zero-Configuration:** Automatic device discovery (mDNS), no IP addresses required.
*   **Privacy:** Direct TCP/IP transfer within the local network without using external servers.

## Installation and Usage

The utility is portable (no installation required).

1.  Download the latest release and extract the archive.
2.  Run the executable:
    *   Windows: `ShareClipbrdApp.exe`
    *   Linux: `ShareClipbrdApp`
3.  Applications on different computers will find each other automatically (with default settings).
4.  To transfer data, copy it to the clipboard, switch to the program window, and click the send button (or press Ctrl+V).

## Connection Setup

Settings are accessed via the context menu of the program window -> Settings.

**Profiles (1, 2, 3):**
The program allows storing **3 independent configuration sets**. You can configure different connection parameters for three different computers (e.g., on slots 1, 2, and 3) and switch between them with a single click in the main window.

### 1. Automatic Mode (Recommended)
Works "out of the box" for most networks. Applications discover each other via mDNS.
*   **Host address:** (leave empty)
*   **Partner address:** (leave empty)

### 2. Automatic with ID Separation
Used if there are multiple pairs of users in the network and you need to avoid overlapping.
*   **Host address:** `mdns:MY_UNIQUE_ID:PORT` (e.g., `mdns:GroupA:61001`)
*   **Partner address:** (leave empty)

### 3. Direct IP Connection
For complex networks or VPNs where mDNS multicast requests do not pass through.
*   **Host address:** `IP:PORT` (local address, e.g., `192.168.1.10:61001` or `:61001` for all interfaces).
*   **Partner address:** `IP:PORT` (remote computer address, e.g., `192.168.1.20:61001`).

**Note:** Ensure that inbound connections for the selected port are allowed in your Firewall settings.

## Technical Details

*   **Framework:** .NET 10, Avalonia UI 11.
*   **Protocols:** TCP/IP for data, mDNS for service discovery.

## License
MIT License.
