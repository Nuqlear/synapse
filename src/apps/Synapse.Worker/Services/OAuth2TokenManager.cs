﻿/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

namespace Synapse.Worker.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IOAuth2TokenManager"/> interface
    /// </summary>
    public class OAuth2TokenManager
        : IOAuth2TokenManager
    {

        /// <summary>
        /// Initializes a new <see cref="OAuth2TokenManager"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="jsonSerializer">The service used to serialize/deserialize to/from JSON</param>
        /// <param name="httpClient">The service used to perform HTTP requests</param>
        public OAuth2TokenManager(ILogger<OAuth2TokenManager> logger, IJsonSerializer jsonSerializer, HttpClient httpClient)
        {
            this.Logger = logger;
            this.JsonSerializer = jsonSerializer;
            this.HttpClient = httpClient;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to serialize/deserialize to/from JSON
        /// </summary>
        protected IJsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Gets the service used to perform HTTP requests
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey, TValue}"/> containing all active <see cref="OAuth2Token"/>s
        /// </summary>
        protected Dictionary<string, OAuth2Token> Tokens { get; } = new();

        /// <inheritdoc/>
        public virtual async Task<OAuth2Token> GetTokenAsync(OAuth2AuthenticationProperties oauthProperties, CancellationToken cancellationToken = default)
        {
            if (oauthProperties == null)
                throw new ArgumentNullException(nameof(oauthProperties));
            var tokenKey = $"{oauthProperties.ClientId}@{oauthProperties.Authority}";
            var properties = oauthProperties.ToDictionary();
            if(this.Tokens.TryGetValue(tokenKey, out var token)
                && token != null)
            {
                if (token.HasExpired
                    && !string.IsNullOrWhiteSpace(token.RefreshToken))
                {
                    properties["grant_type"] = "refresh_token";
                    properties["refresh_token"] = token.RefreshToken;
                }
                else
                {
                    return token;
                }
            }
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(oauthProperties.Authority, "/connect/token/"))
            {
                Content = new FormUrlEncodedContent(properties)
            };
            using var response = await this.HttpClient.SendAsync(request, cancellationToken);
            var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
            if (!response.IsSuccessStatusCode)
            {
                this.Logger.LogError("An error occured while generating a new JWT token: {details}", json);
                response.EnsureSuccessStatusCode();
            }
            token = await this.JsonSerializer.DeserializeAsync<OAuth2Token>(json, cancellationToken);
            this.Tokens[tokenKey] = token;
            return token;
        }

    }

}
