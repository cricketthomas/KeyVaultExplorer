﻿using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.Resources;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using KeyVaultExplorer.Database;
using KeyVaultExplorer.Exceptions;
using KeyVaultExplorer.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KeyVaultExplorer.Services;
/* Call me a bad person for abstracting away/wrapping a library already doing all the work. */

public partial class VaultService
{
    public VaultService(AuthService authService, IMemoryCache memoryCache, KvExplorerDb dbContext)
    {
        _authService = authService;
        _memoryCache = memoryCache;
        _dbContext = dbContext;
    }

    public AuthService _authService { get; set; }
    public IMemoryCache _memoryCache { get; set; }
    public KvExplorerDb _dbContext { get; set; }

    public async IAsyncEnumerable<SubscriptionResourceWithNextPageToken> GetAllSubscriptions(CancellationToken cancellationToken = default, string continuationToken = null)
    {
        var armClient = new ArmClient(new CustomTokenCredential(await _authService.GetAzureArmTokenSilent()));
        var subscriptionsPageable = armClient.GetSubscriptions().GetAllAsync(cancellationToken).AsPages(continuationToken);

        await foreach (var subscription in subscriptionsPageable)
        {
            foreach (var subscriptionResource in subscription.Values)
            {
                yield return new SubscriptionResourceWithNextPageToken(subscriptionResource, subscription.ContinuationToken);
            }
        }
    }

    public async Task<KeyVaultCertificateWithPolicy> GetCertificate(Uri kvUri, string name)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new CertificateClient(kvUri, token);
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

