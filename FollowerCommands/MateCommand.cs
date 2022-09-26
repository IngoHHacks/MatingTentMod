using COTL_API.CustomFollowerCommand;
using COTL_API.CustomStructures;
using COTL_API.Helpers;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using MatingTentMod.Structures;
using MatingTentMod.Tasks;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MatingTentMod.FollowerCommands;

public class MateCommand : CustomFollowerCommand
{
    public override string InternalName => "MATE_COMMAND";
    public override Sprite CommandIcon => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/MatingTent.png"));

    public override string GetTitle(Follower follower)
    {
        return "Mate";
    }

    public override string GetDescription(Follower follower)
    {
        return "Mate with another follower.";
    }

    public override bool ShouldAppearFor(Follower follower)
    {
        return Structure.Structures.FirstOrDefault<Structure>(str => str.Structure_Info != null && str.Structure_Info.Type == CustomStructureManager.GetStructureByType<MatingTent>()) != null;
    }

    public override bool IsAvailable(Follower follower)
    {
        return TaskUtils.GetAvailableStructureOfType<MatingTent>() != null && FollowerBrain
            .AllAvailableFollowerBrains().Any(x => x != follower.Brain && !FollowerManager.FollowerLocked(x.Info.ID, true) && x.CurrentTask is not MateLead && x.CurrentTask is not MateFollow);
    }

    public override string GetLockedDescription(Follower follower)
    {
        return "Unable to mate.";
    }

    public override void Execute(interaction_FollowerInteraction interaction, global::FollowerCommands finalCommand = global::FollowerCommands.None)
    {
        if (TaskUtils.GetAvailableStructureOfType<MatingTent>() == null)
        {
            interaction.CloseAndSpeak("NoTents");
            return;
        }
        if (!FollowerBrain.AllAvailableFollowerBrains().Any(x => x != interaction.follower.Brain && !FollowerManager.FollowerLocked(x.Info.ID, true) && x.CurrentTask is not MateLead && x.CurrentTask is not MateFollow))
        {
            interaction.CloseAndSpeak("NoFollowers");
            return;
        }
        
        UIFollowerSelectMenuController mateFollowerSelect =  Object.Instantiate(MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate);
        mateFollowerSelect.Show(FollowerBrain.AllAvailableFollowerBrains().Where(x => x != interaction.follower.Brain && !FollowerManager.FollowerLocked(x.Info.ID, true)).ToList(), null, instant: false, UpgradeSystem.Type.Count, hideOnSelection: true, cancellable: false);
        mateFollowerSelect.OnFollowerSelected = (selectedFollower) =>
        {
            if (selectedFollower == null || selectedFollower.ID == interaction.follower.Brain.Info.ID) interaction.Close();
            interaction.StartCoroutine(interaction.FrameDelayCallback(delegate
            {
                interaction.eventListener.PlayFollowerVO(interaction.generalAcknowledgeVO);
                FollowerBrain selectedFollowerBrain = FollowerBrain.FindBrainByID(selectedFollower.ID);
                MateLead lead = new(selectedFollowerBrain);
                interaction.follower.Brain.HardSwapToTask(lead);
                interaction.follower.Brain.CurrentOverrideTaskType = lead.Type;
                MateFollow follow = new(interaction.follower.Brain);
                selectedFollowerBrain.HardSwapToTask(follow);
                selectedFollowerBrain.CurrentOverrideTaskType = follow.Type;
            }));
            interaction.Close();
        };
    }

}