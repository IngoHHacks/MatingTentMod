using COTL_API.CustomFollowerCommand;
using COTL_API.CustomStructures;
using COTL_API.Helpers;
using MatingTentMod.Structures;
using MatingTentMod.Tasks;
using MatingTentMod.Utils;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MatingTentMod.FollowerCommands;

public class MateLamb : CustomFollowerCommand
{
    public override string InternalName => "MATE_COMMAND_LAMB";

    public override Sprite CommandIcon =>
        TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/MatingTent.png"));

    public override string GetTitle(Follower follower)
    {
        return "Mate with Lamb";
    }

    public override string GetDescription(Follower follower)
    {
        return "Mate with the Lamb.";
    }

    public override bool ShouldAppearFor(Follower follower)
    {
        if (follower.Brain.Info.OldAge || follower.Brain.Info.Age < 18)
        {
            return false;
        }

        return Structure.Structures.FirstOrDefault(str =>
            str.Structure_Info != null &&
            str.Structure_Info.Type == CustomStructureManager.GetStructureByType<MatingTent>()) != null;
    }

    public override bool IsAvailable(Follower follower)
    {
        return TaskUtils.GetAvailableStructureOfType<MatingTent>() != null;
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

        interaction.StartCoroutine(interaction.FrameDelayCallback(delegate
        {
            interaction.eventListener.PlayFollowerVO(interaction.generalAcknowledgeVO);
            MateActions.BeginMate(interaction.follower.Brain, null);
            interaction.StartCoroutine(LambPerformMate(interaction));
        }));
    }

    private IEnumerator LambPerformMate(interaction_FollowerInteraction interaction)
    {
        float oldStoppingDistance = PlayerFarming.Instance.unitObject.StoppingDistance;
        PlayerFarming.Instance.unitObject.StoppingDistance = 0.01f;
        GameManager.GetInstance().OnConversationNext(interaction.follower.gameObject, 4f);
        GameManager.GetInstance().AddPlayerToCamera();
        GameManager.GetInstance().CameraSetOffset(Vector3.zero);
        yield return new WaitUntil(() => interaction.follower.Brain.CurrentTask is MateWithLamb);
        PlayerFarming.Instance.GoToAndStop(interaction.follower.Brain.CurrentTask.GetDestination(interaction.follower) +
                                           new Vector3(0.4f, 0f, 0.4f));
        PlayerFarming.Instance.GoToAndStopping = true;
        yield return new WaitUntil(() =>
            interaction.follower.Brain.CurrentTask.State == FollowerTaskState.Doing &&
            !PlayerFarming.Instance.GoToAndStopping);
        PlayerFarming.Instance.playerController.transform.position =
            interaction.follower.Brain.CurrentTask.GetDestination(interaction.follower) +
            new Vector3(0.4f, 0f, 0.4f);
        PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
        PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "dance", true);
        interaction.follower.FacePosition(PlayerFarming.Instance.playerController.transform.position);
        PlayerFarming.Instance.state.facingAngle = global::Utils.GetAngle(PlayerFarming.Instance.transform.position,
            interaction.follower.transform.position);
        yield return new WaitForEndOfFrame();
        interaction.follower.TimedAnimation("dance", 5f);
        AudioManager.Instance.SetFollowersDance(1f);
        yield return new WaitForSeconds(5f);
        AudioManager.Instance.SetFollowersDance(0f);
        PlayerFarming.Instance.unitObject.StoppingDistance = oldStoppingDistance;
        interaction.Close(true, reshowMenu: false);
    }
}