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
//* クラス名        ：OAuth2ResourceServerController
//* クラス日本語名  ：OAuth2ResourceServerのApiController
//*
//* 作成日時        ：－
//* 作成者          ：－
//* 更新履歴        ：－
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2017/04/24  西野 大介         新規
//*  2018/12/26  西野 大介         分割
//**********************************************************************************

using MultiPurposeAuthSite.Co;
using MultiPurposeAuthSite.Data;
using MultiPurposeAuthSite.Network;

using MultiPurposeAuthSite.Extensions.Sts;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Touryo.Infrastructure.Business.Presentation;
using Touryo.Infrastructure.Framework.Authentication;

/// <summary>MultiPurposeAuthSite.Controllers</summary>
namespace MultiPurposeAuthSite.Controllers
{
    /// <summary>OAuth2ResourceServerのApiController（ライブラリ）</summary>
    [EnableCors]
    //[ApiController]
    [MyBaseAsyncApiController()]
    public class OAuth2ResourceServerController : ControllerBase
    {
        #region Hybrid Flow

        /// <summary>
        /// Hybrid Flowのテスト用エンドポイント
        /// POST: /TestHybridFlow
        /// </summary>
        /// <param name="formData">code</param>
        /// <returns>Dictionary(string, string)</returns>
        [HttpPost]
        public async Task<Dictionary<string, object>> TestHybridFlow(IFormCollection formData)
        {
            // 変数
            string code = formData[OAuth2AndOIDCConst.code];

            // Tokenエンドポイントにアクセス
            Uri tokenEndpointUri = new Uri(
                Config.OAuth2AuthorizationServerEndpointsRootURI + Config.OAuth2TokenEndpoint);

            // 結果を格納する変数。
            Dictionary<string, object> dic = null;

            //  client_Idから、client_secretを取得。
            string client_id = Helper.GetInstance().GetClientIdByName("TestClient");
            string client_secret = Helper.GetInstance().GetClientSecret(client_id);

            // Hybridは、Implicitのredirect_uriを使用
            string redirect_uri 
                = Config.OAuth2ClientEndpointsRootURI
                + Config.OAuth2ImplicitGrantClient_Account;

            // Tokenエンドポイントにアクセス
            string response = await Helper.GetInstance()
            .GetAccessTokenByCodeAsync(tokenEndpointUri, client_id, client_secret, redirect_uri, code, "");
            dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

            // UserInfoエンドポイントにアクセス
            dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                await Helper.GetInstance().GetUserInfoAsync((string)dic[OAuth2AndOIDCConst.AccessToken]));

            return dic;
        }

        #endregion

        #region Chage

        /// <summary>
        /// 課金テスト用エンドポイント
        /// POST: /TestChageToUser
        /// </summary>
        /// <param name="formData">
        /// - currency
        /// - amount
        /// </param>
        /// <returns>string</returns>
        [HttpPost]
        public async Task<string> TestChageToUser(IFormCollection formData)
        {
            // Claimを取得する。
            MyBaseAsyncApiController.GetClaims(
                out string userName, out string roles, out string scopes, out string ipAddress);

            // ユーザの検索
            ApplicationUser user = CmnUserStore.FindByName(userName);

            if (user != null)
            {
                // 変数
                string currency = formData["currency"];
                string amount = formData["amount"];

                if (Config.CanEditPayment
                    && Config.EnableEditingOfUserAttribute
                    && Config.IsDebug)
                {
                    // 課金のテスト処理
                    JObject jobj = await WebAPIHelper.GetInstance()
                        .ChargeToOnlinePaymentCustomersAsync(user.PaymentInformation, currency, amount);

                    return "OK";
                }
            }

            return "NG";
        }

        #endregion
    }
}