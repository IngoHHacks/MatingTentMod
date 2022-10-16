using COTL_API.CustomTasks;
using System;
using UnityEngine;

namespace MatingTentMod.Tasks;

public class MateFollow : CustomTask
{
    public override string InternalName => "MATE_FOLLOW";
    
    private Structure _structure;

    private Follower _follower;
    private FollowerBrain _linkedBrain;
    private float delay;

    private bool mating = false;
    private int _timeNotMating = 0;

    public override FollowerLocation Location => GetStructure().Brain.Data.Location;

    public override bool BlockReactTasks => true;
    public override bool BlockTaskChanges => true;
    public override bool BlockSocial => true;

    public MateFollow(FollowerBrain linkedBrain)
    {
        this._linkedBrain = linkedBrain;
    }


    private Structure GetStructure()
    {
        if (_structure != null) return _structure;
        else return _structure = ((MateLead) this._linkedBrain.CurrentTask).LinkedStructure;
    }

    public override void TaskTick(float deltaGameTime)
    {
        try
        {
            if (!mating && (delay > 0 || (State == FollowerTaskState.Doing && _linkedBrain?.CurrentTask is MateLead &&
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
                    _follower.TimedAnimation("dance", 5f, Complete);
                }
            }
            else if (!this.mating)
            {
                if (this._linkedBrain.CurrentTask is not MateLead)
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

    public override Vector3 UpdateDestination(Follower follower)
    {
        StructureBrain structureByID = GetStructure().Brain;
        Vector3 pos = structureByID.Data.Position;
        return new Vector3(pos.x + 0.2f, pos.y, pos.z + 0.2f);
    }


    public override void OnStart()
    {
        SetState(FollowerTaskState.GoingTo);
        delay = 0;
    }
}