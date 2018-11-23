using System;
using System.Collections.Generic;
using System.Linq;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

namespace watchapi {
    public static class VaultDataManager {
        private const string TOKEN = "3BF2TCJt2j4t8mWC4kRxuJl1";
        private const string HOST = "http://10.33.67.13";
        private static Dictionary<string, int> cachedSecretMetadata = new Dictionary<string, int> ();
        private static bool IsCacheValid (Dictionary<string, int> secrets) {
            var invalidKeyList = new List<string> ();
            foreach (var kv in secrets) {
                var key = kv.Key;
                if (!(cachedSecretMetadata.ContainsKey (key) && cachedSecretMetadata[key] == secrets[key])) 
                {
                    invalidKeyList.Add (key);
                }
            }
            Console.WriteLine ("Invalid keys are...");
            foreach (var invalidKey in invalidKeyList) 
            {
                Console.WriteLine (invalidKey);
            }
            Console.WriteLine ("Invalid keys finished...");
            return invalidKeyList.Count () > 0;
        }
        public static void RefreshSecretMetadata () {
            try {
                var authMethod = new TokenAuthMethodInfo (TOKEN);
                var vaultClientSettings = new VaultClientSettings (HOST, authMethod);
                var vaultClient = new VaultClient (vaultClientSettings);
                var kv2Secret = vaultClient.V1.Secrets.KeyValue.V2.ReadSecretPathsAsync (string.Empty).Result; //.ReadSecretMetadataAsync("s1").Result;
                var metadataVersionMap = new Dictionary<string, int> ();

                foreach (var secret in kv2Secret.Data.Keys) {
                    try {
                        var metadata = vaultClient.V1.Secrets.KeyValue.V2.ReadSecretMetadataAsync (secret).Result;
                        var currentVersion = metadata.Data.CurrentVersion;
                        metadataVersionMap.Add (secret, currentVersion);
                    } catch (Exception exc) {

                    }

                }

                if (!IsCacheValid (metadataVersionMap)) {
                    cachedSecretMetadata = metadataVersionMap;
                }
            } catch (Exception exc) {
                Console.WriteLine (exc.Message);
            }
        }
    }
}