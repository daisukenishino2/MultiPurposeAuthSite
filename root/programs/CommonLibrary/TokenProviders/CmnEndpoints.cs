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
//* クラス名        ：CmnEndpoints
//* クラス日本語名  ：CmnEndpoints（ライブラリ）
//*
//* 作成日時        ：－
//* 作成者          ：－
//* 更新履歴        ：－
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2017/04/24  西野 大介         新規
//*  2019/02/07  西野 大介         - Code, Token生成処理の集約
//*                                - CheckClientModeの集約
//*                                - Client認証のclient_idとToken類のaudをチェック追加
//*                                - オペレーション・トレース・ログ出力の集約
//*                                  - 情報源
//*                                    - Client情報はclient_idから取得する。
//*                                    - User情報はTokenのsubから取得する。
//*                                  - 以下は、Client = User
//*                                    - GrantClientCredentials
//*                                    - GrantJwtBearerTokenCredentials
//*                                - CheckClientModeの再チェック（PKCE、Hybrid部分
//*  2019/02/08  西野 大介         - F-API2, Confidential Client実装
//*  2019/08/01  西野 大介         - client_secret_postのサポートを追加
//*  2019/08/01  西野 大介         - PKCEのClient認証方法を変更
//**********************************************************************************

using MultiPurposeAuthSite.Co;
#if NETFX
using MultiPurposeAuthSite.Entity;
#else
using MultiPurposeAuthSite;
#endif
using MultiPurposeAuthSite.Data;
using MultiPurposeAuthSite.Password;
using MultiPurposeAuthSite.Log;
using MultiPurposeAuthSite.Extensions.Sts;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

#if NETFX
using Microsoft.AspNet.Identity;
#else
using Microsoft.AspNetCore.Identity;
# endif

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Touryo.Infrastructure.Framework.Authentication;
using Touryo.Infrastructure.Public.Str;
using Touryo.Infrastructure.Public.FastReflection;

namespace MultiPurposeAuthSite.TokenProviders
{
    /// <summary>CmnEndpoints</summary>
    public class CmnEndpoints
    {
        #region .well-known/openid-configuration

        /// <summary>OpenIDConfig</summary>
        /// <returns>Dictionary(string, object)</returns>
        public static Dictionary<string, object> OpenIDConfig()
        {
            Dictionary<string, object> OpenIDConfig = new Dictionary<string, object>();

            #region 基本

            OpenIDConfig.Add("issuer", Config.IssuerId);

            OpenIDConfig.Add("authorization_endpoint",
                Config.OAuth2AuthorizationServerEndpointsRootURI + Config.OAuth2AuthorizeEndpoint);

            OpenIDConfig.Add("token_endpoint",
                Config.OAuth2AuthorizationServerEndpointsRootURI + Config.OAuth2TokenEndpoint);

            OpenIDConfig.Add("userinfo_endpoint",
                Config.OAuth2AuthorizationServerEndpointsRootURI + Config.OAuth2UserInfoEndpoint);

            #endregion

            #region オプション

            List<string> scopes_supported = new List<string>();
            List<string> grant_types_supported = new List<string>();
            List<string> response_types_supported = new List<string>();
            List<string> response_modes_supported = new List<string>();

            OpenIDConfig.Add("scopes_supported", scopes_supported);
            OpenIDConfig.Add("grant_types_supported", grant_types_supported);
            OpenIDConfig.Add("response_types_supported", response_types_supported);
            OpenIDConfig.Add("response_modes_supported", response_modes_supported);

            #region token
            OpenIDConfig.Add("token_endpoint_auth_methods_supported", new List<string> {
                OAuth2AndOIDCEnum.AuthMethods.client_secret_basic.ToStringByEmit(),
                OAuth2AndOIDCEnum.AuthMethods.client_secret_post.ToStringByEmit(),
                OAuth2AndOIDCEnum.AuthMethods.private_key_jwt.ToStringByEmit(),
                OAuth2AndOIDCEnum.AuthMethods.tls_client_auth.ToStringByEmit()
            });

            OpenIDConfig.Add("token_endpoint_auth_signing_alg_values_supported", new List<string> {
                "RS256"
            });
            #endregion

            #region scopes
            scopes_supported.Add(OAuth2AndOIDCConst.Scope_Profile);
            scopes_supported.Add(OAuth2AndOIDCConst.Scope_Email);
            scopes_supported.Add(OAuth2AndOIDCConst.Scope_Phone);
            scopes_supported.Add(OAuth2AndOIDCConst.Scope_Address);
            scopes_supported.Add(OAuth2AndOIDCConst.Scope_Auth);
            scopes_supported.Add(OAuth2AndOIDCConst.Scope_UserID);
            scopes_supported.Add(OAuth2AndOIDCConst.Scope_Roles);
            //scopes_supported.Add(OAuth2AndOIDCConst.Scope_Openid);↓で追加
            #endregion

            #region grant and response_types
            if (Config.EnableAuthorizationCodeGrantType)
            {
                grant_types_supported.Add(OAuth2AndOIDCConst.AuthorizationCodeGrantType);
                response_types_supported.Add(OAuth2AndOIDCConst.AuthorizationCodeResponseType);
            }

            if (Config.EnableImplicitGrantType)
            {
                grant_types_supported.Add(OAuth2AndOIDCConst.ImplicitGrantType);
                response_types_supported.Add(OAuth2AndOIDCConst.ImplicitResponseType);
            }

            if (Config.EnableResourceOwnerPasswordCredentialsGrantType)
            {
                grant_types_supported.Add(OAuth2AndOIDCConst.ResourceOwnerPasswordCredentialsGrantType);
            }

            if (Config.EnableClientCredentialsGrantType)
            {
                grant_types_supported.Add(OAuth2AndOIDCConst.ClientCredentialsGrantType);
            }

            if (Config.EnableRefreshToken)
            {
                grant_types_supported.Add(OAuth2AndOIDCConst.RefreshTokenGrantType);
            }

            if (Config.EnableJwtBearerTokenFlowGrantType)
            {
                grant_types_supported.Add(OAuth2AndOIDCConst.JwtBearerTokenFlowGrantType);
            }
            #endregion

            #region response_modes
            response_modes_supported.Add(OAuth2AndOIDCEnum.ResponseMode.query.ToStringByEmit());
            response_modes_supported.Add(OAuth2AndOIDCEnum.ResponseMode.fragment.ToStringByEmit());
            response_modes_supported.Add(OAuth2AndOIDCEnum.ResponseMode.form_post.ToStringByEmit());
            #endregion

            #region OpenID Connect

            if (Config.EnableOpenIDConnect)
            {
                scopes_supported.Add(OAuth2AndOIDCConst.Scope_Openid);

                #region response_types
                response_types_supported.Add(OAuth2AndOIDCConst.OidcImplicit2_ResponseType);
                response_types_supported.Add(OAuth2AndOIDCConst.OidcHybrid2_Token_ResponseType);
                response_types_supported.Add(OAuth2AndOIDCConst.OidcHybrid2_IdToken_ResponseType);
                response_types_supported.Add(OAuth2AndOIDCConst.OidcHybrid3_ResponseType);
                #endregion

                #region id_token
                OpenIDConfig.Add("id_token_signing_alg_values_supported", new List<string> {
                    "RS256", "ES256"
                });

                OpenIDConfig.Add("id_token_encryption_alg_values_supported", new List<string> {
                    "RSA-OAEP"
                });
                #endregion

                // subject_types
                OpenIDConfig.Add("subject_types_supported", new List<string> {
                    "public"
                });

                #region claims
                OpenIDConfig.Add("claims_parameter_supported", false); // RequestObjectでのみサポート
                OpenIDConfig.Add("claims_supported", new List<string> {
                    //Jwt
                    OAuth2AndOIDCConst.iss,
                    OAuth2AndOIDCConst.aud,
                    OAuth2AndOIDCConst.sub,
                    OAuth2AndOIDCConst.exp,
                    OAuth2AndOIDCConst.nbf,
                    OAuth2AndOIDCConst.iat,
                    OAuth2AndOIDCConst.jti,
                    // scope
                    // 標準
                    OAuth2AndOIDCConst.Scope_Email,
                    OAuth2AndOIDCConst.email_verified,
                    OAuth2AndOIDCConst.phone_number,
                    OAuth2AndOIDCConst.phone_number_verified,
                    // 拡張
                    OAuth2AndOIDCConst.scopes,
                    OAuth2AndOIDCConst.Scope_Roles,
                    OAuth2AndOIDCConst.Scope_UserID,
                    // OIDC, FAPI1
                    OAuth2AndOIDCConst.nonce,
                    OAuth2AndOIDCConst.at_hash,
                    OAuth2AndOIDCConst.c_hash,
                    OAuth2AndOIDCConst.s_hash //,
                    //OAuth2AndOIDCConst.auth
                });
                #endregion

                #region RequestObject
                OpenIDConfig.Add("request_object_signing_alg_values_supported", new List<string> {
                    "RS256"
                });
                OpenIDConfig.Add("request_parameter_supported", false);
                OpenIDConfig.Add("request_uri_parameter_supported", true);
                OpenIDConfig.Add("request_object_endpoint",
                    Config.OAuth2AuthorizationServerEndpointsRootURI + OAuth2AndOIDCParams.RequestObjectRegUri);
                #endregion

                #region ResponseObject(JARM)
                // 「.」がね...。
                response_modes_supported.Add("query.jwt");
                response_modes_supported.Add("fragment.jwt");
                response_modes_supported.Add("form_post.jwt");
                #endregion
            }

            OpenIDConfig.Add("jwks_uri",
                    Config.OAuth2AuthorizationServerEndpointsRootURI + OAuth2AndOIDCParams.JwkSetUri);

            #endregion

            #region OAuth2拡張

            #region Revocation
            OpenIDConfig.Add("revocation_endpoint",
                Config.OAuth2AuthorizationServerEndpointsRootURI + Config.OAuth2RevokeTokenEndpoint);

            OpenIDConfig.Add("revocation_endpoint_auth_methods_supported", new List<string> {
               OAuth2AndOIDCEnum.AuthMethods.client_secret_basic.ToStringByEmit(),
               OAuth2AndOIDCEnum.AuthMethods.client_secret_post.ToStringByEmit()
            });
            #endregion

            #region Introspect
            OpenIDConfig.Add("introspection_endpoint",
                Config.OAuth2AuthorizationServerEndpointsRootURI + Config.OAuth2IntrospectTokenEndpoint);

            OpenIDConfig.Add("introspection_endpoint_auth_methods_supported", new List<string> {
               OAuth2AndOIDCEnum.AuthMethods.client_secret_basic.ToStringByEmit(),
               OAuth2AndOIDCEnum.AuthMethods.client_secret_post.ToStringByEmit()
            });
            #endregion

            #region OAuth PKCE

            OpenIDConfig.Add("code_challenge_methods_supported", new List<string> {
                OAuth2AndOIDCConst.PKCE_plain,
                OAuth2AndOIDCConst.PKCE_S256
            });

            #endregion

            #endregion

            #region FAPI

            OpenIDConfig.Add("mutual_tls_sender_constrained_access_tokens", "true");

            #endregion

            #region その他
            OpenIDConfig.Add("display_values_supported", new List<string> {
                "page"
            });

            OpenIDConfig.Add("service_documentation", "・・・");
            #endregion

            #endregion

            return OpenIDConfig;
        }

