﻿using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HarSharp
{
    /// <summary>
    /// Provides methods for converting between HTTP Archive Format (HAR) and HAR entities.
    /// </summary>
    public static class HarConvert
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Deserialize HAR content to a HAR entity.
        /// </summary>
        /// <param name="har">The HAR content to be deserialized.</param>
        /// <returns>The HAR entity.</returns>
        public static Har Deserialize(string harJson)
        {
            if (string.IsNullOrWhiteSpace(harJson))
            {
                throw new ArgumentNullException(nameof(harJson));
            }

            var result = JsonSerializer.Deserialize<Har>(harJson, _options);

            TransformPartialRedirectUrlToFull(result);

            return result;
        }

        /// <summary>
        /// Deserialize a HAR file to a HAR entity.
        /// </summary>
        /// <param name="fileName">The HAR file name to be deserialized.</param>
        /// <returns>The HAR entity.</returns>
        public static Har DeserializeFromFile(string fileName)
        {
            using var stream = File.OpenRead(fileName);
            var result = JsonSerializer.Deserialize<Har>(stream, _options);

            TransformPartialRedirectUrlToFull(result);

            return result;
        }

        /// <summary>
        /// Transform the partial redirect URL to a full one.
        /// </summary>
        /// <param name="har">The HAR.</param>
        private static void TransformPartialRedirectUrlToFull(Har har)
        {
            var responsesWithPartialRedirectUrl = har.Log.Entries
                .Where(e => e.Response.RedirectUrl != null && e.Response.RedirectUrl.OriginalString.StartsWith("/", StringComparison.OrdinalIgnoreCase));

            foreach (var entry in responsesWithPartialRedirectUrl)
            {
                var requestUrl = entry.Request.Url;
                entry.Response.RedirectUrl = new Uri($"{requestUrl.GetLeftPart(UriPartial.Authority)}{(entry.Response.RedirectUrl.IsAbsoluteUri ? entry.Response.RedirectUrl.AbsolutePath : entry.Response.RedirectUrl.OriginalString)}");
            }
        }        
    }
}
