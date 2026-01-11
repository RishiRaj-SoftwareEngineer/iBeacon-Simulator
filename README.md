# iBeacon Simulator

A Windows application to broadcast iBeacon signals for testing and development.

## Features

- Configure UUID, Major, and Minor values
- Easy start/stop broadcasting
- Standalone EXE - no installation required
- Simple and intuitive interface

## Download

Download the latest version from [Releases](https://github.com/sagarchaulagai/ibeacon-simulator-windows/releases)

##  Usage

1. Download `IBeaconSimulator.exe`
2. Run the application
3. Configure your beacon parameters:
   - **UUID:** Unique identifier for your beacon
   - **Major:** Major value (0-65535)
   - **Minor:** Minor value (0-65535)
4. Click "Start Beacon" to begin broadcasting
5. Click "Stop Beacon" to stop

## Requirements

- Windows 10 version 1809 or later
- Bluetooth support

##  Development

Built with:
- WinUI 3
- .NET 8
- Windows App SDK

### Building from Source
```bash
git clone https://github.com/sagarchaulagai/ibeacon-simulator-windows.git
cd iBeacon_Simulator
dotnet publish -c Release -r win-x64 --self-contained true
