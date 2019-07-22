namespace FleetTrack.Core.Interfaces
{
    public interface INotifier
    {
        void Notify(string subject, string message);
    }
}
