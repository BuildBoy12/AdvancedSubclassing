namespace Subclass.Events.Handlers
{
    using EventArgs;
    using Exiled.Events.Extensions;
    using static Exiled.Events.Events;

    public static class Player
    {
        public static event CustomEventHandler<ReceivingSubclassEventArgs> ReceivingSubclass;

        public static event CustomEventHandler<ReceivedSubclassEventArgs> ReceivedSubclass;

        public static void OnReceivingSubclass(ReceivingSubclassEventArgs ev) => ReceivingSubclass.InvokeSafely(ev);

        public static void OnReceivedSubclass(ReceivedSubclassEventArgs ev) => ReceivedSubclass.InvokeSafely(ev);
    }
}