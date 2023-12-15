namespace EmoteForAll.Commands.Emote
{
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;
    using global::EmoteForAll.Classes;
    using PlayerRoles;
    using System;

    [CommandHandler(typeof(ClientCommandHandler))]
    public class EmoteCommand : ICommand
    {
        public string Command => "emote";

        public string[] Aliases => new string[]
        {
            "dance"
        };

        public string Description => "As a Human Role, Use this to Dance Like Skeleton";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out Player player))
            {
                response = "This command can only be ran by a player!";
                return true;
            }

            if (!sender.CheckPermission("EmoteForAll.Emote"))
            {
                response = "You do not have access to the Emote command!";
                return false;
            }

            if (player.Role.Team == Team.Dead || player.Role.Team == Team.SCPs)
            {
                response = "Please wait until you spawn in as a Human class.";
                return false;
            }

            bool result = EmoteHandler.MakePlayerEmote(player);
            response = result
                ? "Emote Triggered!"
                : "Couldn't trigger an Emote. Maybe you're already Emoting...";
            return result;
        }
    }
}