    public async Task<List<CertificateProperties>> GetCertificateProperties(Uri kvUri, string name)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new CertificateClient(kvUri, token);
        List<CertificateProperties> list = new();
        try
        {
            var response = client.GetPropertiesOfCertificateVersionsAsync(name);
            await foreach (CertificateProperties item in response)
            {
                list.Add(item);
            }
            return list;
        }
        catch (Exception ex) when (ex.Message.Contains("404"))
        {
            throw new KeyVaultItemNotFoundException(ex.Message, ex);
        }
    }

    public async Task<KeyVaultKey> GetKey(Uri kvUri, string name)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new KeyClient(kvUri, token);
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

    public async Task<List<KeyProperties>> GetKeyProperties(Uri kvUri, string name)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new KeyClient(kvUri, token);
        List<KeyProperties> list = new();
        try
        {
            var response = client.GetPropertiesOfKeyVersionsAsync(name);
            await foreach (KeyProperties item in response)
            {
                list.Add(item);
            }
            return list;
        }
        catch (Exception ex) when (ex.Message.Contains("404"))
        {
            throw new KeyVaultItemNotFoundException(ex.Message, ex);
        }
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

    /// <summary>
    /// returns all key vaults based on all the subscriptions the user has rights to view.
    /// </summary>
    /// <returns></returns>
    public async IAsyncEnumerable<KvSubscriptionModel> GetKeyVaultResourceBySubscription()
    {
        var token = new CustomTokenCredential(await _authService.GetAzureArmTokenSilent());
        var armClient = new ArmClient(token);

        var placeholder = new KeyVaultResourcePlaceholder();
        var rgPlaceholder = new KvResourceGroupModel() //needed to show chevron
        {
            KeyVaultResources = [placeholder]
        };

        var subscriptions = await _memoryCache.GetOrCreateAsync("subscriptions", async (f) =>
        {
            f.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

            var savedSubscriptions = await _dbContext.GetStoredSubscriptions();
            List<SubscriptionResource> subscriptionCollection = [];
            foreach (var sub in savedSubscriptions)
            {
                var sr = await armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(sub.SubscriptionId)).GetAsync();
                subscriptionCollection.Add(sr.Value);
            }
            if (subscriptionCollection.Any())
                return subscriptionCollection;

            return armClient.GetSubscriptions().AsEnumerable();
        });

        //foreach (var subscription in armClient.GetSubscriptions())
        foreach (var subscription in subscriptions)
        {
            var resource = new KvSubscriptionModel
            {
                SubscriptionDisplayName = subscription.Data.DisplayName,
                SubscriptionId = subscription.Data.Id,
                Subscription = subscription,
                ResourceGroups = [rgPlaceholder]
            };
            yield return resource;
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

    public async IAsyncEnumerable<KeyVaultResource> GetKeyVaultsByResourceGroup(ResourceGroupResource resource)
    {
        var armClient = new ArmClient(new CustomTokenCredential(await _authService.GetAzureArmTokenSilent()));

        await foreach (var kvResource in resource.GetKeyVaults())
        {
            yield return kvResource;
        }
    }

    public async IAsyncEnumerable<KeyVaultResource> GetKeyVaultsBySubscription(KvSubscriptionModel resource)
    {
        var armClient = new ArmClient(new CustomTokenCredential(await _authService.GetAzureArmTokenSilent()));
        resource.Subscription = armClient.GetSubscriptionResource(resource.Subscription.Id);

        foreach (var kvResource in resource.Subscription.GetKeyVaults())
        {
            yield return kvResource;
        }
    }

    public async IAsyncEnumerable<ResourceGroupResource> GetResourceGroupBySubscription(KvSubscriptionModel resource)
    {
        var armClient = new ArmClient(new CustomTokenCredential(await _authService.GetAzureArmTokenSilent()));
        resource.Subscription = armClient.GetSubscriptionResource(resource.Subscription.Id);

        foreach (var kvResourceGroup in resource.Subscription.GetResourceGroups())
        {
            yield return kvResourceGroup;
        }
    }

    public async Task<KeyVaultSecret> GetSecret(Uri kvUri, string secretName)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new SecretClient(kvUri, token);
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

    public async Task<List<SecretProperties>> GetSecretProperties(Uri kvUri, string name)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new SecretClient(kvUri, token);
        List<SecretProperties> list = new();
        try
        {
            var response = client.GetPropertiesOfSecretVersionsAsync(name);
            await foreach (SecretProperties item in response)
            {
                list.Add(item);
            }
            return list;
        }
        catch (Exception ex) when (ex.Message.Contains("404"))
        {
            throw new KeyVaultItemNotFoundException(ex.Message, ex);
        }
    }

    public async Task<Dictionary<string, KeyVaultResource>> GetStoredSelectedSubscriptions(string subsriptionId)
    {
        var resource = new ResourceIdentifier(subsriptionId);
        var armClient = new ArmClient(new CustomTokenCredential(await _authService.GetAzureArmTokenSilent()));
        SubscriptionResource subscription = armClient.GetSubscriptionResource(resource);

        var vaults = subscription.GetKeyVaultsAsync();
        Dictionary<string, KeyVaultResource> savedSubs = [];
        await foreach (var vault in vaults)
        {
            savedSubs.Add(resource.SubscriptionId!, vault);
        }

        return savedSubs;
    }

    public record SubscriptionResourceWithNextPageToken(SubscriptionResource SubscriptionResource, string ContinuationToken);

    public async IAsyncEnumerable<CertificateProperties> GetVaultAssociatedCertificates(Uri kvUri)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new CertificateClient(kvUri, token);
        await foreach (var certProperties in client.GetPropertiesOfCertificatesAsync())
        {
            yield return certProperties;
        }
    }

    public async IAsyncEnumerable<KeyProperties> GetVaultAssociatedKeys(Uri kvUri)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
        var client = new KeyClient(kvUri, token);
        await foreach (var keyProperties in client.GetPropertiesOfKeysAsync())
        {
            yield return keyProperties;
        }
    }

    public async IAsyncEnumerable<SecretProperties> GetVaultAssociatedSecrets(Uri kvUri)
    {
        if (kvUri is not null)
        {
            var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
            var client = new SecretClient(kvUri, token);
            await foreach (var secretProperties in client.GetPropertiesOfSecretsAsync())
            {
                yield return secretProperties;
            }
        }
    }

    public async IAsyncEnumerable<KeyVaultResource> GetWithKeyVaultsBySubscriptionAsync(KvSubscriptionModel resource)
    {
        await foreach (var kvResource in resource.Subscription.GetKeyVaultsAsync())
        {
            yield return kvResource;
        }
    }

    public async Task CreateSecret(KeyVaultSecret keyVaultSecret, Uri KeyVaultUri)
    {
        try
        {
            var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());
            SecretClient client = new SecretClient(KeyVaultUri, token);
            await client.SetSecretAsync(keyVaultSecret);
        }
        catch (Exception ex)
        {
            throw new KeyVaultItemNotFoundException(ex.Message, ex);
        }
    }

    public async Task UpdateSecret(SecretProperties secretProperties, Uri KeyVaultUri)
    {
        var token = new CustomTokenCredential(await _authService.GetAzureKeyVaultTokenSilent());

        try
        {
            if (secretProperties is not null)
            {
                SecretClient client = new SecretClient(KeyVaultUri, token);

                await client.UpdateSecretPropertiesAsync(secretProperties);
            }
        }
        catch (Exception ex)
        {
            throw new KeyVaultItemNotFoundException(ex.Message, ex);
        }
    }
}