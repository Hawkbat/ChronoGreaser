using UnityEngine;

public class ChronalEngineControlsController : MonoBehaviour
{
    [SerializeField] SliderControl timeLinePositionSlider;
    [SerializeField] SliderControl timeLineSpeedSlider;

    bool timelinePositionChanging = false;

    void Update()
    {
        timeLinePositionSlider.SetMinMax(0f, TimeLoop.TotalDuration);
        timeLinePositionSlider.Locked = !TimeLoop.IsPlaying;
        if (timeLinePositionSlider.IsInteracting)
        {
            timelinePositionChanging = true;
        }
        else
        {
            if (timelinePositionChanging)
            {
                timelinePositionChanging = false;
                TimeLoop.SetTargetTime(timeLinePositionSlider.Value);
            }
            timeLinePositionSlider.Value = TimeLoop.CurrentTime;
        }
        timeLineSpeedSlider.Locked = !TimeLoop.IsPlaying;
        if (timeLineSpeedSlider.IsInteracting)
        {
            TimeLoop.SetTimeScale(timeLineSpeedSlider.Value);
        }
        else
        {
            if (TimeLoop.IsPlaying)
                timeLineSpeedSlider.Value = TimeLoop.RawTimeScale;
            else
                timeLineSpeedSlider.Value = TimeLoop.TimeScale;
        }
    }
}
