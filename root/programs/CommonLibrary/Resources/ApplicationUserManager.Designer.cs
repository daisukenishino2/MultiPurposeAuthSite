﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace MultiPurposeAuthSite.Resources {
    using System;
    
    
    /// <summary>
    ///   ローカライズされた文字列などを検索するための、厳密に型指定されたリソース クラスです。
    /// </summary>
    // このクラスは StronglyTypedResourceBuilder クラスが ResGen
    // または Visual Studio のようなツールを使用して自動生成されました。
    // メンバーを追加または削除するには、.ResX ファイルを編集して、/str オプションと共に
    // ResGen を実行し直すか、または VS プロジェクトをビルドし直します。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ApplicationUserManager {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ApplicationUserManager() {
        }
        
        /// <summary>
        ///   このクラスで使用されているキャッシュされた ResourceManager インスタンスを返します。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MultiPurposeAuthSite.Resources.ApplicationUserManager", typeof(ApplicationUserManager).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   厳密に型指定されたこのリソース クラスを使用して、すべての検索リソースに対し、
        ///   現在のスレッドの CurrentUICulture プロパティをオーバーライドします。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Email Code に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string EmailCode {
            get {
                return ResourceManager.GetString("EmailCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Your security code is {0}. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string EmailCode_body {
            get {
                return ResourceManager.GetString("EmailCode_body", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Security Code に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string EmailCode_sub {
            get {
                return ResourceManager.GetString("EmailCode_sub", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Phone Code に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string PhoneCode {
            get {
                return ResourceManager.GetString("PhoneCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Your security code is {0}. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string PhoneCode_msg {
            get {
                return ResourceManager.GetString("PhoneCode_msg", resourceCulture);
            }
        }
    }
}
