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
//* クラス名        ：CreateJwtBearerTokenFlowAssertion.Program
//* クラス日本語名  ：Jwt Bearer Token FlowのJwt Assertion生成ツール（ライブラリ）
//*
//* 作成日時        ：－
//* 作成者          ：－
//* 更新履歴        ：－
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2017/12/25  西野 大介         新規
//*  2018/11/27  西野 大介         XML(Base64) ---> Jwk(Base64Url)に変更。
//*  2019/02/13  西野 大介         自動生成から、証明書利用に変更。
//**********************************************************************************

using System;
using System.Security.Cryptography;

using Newtonsoft.Json.Linq;

using Touryo.Infrastructure.Framework.Authentication;

using Touryo.Infrastructure.Public.Str;
using Touryo.Infrastructure.Public.Util;
using Touryo.Infrastructure.Public.Security;
using Touryo.Infrastructure.Public.Security.Jwt;

namespace CreateJwtBearerTokenFlowAssertion
{
    class Program
    {
        static void Main(string[] args)
        {
#if NETCORE
            // configの初期化
            GetConfigParameter.InitConfiguration("appsettings.json");
#endif

            string jwkPrivateKey = "";
            string jwkPublicKey = "";

            string iss = CmnClientParams.Isser;
            string aud = OAuth2AndOIDCParams.Audience;
            string scopes = "hoge1 hoge2 hoge3";
            JObject jobj = null;

            DigitalSignX509 dsX509 = new DigitalSignX509(
                CmnClientParams.RsaPfxFilePath,
                CmnClientParams.RsaPfxPassword, HashAlgorithmName.SHA256);

            #region PrivateKey
            Console.WriteLine("PrivateKey:");

            RsaPrivateKeyConverter rpvkc = new RsaPrivateKeyConverter(JWS_RSA.RS._256);
            jwkPrivateKey = rpvkc.ParamToJwk(((RSA)dsX509.X509Certificate.PrivateKey).ExportParameters(true));
            jwkPrivateKey = CustomEncode.ToBase64UrlString(CustomEncode.StringToByte(jwkPrivateKey, CustomEncode.us_ascii));

            Console.WriteLine(jwkPrivateKey);
            Console.WriteLine("");
            #endregion

            #region PublicKey
            Console.WriteLine("PublicKey:");

            RsaPublicKeyConverter rpbkc = new RsaPublicKeyConverter(JWS_RSA.RS._256);
            jwkPublicKey = rpbkc.ParamToJwk(((RSA)dsX509.X509Certificate.PublicKey.Key).ExportParameters(false));
            jwkPublicKey = CustomEncode.ToBase64UrlString(CustomEncode.StringToByte(jwkPublicKey, CustomEncode.us_ascii));

            Console.WriteLine(jwkPublicKey);
            Console.WriteLine("");
            #endregion

            #region Check
            string jwtAssertion = JwtAssertion.Create(
                CmnClientParams.Isser, OAuth2AndOIDCParams.Audience, new TimeSpan(0, 30, 0), scopes,
                CustomEncode.ByteToString(CustomEncode.FromBase64UrlString(jwkPrivateKey), CustomEncode.us_ascii));

            if (JwtAssertion.Verify(
                jwtAssertion, out iss, out aud, out scopes, out jobj,
                CustomEncode.ByteToString(CustomEncode.FromBase64UrlString(jwkPublicKey), CustomEncode.us_ascii)))
            {
                if (iss == CmnClientParams.Isser
                    && aud == OAuth2AndOIDCParams.Audience)
                {
                    Console.WriteLine("JwtAssertion:");
                    Console.WriteLine(jwtAssertion);
                    Console.WriteLine("");
                    Console.ReadLine();

                    return;
                }
            }

            #endregion

            Console.WriteLine("Error");
            Console.ReadLine();
        }
    }
}
