using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;

namespace IsleServerLauncher.Services
{
    public class SystemSetupService
    {
        private readonly string _serverFolder;
        private readonly ILogger _logger;

        public SystemSetupService(string serverFolder, ILogger logger)
        {
            _serverFolder = serverFolder ?? throw new ArgumentNullException(nameof(serverFolder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.Info($"SystemSetupService initialized. Server folder: {_serverFolder}");
        }

        /// <summary>
        /// Checks if current process has administrator privileges
        /// </summary>
        public bool IsAdministrator()
        {
            try
            {
                bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                    .IsInRole(WindowsBuiltInRole.Administrator);

                _logger.Debug($"Administrator check: {isAdmin}");
                return isAdmin;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error checking administrator status: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Runs server setup: firewall, VC++ runtime, IE ESC disable
        /// </summary>
        public async Task RunServerSetupAsync()
        {
            if (!IsAdministrator())
            {
                _logger.Error("Server setup requires administrator privileges");
                throw new UnauthorizedAccessException("Administrator privileges required");
            }

            _logger.Info("Starting server setup");

            string scriptPath = Path.Combine(_serverFolder, "setup.ps1");
            string scriptContent = @"
Write-Host '>>> STARTING SERVER SETUP...' -ForegroundColor Cyan

Write-Host '1. Configuring Windows Firewall...' -ForegroundColor Yellow
New-NetFirewallRule -DisplayName 'The Isle - Game Port' -Direction Inbound -LocalPort 7777 -Protocol UDP -Action Allow -Force -ErrorAction SilentlyContinue
New-NetFirewallRule -DisplayName 'The Isle - RCON Port' -Direction Inbound -LocalPort 8888 -Protocol TCP -Action Allow -Force -ErrorAction SilentlyContinue

Write-Host '2. Installing Visual C++ Runtime...' -ForegroundColor Yellow
$vcUrl = 'https://aka.ms/vs/17/release/vc_redist.x64.exe'
$vcFile = '$env:TEMP\vc_redist.x64.exe'
Invoke-WebRequest -Uri $vcUrl -OutFile $vcFile -UseBasicParsing
Start-Process -FilePath $vcFile -ArgumentList '/install', '/passive', '/norestart' -Wait
Remove-Item $vcFile -ErrorAction SilentlyContinue

Write-Host '3. Tuning Windows Settings...' -ForegroundColor Yellow
function Disable-IEESC {
    $AdminKey = 'HKLM:\SOFTWARE\Microsoft\Active Setup\Installed Components\{A509B1A7-37EF-4b3f-8CFC-4F3A74704073}'
    $UserKey = 'HKLM:\SOFTWARE\Microsoft\Active Setup\Installed Components\{A509B1A8-37EF-4b3f-8CFC-4F3A74704073}'
    Set-ItemProperty -Path $AdminKey -Name 'IsInstalled' -Value 0 -ErrorAction SilentlyContinue
    Set-ItemProperty -Path $UserKey -Name 'IsInstalled' -Value 0 -ErrorAction SilentlyContinue
}
Disable-IEESC

Write-Host '>>> SETUP COMPLETE!' -ForegroundColor Cyan
Start-Sleep -Seconds 3";

            try
            {
                await RunPowerShellScriptAsync(scriptPath, scriptContent);
                _logger.Info("Server setup completed successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Server setup failed: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Applies network optimization for VPS/Cloud servers
        /// </summary>
        public async Task RunNetworkOptimizationAsync()
        {
            if (!IsAdministrator())
            {
                _logger.Error("Network optimization requires administrator privileges");
                throw new UnauthorizedAccessException("Administrator privileges required");
            }

            _logger.Info("Starting network optimization");

            string scriptPath = Path.Combine(_serverFolder, "network_fix.ps1");
            string scriptContent = @"
Write-Host '>>> APPLYING UE5 NETWORK OPTIMIZATION...' -ForegroundColor Cyan

Write-Host 'Optimizing Network Adapters...' -ForegroundColor Yellow
# Fixes 'Stuck Connecting' and silent packet drops on VirtIO/VPS adapters
Get-NetAdapter | Where-Object { $_.Status -eq 'Up' } | ForEach-Object {
    Write-Host ""Processing: $($_.Name)"" -ForegroundColor Cyan
    
    # Disable UDP Checksum Offload (critical for UE5 packet delivery)
    Disable-NetAdapterChecksumOffload -Name $_.Name -UdpIPv4 -Confirm:$false -ErrorAction SilentlyContinue
    Write-Host ""  ✓ Disabled UDP Checksum Offload"" -ForegroundColor Green
    
    # Disable RSC (fixes connection freeze on Windows Server)
    try {
        Disable-NetAdapterRsc -Name $_.Name -IPv4 -Confirm:$false -ErrorAction SilentlyContinue
        Write-Host ""  ✓ Disabled RSC"" -ForegroundColor Green
    } catch {
        Write-Host ""  - RSC not supported/already disabled"" -ForegroundColor Yellow
    }
    
    # Disable LSO (reduces fragmentation)
    Disable-NetAdapterLso -Name $_.Name -IPv4 -Confirm:$false -ErrorAction SilentlyContinue
    Write-Host ""  ✓ Disabled LSO"" -ForegroundColor Green
    
    Write-Host """" -ForegroundColor White
}

Write-Host '>>> OPTIMIZATION COMPLETE!' -ForegroundColor Cyan
Write-Host 'CRITICAL: You must RESTART your VPS for changes to take full effect.' -ForegroundColor Yellow
Start-Sleep -Seconds 5";

            try
            {
                await RunPowerShellScriptAsync(scriptPath, scriptContent);
                _logger.Info("Network optimization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Network optimization failed: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Installs Amazon Root CA 1 certificate to fix SSL errors
        /// </summary>
        public async Task FixSSLCertificateAsync()
        {
            if (!IsAdministrator())
            {
                _logger.Error("SSL certificate fix requires administrator privileges");
                throw new UnauthorizedAccessException("Administrator privileges required");
            }

            _logger.Info("Starting SSL certificate fix");

            string scriptPath = Path.Combine(_serverFolder, "fix_ssl.ps1");
            string scriptContent = @"
Write-Host '>>> FIXING SSL ERROR 60 (Missing Root CA)...' -ForegroundColor Cyan

Write-Host 'Downloading Amazon Root CA 1...' -ForegroundColor Cyan
$certUrl = 'https://www.amazontrust.com/repository/AmazonRootCA1.cer'
$certFile = ""$env:TEMP\AmazonRootCA1.cer""

try {
    # Download the cert
    Invoke-WebRequest -Uri $certUrl -OutFile $certFile
    
    # Import into Local Machine Trusted Root
    Import-Certificate -FilePath $certFile -CertStoreLocation Cert:\LocalMachine\Root
    
    Write-Host 'SUCCESS! Certificate Installed.' -ForegroundColor Green
    Write-Host 'You can now connect to Epic Online Services.' -ForegroundColor Yellow
}
catch {
    Write-Host ""Error installing certificate: $_"" -ForegroundColor Red
}

# Cleanup
Remove-Item $certFile -ErrorAction SilentlyContinue
Start-Sleep -Seconds 5";

            try
            {
                await RunPowerShellScriptAsync(scriptPath, scriptContent);
                _logger.Info("SSL certificate fix completed successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"SSL certificate fix failed: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Executes a PowerShell script as admin
        /// </summary>
        private async Task RunPowerShellScriptAsync(string scriptPath, string scriptContent)
        {
            try
            {
                if (!Directory.Exists(_serverFolder))
                {
                    Directory.CreateDirectory(_serverFolder);
                    _logger.Debug($"Created server folder: {_serverFolder}");
                }

                await File.WriteAllTextAsync(scriptPath, scriptContent);
                _logger.Debug($"PowerShell script written to: {scriptPath}");

                Process process = new Process();
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";

                _logger.Info($"Starting PowerShell script: {Path.GetFileName(scriptPath)}");
                process.Start();

                await process.WaitForExitAsync();
                _logger.Info($"PowerShell script completed with exit code: {process.ExitCode}");

                if (process.ExitCode != 0)
                {
                    _logger.Warning($"PowerShell script exited with non-zero code: {process.ExitCode}");
                }

                // Cleanup
                try
                {
                    File.Delete(scriptPath);
                    _logger.Debug($"Cleaned up script file: {scriptPath}");
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Failed to delete script file: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error running PowerShell script: {ex.Message}", ex);
                throw;
            }
        }
    }
}