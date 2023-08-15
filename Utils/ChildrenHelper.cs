using DG.Tweening;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MatingTentMod.Utils;

[HarmonyPatch]
public class ChildrenHelper
{
    private static float AGING = 7.5f;
    private static float AG_TIME = AGING / 2.5f;
    
    private static float _time = 0;
    
    [HarmonyPatch(typeof(TimeManager), "Simulate", typeof(float))]
    [HarmonyPostfix]
    private static void PlacementRegion_GetHoveredStructure(float deltaGameTime)
    {
        _time += deltaGameTime;
        if (_time >= AGING)
        {
            _time -= AGING;
            UpdateAllChildren();
        }
    }

    public static void UpdateAllChildren()
    {
        foreach (Follower follower in FollowerManager.FollowersAtLocation(FollowerLocation.Base).Where(f => f.Brain.Info.CursedState == CustomCursedStates.CHILD))
        {
            try
            {
                if (follower.Brain.Info.Age < 18)
                {
                    follower.transform.localScale = Vector3.one * (0.5f + (follower.Brain.Info.Age * 0.0277778f));
                    follower.Brain.Info.Age += 1;
                    follower.transform.DOScale(Vector3.one * (0.5f + (follower.Brain.Info.Age * 0.0277778f)), AG_TIME)
                        .SetEase(Ease.OutSine);
                }
                else
                {
                    follower.transform.localScale = Vector3.one;
                    follower.TimedAnimation("Reactions/react-happy1", 2.9f, Loop: false, onComplete: delegate
                    {
                        follower.Brain.RemoveCurseState(CustomCursedStates.CHILD);
                        follower.Brain.HardSwapToTask(new FollowerTask_Idle());
                    });
                }
            } catch (Exception all) { /* ignore */ }
        }
    }
}