# Build StadiaLink


### Requirements

- Windows 11 x64 and the included Windows PowerShell 5.1.
- [Git for Windows](https://git-scm.com/downloads/win), or GitHub's source ZIP.
- [Visual Studio 2022 or Build Tools 2022](https://visualstudio.microsoft.com/downloads/) with **Desktop development with C++**, MSVC x64 and Windows SDK **10.0.26100.0**.
- .NET Framework **4.8**. Scripts use `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe` and Windows WPF libraries. Node.js and `dotnet build` are not required.
- Internet for the first driver build. The script downloads `Microsoft.Windows.WDK.x64` **10.0.26100.6584** from NuGet, uses UMDF **2.31** and caches the WDK in `.build/driver/.wdk`.

Open Windows PowerShell in a working folder. Building does not require administrator privileges; installing the driver does.

```powershell
git clone https://github.com/bernhardsf/StadiaLink.git
cd StadiaLink

# If scripts are blocked; applies only to this session.
Set-ExecutionPolicy -Scope Process -ExecutionPolicy RemoteSigned

# Driver, application, native library and installer.
.\app\package.ps1 -Driver

# Input translation and application model tests.
.\tests\run.ps1
```

If using the ZIP, extract it and open PowerShell in the folder containing `app`, `driver` and `tests`. Review scripts before unblocking them if Windows marks them as Internet downloads. The example does not change the machine's permanent execution policy.

| Path | Output |
| --- | --- |
| `release/StadiaLink.exe` | x64 WPF application. |
| `release/StadiaLink.exe.config` | .NET Framework configuration. |
| `release/StadiaDevice.dll` | Native battery reader. |
| `release/StadiaLink.Setup.exe` | Installer with components and embedded `Source.zip`. |
| `release/StadiaLink.Setup.exe.config` | Installer configuration. |
| `release/SHA256.txt` | Hash of your locally built installer. |
| `driver/out/pkg/` | Compiled UMDF driver package. |
| `.build/` | Dependencies, object files and test results. |

Run `release/StadiaLink.Setup.exe` to install your build. Running the application alone does not install the driver. For separate build steps:

```powershell
.\driver\build.ps1       # Driver only.
.\app\build.ps1          # Application and native library.
.\app\package.ps1        # Installer using the existing driver build.
.\tests\run.ps1          # Requires the built application.
```

If MSVC is missing, check Visual Studio's C++ workload. Missing headers or libraries usually require the specified SDK. Check NuGet access if the WDK download fails. Driver builds include a build date and version; binaries are not necessarily identical between builds.


[Back to README](../README.md)
