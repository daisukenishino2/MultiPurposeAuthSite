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
    public class AccountController {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AccountController() {
        }
        
        /// <summary>
        ///   このクラスで使用されているキャッシュされた ResourceManager インスタンスを返します。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MultiPurposeAuthSite.Resources.AccountController", typeof(AccountController).Assembly);
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
        ///   Invalid code. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string InvalidCode {
            get {
                return ResourceManager.GetString("InvalidCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   E-Mail Confirmation is needed ! に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string Login_emailconfirm {
            get {
                return ResourceManager.GetString("Login_emailconfirm", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Invalid Sign-in attempt. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string Login_Error {
            get {
                return ResourceManager.GetString("Login_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Confirm e-mail account に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string SendEmail_emailconfirm {
            get {
                return ResourceManager.GetString("SendEmail_emailconfirm", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Please confirm your e-mail account by clicking &lt;a href=&quot;{0}&quot;&gt;here&lt;/a&gt;. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string SendEmail_emailconfirm_msg {
            get {
                return ResourceManager.GetString("SendEmail_emailconfirm_msg", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Reset password に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string SendEmail_passwordreset {
            get {
                return ResourceManager.GetString("SendEmail_passwordreset", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Please reset your password by clicking &lt;a href=&quot;{0}&quot;&gt;here&lt;/a&gt;. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        public static string SendEmail_passwordreset_msg {
            get {
                return ResourceManager.GetString("SendEmail_passwordreset_msg", resourceCulture);
            }
        }
    }
}
