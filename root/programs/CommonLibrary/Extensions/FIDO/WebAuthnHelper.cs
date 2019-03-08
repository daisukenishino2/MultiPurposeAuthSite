﻿//**********************************************************************************
//* Copyright (C) 2017 Hitachi Solutions,Ltd.
//**********************************************************************************

#region Apache License
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

//**********************************************************************************
//* クラス名        ：WebAuthnHelper
//* クラス日本語名  ：WebAuthnHelper（ライブラリ）
//*
//* 作成日時        ：－
//* 作成者          ：－
//* 更新履歴        ：－
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2019/03/07  西野 大介         新規
//**********************************************************************************

using MultiPurposeAuthSite.Co;
using MultiPurposeAuthSite.Util;

using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Fido2NetLib;
using Fido2NetLib.Objects;
using Fido2NetLib.Development;
using static Fido2NetLib.Fido2;

namespace MultiPurposeAuthSite.Extensions.FIDO
{
    /// <summary>
    /// WebAuthnHelper（ライブラリ）
    /// https://github.com/abergs/fido2-net-lib/blob/master/Fido2Demo/Controller.cs
    /// </summary>
    public class WebAuthnHelper
    {
        /// <summary>
        /// 開発時のストレージ
        /// ・User
        /// ・StoredCredential
        ///   - byte[] UserId
        ///   - PublicKeyCredentialDescriptor Descriptor
        ///     - byte[] Id (CredentialId)
        ///     - enum PublicKeyCredentialType? Type
        ///     - enum AuthenticatorTransport[] Transports
        ///   - byte[] PublicKey
        ///   - byte[] UserHandle = UserId
        ///   - uint SignatureCounter
        ///   - string CredType
        ///   - DateTime RegDate
        ///   - Guid AaGuid
        /// </summary>
        /// <see cref="https://techinfoofmicrosofttech.osscons.jp/index.php?fido2-net-lib#sabce498"/>
        private static readonly DevelopmentInMemoryStore DemoStorage = new DevelopmentInMemoryStore();

        //private StoredCredential sc = new StoredCredential();
        //private PublicKeyCredentialDescriptor pd = new PublicKeyCredentialDescriptor();
        //private PublicKeyCredentialType pc = new PublicKeyCredentialType();
        //private AuthenticatorTransport at = new AuthenticatorTransport();
        //private AttestationVerificationSuccess avs = new AttestationVerificationSuccess();

        #region mem & prop & constructor

        #region mem & prop

        /// <summary>
        /// fido2-net-lib
        /// https://techinfoofmicrosofttech.osscons.jp/index.php?fido2-net-lib
        /// </summary>
        private Fido2 _lib;

        /// <summary>
        /// FIDO Alliance MetaData Service
        /// https://techinfoofmicrosofttech.osscons.jp/index.php?FIDO%E8%AA%8D%E8%A8%BC%E5%99%A8#d6659b25
        /// </summary>
        private IMetadataService _mds;

        /// <summary>
        /// Origin of the website: "http(s)://..."
        /// </summary>
        private string _origin;

        #endregion

        #region constructor

        /// <summary>constructor</summary>
        public WebAuthnHelper()
        {
            // this._mds = MDSMetadata.Instance("accesskey", "cachedirPath");
            this._origin = Config.OAuth2AuthorizationServerEndpointsRootURI;
            
            Uri uri = new Uri(this._origin);
            this._lib = new Fido2(new Configuration()
            {
                ServerDomain = uri.GetDomain(),
                ServerName = uri.GetHost(),
                Origin = this._origin,
                // Only create and use Metadataservice if we have an acesskey
                MetadataService = this._mds
            });
        }

        #endregion

        #endregion

        #region methods

        #region 登録フロー

