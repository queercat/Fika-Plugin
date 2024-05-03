using EFT;
using EFT.UI.Matchmaker;
using Fika.Core.Networking.Http;
using Fika.Core.Networking.Http.Models;
using System;
using System.Reflection;
using UnityEngine;

namespace Fika.Core.Coop.Matchmaker
{
    public enum EMatchmakerType
    {
        Single = 0,
        GroupPlayer = 1,
        GroupLeader = 2
    }

    public static class MatchmakerAcceptPatches
    {
        #region Fields/Properties
        public static MatchMakerAcceptScreen MatchMakerAcceptScreenInstance { get; set; }
        public static Profile Profile { get; set; }
        public static string PMCName { get; set; }
        public static EMatchmakerType MatchingType { get; set; } = EMatchmakerType.Single;
        public static bool IsServer => MatchingType == EMatchmakerType.GroupLeader;
        public static bool IsClient => MatchingType == EMatchmakerType.GroupPlayer;
        public static bool IsSinglePlayer => MatchingType == EMatchmakerType.Single;
        public static PlayersRaidReadyPanel PlayersRaidReadyPanel { get; set; }
        public static MatchMakerGroupPreview MatchMakerGroupPreview { get; set; }
        public static int HostExpectedNumberOfPlayers { get; set; } = 1;
        public static WeatherClass[] Nodes { get; set; } = null;
        public static string groupId { get; set; }
        public static long timestamp { get; set; }
        #endregion

        #region Static Fields

        public static object MatchmakerScreenController
        {
            get
            {
                var fields = typeof(MatchMakerAcceptScreen).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

                foreach (var field in fields)
                    if (field.Name == "ScreenController") return field.GetValue(MatchMakerAcceptScreenInstance);

                return null;
            }
        }

        public static GameObject EnvironmentUIRoot { get; internal set; }
        public static MatchmakerTimeHasCome.GClass3163 GClass3163 { get; internal set; }
        #endregion

        public static bool JoinMatch(RaidSettings settings, string profileId, string serverId, out CreateMatch result, out string errorMessage)
        {
            errorMessage = $"No server matches the data provided or the server no longer exists";
            result = new CreateMatch();

            if (MatchMakerAcceptScreenInstance == null) return false;

            var body = new MatchJoinRequest(serverId, profileId);
            result = FikaRequestHandler.RaidJoin(body);

            if (result.GameVersion != FikaPlugin.EFTVersionMajor)
            {
                errorMessage = $"You are attempting to use a different version of EFT {FikaPlugin.EFTVersionMajor} than what the server is running {result.GameVersion}";
                return false;
            }

            var detectedFikaVersion = Assembly.GetExecutingAssembly().GetName().Version;
            if (result.FikaVersion != detectedFikaVersion)
            {
                errorMessage = $"You are attempting to use a different version of Fika {detectedFikaVersion} than what the server is running {result.FikaVersion}";
                return false;
            }

            return true;
        }

        public static void CreateMatch(string profileId, string hostUsername, RaidSettings raidSettings)
        {
            var body = new CreateMatch(profileId, hostUsername, timestamp, raidSettings, HostExpectedNumberOfPlayers, raidSettings.Side, raidSettings.SelectedDateTime);

            FikaRequestHandler.RaidCreate(body);

            groupId = profileId;
            timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

            MatchingType = EMatchmakerType.GroupLeader;
        }
    }
}
