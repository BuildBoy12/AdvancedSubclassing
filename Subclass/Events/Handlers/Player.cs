// <copyright file="Player.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Events.Handlers
{
    using Exiled.Events.Extensions;
    using Subclass.Events.EventArgs;
    using static Exiled.Events.Events;

    public static class Player
    {
        public static event CustomEventHandler<ReceivingSubclassEventArgs> ReceivingSubclass;

        public static event CustomEventHandler<ReceivedSubclassEventArgs> ReceivedSubclass;

        public static void OnReceivingSubclass(ReceivingSubclassEventArgs ev) => ReceivingSubclass.InvokeSafely(ev);

        public static void OnReceivedSubclass(ReceivedSubclassEventArgs ev) => ReceivedSubclass.InvokeSafely(ev);
    }
}