        #endregion

        #region AuthZAuthNEndpoint

        #region ValidateAuthZReqParam

        /// <summary>ValidateAuthZReqParam</summary>
        /// <param name="grant_type">string</param>
        /// <param name="client_id">string</param>
        /// <param name="redirect_uri">string</param>
        /// <param name="response_type">string</param>
        /// <param name="scope">string</param>
        /// <param name="nonce">string</param>
        /// <param name="valid_redirect_uri">string</param>
        /// <param name="err">string</param>
        /// <param name="errDescription">string</param>
        /// <returns>成功 or 失敗</returns>
        public static bool ValidateAuthZReqParam(string client_id, string redirect_uri,
            string response_type, string scope, string nonce,
            out string valid_redirect_uri, out string err, out string errDescription)
        {
            valid_redirect_uri = "";
            err = "";
            errDescription = "";

            #region response_type

            // response_typeチェック
            if (!string.IsNullOrEmpty(response_type))
            {
                if (response_type.ToLower() == OAuth2AndOIDCConst.AuthorizationCodeResponseType)
                {
                    if (!Config.EnableAuthorizationCodeGrantType)
                    {
                        err = "server_error";
                        errDescription = Resources.ApplicationOAuthBearerTokenProvider.EnableAuthorizationCodeGrantType;
                        return false;
                    }
                }
                else if (response_type.ToLower() == OAuth2AndOIDCConst.ImplicitResponseType)
                {
                    if (!Config.EnableImplicitGrantType)
                    {
                        err = "server_error";
                        errDescription = Resources.ApplicationOAuthBearerTokenProvider.EnableImplicitGrantType;
                        return false;
                    }
                }
                else if (response_type.ToLower() == OAuth2AndOIDCConst.OidcImplicit1_ResponseType
                            || response_type.ToLower() == OAuth2AndOIDCConst.OidcImplicit2_ResponseType
                            || response_type.ToLower() == OAuth2AndOIDCConst.OidcHybrid2_IdToken_ResponseType
                            || response_type.ToLower() == OAuth2AndOIDCConst.OidcHybrid2_Token_ResponseType
                            || response_type.ToLower() == OAuth2AndOIDCConst.OidcHybrid3_ResponseType)
                {
                    // OIDCチェック１
                    if (!scope.Split(' ').Any(x => x == OAuth2AndOIDCConst.Scope_Openid))
                    {
                        // OIDC無効
                        err = "server_error";
                        errDescription = "This response_type is valid only for oidc.";
                        return false;
                    }
                }
                else
                {
                    err = "server_error";
                    errDescription = "This response_type is unknown.";
                    return false;
                }
            }
            else
            {
                err = "server_error";
                errDescription = "response_type is empty.";
                return false;
            }

            // OIDCチェック２
            if (scope.Split(' ').Any(x => x == OAuth2AndOIDCConst.Scope_Openid))
            {
                // OIDC有効
                if (!Config.EnableOpenIDConnect)
                {
                    err = "server_error";
                    errDescription = "OIDC is not enabled.";
                    return false;
                }

                // nonceパラメタ 必須
                if (string.IsNullOrEmpty(nonce))
                {
                    err = "server_error";
                    errDescription = "There was no nonce in query.";
                    return false;
                }
            }

            #endregion

            #region redirect_uri

            // redirect_uriのチェック
            if (string.IsNullOrEmpty(redirect_uri))
            {
                // redirect_uriの指定が無い。

                // クライアント識別子に対応する事前登録したredirect_uriを取得する。
                redirect_uri = Helper.GetInstance().GetClientsRedirectUri(client_id, response_type);

                if (!string.IsNullOrEmpty(redirect_uri))
                {
                    // 事前登録されている。
                    if (redirect_uri.ToLower() == "test_self_code")
                    {
                        // Authorization Codeグラント種別のテスト用のセルフRedirectエンドポイント
                        valid_redirect_uri = Config.OAuth2ClientEndpointsRootURI + Config.OAuth2AuthorizationCodeGrantClient_Account;
                    }
                    else if (redirect_uri.ToLower() == "test_self_token")
                    {
                        // Implicitグラント種別のテスト用のセルフRedirectエンドポイント
                        valid_redirect_uri = Config.OAuth2ClientEndpointsRootURI + Config.OAuth2ImplicitGrantClient_Account;
                    }
                    else if (redirect_uri.ToLower() == "id_federation_code")
                    {
                        // ID連携時のエンドポイント
                        valid_redirect_uri = Config.IdFederationRedirectEndPoint;
                    }
                    else
                    {
                        // 事前登録した、redirect_uriをそのまま使用する。
                        valid_redirect_uri = redirect_uri;
                    }

                    return true;
                }
            }
            else
            {
                // redirect_uriの指定が有る。

                // 指定されたredirect_uriを使用する場合は、チェックが必要になる。
                if (
                    // self_code : Authorization Codeグラント種別
                    redirect_uri == (Config.OAuth2ClientEndpointsRootURI + Config.OAuth2AuthorizationCodeGrantClient_Manage))
                {
                    // 不特定多数のクライアント識別子に許可されたredirect_uri
                    valid_redirect_uri = redirect_uri;
                    return true;
                }
                else
                {
                    // クライアント識別子に対応する事前登録したredirect_uriに
                    string preRegisteredUri = Helper.GetInstance().GetClientsRedirectUri(client_id, response_type);

                    //if (redirect_uri.StartsWith(preRegisteredUri))
                    if (redirect_uri == preRegisteredUri)
                    {
                        // 完全一致する場合。
                        valid_redirect_uri = redirect_uri;
                        return true;
                    }
                    else
                    {
                        // 完全一致しない場合。
                        err = "server_error";
                        errDescription = Resources.ApplicationOAuthBearerTokenProvider.Invalid_redirect_uri;
                        return false;
                    }
                }
            }

            #endregion

            // 結果を返す。
            return false;
        }

