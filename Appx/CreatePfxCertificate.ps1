param (
    [Parameter(Mandatory = $true, HelpMessage = "Enter the DNS name for the certificate")]
    [string]$CertName,
    
    [Parameter(Mandatory = $true, HelpMessage = "Enter the file path where the .pfx file will be saved (e.g., C:\path\to\your\certificate.pfx)")]
    [string]$PfxFilePath,
    
    [Parameter(Mandatory = $true, HelpMessage = "Enter the password to protect the .pfx file")]
    [string]$PfxPassword,

    [Parameter(Mandatory = $true, HelpMessage = "Enter the number of years the certificate will be valid")]
    [int]$YearsValid
)

# 将 PfxPassword 转换为安全字符串
$SecurePfxPassword = ConvertTo-SecureString -String $PfxPassword -Force -AsPlainText

# 设置证书的存储位置
$CertStoreLocation = "Cert:\LocalMachine\My"

$validDate = (Get-Date).AddYears($YearsValid)

# 创建自签名证书
$Cert = New-SelfSignedCertificate -Type Custom -DnsName $CertName -KeyUsage DigitalSignature -CertStoreLocation $CertStoreLocation -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}") -NotAfter $validDate -KeyExportPolicy Exportable -FriendlyName $CertName

# 检查证书是否创建成功
if ($null -eq $Cert) {
    Write-Error "Failed to create certificate."
    exit
}

# 导出证书到 .pfx 文件
Export-PfxCertificate `
    -Cert $Cert `
    -FilePath $PfxFilePath `
    -Password $SecurePfxPassword

# 检查 .pfx 文件是否导出成功
if (Test-Path $PfxFilePath) {
    Write-Host "Certificate exported to $PfxFilePath successfully."
} else {
    Write-Error "Failed to export certificate."
}
