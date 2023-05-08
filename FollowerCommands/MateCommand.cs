using COTL_API.CustomFollowerCommand;
using COTL_API.CustomStructures;
using COTL_API.Helpers;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using MatingTentMod.Structures;
using MatingTentMod.Tasks;
using MatingTentMod.Utils;
using src.Extensions;
using System.Collections.Generic;
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
        if (LocalizationManager.CurrentLanguageCode == "zh-CN")
            return "交配";
        else
            return "Mate";
    }

    public override string GetDescription(Follower follower)
    {
        if (LocalizationManager.CurrentLanguageCode == "zh-CN")
            return "与另一位教徒交配。";
        else
            return "Mate with another follower.";
    }

    public override bool ShouldAppearFor(Follower follower)
    {
        if (follower.Brain.Info.OldAge || follower.Brain.Info.Age < 18) return false;
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

    public override void Execute(interaction_FollowerInteraction interaction,
        global::FollowerCommands finalCommand = global::FollowerCommands.None)
    {
        if (TaskUtils.GetAvailableStructureOfType<MatingTent>() == null)
        {
            interaction.eventListener.PlayFollowerVO(interaction.negativeAcknowledgeVO);
            interaction.CloseAndSpeak("NoTents");
            return;
        }

        if (!FollowerBrain.AllAvailableFollowerBrains().Any(x =>
                x != interaction.follower.Brain && !FollowerManager.FollowerLocked(x.Info.ID, true) &&
                x.CurrentTask is not MateLead && x.CurrentTask is not MateFollow))
        {
            interaction.eventListener.PlayFollowerVO(interaction.negativeAcknowledgeVO);
            interaction.CloseAndSpeak("NoFollowers");
            return;
        }

        UIFollowerSelectMenuController mateFollowerSelect =
            MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
        List<FollowerBrain> validBrains = FollowerBrain.AllAvailableFollowerBrains().Where(x =>
            x != interaction.follower.Brain && !FollowerManager.FollowerLocked(x.Info.ID, true) &&
            x.Info.CursedState != CustomCursedStates.CHILD && x.Info.CursedState != Thought.OldAge).ToList();
        mateFollowerSelect.Show(validBrains);
        mateFollowerSelect.OnFollowerSelected = (selectedFollower) =>
        {
            if (selectedFollower == null || selectedFollower.ID == interaction.follower.Brain.Info.ID)
            {
                interaction.Close(true, reshowMenu: false);
                return;
            }

            interaction.StartCoroutine(interaction.FrameDelayCallback(delegate
            {
                interaction.eventListener.PlayFollowerVO(interaction.generalAcknowledgeVO);
                FollowerBrain selectedFollowerBrain = FollowerBrain.FindBrainByID(selectedFollower.ID);
                MateActions.BeginMate(interaction.follower.Brain, selectedFollowerBrain);
                interaction.Close(true, reshowMenu: false);
                interaction.Close(true, reshowMenu: false);
            }));
        };
        mateFollowerSelect.OnCancel = delegate { interaction.Close(true, reshowMenu: false); };
    }

}