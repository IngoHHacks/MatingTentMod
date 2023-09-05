using COTL_API.CustomTasks;
using System;
using UnityEngine;

namespace MatingTentMod.Tasks;

public class MateFollow : CustomTask
{
    private Follower _follower;
    private readonly FollowerBrain _linkedBrain;

    private Structure _structure;
    private int _timeNotMating;
    private float delay;

    private bool mating;

    public MateFollow(FollowerBrain linkedBrain)
    {
        this._linkedBrain = linkedBrain;
    }

    public override string InternalName => "MATE_FOLLOW";

    public override FollowerLocation Location => GetStructure().Brain.Data.Location;

    public override bool BlockReactTasks => true;
    public override bool BlockTaskChanges => true;
    public override bool BlockSocial => true;
    public override bool DisablePickUpInteraction => true;


    private Structure GetStructure()
    {
        if (this._structure != null)
        {
            return this._structure;
        }

        return this._structure = ((MateLead)this._linkedBrain.CurrentTask).LinkedStructure;
    }

    public override void TaskTick(float deltaGameTime)
    {
        try
        {
            if (!this.mating && (this.delay > 0 || (this.State == FollowerTaskState.Doing &&
                                                    this._linkedBrain?.CurrentTask is MateLead &&
                                                    this._linkedBrain.CurrentTask.State == FollowerTaskState.Doing)))
            {
                if (this.delay < 1)
                {
                    this.delay += deltaGameTime;
                }
                else
                {
                    this.mating = true;
                    this._follower.FacePosition(this._linkedBrain.LastPosition);
                    this._follower.TimedAnimation("dance", 5f, Complete);
                }
            }
            else if (!this.mating)
            {
                if (this._linkedBrain.CurrentTask is not MateLead)
                {
                    this._timeNotMating += 1;
                    if (this._timeNotMating >= 3)
                    {
                        End();
                    }
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
        this.delay = 0;
    }
}