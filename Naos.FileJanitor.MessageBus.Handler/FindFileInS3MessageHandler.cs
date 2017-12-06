﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FindFileInS3MessageHandler.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.FileJanitor.MessageBus.Handler
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Its.Configuration;
    using Its.Log.Instrumentation;

    using Naos.AWS.S3;
    using Naos.FileJanitor.Core;
    using Naos.FileJanitor.Domain;
    using Naos.FileJanitor.MessageBus.Scheduler;
    using Naos.MessageBus.Domain;

    using Spritely.Recipes;
    using Spritely.Redo;

    using static System.FormattableString;

    /// <summary>
    /// Message handler to fetch a file from S3.
    /// </summary>
    public class FindFileInS3MessageHandler : MessageHandlerBase<FindFileMessage>, IShareFileLocation
    {
        /// <inheritdoc />
        public override async Task HandleAsync(FindFileMessage message)
        {
            if (string.IsNullOrEmpty(message.ContainerLocation))
            {
                throw new ApplicationException("Must specify region (container location).");
            }

            if (string.IsNullOrEmpty(message.Container))
            {
                throw new ApplicationException("Must specify bucket name (container).");
            }

            var settings = Settings.Get<FileJanitorMessageHandlerSettings>();
            await this.HandleAsync(message, settings);
        }

        /// <summary>
        /// Handles a <see cref="FindFileMessage"/>.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="settings">Needed settings to handle messages.</param>
        /// <returns>Task to support async await execution.</returns>
        public async Task HandleAsync(FindFileMessage message, FileJanitorMessageHandlerSettings settings)
        {
            new { message }.Must().NotBeNull().OrThrowFirstFailure();
            new { settings }.Must().NotBeNull().OrThrowFirstFailure();

            var correlationId = Guid.NewGuid().ToString().ToUpperInvariant();
            var containerLocation = message.ContainerLocation;
            var container = message.Container;
            var downloadAccessKey = settings.DownloadAccessKey;
            var downloadSecretKey = settings.DownloadSecretKey;
            var keyPrefixSearchPattern = message.KeyPrefixSearchPattern;
            var multipleKeysFoundStrategy = message.MultipleKeysFoundStrategy;

            Log.Write(() => Invariant($"Starting Find File; CorrelationId: {correlationId}, ContainerLocation/Region: {containerLocation}, Container/BucketName: {container}, KeyPrefixSearchPattern: {keyPrefixSearchPattern}, MultipleKeysFoundStrategy: {multipleKeysFoundStrategy}"));
            using (var log = Log.Enter(() => new { CorrelationId = correlationId }))
            {
                var fileManager = new FileManager(downloadAccessKey, downloadSecretKey);

                // share the results
                this.FileLocation = await FileExchanger.FindFile(fileManager, containerLocation, container, keyPrefixSearchPattern, multipleKeysFoundStrategy);

                log.Trace(() => "Completed finding file");
            }
        }

        /// <inheritdoc />
        public FileLocation FileLocation { get; set; }
    }
}
