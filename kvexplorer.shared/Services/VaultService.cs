﻿using Azure.ResourceManager;
using Azure.ResourceManager.KeyVault;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using kvexplorer.shared.Exceptions;
using kvexplorer.shared.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace kvexplorer.shared;
/* Call me a bad person for abstracting away/wrapping a library already doing all the work. */

public class VaultService
{
    public AuthService _authService { get; set; }
    public IMemoryCache _memoryCache { get; set; }


    public VaultService(AuthService authService, IMemoryCache memoryCache)
    {
        _authService = authService;
        _memoryCache = memoryCache;
    }

    public async IAsyncEnumerable<KeyVaultResource> GetKeyVaultResource()
    {
        var token = new CustomTokenCredential(await _authService.GetAzureArmTokenSilent());
        var armClient = new ArmClient(token);

        var subscription = await armClient.GetDefaultSubscriptionAsync();
        await foreach (var kvResource in subscription.GetKeyVaultsAsync())
        {
            yield return kvResource;
        }
    }

    public async IAsyncEnumerable<KeyVaultResource> GetKeyVaultResources()
    {
        var token = new CustomTokenCredential(await _authService.GetAzureArmTokenSilent());
        var armClient = new ArmClient(token);
        foreach (var subscription in armClient.GetSubscriptions().ToArray())
        {
            await foreach (var kvResource in subscription.GetKeyVaultsAsync())
            {
                yield return kvResource;
            }
        }
    }

    // needed to make the tree
    public class KeyVaultResourcePlaceholder : KeyVaultResource
    { }

    /// <summary>
    /// returns all key vaults based on all the subscriptions the user has rights to view.
    /// </summary>
    /// <returns></returns>
    public async IAsyncEnumerable<KeyVaultModel> GetKeyVaultResourceBySubscriptionAndResourceGroup()
    {
        var token = new CustomTokenCredential(await _authService.GetAzureArmTokenSilent());
        var armClient = new ArmClient(token);

        var placeholder = new KeyVaultResourcePlaceholder();


        var subscriptions =  _memoryCache.GetOrCreate("subscriptions", (f) =>
        {
            f.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return armClient.GetSubscriptions();
        });


        //foreach (var subscription in armClient.GetSubscriptions())
        foreach (var subscription in subscriptions)
        {
            var resource = new KeyVaultModel
            {
                SubscriptionDisplayName = subscription.Data.DisplayName,
                SubscriptionId = subscription.Data.Id,
                Subscription = subscription,
                KeyVaultResources = new List<KeyVaultResource>() { placeholder }
            };
            yield return resource;
        }
    }

    public void UpdateSubscriptionWithKeyVaults(ref KeyVaultModel resource)
    {
        resource.KeyVaultResources.Clear();
        foreach (var kvResource in resource.Subscription.GetKeyVaults())
        {
            resource.KeyVaultResources.Add(kvResource);
        }
    }

    public async IAsyncEnumerable<KeyVaultResource> GetKeyVaultsBySubscription(KeyVaultModel resource)
    {

        var armClient = new ArmClient(new CustomTokenCredential(await _authService.GetAzureArmTokenSilent()));
        resource.Subscription = armClient.GetSubscriptionResource(resource.Subscription.Id);
        
        foreach (var kvResource in resource.Subscription.GetKeyVaults())
        {
            yield return kvResource;
        }
    }

    public async IAsyncEnumerable<KeyVaultResource> GetKeyVaultsBySubscriptionAsync(KeyVaultModel resource)
    {
        resource.KeyVaultResources.Clear();
        foreach (var kvResource in resource.Subscription.GetKeyVaults())
        {
            yield return kvResource;
        }
    }

    public async IAsyncEnumerable<KeyVaultResource> GetWithKeyVaultsBySubscriptionAsync(KeyVaultModel resource)
    {
        await foreach (var kvResource in resource.Subscription.GetKeyVaultsAsync())
        {
            yield return kvResource;
        }
    }

    public async IAsyncEnumerable<KeyProperties> GetVaultAssociatedKeys(Uri KvUri)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new KeyClient(KvUri, token);
        await foreach (var keyProperties in client.GetPropertiesOfKeysAsync())
        {
            yield return keyProperties;
        }
    }

    public async IAsyncEnumerable<SecretProperties> GetVaultAssociatedSecrets(Uri KvUri)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new SecretClient(KvUri, token);
        await foreach (var secretProperties in client.GetPropertiesOfSecretsAsync())
        {
            yield return secretProperties;
        }
    }

    public async IAsyncEnumerable<CertificateProperties> GetVaultAssociatedCertificates(Uri KvUri)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new CertificateClient(KvUri, token);
        await foreach (var certProperties in client.GetPropertiesOfCertificatesAsync())
        {
            yield return certProperties;
        }
    }

    public async Task<KeyVaultSecret> GetSecret(Uri KvUri, string secretName)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new SecretClient(KvUri, token);
        try
        {
            var secret = await client.GetSecretAsync(secretName);
            return secret;
        }
        catch (Exception ex) when (ex.Message.Contains("404"))
        {
            throw new KeyVaultItemNotFoundException(ex.Message, ex);
        }
    }

    public async Task<KeyVaultCertificateWithPolicy> GetCertificate(Uri KvUri, string name)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new CertificateClient(KvUri, token);
        try
        {
            var response = await client.GetCertificateAsync(name);
            return response;
        }
        catch (Exception ex) when (ex.Message.Contains("404"))
        {
            throw new KeyVaultItemNotFoundException(ex.Message, ex);
        }
    }

    public async Task<KeyVaultKey> GetKey(Uri KvUri, string name)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new KeyClient(KvUri, token);
        try
        {
            var response = await client.GetKeyAsync(name);
            return response;
        }
        catch (Exception ex) when (ex.Message.Contains("404"))
        {
            throw new KeyVaultItemNotFoundException(ex.Message, ex);
        }
    }
    public async Task<List<KeyProperties>> GetKeyProperties(Uri KvUri, string name)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new KeyClient(KvUri, token);
        List<KeyProperties> keyProperties = new();
        try
        {
            var response =  client.GetPropertiesOfKeyVersionsAsync(name);
            await foreach(KeyProperties item in response)
            {
                keyProperties.Add(item);
            }
            return keyProperties;
        }
        catch (Exception ex) when (ex.Message.Contains("404"))
        {
            throw new KeyVaultItemNotFoundException(ex.Message, ex);
        }
    }
}