        /// <summary>CredentialCreationOptions</summary>
        /// <param name="username">string</param>
        /// <param name="attType">string</param>
        /// <param name="authType">string</param>
        /// <param name="requireResidentKey">string</param>
        /// <param name="userVerification">string</param>
        public CredentialCreateOptions CredentialCreationOptions(
            string username, string attType, string authType, bool requireResidentKey, string userVerification)
        {
            // 1. Get user from DB by username (in our example, auto create missing users)
            // https://www.w3.org/TR/webauthn/#dom-publickeycredentialcreationoptions-user
            User user = DemoStorage.GetOrAddUser(username, () => new User
            {
                DisplayName = username,
                Name = username,
                Id = Encoding.UTF8.GetBytes(username) // byte representation of userID is required
            });

            // 2. Get user existing keys by username
            // https://www.w3.org/TR/webauthn/#dictdef-publickeycredentialdescriptor
            List<PublicKeyCredentialDescriptor> existingPubCredDescriptor 
                = DemoStorage.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

            #region 3. Create options

            // https://www.w3.org/TR/webauthn/#dictdef-authenticatorselectioncriteria
            AuthenticatorSelection authenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = requireResidentKey,
                UserVerification = userVerification.ToEnum<UserVerificationRequirement>()
            };

            // https://www.w3.org/TR/webauthn/#enumdef-authenticatorattachment
            if (!string.IsNullOrEmpty(authType))
                authenticatorSelection.AuthenticatorAttachment = authType.ToEnum<AuthenticatorAttachment>();

            // https://www.w3.org/TR/webauthn/#dictdef-authenticationextensionsclientinputs
            // https://www.w3.org/TR/webauthn/#sctn-defined-extensions
            AuthenticationExtensionsClientInputs exts = new AuthenticationExtensionsClientInputs()
            {
                // https://www.w3.org/TR/webauthn/#sctn-supported-extensions-extension
                Extensions = true,
                // https://www.w3.org/TR/webauthn/#sctn-uvi-extension
                UserVerificationIndex = true,
                // https://www.w3.org/TR/webauthn/#sctn-location-extension
                Location = true,
                // https://www.w3.org/TR/webauthn/#sctn-uvm-extension
                UserVerificationMethod = true,
                // https://www.w3.org/TR/webauthn/#sctn-authenticator-biometric-criteria-extension
                BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds
                {
                    FAR = float.MaxValue,
                    FRR = float.MaxValue
                }
            };

            // https://www.w3.org/TR/webauthn/#dictdef-publickeycredentialcreationoptions
            CredentialCreateOptions options = _lib.RequestNewCredential(
                // https://www.w3.org/TR/webauthn/#dom-publickeycredentialcreationoptions-user
                user,
                // https://www.w3.org/TR/webauthn/#dictdef-publickeycredentialdescriptor
                existingPubCredDescriptor,
                // https://www.w3.org/TR/webauthn/#dictdef-authenticatorselectioncriteria
                authenticatorSelection,
                // https://www.w3.org/TR/webauthn/#enumdef-attestationconveyancepreference
                attType.ToEnum<AttestationConveyancePreference>(),
                // https://www.w3.org/TR/webauthn/#dictdef-authenticationextensionsclientinputs
                exts);

            #endregion

            // 4. Temporarily store options, session/in-memory cache/redis/db
            JsonOptions1 = options.ToJson();

            // 5. return options
            return options;
        }

        /// <summary>あとで session/in-memory cache/redis/db に変更する。</summary>
        string JsonOptions1 = "";

        /// <summary>AuthenticatorAttestation</summary>
        /// <param name="attestationResponse">AuthenticatorAttestationRawResponse</param>
        /// <returns>CredentialMakeResultを非同期的に返す</returns>
        public async Task<CredentialMakeResult> AuthenticatorAttestation(
            // https://www.w3.org/TR/webauthn/#authenticatorattestationresponse
            AuthenticatorAttestationRawResponse attestationResponse)
        {
            // 1. get the options we sent the client
            // https://www.w3.org/TR/webauthn/#dictdef-publickeycredentialcreationoptions
            CredentialCreateOptions options = CredentialCreateOptions.FromJson(JsonOptions1);

            // 2. Verify and make the credentials
            CredentialMakeResult result =
                await _lib.MakeNewCredentialAsync(attestationResponse, options,
                async (IsCredentialIdUniqueToUserParams args) =>
                {
                    // Create callback so that lib can verify credential id is unique to this user
                    List<User> users = await DemoStorage.GetUsersByCredentialIdAsync(args.CredentialId);

                    if (0 < users.Count)
                        return false;
                    else
                        return true;
                });

            // 3. Store the credentials in db
            DemoStorage.AddCredentialToUser(
                options.User, new StoredCredential
                {
                    // https://www.w3.org/TR/webauthn/#dictdef-publickeycredentialdescriptor
                    Descriptor = new PublicKeyCredentialDescriptor(result.Result.CredentialId),
                    PublicKey = result.Result.PublicKey,
                    UserHandle = result.Result.User.Id,
                    SignatureCounter = result.Result.Counter,
                    CredType = result.Result.CredType,
                    RegDate = DateTime.Now,
                    AaGuid = result.Result.Aaguid
                });

            // 4. return result
            return result;
        }

