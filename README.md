# Modern Standby Fix

Inspired by [**LTT Video about Modern Standby issue**](https://www.youtube.com/watch?v=OHKKcd3sx2c).

[Modern Standby](https://learn.microsoft.com/en-us/windows-hardware/design/device-experiences/modern-standby) is a mode your PC enters when it goes into a sleep. While it brings a number of benefits as it tries to make your PC behave more similar to smartphones/tables, the number of users are reporting heavy battery drain, which renders this feature unusable.

As concluded in the video listed above, this probably happens because of a network connection being kept enabled if you enter a sleep mode while your PC is plugged into the power adapter. If you unplug it later, while it is already in the sleep, it will not disable network adapters. This might cause your PC to still communicate on the network (download updates, etc), which will cause unwanted battery drain.


This utility will detect when PC goes into sleep and will automatically disable all active network connections regardless if your PC is connected to a power adapter or not.

**Beware, this might not be the expected behavior for your use case!**

Upon returning from a sleep, it will enable all network adapters that were disabled back on. You can make this app to start on a system startup so it will be always running and monitoring going into sleep and out of it. As it requires an admin elevation to receive notifications about sleep modes, you will be prompted with UCI dialog when the app is started (or on the system startup, if you set it to start automatically).

## Requires [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

# [**Download the latest release here**](https://github.com/martinchrzan/ModernStandbyFix/releases/latest)