        #endregion

        #region CreateCodeInAuthZNRes

        /// <summary>CreateCodeInAuthZNRes</summary>
        /// <param name="identity">ClaimsIdentity</param>
        /// <param name="queryString">NameValueCollection</param>
        /// <param name="client_id">string</param>
        /// <param name="state">string</param>
        /// <param name="scopes">string</param>
        /// <param name="claims">JObject</param>
        /// <param name="nonce">string</param>
        /// <returns>code</returns>
        public static string CreateCodeInAuthZNRes(
            ClaimsIdentity identity, NameValueCollection queryString,
            string client_id, string state, IEnumerable<string> scopes, JObject claims, string nonce)
        {
            // ClaimsIdentityに、その他、所定のClaimを追加する。
            Helper.AddClaim(identity, client_id, state, scopes, claims, nonce);

            // Codeの生成
            string code = AuthorizationCodeProvider.Create(identity, queryString);

            // オペレーション・トレース・ログ出力
            string name = Helper.GetInstance().GetClientName(client_id);
            Logging.MyOperationTrace(string.Format(
                "{0}({1}) passed the authorization endpoint of Hybrid by {2}({3}).",
                client_id, name,                                                        // Client Account
                Helper.GetInstance().GetClientIdByName(identity.Name), identity.Name)); // User Account

            return code;
        }

        #endregion

        #region CreateAuthZRes4ImplicitFlow

        /// <summary>CreateAuthZRes4ImplicitFlow</summary>
        /// <param name="identity">ClaimsIdentity</param>
        /// <param name="queryString">NameValueCollection</param>
        /// <param name="response_type">string</param>
        /// <param name="client_id">string</param>
        /// <param name="state">string</param>
        /// <param name="scopes">IEnumerable(string)</param>
        /// <param name="claims">JObject</param>
        /// <param name="nonce">string</param>
        /// <param name="access_token">out string</param>
        /// <param name="id_token">out string</param>
        public static void CreateAuthZRes4ImplicitFlow(
            ClaimsIdentity identity, NameValueCollection queryString, string response_type,
            string client_id, string state, IEnumerable<string> scopes, JObject claims, string nonce,
            out string access_token, out string id_token)
        {
            string jwkString = "";

            access_token = ""; // 初期化
            id_token = "";     // 初期化

            if (Config.EnableImplicitGrantType)
            {
                #region CheckClientMode

                // このフローが認められるか？
                Dictionary<string, string> err = new Dictionary<string, string>();
                if (CmnEndpoints.CheckClientMode(client_id, OAuth2AndOIDCEnum.ClientMode.normal, out jwkString, out err))
                {
                    // 継続可
                }
                else
                {
                    // 継続不可
                    // err設定済み
                    return;
                }

                #endregion

                #region Token発行

                // ClaimsIdentityに、その他、所定のClaimを追加する。
                Helper.AddClaim(identity, client_id, state, scopes, claims, nonce);

                // AccessTokenの生成
                access_token = CmnAccessToken.CreateFromClaims(
                	client_id, identity.Name, identity.Claims,
                    DateTimeOffset.Now.AddMinutes(Config.OAuth2AccessTokenExpireTimeSpanFromMinutes.TotalMinutes));

                JObject jObj = (JObject)JsonConvert.DeserializeObject(
                    CustomEncode.ByteToString(CustomEncode.FromBase64UrlString(
                        access_token.Split('.')[1]), CustomEncode.us_ascii));

                // id_token
                if (response_type.IndexOf(OAuth2AndOIDCConst.IDToken) != -1)
                {
                    JArray jAry = (JArray)jObj["scopes"];
                    foreach (string s in jAry)
                    {
                        if (s == OAuth2AndOIDCConst.Scope_Openid)
                        {
                            id_token = CmnIdToken.ChangeToIdTokenFromAccessToken(
                                access_token, "", state, // c_hash, は Implicit Flow で生成不可
                                HashClaimType.AtHash | HashClaimType.SHash,
                                Config.RsaPfxFilePath, Config.RsaPfxPassword, jwkString);
                        }
                    }
                }

                // オペレーション・トレース・ログ出力
                string name = Helper.GetInstance().GetClientName(client_id);
                Logging.MyOperationTrace(string.Format(
                    "{0}({1}) passed the authorization endpoint of Hybrid by {2}({3}).",
                    client_id, name,                                                        // Client Account
                    Helper.GetInstance().GetClientIdByName(identity.Name), identity.Name)); // User Account

                #endregion
            }
        }

