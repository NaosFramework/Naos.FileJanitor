﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareFileMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.FileJanitor.MessageBus.Handler
{
    using Its.Log.Instrumentation;

    using Naos.FileJanitor.MessageBus.Contract;
    using Naos.MessageBus.HandlingContract;

    /// <summary>
    /// Message handler to share the provided file path with remaining messages.
    /// </summary>
    public class ShareFileMessageHandler : IHandleMessages<ShareFileMessage>, IShareFilePath
    {
        /// <inheritdoc />
        public void Handle(ShareFileMessage message)
        {
            using (var log = Log.Enter(() => new { Message = message, FilePathToShare = message.FilePathToShare }))
            {
                log.Trace(() => "Sharing file: " + message.FilePathToShare);
                this.FilePath = message.FilePathToShare;
            }
        }

        /// <inheritdoc />
        public string FilePath { get; set; }
    }
}
