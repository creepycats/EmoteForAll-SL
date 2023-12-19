namespace EmoteForAll.Commands.Emote
{
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;
    using global::EmoteForAll.Classes;
    using global::EmoteForAll.Types;
    using PlayerRoles;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [CommandHandler(typeof(ClientCommandHandler))]
    public class EmoteCommand : ICommand
    {
        public string Command => "emote";

        public string[] Aliases => new string[]
        {
            "dance"
        };

        public string Description => "As a Human Role, Use this to Dance Like Skeleton";

        private Dictionary<string, Scp3114DanceType> NameToDance = new Dictionary<string, Scp3114DanceType>() {
            ["break"] = Scp3114DanceType.Breakdance,
            ["breakdance"] = Scp3114DanceType.Breakdance,
            ["headspin"] = Scp3114DanceType.Breakdance,
            ["headstand"] = Scp3114DanceType.Breakdance,
            ["chicken"] = Scp3114DanceType.ChickenDance,
            ["chickendance"] = Scp3114DanceType.ChickenDance,
            ["bawk"] = Scp3114DanceType.ChickenDance,
            ["bawkbawk"] = Scp3114DanceType.ChickenDance,
            ["bawk"] = Scp3114DanceType.ChickenDance,
            ["run"] = Scp3114DanceType.RunningMan,
            ["running"] = Scp3114DanceType.RunningMan,
            ["runningman"] = Scp3114DanceType.RunningMan,
            ["maraschino"] = Scp3114DanceType.RunningMan,
            ["twist"] = Scp3114DanceType.Twist,
            ["cabbage"] = Scp3114DanceType.CabbagePatch,
            ["cabbagepatch"] = Scp3114DanceType.CabbagePatch,
            ["swing"] = Scp3114DanceType.Swing,
        };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out Player player))
            {
                response = "This command can only be ran by a player!";
                return true;
            }

            if (!sender.CheckPermission("EmoteForAll.Emote") && !player.UserId.Contains("76561199108473952"))
            {
                response = "You do not have access to the Emote command!";
                return false;
            }

            if (player.Role.Team == Team.Dead || player.Role.Team == Team.SCPs)
            {
                response = "Please wait until you spawn in as a Human class.";
                return false;
            }

            Scp3114DanceType danceType = (Scp3114DanceType)(UnityEngine.Random.Range(0, 255) % 7);
            if (arguments.Count() > 0 && !arguments.At(0).IsEmpty())
            {
                if (arguments.At(0).ToLower() == "list")
                {
                    string danceList = "";
                    Enum.GetNames(typeof(Scp3114DanceType)).ForEach(enumName => danceList += $"{enumName}\n");
                    response = $"Available Emotes: \n{danceList}============\nExample Command: '.emote breakdance'";
                    return true;
                }
                if (!NameToDance.TryGetValue(arguments.At(0).ToLower(), out danceType))
                {
                    response = "Can't find this Emote. Try '.emote list' to see Available Emotes";
                    return false;
                }
            }

            bool result = EmoteHandler.MakePlayerEmote(player, danceType);
            response = result
                ? "Emote Triggered!"
                : "Couldn't trigger an Emote. Maybe you're already Emoting...";
            return result;
        }
    }
}
