# Protecting Legacy XML sections

Specify the section and provider you wish to use to encrypt/decrypt. The default provider of `DataProtectionConfigurationProvider` is 
typically used which uses the DPAPIProtectedConfigurationProvider (Windows Data Protection API) however `RsaProtectedConfigurationProvider` 
is also valid.

The encryption keys used for DPAPI are based on the user's password.

## Encrypting a specific section

```powershell
C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis.exe -pef "connectionStrings" "B:\gitrepo\personalcode\AnyConfig\AnyConfig\AnyConfig.Tests" -prov "DataProtectionConfigurationProvider"
```

## Decrypting a specific section

```powershell
C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis.exe -pdf "connectionStrings" "B:\gitrepo\personalcode\AnyConfig\AnyConfig\AnyConfig.Tests" -prov "DataProtectionConfigurationProvider"
```

Documentation [via](https://docs.microsoft.com/en-us/aspnet/web-forms/overview/data-access/advanced-data-access-scenarios/protecting-connection-strings-and-other-configuration-information-cs#step-3-encrypting-configuration-sections-usingaspnet_regiisexe)

More details on custom providers [via](https://techcommunity.microsoft.com/t5/iis-support-blog/connection-string-encryption-and-decryption/ba-p/830094#)
