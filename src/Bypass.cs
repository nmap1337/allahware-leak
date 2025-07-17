using HarmonyLib;
using IronSourceJSON;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Internal;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace AllahWare
{
    public class Bypass
    {
        private static bool spoofValues;
        public static bool banned = true;
        public static string randomID;
        public static string customID;
        public static string realID;
        public static string sessionTicket;
        public static string name;
        public static bool spoofID = true;
        private static readonly System.Random random = new System.Random();
        private static string badgePayload = "\"BDGAFFAEDHCDDDJ\":" + Bypass.userDataRecord + ",\"Badge\":" + Bypass.userDataRecord;
        private static string userDataRecord = "{\"LastUpdated\":\"2025-02-27T00:10:32.228Z\",\"Permission\":1,\"Value\":\"" + Bypass.badgeID + "short guy\"}";
        public static string badgeID = "6";
        public static bool antiKick;
        // GoreBoxCheat.Bypass
        // Token: 0x06000116 RID: 278 RVA: 0x00004F4C File Offset: 0x0000314C
        public static string GenerateRandomId(bool nullID = false)
        {
            string result;
            if (nullID)
            {
                result = "Subscribe to Bx35mm!";
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder(16);
                System.Random random = new System.Random();
                for (int i = 0; i < 16; i++)
                {
                    stringBuilder.Append("ABCDEF0123456789"[random.Next("ABCDEF0123456789".Length)]);
                }
                result = stringBuilder.ToString();
            }
            return result;
        }
        public static string GenerateAndroidID()
        {
            var sb = new StringBuilder(16);
            for (int i = 0; i < 16; i++)
            {
                sb.Append(random.Next(0, 16).ToString("x")); // hexadecimal [0-9a-f]
            }
            return sb.ToString();
        }
       
        [HarmonyPatch(typeof(LoadBalancingPeer), nameof(LoadBalancingPeer.OpAuthenticate))]
        public static class PhotonAuthPatch
        {
            private static bool Prefix(string appId, string appVersion, ref AuthenticationValues authValues, string regionCode, bool getLobbyStatistics)
            {
                // KAZOO AUTH TOKEN
                authValues.AuthGetParameters = "username=5EFFE94E106EC3FF&token=es4z3kxkycnjhbf5gsjcuozdxo4838qz4d3xc8b5yrsmpbm3q9";
                // ME
                //authValues.AuthGetParameters = "username=2146843E5B184C12&token=pscbi6agjbxo6iuytzjmgf81z11zo7c7h7p9ifqceuxmwpim5m";
                Debug.Log("ALLAHWARE[] - " + authValues.AuthGetParameters);
                return true;
            }
        }
        [HarmonyPatch(typeof(PhotonNetwork), "LeaveRoom")]
        public static class LeaveBypass
        {
            // Token: 0x06000128 RID: 296 RVA: 0x00005664 File Offset: 0x00003864
            private static bool Prefix()
            {
                bool antiKick = Bypass.antiKick;
                return !antiKick;
            }
        }
        [HarmonyPatch(typeof(PhotonNetwork), "Disconnect")]
        public static class DisconnectBypass
        {
            // Token: 0x06000127 RID: 295 RVA: 0x00005664 File Offset: 0x00003864
            private static bool Prefix()
            {
                bool antiKick = Bypass.antiKick;
                return !antiKick;
            }
        }
        

        /* [HarmonyPatch(typeof(PlayFabClientAPI), nameof(PlayFabClientAPI.GetAccountInfo))]
public class GetAccountInfoBypass
{
    public static bool Prefix(
        GetAccountInfoRequest request,
        Action<GetAccountInfoResult> resultCallback,
        Action<PlayFabError> errorCallback)
    {
        request.PlayFabId = "ALLAHWAREBEST";
        return true;

    }
}*/

       /*  [HarmonyPatch(typeof(PlayFabClientAPI), nameof(PlayFabClientAPI.LoginWithSteam))]
         public class PlayFabClientAPIPatch
         {
             public static bool Prefix(Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback)
             {
                 var request = new LoginWithAndroidDeviceIDRequest
                 {
                     AndroidDeviceId = GenerateAndroidID(),
                     CreateAccount = new Il2CppSystem.Nullable<bool>(true),
                 };
                 Debug.Log("Using Spoofed ID to authenticate with PlayFab...");
                 PlayFabClientAPI.LoginWithAndroidDeviceID(request, resultCallback, errorCallback);
                 return false;
             }
         }*/

        [HarmonyPatch(typeof(PlayFabUnityHttp), "OnResponse")]
        public class PlayFabBypass
        {
            private static bool Prefix(ref string response, CallRequestContainer reqContainer)
            {
                Debug.Log(reqContainer.FullUrl);
                if (reqContainer.FullUrl.Contains("LoginWithSteam"))
                {
                    if (response.Contains("AccountBanned"))
                    {
                        Debug.Log("Account banned, using proxy");
                        Bypass.banned = true;
                    }
                    if (true)
                    {
                        string spoofedResponse = "{\"code\":200,\"status\":\"OK\",\"data\":{\"SessionTicket\":\"2146843E5B184C12-5D3C20396E7501F7-D7516ED1E76B42B4-D4EDA-8DDBE7CCA6D2085-j9mqYnR/cBZtdD0G002DiDmoTfvpbNoGD+1o5KwNYRc=\",\"PlayFabId\":\"5EFFE94E106EC3FF\",\"NewlyCreated\":false,\"SettingsForUser\":{\"NeedsAttribution\":false,\"GatherDeviceInfo\":false,\"GatherFocusInfo\":false},\"LastLoginTime\":\"2025-07-09T00:07:34.705Z\",\"EntityToken\":{\"EntityToken\":\"NHxPZ2ZXVGM0ZUVnTnZmRHQ4Zjh1YWVCYlZSdmVpNnRKWjNoK2dUUWhJb1g0PXx7ImkiOiIyMDI1LTA3LTA5VDAwOjA4OjUzWiIsImlkcCI6IlN0ZWFtIiwiZSI6IjIwMjUtMDctMTBUMDA6MDg6NTNaIiwiZmkiOiIyMDI1LTA3LTA5VDAwOjA4OjUzWiIsInRpZCI6ImJuSU1pcWVEWkxNIiwiaWRpIjoiNzY1NjExOTk4NjMwMzc4MjciLCJoIjoiaW50ZXJuYWwiLCJlYyI6InRpdGxlX3BsYXllcl9hY2NvdW50ITVEM0MyMDM5NkU3NTAxRjcvRDRFREEvMjE0Njg0M0U1QjE4NEMxMi9ENzUxNkVEMUU3NkI0MkI0LyIsImVpIjoiRDc1MTZFRDFFNzZCNDJCNCIsImV0IjoidGl0bGVfcGxheWVyX2FjY291bnQifQ==\",\"TokenExpiration\":\"2025-07-10T00:08:53Z\",\"Entity\":{\"Id\":\"D7516ED1E76B42B4\",\"Type\":\"title_player_account\",\"TypeString\":\"title_player_account\"}},\"TreatmentAssignment\":{\"Variants\":[],\"Variables\":[]}}}";
                        JSONNode sigma = JSON.Parse(spoofedResponse);
                        Bypass.realID = sigma["data"]["PlayFabId"].Value;
                     // sigma["data"]["PlayFabId"] = Bypass.customID;
                        response = sigma.ToString();
                    }

                    Debug.Log(response);
                }
                bool spoofID2 = true;
                if (spoofID2)
                {
                    if (reqContainer.FullUrl.Contains("GetAccountInfo"))
                    {
                        string allah = " {\"code\":200,\"status\":\"OK\",\"data\":{\"AccountInfo\":{\"PlayFabId\":\"sigma\",\"Created\":\"2025-06-03T21:11:45.67Z\",\"TitleInfo\":{\"DisplayName\":\"BxDev\",\"Origination\":\"Steam\",\"Created\":\"2025-06-03T21:12:23.702Z\",\"LastLogin\":\"2025-07-08T23:54:59.587Z\",\"FirstLogin\":\"2025-06-03T21:12:23.702Z\",\"isBanned\":false,\"TitlePlayerAccount\":{\"Id\":\"D7516ED1E76B42B4\",\"Type\":\"title_player_account\",\"TypeString\":\"title_player_account\"}},\"PrivateInfo\":{},\"SteamInfo\":{\"SteamId\":\"AllahV2\",\"SteamName\":\"allah\",\"SteamCountry\":\"GB\",\"SteamCurrency\":\"GBP\"}}}}";
                        JSONNode json = JSON.Parse(allah);
                        json["data"]["AccountInfo"]["PlayFabId"] = Bypass.customID;
                        json["data"]["AccountInfo"]["TitleInfo"]["DisplayName"] = Bypass.name;
                        response = json.ToString();
                    }
                    bool flag6 = reqContainer.FullUrl.Contains("GetPhotonAuthenticationToken");
                    if (flag6)
                    {

                     // response = "{\"code\":200,\"status\":\"OK\",\"data\":{\"PhotonCustomAuthenticationToken\":\"pscbi6agjbxo6iuytzjmgf81z11zo7c7h7p9ifqceuxmwpim5m\",\"PhotonCustomAuthenticationType\":\"Steam\"}}";

                      response = "{\"code\":200,\"status\":\"OK\",\"data\":{\"PhotonCustomAuthenticationToken\":\"es4z3kxkycnjhbf5gsjcuozdxo4838qz4d3xc8b5yrsmpbm3q9\",\"PhotonCustomAuthenticationType\":\"Steam\"}}";

                    }
                    bool flag4 = reqContainer.FullUrl.Contains("ReportDeviceInfo");
                    if (flag4)
                    {
                        response = "{\"code\":200,\"status\":\"OK\",\"data\":{}}";
                    }
                    else
                    {
                        bool flag5 = reqContainer.FullUrl.Contains("WriteEvents");
                        if (flag5)
                        {
                            response = "{\"code\":200,\"status\":\"OK\",\"data\":{\"AssignedEventIds\":[\"2bc4a1d2a11549469c704e1791101edf\"]}}";
                        }
                        else
                        {
                                bool flag7 = reqContainer.FullUrl.Contains("GetFriendsList");
                                if (flag7)
                                {
                                    response = "{\"code\":200,\"status\":\"OK\",\"data\":{\"Friends\":[]}}";
                                }
                        }
                    }
                    if (reqContainer.FullUrl.Contains("UpdateUserTitleDisplayName"))
                    {
                        string spoofed = "{\"code\":200,\"status\":\"OK\",\"data\":{\"DisplayName\":\"placeholder\"}}";
                        JSONNode spoofed1 = JSON.Parse(spoofed);
                        spoofed1["data"]["DisplayName"] = Bypass.name;
                        response = spoofed1.ToString();
                    }
                    if (reqContainer.FullUrl.Contains("GetUserData"))
                    {
                        string spoofed = "{\"code\":200,\"status\":\"OK\",\"data\":{\"Data\":{\"Badge\":{\"Value\":\"6BxDev\",\"LastUpdated\":\"2025-07-08T23:54:59.587Z\",\"Permission\":1}},\"PlayFabId\":\"YOUR_PLAYFAB_ID_HERE\"}}";
                        JSONNode allah = JSON.Parse(spoofed);
                        response = allah.ToString();
                    }
                }
                return true;
            }
        }
    }
}