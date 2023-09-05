using MatingTentMod.Tasks;

namespace MatingTentMod.Utils;

public class MateActions
{
    public static void BeginMate(FollowerBrain brain1, FollowerBrain brain2)
    {
        if (brain2 != null)
        {
            MateLead lead = new(brain2);
            brain1.ClearPersonalOverrideTaskProvider();
            brain1.OverrideTaskCompleted = false;
            brain1.CurrentOverrideTaskType = lead.Type;
            brain1.OverrideDayIndex = TimeManager.CurrentDay;
            brain1.ShouldReconsiderTask = false;
            MateFollow follow = new(brain1);
            brain2.ClearPersonalOverrideTaskProvider();
            brain2.OverrideTaskCompleted = false;
            brain2.CurrentOverrideTaskType = follow.Type;
            brain2.OverrideDayIndex = TimeManager.CurrentDay;
            brain2.ShouldReconsiderTask = false;
            brain1.HardSwapToTask(lead);
            brain2.HardSwapToTask(follow);
        }
        else
        {
            MateWithLamb lambMate = new();
            brain1.ClearPersonalOverrideTaskProvider();
            brain1.OverrideTaskCompleted = false;
            brain1.CurrentOverrideTaskType = lambMate.Type;
            brain1.OverrideDayIndex = TimeManager.CurrentDay;
            brain1.ShouldReconsiderTask = false;
            brain1.HardSwapToTask(lambMate);
        }
    }
}