using COTL_API.CustomTasks;
using COTL_API.Helpers;
using DG.Tweening;
using MatingTentMod.Structures;
using MatingTentMod.Utils;
using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Event = Spine.Event;
using Random = UnityEngine.Random;

namespace MatingTentMod.Tasks;

public class MateWithLamb : CustomTask
{
    private Follower _follower;

    private bool _hasMated;

    private Structure _structure;
    private float delay;

    private bool mating;
    private float progress;
    public override string InternalName => "MATE_LAMB";

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

        return this._structure = TaskUtils.GetAvailableStructureOfType<MatingTent>();
    }

    public override void TaskTick(float deltaGameTime)
    {
        try
        {
            if (!this.mating && (this.delay > 0 || this.State == FollowerTaskState.Doing) &&
                !PlayerFarming.Instance.GoToAndStopping)
            {
                if (this.delay < 1)
                {
                    this.delay += deltaGameTime;
                }
                else
                {
                    this.mating = true;
                    this._follower.FacePosition(PlayerFarming.Instance.playerController.transform.position);
                    this._follower.Spine.AnimationState.Event += CreateFollower;
                    this._follower.TimedAnimation("poop", 1.5333333f, delegate
                    {
                        this._follower.Spine.AnimationState.Event -= CreateFollower;
                        this._follower.TimedAnimation("Reactions/react-embarrassed", 3f, End, false);
                    });
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

            List<FollowerTrait.TraitType> traits = new();
            traits.AddRange(parent1.Info.Traits);
            int numTraits = traits.Count;

            traits = traits.OrderBy(x => Random.value).ToList();

            List<FollowerTrait.TraitType> selectedTraits = new();
            foreach (FollowerTrait.TraitType trait in traits)
            {
                if (selectedTraits.Count >= numTraits)
                {
                    break;
                }

                if (selectedTraits.Contains(trait))
                {
                    continue;
                }

                if (selectedTraits.Any(x =>
                        FollowerTrait.ExclusiveTraits.ContainsKey(x) && FollowerTrait.ExclusiveTraits[x] == trait))
                {
                    continue;
                }

                selectedTraits.Add(trait);
            }

            float fLevel = (100 * parent1.Info.XPLevel) - 1 + parent1.Stats.Adoration;
            fLevel /= 2;
            float adoration = fLevel % 100;
            int level = ((int)fLevel / 100) + 1;

            string skin = parent1.Info.SkinName;

            Follower f = FollowerManager.CreateNewFollower(PlayerFarming.Location, this._follower.transform.position);
            f.Brain.Info.Outfit = FollowerOutfitType.Follower;
            f.SetOutfit(FollowerOutfitType.Follower, false);
            LerpFollower(f.gameObject);
            f.Brain.HardSwapToTask(new FollowerTask_ManualControl());
            f.Brain.ApplyCurseState(CustomCursedStates.CHILD);
            f.TimedAnimation("idle", 60f, End, true);
            StructureBrain structureByID = GetStructure().Brain;
            structureByID.ReservedForTask = false;
            f.Brain.Info.Traits.Clear();
            selectedTraits.ForEach(trait => f.AddTrait(trait));
            if (Random.Range(0, 10) != 0)
            {
                f.Brain.Info.SkinName = skin;
                f.Brain.Info.SkinCharacter = WorshipperData.Instance.GetSkinIndexFromName(skin);
                f.Brain.Info.SkinColour = Random.Range(0,
                    WorshipperData.Instance.GetColourData(f.Brain.Info.SkinName).StartingSlotAndColours.Count);
                f.Brain.Info.SkinVariation = Random.Range(0,
                    WorshipperData.Instance.Characters[f.Brain.Info.SkinCharacter].Skin.Count);
                Skin spineSkin = f.Spine.Skeleton.Data.FindSkin(f.Brain.Info.SkinName);
                string outfitSkinName = f.Outfit.GetOutfitSkinName(f.Brain.Info.Outfit);
                spineSkin.AddSkin(f.Spine.Skeleton.Data.FindSkin(outfitSkinName));
                f.Spine.Skeleton.SetSkin(skin);
                foreach (WorshipperData.SlotAndColor slotAndColour in WorshipperData.Instance
                             .GetColourData(f.Brain.Info.SkinName).SlotAndColours[f.Brain.Info.SkinColour]
                             .SlotAndColours)
                {
                    f.Spine.skeleton.FindSlot(slotAndColour.Slot)?.SetColor(slotAndColour.color);
                }
            }

            f.Brain.Info.Age = 0;
            f.Brain.Stats.Adoration = adoration;
            f.Brain.Info.XPLevel = level;
            f.Brain.HardSwapToTask(new FollowerTask_FakeLeisure());
        }
    }

    private void LerpFollower(GameObject follower)
    {
        Vector3 position = follower.transform.position;
        position = new Vector3(position.x + 1f - (Random.value * 2f), position.y,
            position.z + 1f - (Random.value * 2f));
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
        this.progress = 0;
    }
}