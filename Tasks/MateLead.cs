using COTL_API.CustomTasks;
using COTL_API.Helpers;
using DG.Tweening;
using MatingTentMod.Structures;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Event = Spine.Event;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MatingTentMod.Tasks;

public class MateLead : CustomTask
{
    public override string InternalName => "MATE_LEAD";
    
    private Structure _structure;
    private float progress;

    private Follower _follower;
    private FollowerBrain _linkedBrain;
    private float delay;

    private bool mating = false;
    private int _timeNotMating = 0;
    
    private bool _hasMated = false;

    public override FollowerLocation Location => GetStructure().Brain.Data.Location;

    public override bool BlockReactTasks => true;
    public override bool BlockTaskChanges => true;
    public override bool BlockSocial => true;

    public MateLead(FollowerBrain linkedBrain)
    {
        this._linkedBrain = linkedBrain;
    }
    
    private Structure GetStructure()
    {
        if (_structure != null) return _structure;
        return _structure = TaskUtils.GetAvailableStructureOfType<MatingTent>();
    }

    public Structure LinkedStructure => GetStructure();

    public override void TaskTick(float deltaGameTime)
    {
        try
        {
            if (!mating && (delay > 0 || (State == FollowerTaskState.Doing && _linkedBrain?.CurrentTask is MateFollow &&
                                          _linkedBrain.CurrentTask.State == FollowerTaskState.Doing)))
            {
                if (delay < 1)
                {
                    delay += deltaGameTime;
                }
                else
                {
                    mating = true;
                    _follower.FacePosition(_linkedBrain.LastPosition);
                    _follower.TimedAnimation("dance", 5f, delegate
                    {
                        _follower.Spine.AnimationState.Event += CreateFollower;
                        _follower.TimedAnimation("poop", 1.5333333f, delegate
                        {
                            _follower.Spine.AnimationState.Event -= CreateFollower;
                            _follower.TimedAnimation("Reactions/react-embarrassed", 3f, base.End, Loop: false);
                        });
                    });
                }
            }
            else if (!this.mating)
            {
                if (this._linkedBrain.CurrentTask is not MateFollow)
                {
                    _timeNotMating += 1;
                    if (_timeNotMating >= 3) this.End();
                }
            }
        }
        catch (Exception e)
        {
            End();
        }
    }

    private void CreateFollower(TrackEntry trackentry, Event e)
    {
        string name = e.Data.Name;
        if (name == "Poop")
        {
            this._hasMated = true;
            Follower f = FollowerManager.CreateNewFollower(PlayerFarming.Location, _follower.transform.position);
            f.Brain.Info.Outfit = FollowerOutfitType.Follower;
            f.SetOutfit(FollowerOutfitType.Follower, hooded: false);
            LerpFollower(f.gameObject);
            f.Brain.HardSwapToTask(new FollowerTask_ManualControl());
            f.TimedAnimation("idle", 60f, base.End, Loop: true);
            f.StartCoroutine(FollowerGrow(f));

        }
    }

    private IEnumerator FollowerGrow(Follower follower)
    {
        yield return new WaitForSeconds(60f);
        try
        {
            follower.TimedAnimation("Reactions/react-happy1", 3f, Loop: false, onComplete: delegate
            {
                FollowerManager.CreateNewRecruit(follower.Brain.Info._info, follower.transform.position);
                NotificationCentre.Instance.PlayFollowerNotification(NotificationCentre.NotificationType.NewRecruit, follower.Brain.Info, NotificationFollower.Animation.Normal);
                FollowerManager.RemoveFollower(follower.Brain.Info.ID);
                follower.Brain.Cleanup();
                follower.Brain.ClearDwelling();
                if ((bool)follower.gameObject)
                {
                    Object.Destroy(follower.gameObject);
                }
            });
        }
        catch (Exception e)
        {
            // ignored
        }
        StructureBrain structureByID = GetStructure().Brain;
        structureByID.ReservedForTask = false;
    }

    
    private void LerpFollower(GameObject follower)
    {
        Vector3 position = follower.transform.position;
        position = new Vector3(position.x + 1f - (Random.value * 2f), position.y, position.z + 1f - (Random.value * 2f));
        follower.transform.localScale = Vector3.one * 0.5f;
        follower.transform.DOMove(position, 0.25f).SetEase(Ease.OutSine);
        follower.transform.DOScale(1f, 60f).SetEase(Ease.OutSine);
    }


    public override void OnDoingBegin(Follower follower)
    {
        base.OnDoingBegin(follower);
        this._follower = follower;
    }

    public override void ClaimReservations()
    {
        StructureBrain structureByID = GetStructure().Brain;
        structureByID.ReservedForTask = true;
    }
    
    public override void ReleaseReservations()
    {
        if (!this._hasMated)
        {
            StructureBrain structureByID = GetStructure().Brain;
            structureByID.ReservedForTask = false;
        }
    }

    public override Vector3 UpdateDestination(Follower follower)
    {
        StructureBrain structureByID = GetStructure().Brain;
        Vector3 pos = structureByID.Data.Position;
        return new Vector3(pos.x - 0.2f, pos.y, pos.z - 0.2f);
    }


    public override void OnStart()
    {
        SetState(FollowerTaskState.GoingTo);
        progress = 0;
    }
}