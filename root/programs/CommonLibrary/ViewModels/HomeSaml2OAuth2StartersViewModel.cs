﻿//**********************************************************************************
//* テンプレート
//**********************************************************************************

// 以下のLicenseに従い、このProjectをTemplateとして使用可能です。Release時にCopyright表示してSublicenseして下さい。
// https://github.com/OpenTouryoProject/MultiPurposeAuthSite/blob/master/license/LicenseForTemplates.txt

//**********************************************************************************
//* クラス名        ：HomeSaml2OAuth2StartersViewModel
//* クラス日本語名  ：Home > Saml2OAuth2StartersのVM（テンプレート）
//*
//* 作成日時        ：－
//* 作成者          ：－
//* 更新履歴        ：－
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2019/02/16  西野 大介         新規
//*  2019/05/2*  西野 大介         SAML2対応実施
//**********************************************************************************

using MultiPurposeAuthSite.Co;

#if NETFX
using System.Web.Mvc;
#elif NETCORE
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

using Touryo.Infrastructure.Framework.Authentication;
using Touryo.Infrastructure.Public.FastReflection;

/// <summary>MultiPurposeAuthSite.ViewModels</summary>
namespace MultiPurposeAuthSite.ViewModels
{
    /// <summary>Home > Saml2OAuth2StartersのVM</summary>
    public class HomeSaml2OAuth2StartersViewModel : BaseViewModel
    {
        /// <summary>ClarifyRedirectUri</summary>
        [Display(Name = "ClarifyRedirectUri", ResourceType = typeof(Resources.CommonViewModels))]
        public bool ClarifyRedirectUri { get; set; }

        /// <summary>ClientType</summary>
        [Display(Name = "ClientType", ResourceType = typeof(Resources.CommonViewModels))]
        public string ClientType { get; set; }

        /// <summary>ClientTypeアイテムリスト</summary>
        public List<SelectListItem> DdlClientTypeItems
        {
            get
            {
                return new List<SelectListItem>()
                {
                    new SelectListItem() {
                        Text = "Saml2 / OAuth2.0 / OIDC用 Client",
                        Value = OAuth2AndOIDCEnum.ClientMode.normal.ToStringByEmit() },
                    new SelectListItem() {
                        Text = "Financial-grade API - Part1用 Client",
                        Value = OAuth2AndOIDCEnum.ClientMode.fapi1.ToStringByEmit() },
                    new SelectListItem() {
                        Text = "Financial-grade API - Part2用 Client",
                        Value = OAuth2AndOIDCEnum.ClientMode.fapi2.ToStringByEmit() },
                    new SelectListItem() {
                        Text = "ログイン・ユーザの Client",
                        Value = "login User" }
                };
            }
        }

        /// <summary>ResponseMode</summary>
        [Display(Name = "ResponseMode", ResourceType = typeof(Resources.CommonViewModels))]
        public string ResponseMode { get; set; }

        /// <summary>ResponseModeアイテムリスト</summary>
        public List<SelectListItem> DdlResponseModeItems
        {
            get
            {
                return new List<SelectListItem>()
                {
                    new SelectListItem() {
                        Text = "default",
                        Value = "" },
                    new SelectListItem() {
                        Text = OAuth2AndOIDCEnum.ResponseMode.query.ToStringByEmit(),
                        Value = OAuth2AndOIDCEnum.ResponseMode.query.ToStringByEmit() },
                    new SelectListItem() {
                        Text = OAuth2AndOIDCEnum.ResponseMode.fragment.ToStringByEmit(),
                        Value = OAuth2AndOIDCEnum.ResponseMode.fragment.ToStringByEmit() },
                    new SelectListItem() {
                        Text = OAuth2AndOIDCEnum.ResponseMode.form_post.ToStringByEmit(),
                        Value = OAuth2AndOIDCEnum.ResponseMode.form_post.ToStringByEmit() },
                                        new SelectListItem() {
                        Text = OAuth2AndOIDCEnum.ResponseMode.query_jwt.ToStringByEmit(),
                        Value = OAuth2AndOIDCEnum.ResponseMode.query_jwt.ToStringByEmit() },
                    new SelectListItem() {
                        Text = OAuth2AndOIDCEnum.ResponseMode.fragment_jwt.ToStringByEmit(),
                        Value = OAuth2AndOIDCEnum.ResponseMode.fragment_jwt.ToStringByEmit() },
                    new SelectListItem() {
                        Text = OAuth2AndOIDCEnum.ResponseMode.form_post_jwt.ToStringByEmit(),
                        Value = OAuth2AndOIDCEnum.ResponseMode.form_post_jwt.ToStringByEmit() }
                };
            }
        }
    }
}