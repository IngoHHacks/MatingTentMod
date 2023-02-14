using COTL_API.CustomTasks;
using COTL_API.Helpers;
using DG.Tweening;
using MatingTentMod.Structures;
using MatingTentMod.Utils;
using Spine;
using Spine.Unity;
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
            
            
            FollowerBrain parent1 = this._follower.Brain;
            FollowerBrain parent2 = this._linkedBrain;
            
            List<FollowerTrait.TraitType> traits = new();
            traits.AddRange(parent1.Info.Traits);
            int numTraits1 = traits.Count;
            traits.AddRange(parent2.Info.Traits);
            int numTraits2 = traits.Count - numTraits1;
            
            int numTraits = Random.Range(numTraits1, numTraits2 + 1);
            
            traits = traits.OrderBy(x => Random.value).ToList();

            List<FollowerTrait.TraitType> selectedTraits = new();
            foreach (FollowerTrait.TraitType trait in traits)
            {
                if (selectedTraits.Count >= numTraits) break;
                if (selectedTraits.Contains(trait)) continue;
                if (selectedTraits.Any(x => FollowerTrait.ExclusiveTraits.ContainsKey(x) && FollowerTrait.ExclusiveTraits[x] == trait)) continue;
                selectedTraits.Add(trait);
            }
            
            float fLevel = (100 * parent1.Info.XPLevel - 1) +  parent1.Stats.Adoration + (100 * parent2.Info.XPLevel - 1) + parent2.Stats.Adoration;
            fLevel /= 4;
            float adoration = fLevel % 100;
            int level = ((int) fLevel / 100) + 1;

            string skin = Random.Range(0, 2) == 0 ? parent1.Info.SkinName : parent2.Info.SkinName;
            
            Follower f = FollowerManager.CreateNewFollower(PlayerFarming.Location, _follower.transform.position);
            f.Brain.Info.Outfit = FollowerOutfitType.Follower;
            f.SetOutfit(FollowerOutfitType.Follower, hooded: false);
            LerpFollower(f.gameObject);
            f.Brain.HardSwapToTask(new FollowerTask_ManualControl());
            f.Brain.ApplyCurseState(CustomCursedStates.CHILD);
            f.TimedAnimation("idle", 60f, base.End, Loop: true);
            StructureBrain structureByID = GetStructure().Brain;
            structureByID.ReservedForTask = false;
            f.Brain.Info.Traits.Clear();
            selectedTraits.ForEach(trait => f.AddTrait(trait));
            if (Random.Range(0, 10) != 0)
            {
                f.Brain.Info.SkinName = skin;
                f.Brain.Info.SkinCharacter = WorshipperData.Instance.GetSkinIndexFromName(skin);
                f.Brain.Info.SkinColour = Random.Range(0, WorshipperData.Instance.GetColourData(f.Brain.Info.SkinName).StartingSlotAndColours.Count);
                f.Brain.Info.SkinVariation = Random.Range(0, WorshipperData.Instance.Characters[f.Brain.Info.SkinCharacter].Skin.Count);
                Skin spineSkin = f.Spine.Skeleton.Data.FindSkin(f.Brain.Info.SkinName);
                string outfitSkinName = f.Outfit.GetOutfitSkinName(f.Brain.Info.Outfit);
                spineSkin.AddSkin(f.Spine.Skeleton.Data.FindSkin(outfitSkinName));
                f.Spine.Skeleton.SetSkin(skin);
                foreach (WorshipperData.SlotAndColor slotAndColour in WorshipperData.Instance.GetColourData(f.Brain.Info.SkinName).SlotAndColours[f.Brain.Info.SkinColour].SlotAndColours)
                {
                    f.Spine.skeleton.FindSlot(slotAndColour.Slot)?.SetColor(slotAndColour.color);
                }
            }
            f.Brain.Info.Age = 0;
            f.Brain.Stats.Adoration = adoration;
            f.Brain.Info.XPLevel = level;
        }
    }
    
    private void LerpFollower(GameObject follower)
    {
        Vector3 position = follower.transform.position;
        position = new Vector3(position.x + 1f - (Random.value * 2f), position.y, position.z + 1f - (Random.value * 2f));
        follower.transform.localScale = Vector3.one * 0.5f;
        follower.transform.DOMove(position, 0.25f).SetEase(Ease.OutSine);
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