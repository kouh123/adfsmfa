﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public class DevelopmentInMemoryStore
    {
        private readonly ConcurrentDictionary<string, Fido2User> _storedUsers = new ConcurrentDictionary<string, Fido2User>();
        private readonly List<StoredCredential> _storedCredentials = new List<StoredCredential>();

        public Fido2User GetOrAddUser(string username, Func<Fido2User> addCallback)
        {
            return _storedUsers.GetOrAdd(username, addCallback());
        }

        public Fido2User GetUser(string username)
        {
            _storedUsers.TryGetValue(username, out var user);
            return user;
        }

        public List<StoredCredential> GetCredentialsByUser(Fido2User user)
        {
            return _storedCredentials.Where(c => c.UserId.SequenceEqual(user.Id)).ToList();
        }

        public StoredCredential GetCredentialById(byte[] id)
        {
            return _storedCredentials.Where(c => c.Descriptor.Id.SequenceEqual(id)).FirstOrDefault();
        }

        public List<StoredCredential> GetCredentialsByUserHandle(byte[] userHandle)
        {
            return _storedCredentials.Where(c => c.UserHandle.SequenceEqual(userHandle)).ToList();
        }

        public void UpdateCounter(byte[] credentialId, uint counter)
        {
            var cred = _storedCredentials.Where(c => c.Descriptor.Id.SequenceEqual(credentialId)).FirstOrDefault();
            cred.SignatureCounter = counter;
        }

        public void AddCredentialToUser(Fido2User user, StoredCredential credential)
        {
            credential.UserId = user.Id;
            _storedCredentials.Add(credential);
        }

        public void RemoveCredentialToUser(Fido2User user, string aaguid)
        {
            _storedCredentials.RemoveAll(c => c.UserId.SequenceEqual(user.Id) && c.AaGuid.ToString().Equals(aaguid));
        }

        public List<Fido2User> GetUsersByCredentialId(byte[] credentialId)
        {
            // our in-mem storage does not allow storing multiple users for a given credentialId. Yours shouldn't either.
            var cred = _storedCredentials.Where(c => c.Descriptor.Id.SequenceEqual(credentialId)).FirstOrDefault();

            if (cred == null)
                return new List<Fido2User>();

            return _storedUsers.Where(u => u.Value.Id.SequenceEqual(cred.UserId)).Select(u => u.Value).ToList();
        }
    }

    public class StoredCredential
    {
        public byte[] UserId { get; set; }
        public PublicKeyCredentialDescriptor Descriptor { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] UserHandle { get; set; }
        public uint SignatureCounter { get; set; }
        public string CredType { get; set; }
        public DateTime RegDate { get; set; }
        public Guid AaGuid { get; set; }
    }
}