namespace eTimeTrack.Models
{
    interface ITrackableModel
    {
        string GetId();
        string ToJson();
    }
}