        #endregion

        #region CreateAuthNRes4HybridFlow

        /// <summary>CreateAuthNRes4HybridFlow</summary>
        /// <param name="identity">ClaimsIdentity</param>
        /// <param name="queryString">NameValueCollection</param>
        /// <param name="client_id">string</param>
        /// <param name="state">string</param>
        /// <param name="scopes">IEnumerable(string)</param>
        /// <param name="claims">JObject</param>
        /// <param name="nonce">string</param>
        /// <param name="access_token">out string</param>
        /// <param name="id_token">out string</param>
        /// <returns></returns>
        public static string CreateAuthNRes4HybridFlow(
            ClaimsIdentity identity, NameValueCollection queryString,
            string client_id, string state, IEnumerable<string> scopes, JObject claims, string nonce,
            out string access_token, out string id_token)
        {
            string code = "";
            string jwkString = "";

            access_token = ""; // 初期化
            id_token = "";     // 初期化

            if (Config.EnableOpenIDConnect)
            {
                #region CheckClientMode

                // 初期値の許容レベルは最低レベルに設定
                OAuth2AndOIDCEnum.ClientMode permittedLevel = OAuth2AndOIDCEnum.ClientMode.normal;

                // ★ 未実装
                // TokenBindingの有無で、permittedLevelを変更する。
                // TokenBindingの無
                //permittedLevel = OAuth2AndOIDCEnum.ClientMode.normal;
                // TokenBindingの有
                //permittedLevel = OAuth2AndOIDCEnum.ClientMode.fapi2;

                // このフローが認められるか？
                Dictionary<string, string> err = new Dictionary<string, string>();
                if (CmnEndpoints.CheckClientMode(client_id, permittedLevel, out jwkString, out err))
                {
                    // 継続可
                }
                else
                {
                    // 継続不可
                    return "";
                }

                #endregion

                #region Token発行

                // ClaimsIdentityに、その他、所定のClaimを追加する。
                Helper.AddClaim(identity, client_id, state, scopes, claims, nonce);

                // Codeの生成
                code = AuthorizationCodeProvider.Create(identity, queryString);

                string tokenPayload = AuthorizationCodeProvider.GetAccessTokenPayload(code);

                // ★ 必要に応じて、scopeを調整する。

                // access_token
                access_token = CmnAccessToken.ProtectFromPayload(
                	client_id, tokenPayload,
                    DateTimeOffset.Now.Add(Config.OAuth2AccessTokenExpireTimeSpanFromMinutes),
                    null, permittedLevel, out string aud, out string sub);

                // Client認証のclient_idとToken類のaudをチェック
                if (client_id != aud) { throw new Exception("[client_id != aud]"); }

                JObject jObj = (JObject)JsonConvert.DeserializeObject(
                                CustomEncode.ByteToString(CustomEncode.FromBase64UrlString(
                                    access_token.Split('.')[1]), CustomEncode.us_ascii));

                // id_token
                JArray jAry = (JArray)jObj["scopes"];
                foreach (string s in jAry)
                {
                    if (s == OAuth2AndOIDCConst.Scope_Openid)
                    {
                        id_token = CmnIdToken.ChangeToIdTokenFromAccessToken(
                            access_token, code, state, // at_hash, c_hash, s_hash
                            HashClaimType.AtHash | HashClaimType.CHash | HashClaimType.SHash,
                            Config.RsaPfxFilePath, Config.RsaPfxPassword, jwkString);
                    }
                }

                // オペレーション・トレース・ログ出力
                string name = Helper.GetInstance().GetClientName(client_id);
                Logging.MyOperationTrace(string.Format(
                    "{0}({1}) passed the authorization endpoint of Hybrid by {2}({3}).",
                    client_id, name,                                    // Client Account
                    Helper.GetInstance().GetClientIdByName(sub), sub)); // User Account

                #endregion
            }

            return code;
        }

        #endregion

        #endregion

        #region TokenEndpoint

        #region GrantAuthorizationCodeCredentials

