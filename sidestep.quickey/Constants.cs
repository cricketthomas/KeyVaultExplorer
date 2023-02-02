﻿using Microsoft.Identity.Client.Extensions.Msal;

namespace sidestep.quickey;

public static class Constants
{
    //The Application or Client ID will be generated while registering the app in the Azure portal. Copy and paste the GUID.
    public static readonly string ClientId = "7c09c1d9-3585-403c-834a-53452958e76f";

    //Leaving the scope to its default values.
    public static readonly string[] Scopes = new string[] { "openid", "offline_access" };


    // Cache settings
    public const string CacheFileName = "quickey_msal_cache.txt";
    public readonly static string CacheDir = MsalCacheHelper.UserRootDirectory;

    public const string KeyChainServiceName = "quickey_msal_service";
    public const string KeyChainAccountName = "quickey_msal_account";

    public const string LinuxKeyRingSchema = "com.contoso.devtools.tokencache";
    public const string LinuxKeyRingCollection = MsalCacheHelper.LinuxKeyRingDefaultCollection;
    public const string LinuxKeyRingLabel = "MSAL token cache for all Contoso dev tool apps.";
    public static readonly KeyValuePair<string, string> LinuxKeyRingAttr1 = new KeyValuePair<string, string>("Version", "1");
    public static readonly KeyValuePair<string, string> LinuxKeyRingAttr2 = new KeyValuePair<string, string>("ProductGroup", "MyApps");

}