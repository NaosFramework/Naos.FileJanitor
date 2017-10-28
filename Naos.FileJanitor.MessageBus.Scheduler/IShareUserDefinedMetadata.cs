﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareUserDefinedMetadata.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.FileJanitor.MessageBus.Scheduler
{
    using Naos.FileJanitor.Domain;
    using Naos.MessageBus.Domain;

    /// <summary>
    /// Interface to support sharing metadata between handlers and future messages.
    /// </summary>
    public interface IShareUserDefinedMetadata : IShare
    {
        /// <summary>
        /// Gets or sets user defined meta data to save with the file.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Has to be an array right now for sharing.")]
        MetadataItem[] UserDefinedMetadata { get; set; }
    }
}