        /// <summary>
        /// GrantAuthorizationCodeCredentials
        /// Authorization Codeグラント種別
        /// </summary>
        /// <param name="grant_type">string</param>
        /// <param name="client_id">string</param>
        /// <param name="client_secret">string</param>
        /// <param name="assertion">string</param>
        /// <param name="x509">X509Certificate2</param>
        /// <param name="code">string</param>
        /// <param name="code_verifier">string</param>
        /// <param name="redirect_uri">string</param>
        /// <param name="ret">Dictionary(string, string)</param>
        /// <param name="err">Dictionary(string, string)</param>
        /// <returns>成否</returns>
        public static bool GrantAuthorizationCodeCredentials(
            string grant_type, string client_id, string client_secret,
            string assertion, X509Certificate2 x509,
            string code, string code_verifier, string redirect_uri,
            out Dictionary<string, string> ret, out Dictionary<string, string> err)
        {
            ret = null;

            string jwkString = "";
            err = new Dictionary<string, string>();

            if (Config.EnableAuthorizationCodeGrantType)
            {
                // 初期値の許容レベルは最低レベルに設定
                OAuth2AndOIDCEnum.ClientMode permittedLevel = OAuth2AndOIDCEnum.ClientMode.normal;

                #region 認証

                bool authned = false;
                if (grant_type.ToLower() == OAuth2AndOIDCConst.AuthorizationCodeGrantType)
                {
                    if (string.IsNullOrEmpty(code_verifier) && string.IsNullOrEmpty(assertion))
                    {
                        // client_id & (client_secret or x509)
                        authned = CmnEndpoints.ClientAuthentication(
                            client_id, client_secret, ref x509, out permittedLevel);
                    }
                    else if (!string.IsNullOrEmpty(code_verifier)
                        && string.IsNullOrEmpty(client_secret))
                    {
                        // PKCE (client_id & code_verifier)
                        AuthorizationCodeProvider.ReceiveChallenge(
                            code, client_id, redirect_uri,
                            out string code_challenge_method, out string code_challenge);

                        if (!string.IsNullOrEmpty(code_challenge_method))
                        {
                            if (!string.IsNullOrEmpty(code_challenge))
                            {
                                if (code_challenge_method.ToLower() == OAuth2AndOIDCConst.PKCE_plain)
                                {
                                    if (code_challenge == code_verifier)
                                    {
                                        // passed.
                                        authned = true;
                                    }
                                }
                                else if (code_challenge_method.ToUpper() == OAuth2AndOIDCConst.PKCE_S256)
                                {
                                    if (code_challenge == OAuth2AndOIDCClient.PKCE_S256_CodeChallengeMethod(code_verifier))
                                    {
                                        // passed.
                                        authned = true;
                                        permittedLevel = OAuth2AndOIDCEnum.ClientMode.fapi1;
                                    }
                                }
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(code_verifier)
                        && !string.IsNullOrEmpty(client_secret))
                    {
                        // "OAuth 2.0 authorization code flow with the PKCE extension"
                        //  (client_id & client_secret & code_verifier)
                        // これを実装する場合、client_id から Native か SPA否かを見極めて、
                        // SPAの場合、通常のPKCE（前カバレッジ）を拒否する実装が必要になる。
                    }
                    else if (!string.IsNullOrEmpty(assertion))
                    {
                        // assertion
                        authned = CmnEndpoints.ClientAuthentication(
                            assertion, out client_id, ref x509, out permittedLevel);
                    }
                }

                #endregion

                if (authned)
                {
                    string tokenPayload = AuthorizationCodeProvider.Receive(code, client_id, redirect_uri);

                    #region CheckClientMode

                    // このフローが認められるか？
                    if (CmnEndpoints.CheckClientMode(client_id, permittedLevel, out jwkString, out err))
                    {
                        // 継続可
                    }
                    else
                    {
                        // 継続不可
                        // err設定済み
                        return false;
                    }

                    #endregion

                    #region 発行

                    // access_token
                    string access_token = CmnAccessToken.ProtectFromPayload(
                        client_id, tokenPayload,
                        DateTimeOffset.Now.Add(Config.OAuth2AccessTokenExpireTimeSpanFromMinutes),
                        x509, permittedLevel, out string aud, out string sub);

                    // Client認証のclient_idとToken類のaudをチェック
                    if (client_id != aud) { throw new Exception("[client_id != aud]"); }

                    // refresh_token
                    string refresh_token = "";
                    if (Config.EnableRefreshToken)
                    {
                        refresh_token = RefreshTokenProvider.Create(tokenPayload);
                    }

                    // オペレーション・トレース・ログ出力
                    string name = Helper.GetInstance().GetClientName(client_id);
                    Logging.MyOperationTrace(string.Format(
                        "{0}({1}) passed the 'Authorization Code flow' by {2}({3}).",
                        client_id, name,                                    // Client Account
                        Helper.GetInstance().GetClientIdByName(sub), sub)); // User Account

                    ret = CmnEndpoints.CreateAccessTokenResponse(access_token, refresh_token, jwkString);

                    return true;

                    #endregion
                }
                else
                {
                    // クライアント認証エラー（Credential不正
                    err.Add("error", "invalid_client");
                    err.Add("error_description", "Invalid credential.");
                }
            }
            else
            {
                // サポートされていない
                err.Add("error", "not_supported");
                err.Add("error_description", Resources.ApplicationOAuthBearerTokenProvider.EnableAuthorizationCodeGrantType);
            }

            return false;
        }

        #endregion

        // 以下は、AuthZAuthNEndpointを参照。
        // GrantImplicitCredentials
        // GrantHybridCredentials

        #region GrantRefreshTokenCredentials

        /// <summary>
        /// GrantRefreshTokenCredentials
        /// Authorization Codeグラント種別
        /// </summary>
        /// <param name="grant_type">string</param>
        /// <param name="client_id">string</param>
        /// <param name="client_secret">string</param>
        /// <param name="x509">X509Certificate2</param>
        /// <param name="refresh_token">string</param>
        /// <param name="ret">Dictionary(string, string)</param>
        /// <param name="err">Dictionary(string, string)</param>
        /// <returns>成否</returns>
        public static bool GrantRefreshTokenCredentials(
            string grant_type, string client_id, string client_secret, X509Certificate2 x509,
            string refresh_token, out Dictionary<string, string> ret, out Dictionary<string, string> err)
        {
            ret = null;

            string jwkString = "";
            err = new Dictionary<string, string>();

            if (Config.EnableRefreshToken)
            {
                #region 認証

                bool authned = false;
                if (grant_type.ToLower() == OAuth2AndOIDCConst.RefreshTokenGrantType)
                {
                    // client_id & (client_secret or x509)
                    authned = CmnEndpoints.ClientAuthentication(client_id, client_secret,
                        ref x509,　out OAuth2AndOIDCEnum.ClientMode permittedLevel);
                }

                #endregion

                if (authned)
                {
                    #region CheckClientMode

                    // このフローが認められるか？
                    if (CmnEndpoints.CheckClientMode(client_id, OAuth2AndOIDCEnum.ClientMode.normal, out jwkString, out err))
                    {
                        // 継続可
                    }
                    else
                    {
                        // 継続不可
                        // err設定済み
                        return false;
                    }

                    #endregion

                    #region 発行

                    string tokenPayload = RefreshTokenProvider.Receive(refresh_token);

                    if (!string.IsNullOrEmpty(tokenPayload))
                    {
                        // access_token
                        string access_token = CmnAccessToken.ProtectFromPayload(
                            client_id, tokenPayload,
                            DateTimeOffset.Now.Add(Config.OAuth2AccessTokenExpireTimeSpanFromMinutes),
                            x509, OAuth2AndOIDCEnum.ClientMode.normal, out string aud, out string sub);

                        // Client認証のclient_idとToken類のaudをチェック
                        if (client_id != aud) { throw new Exception("[client_id != aud]"); }

                        string new_refresh_token = "";
                        if (Config.EnableRefreshToken)
                        {
                            new_refresh_token = RefreshTokenProvider.Create(tokenPayload);
                        }

                        // オペレーション・トレース・ログ出力
                        string name = Helper.GetInstance().GetClientName(client_id);
                        Logging.MyOperationTrace(string.Format(
                            "{0}({1}) passed the 'Refresh Token flow' by {2}({3}).",
                            client_id, name,                                    // Client Account
                            Helper.GetInstance().GetClientIdByName(sub), sub)); // User Account

                        ret = CmnEndpoints.CreateAccessTokenResponse(access_token, new_refresh_token, jwkString);

                        return true;
                    }

                    #endregion
                }
                else
                {
                    // クライアント認証エラー（Credential不正
                    err.Add("error", "invalid_client");
                    err.Add("error_description", "Invalid credential.");
                }
            }
            else
            {
                // サポートされていない
                err.Add("error", "not_supported");
                err.Add("error_description", Resources.ApplicationOAuthBearerTokenProvider.EnableRefreshToken);
            }

            return false;
        }

        #endregion

        #region GrantResourceOwnerCredentials

        /// <summary>GrantResourceOwnerCredentials</summary>
        /// <param name="grant_type">string</param>
        /// <param name="client_id">string</param>
        /// <param name="client_secret">string</param>
        /// <param name="x509">X509Certificate2</param>
        /// <param name="username">string</param>
        /// <param name="password">string</param>
        /// <param name="scopes">string</param>
        /// <param name="ret">Dictionary(string, string)</param>
        /// <param name="err">Dictionary(string, string)</param>
        /// <returns>成否</returns>
        public static bool GrantResourceOwnerCredentials(
            string grant_type, string client_id,
            string client_secret, X509Certificate2 x509,
            string username, string password, string scopes,
            out Dictionary<string, string> ret, out Dictionary<string, string> err)
        {
            ret = null;

            string jwkString = "";
            err = new Dictionary<string, string>();

            if (Config.EnableResourceOwnerPasswordCredentialsGrantType)
            {
                #region 認証

                bool authned = false;
                if (grant_type.ToLower() == OAuth2AndOIDCConst.ResourceOwnerPasswordCredentialsGrantType)
                {
                    // client_id & client_secret
                    authned = CmnEndpoints.ClientAuthentication(client_id, client_secret,
                        ref x509, out OAuth2AndOIDCEnum.ClientMode permittedLevel);
                }

                #endregion

                if (authned)
                {
                    #region CheckClientMode

                    // このフローが認められるか？
                    if (CmnEndpoints.CheckClientMode(client_id, OAuth2AndOIDCEnum.ClientMode.normal, out jwkString, out err))
                    {
                        // 継続可
                    }
                    else
                    {
                        // 継続不可
                        // err設定済み
                        return false;
                    }

                    #endregion

                    #region 発行

                    // username=ユーザ名&password=パスワードとして送付されたクレデンシャルを検証する。
                    ApplicationUser user = CmnUserStore.FindByName(username);

                    if (user != null)
                    {
                        // ユーザーが見つかった場合。
#if NETFX
                        PasswordVerificationResult pvRet = (new CustomPasswordHasher()).VerifyHashedPassword(user.PasswordHash, password);
#else
                        PasswordVerificationResult pvRet = (new CustomPasswordHasher<ApplicationUser>()).VerifyHashedPassword(user, user.PasswordHash, password);
#endif
                        if (pvRet.HasFlag(PasswordVerificationResult.Success))
                        {
                            // ClaimsIdentityにClaimを追加する。
                            ClaimsIdentity identity = new ClaimsIdentity(OAuth2AndOIDCConst.Bearer);

                            // Name Claimを追加
                            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

                            // ClaimsIdentityに、その他、所定のClaimを追加する。
                            identity = Helper.AddClaim(identity, client_id, "", scopes.Split(' '), null, "");

                            // access_token
                            string access_token = CmnAccessToken.CreateFromClaims(
                            	client_id, identity.Name, identity.Claims,
                                DateTimeOffset.Now.Add(Config.OAuth2AccessTokenExpireTimeSpanFromMinutes));

                            // オペレーション・トレース・ログ出力
                            string name = Helper.GetInstance().GetClientName(client_id);
                            Logging.MyOperationTrace(string.Format(
                                "{0}({1}) passed the 'resource owner password credentials flow' by {2}({3}).",
                                user.Id, user.UserName, // User Account
                                client_id, name)); // Client Account

                            ret = CmnEndpoints.CreateAccessTokenResponse(access_token, "", jwkString);
                            return true;
                        }
                        else
                        {
                            // パスワードが一致しない場合。
                            err.Add("error", "access_denied");
                            err.Add("error_description", Resources.ApplicationOAuthBearerTokenProvider.access_denied);
                        }
                    }
                    else
                    {
                        // ユーザーが見つからない場合。
                        err.Add("error", "access_denied");
                        err.Add("error_description", Resources.ApplicationOAuthBearerTokenProvider.access_denied);
                    }

                    #endregion
                }
                else
                {
                    // クライアント認証エラー（Credential不正
                    err.Add("error", "invalid_client");
                    err.Add("error_description", "Invalid credential.");
                }
            }
            else
            {
                // サポートされていない
                err.Add("error", "not_supported");
                err.Add("error_description", Resources.ApplicationOAuthBearerTokenProvider.EnableResourceOwnerCredentialsGrantType);
            }

            return false;
        }

        #endregion

        #region GrantClientCredentials

        /// <summary>
        /// GrantClientCredentials
        /// Client Credentialsグラント種別
        /// </summary>
        /// <param name="grant_type">string</param>
        /// <param name="client_id">string</param>
        /// <param name="client_secret">string</param>
        /// <param name="x509">X509Certificate2</param>
        /// <param name="scopes">string</param>
        /// <param name="ret">Dictionary(string, string)</param>
        /// <param name="err">Dictionary(string, string)</param>
        /// <returns>成否</returns>
        public static bool GrantClientCredentials(
            string grant_type, string client_id, string client_secret, X509Certificate2 x509,
            string scopes, out Dictionary<string, string> ret, out Dictionary<string, string> err)
        {
            ret = null;

            string jwkString = "";
            err = new Dictionary<string, string>();

            if (Config.EnableClientCredentialsGrantType)
            {
                #region 認証

                bool authned = false;
                if (grant_type.ToLower() == OAuth2AndOIDCConst.ClientCredentialsGrantType)
                {
                    // client_id & client_secret
                    authned = CmnEndpoints.ClientAuthentication(client_id, client_secret,
                        ref x509, out OAuth2AndOIDCEnum.ClientMode permittedLevel);
                }

                #endregion

                if (authned)
                {
                    #region CheckClientMode

                    // このフローが認められるか？
                    if (CmnEndpoints.CheckClientMode(client_id, OAuth2AndOIDCEnum.ClientMode.normal, out jwkString, out err))
                    {
                        // 継続可
                    }
                    else
                    {
                        // 継続不可
                        // err設定済み
                        return false;
                    }

                    #endregion

                    #region 発行

                    // client_idに対応するsubを取得する。
                    string sub = Helper.GetInstance().GetClientName(client_id);

                    // ClaimsIdentityにClaimを追加する。
                    ClaimsIdentity identity = new ClaimsIdentity(OAuth2AndOIDCConst.Bearer);

                    // ClaimsIdentityに、その他、所定のClaimを追加する。
                    identity.AddClaim(new Claim(ClaimTypes.Name, sub));
                    identity = Helper.AddClaim(identity, client_id, "", scopes.Split(' '), null, "");

                    // access_token
                    string access_token = CmnAccessToken.CreateFromClaims(
                        client_id, identity.Name, identity.Claims,
                        DateTimeOffset.Now.Add(Config.OAuth2AccessTokenExpireTimeSpanFromMinutes));

                    // オペレーション・トレース・ログ出力
                    Logging.MyOperationTrace(string.Format(
                        "Passed the 'client credentials flow' by {0}({1}).",
                        client_id, sub)); // Client Account

                    ret = CmnEndpoints.CreateAccessTokenResponse(access_token, "", jwkString);
                    return true;

                    #endregion
                }
                else
                {
                    // クライアント認証エラー（Credential不正
                    err.Add("error", "invalid_client");
                    err.Add("error_description", "Invalid credential.");
                }
            }
            else
            {
                // サポートされていない
                err.Add("error", "not_supported");
                err.Add("error_description", Resources.ApplicationOAuthBearerTokenProvider.EnableClientCredentialsGrantType);
            }

            return false;
        }

        #endregion

        #region GrantJwtBearerTokenCredentials

        /// <summary>GrantJwtBearerTokenCredentials</summary>

        /// <summary>
        /// GrantJwtBearerTokenCredentials
        /// Authorization Codeグラント種別
        /// </summary>
        /// <param name="grant_type">string</param>
        /// <param name="assertion">string</param>
        /// <param name="x509">X509Certificate2</param>
        /// <param name="ret">Dictionary(string, string)</param>
        /// <param name="err">Dictionary(string, string)</param>
        /// <returns>成否</returns>
        public static bool GrantJwtBearerTokenCredentials(
            string grant_type, string assertion, X509Certificate2 x509,
            out Dictionary<string, string> ret, out Dictionary<string, string> err)
        {
            ret = null;

            string jwkString = "";
            err = new Dictionary<string, string>();

            if (Config.EnableJwtBearerTokenFlowGrantType &&
                grant_type.ToLower() == OAuth2AndOIDCConst.JwtBearerTokenFlowGrantType)
            {
                Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    CustomEncode.ByteToString(CustomEncode.FromBase64UrlString(
                        assertion.Split('.')[1]), CustomEncode.us_ascii));

                string pubKey = Helper.GetInstance().GetJwkRsaPublickey(dic[OAuth2AndOIDCConst.iss]);
                pubKey = CustomEncode.ByteToString(CustomEncode.FromBase64UrlString(pubKey), CustomEncode.us_ascii);

                if (!string.IsNullOrEmpty(pubKey))
                {
                    if (JwtAssertion.Verify(
                        assertion, out string iss, out string aud, out string scopes, out JObject jobj, pubKey))
                    {
                        // aud 検証
                        if (aud == Config.OAuth2AuthorizationServerEndpointsRootURI + Config.OAuth2TokenEndpoint)
                        {
                            // このフローが認められるか？
                            if (CmnEndpoints.CheckClientMode(iss, OAuth2AndOIDCEnum.ClientMode.normal, out jwkString, out err))
                            {
                                // JwtTokenを作る

                                // issに対応するsubを取得する。
                                string sub = Helper.GetInstance().GetClientName(iss);

                                // ClaimsIdentityにClaimを追加する。
                                ClaimsIdentity identity = new ClaimsIdentity(OAuth2AndOIDCConst.Bearer);

                                // ClaimsIdentityに、その他、所定のClaimを追加する。
                                identity.AddClaim(new Claim(ClaimTypes.Name, sub));
                                identity = Helper.AddClaim(identity, iss, "", scopes.Split(' '), null, "");

                                // access_token
                                string access_token = CmnAccessToken.CreateFromClaims(
                                    iss, identity.Name, identity.Claims,
                                    DateTimeOffset.Now.Add(Config.OAuth2AccessTokenExpireTimeSpanFromMinutes));

                                // オペレーション・トレース・ログ出力
                                Logging.MyOperationTrace(string.Format(
                                    "Passed the 'jwt bearer token flow' by {0}({1}).", iss, sub)); // Client Account

                                ret = CmnEndpoints.CreateAccessTokenResponse(access_token, "", jwkString);
                                return true;
                            }
                            else
                            {
                                // 設定済み
                            }
                        }
                        else
                        {
                            // クライアント認証エラー（Credential（aud）不正
                            err.Add("error", "invalid_client");
                            err.Add("error_description", "Invalid credential.");
                        }
                    }
                    else
                    {
                        // クライアント認証エラー（Credential（署名）不正
                        err.Add("error", "invalid_client");
                        err.Add("error_description", "Invalid credential.");
                    }
                }
                else
                {
                    // クライアント認証エラー（Credential（iss or pubKey）不正
                    err.Add("error", "invalid_client");
                    err.Add("error_description", "Invalid credential or pubkey is not set.");
                }
            }

            return false;
        }

        #endregion

        #endregion

        #region Common (Private, etc.)

        #region　ClientAuthentication

        #region client_id & (client_secret or x509)

        /// <summary>ClientAuthentication</summary>
        /// <param name="client_id">string</param>
        /// <param name="client_secret">string</param>
        /// <param name="x509">X509Certificate2</param>
        /// <param name="permittedLevel">OAuth2AndOIDCEnum.ClientMode</param>
        /// <returns>bool</returns>
        public static bool ClientAuthentication(string client_id, string client_secret,
            ref X509Certificate2 x509, out OAuth2AndOIDCEnum.ClientMode permittedLevel)
        {
            permittedLevel = OAuth2AndOIDCEnum.ClientMode.normal;

            // client_id & client_secret
            if (!string.IsNullOrEmpty(client_id))
            {
                if (!string.IsNullOrEmpty(client_secret))
                {
                    // *.config or Saml2OAuth2Dataテーブルを参照して、
                    // クライアント認証（client_secret）を行なう。
                    if (client_secret == Helper.GetInstance().GetClientSecret(client_id))
                    {
                        //permittedLevel = OAuth2AndOIDCEnum.ClientMode.normal;
                        x509 = null; // client_secretがあった場合、x509を無効化
                        return true;
                    }
                }
                else if (x509 != null)
                {
                    // *.config or Saml2OAuth2Dataテーブルを参照して、
                    // クライアント認証（X509Certificate2）を行なう。
                    if (x509.Subject == Helper.GetInstance().GetTlsClientAuthSubjectDn(client_id))
                    {
                        permittedLevel = OAuth2AndOIDCEnum.ClientMode.fapi2;
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region assertion

        /// <summary>ClientAuthentication</summary>
        /// <param name="client_id">string</param>
        /// <param name="client_secret">string</param>
        /// <param name="x509">X509Certificate2</param>
        /// <param name="permittedLevel">OAuth2AndOIDCEnum.ClientMode</param>
        /// <returns>bool</returns>
        public static bool ClientAuthentication(string assertion, out string client_id,
            ref X509Certificate2 x509, out OAuth2AndOIDCEnum.ClientMode permittedLevel)
        {
            if (!string.IsNullOrEmpty(assertion))
            {
                // assertionがあった場合、x509を無効化
                x509 = null;

                // pubKey
                Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    CustomEncode.ByteToString(CustomEncode.FromBase64UrlString(
                        assertion.Split('.')[1]), CustomEncode.us_ascii));

                string pubKey = Helper.GetInstance().GetJwkRsaPublickey(dic[OAuth2AndOIDCConst.iss]);
                pubKey = CustomEncode.ByteToString(CustomEncode.FromBase64UrlString(pubKey), CustomEncode.us_ascii);

                if (!string.IsNullOrEmpty(pubKey))
                {
                    // 署名検証 ≒ クライアント認証
                    if (JwtAssertion.Verify(
                        assertion, out string iss, out string aud, out string scopes, out JObject jobj, pubKey))
                    {
                        // aud 検証
                        if (aud == Config.OAuth2AuthorizationServerEndpointsRootURI + Config.OAuth2TokenEndpoint)
                        {
                            permittedLevel = OAuth2AndOIDCEnum.ClientMode.fapi1;
                            client_id = iss;
                            return true;
                        }
                    }
                }
            }

            permittedLevel = OAuth2AndOIDCEnum.ClientMode.normal;
            client_id = "";
            return false;
        }

        #endregion

        #endregion

        #region CheckClientMode

        /// <summary>CheckClientMode</summary>
        /// <param name="client_id">ClientId</param>
        /// <param name="permittedLevel">当該フローのClientModeの許容レベル</param>
        /// <param name="jwkString">jwkString</param>
        /// <param name="err">Dictionary(string, string)</param>
        /// <returns>継続の可否</returns>
        private static bool CheckClientMode(
            string client_id,
            OAuth2AndOIDCEnum.ClientMode permittedLevel,
            out string jwkString,
            out Dictionary<string, string> err)
        {
            // ret
            bool retval = false;

            // out
            jwkString = "";
            err = new Dictionary<string, string>();

            // 要求値を最大値に設定
            OAuth2AndOIDCEnum.ClientMode clientModeEnum = OAuth2AndOIDCEnum.ClientMode.fapi2;

            // clientMode <= permittedLevel であればOK。
            string clientModeString = "";
            if (string.IsNullOrEmpty(client_id))
            {
                err.Add("error", "invalid_client");
                err.Add("error_description", string.Format("client_id is not set."));
                return false; // NullOrEmptyだとmode無しとかになるのでここで切る。
            }
            else
            {
                clientModeString = Helper.GetInstance().GetClientMode(client_id);

                if (clientModeString == OAuth2AndOIDCEnum.ClientMode.normal.ToStringByEmit())
                {
                    clientModeEnum = (int)OAuth2AndOIDCEnum.ClientMode.normal;
                }
                else if (clientModeString == OAuth2AndOIDCEnum.ClientMode.fapi1.ToStringByEmit())
                {
                    clientModeEnum = OAuth2AndOIDCEnum.ClientMode.fapi1;
                }
                else if (clientModeString == OAuth2AndOIDCEnum.ClientMode.fapi2.ToStringByEmit())
                {
                    clientModeEnum = OAuth2AndOIDCEnum.ClientMode.fapi2;
                    jwkString = CustomEncode.ByteToString(CustomEncode.FromBase64UrlString(
                        Helper.GetInstance().GetJwkRsaPublickey(client_id)), CustomEncode.us_ascii);
                }
            }

            if ((int)clientModeEnum <= (int)permittedLevel)
            {
                // permittedLevel == normal
                retval = true;
            }
            else
            {
                // permittedLevel != normal
                retval = false;
            }

            if (!retval)
            {
                // エラーを追加
                err.Add("error", "not_supported");

                if (string.IsNullOrEmpty(clientModeString))
                {
                    err.Add("error_description", string.Format("This client is not set the mode."));
                }
                else
                {
                    err.Add("error_description", string.Format(
                        "This client is set the {0} mode, but this flow permitted up to {1} mode.",
                        clientModeString, permittedLevel.ToStringByEmit()));
                }
            }

            return retval;
        }

        #endregion

        #region CreateAccessTokenResponse

        /// <summary>CreateAccessTokenResponse</summary>
        /// <param name="access_token">string</param>
        /// <param name="refresh_token">string</param>
        /// <param name="jwkString">string</param>
        /// <returns>Dictionary(string, string)</returns>
        private static Dictionary<string, string> CreateAccessTokenResponse(
            string access_token, string refresh_token, string jwkString)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            // token_type
            ret.Add(OAuth2AndOIDCConst.token_type, OAuth2AndOIDCConst.Bearer.ToLower());

            // access_token
            ret.Add(OAuth2AndOIDCConst.AccessToken, access_token);

            // refresh_token
            if (!string.IsNullOrEmpty(refresh_token))
            {
                ret.Add(OAuth2AndOIDCConst.RefreshToken, refresh_token);
            }

            JObject jObj = (JObject)JsonConvert.DeserializeObject(
                            CustomEncode.ByteToString(CustomEncode.FromBase64UrlString(
                                access_token.Split('.')[1]), CustomEncode.us_ascii));

            // id_token
            JArray jAry = (JArray)jObj["scopes"];
            foreach (string s in jAry)
            {
                if (s == OAuth2AndOIDCConst.Scope_Openid)
                {
                    string id_token = "";
                    if (string.IsNullOrEmpty(jwkString))
                    {
                        id_token = CmnIdToken.ChangeToIdTokenFromAccessToken(
                            access_token, "", "", // c_hash, s_hash は /token で生成不可
                            HashClaimType.None, Config.RsaPfxFilePath, Config.RsaPfxPassword, jwkString);
                    }
                    else
                    {
                        id_token = CmnIdToken.ChangeToIdTokenFromAccessToken(
                            access_token, "", "", // c_hash, s_hash は /token で生成不可
                            HashClaimType.None, Config.EcdsaPfxFilePath, Config.EcdsaPfxPassword, jwkString);
                    }

                    if (!string.IsNullOrEmpty(id_token))
                    {
                        ret.Add(OAuth2AndOIDCConst.IDToken, id_token);
                    }
                }
            }

            // expires_in
            ret.Add(OAuth2AndOIDCConst.expires_in, Config.OAuth2AccessTokenExpireTimeSpanFromMinutes.Seconds.ToString());

            return ret;
        }

        #endregion

        #endregion
    }
}