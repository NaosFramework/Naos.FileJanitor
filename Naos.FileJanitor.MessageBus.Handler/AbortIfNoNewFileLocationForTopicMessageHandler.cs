﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfNoNewFileLocationForTopicMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.FileJanitor.MessageBus.Handler
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Its.Log.Instrumentation;

    using Naos.FileJanitor.MessageBus.Contract;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    /// <summary>
    /// Message handler for <see cref="AbortIfNoNewFileLocationForTopicMessage"/>.
    /// </summary>
    public class AbortIfNoNewFileLocationForTopicMessageHandler : IHandleMessages<AbortIfNoNewFileLocationForTopicMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(AbortIfNoNewFileLocationForTopicMessage message)
        {
            if (message.FileLocation == null)
            {
                throw new ArgumentException("Must provide a file location.");
            }

            if (message.TopicStatusReports == null)
            {
                throw new ArgumentException("Must supply topic status reports or empty collection.");
            }

            if (message.TopicToCheckAffectedItemsFor == null)
            {
                throw new ArgumentException("Must supply topic to check.");
            }

            var correlationId = await Task.Run(() => Guid.NewGuid().ToString().ToUpperInvariant());
            Log.Write(() => $"Starting Abort if no change in file location; CorrelationId: {correlationId}, ContainerLocation: {message.FileLocation.ContainerLocation}, Container: {message.FileLocation.Container}, Key: {message.FileLocation.Key}, Topic: {message.TopicToCheckAffectedItemsFor}");
            using (var log = Log.Enter(() => new { CorrelationId = correlationId }))
            {
                // get status report
                var matchingReport = message.TopicStatusReports.SingleOrDefault(_ => _.Topic.ToNamedTopic() == message.TopicToCheckAffectedItemsFor);
                if (matchingReport == null)
                {
                    log.Trace(() => $"Did not find matching reports for topic: {message.TopicToCheckAffectedItemsFor.Name}");
                }
                else
                {
                    log.Trace(() => $"Found matching reports for topic: {message.TopicToCheckAffectedItemsFor.Name} with affects completed on: {matchingReport.AffectsCompletedDateTimeUtc}");
                    var searchToken = nameof(FileLocationAffectedItem.FileLocationAffectedItemMessage);
                    var matchingAffectedItem = matchingReport.AffectedItems?.SingleOrDefault(_ => (_.Id ?? string.Empty).ToUpperInvariant().Contains(searchToken.ToUpperInvariant()));
                    if (matchingAffectedItem == null)
                    {
                        log.Trace(() => $"Did not fine any affected items with expected token: {searchToken}");
                    }
                    else
                    {
                        log.Trace(() => $"Found affected item: {matchingAffectedItem}");
                        var previousFileLocation = matchingAffectedItem.Id.FromJson<FileLocationAffectedItem>().FileLocation;
                        if (message.FileLocation == previousFileLocation)
                        {
                            throw new AbortParcelDeliveryException($"Found that the affected items for affects complete {matchingReport.AffectsCompletedDateTimeUtc} matched specified file location.");
                        }
                        else
                        {
                            log.Trace(() => "Affected items did not match, NOT aborting.");
                        }
                    }
                }
            }
        }
    }
}