        #endregion

        #region 認証フロー

        /// <summary>CredentialGetOptions</summary>
        /// <param name="username">string</param>
        /// <returns>AssertionOptions</returns>
        public AssertionOptions CredentialGetOptions(string username)
        {
            // 1. Get user from DB
            // https://www.w3.org/TR/webauthn/#dom-publickeycredentialcreationoptions-user
            User user = DemoStorage.GetUser(username);

            // 2. Get registered credentials from database
            // https://www.w3.org/TR/webauthn/#dictdef-publickeycredentialdescriptor
            List<PublicKeyCredentialDescriptor> existingPubCredDescriptor
                = DemoStorage.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

            if (user == null) throw new ArgumentException("Username was not registered");

            // https://www.w3.org/TR/webauthn/#dictdef-authenticationextensionsclientinputs
            // https://www.w3.org/TR/webauthn/#sctn-defined-extensions
            AuthenticationExtensionsClientInputs exts = new AuthenticationExtensionsClientInputs()
            {
                // https://www.w3.org/TR/webauthn/#sctn-appid-extension
                AppID = _origin,
                // https://www.w3.org/TR/webauthn/#sctn-simple-txauth-extension
                SimpleTransactionAuthorization = "FIDO",
                // https://www.w3.org/TR/webauthn/#sctn-generic-txauth-extension
                GenericTransactionAuthorization = new TxAuthGenericArg
                {
                    ContentType = "text/plain",
                    Content = new byte[] { 0x46, 0x49, 0x44, 0x4F }
                },
                // https://www.w3.org/TR/webauthn/#sctn-supported-extensions-extension
                // Extensions = true,
                // https://www.w3.org/TR/webauthn/#sctn-uvi-extension
                UserVerificationIndex = true,
                // https://www.w3.org/TR/webauthn/#sctn-location-extension
                Location = true,
                // https://www.w3.org/TR/webauthn/#sctn-uvm-extension
                UserVerificationMethod = true
            };

            // 3. Create options
            // https://www.w3.org/TR/webauthn/#assertion-options
            AssertionOptions options = _lib.GetAssertionOptions(
                // https://www.w3.org/TR/webauthn/#dictdef-publickeycredentialdescriptor
                existingPubCredDescriptor,
                // https://www.w3.org/TR/webauthn/#enumdef-userverificationrequirement
                UserVerificationRequirement.Discouraged,
                // https://www.w3.org/TR/webauthn/#sctn-defined-extensions
                exts
            );

            // 4. Temporarily store options, session/in-memory cache/redis/db
            JsonOptions2 = options.ToJson();

            // 5. Return options to client
            return options;
        }

        /// <summary>あとで session/in-memory cache/redis/db に変更する。</summary>
        string JsonOptions2 = "";

        /// <summary>AuthenticatorAssertion</summary>
        /// <param name="clientResponse">AuthenticatorAssertionRawResponse</param>
        /// <returns>AssertionVerificationResultを非同期的に返す</returns>
        public async Task<AssertionVerificationResult> AuthenticatorAssertion(AuthenticatorAssertionRawResponse clientResponse)
        {
            // 1. Get the assertion options we sent the client
            AssertionOptions options = AssertionOptions.FromJson(JsonOptions2);

            // 2. Get registered credential from database
            StoredCredential storedCred = DemoStorage.GetCredentialById(clientResponse.Id);

            // 3. Get credential counter from database
            uint storedCounter = storedCred.SignatureCounter;

            // 4. Make the assertion
            AssertionVerificationResult result = await _lib.MakeAssertionAsync(
                clientResponse, options, storedCred.PublicKey, storedCounter, async (args) =>
                {
                    // Create callback to check if userhandle owns the credentialId
                    List<StoredCredential> storedCreds = await DemoStorage.GetCredentialsByUserHandleAsync(args.UserHandle);
                    return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
                });

            // 5. Store the updated counter
            DemoStorage.UpdateCounter(result.CredentialId, result.Counter);

            // 6. return result
            return result;
        }

        #endregion

        #endregion
    }
}