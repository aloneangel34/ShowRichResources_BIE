//引入命名空间（本.cs文件需要用到的、不同于“本文件命名空间”的命名空间，要加在这里）
//想要引入的命名空间找不到，则可能是没有将相应.dll（或其他类似的东西）加入项目引用的缘故
using BepInEx;
using BepInEx.Logging;
using GameData;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaiwuUIKit.GameObjects;
using UnityEngine;
using UnityUIKit.Core.GameObjects;
using UnityUIKit.Components;
using UnityUIKit.GameObjects;
using YanLib.ModHelper;

//本.cs文件的命名空间
namespace ShowRichResources
{
    //插件描述特性，小括号内依次分别为：插件ID 插件名字 插件版本(必须为数字)
    //插件ID：用于识别不同的插件
    //插件名字：在BepInEx的控制台里所输出的插件显示名称（但并非是太吾游戏中MOD设置界面的显示名）
    //插件版本：只能有数字，用英文半角句号“.”分隔，不能含有字母、特殊字符
    //注意下方用的是字符串常数（方便更改，后续代码中可能还会调用），也可以直接手输字符串
    /// <summary> Mod 入口 </summary> 插件描述特性
    [BepInPlugin(GUID, ModDisplayName, Version)]
    //限制插件所能加载的程序（这行特性可以“复制粘贴改程序名”这样来重复加：用来指定多个程序。也可以不加删掉：不作限制）
    [BepInProcess("The Scroll Of Taiwu Alpha V1.0.exe")]
    //插件的硬性前置依赖MOD/插件（会先载入依赖再载入本MOD，没找到前置则不载入）
    //这里的前置依赖是：YanCore插件，提供了太吾游戏内的MOD设置界面和其他一些功能（具体去翻YanCore插件的发布页面）
    [BepInDependency("0.0Yan.Lib")]
    public class Main : BaseUnityPlugin
    {
        /// <summary>插件版本</summary>
        public const string Version = "1.2.1";
        /// <summary>插件名字</summary>
        public const string ModDisplayName = "ShowRichResources/显示丰富资源";
        //注意这个GUID好像是YanCore要用到的
        /// <summary>插件ID</summary>
        public const string GUID = "TaiwuMOD.ShowRichResources";
        /// <summary>日志</summary> （声明公开静态……？不懂）
        public static new ManualLogSource Logger;
        //YanLib的MOD设置界面（声明公开静态……？不懂）
        /// <summary>MOD设置界面</summary>
        public static ModHelper Mod;
        //Setting.cs里的设置选项（声明公开静态……？不懂）
        /// <summary>设置</summary>
        public static Settings Setting;

        //不是很懂Awake/Main 的区别
        //本MOD迁移前是没有Awake的；然后作为BIE插件示例的GOD MOD里，则是只有Awake，没有Main。
        //我还没学到Awake，于是胡乱把Awake硬套进Main里面去了…居然没出错…不懂…
        private void Awake()
        {
            //不懂
            DontDestroyOnLoad(this);
            //不懂
            Logger = base.Logger;
            //为啥这里后面有new和()？上面就没？不懂，还没学到
            Setting = new Settings();
            //执行设置选项初始化
            Setting.Init(Config);

            #region 自动加载Patch相关，本MOD未用到的部分，如需自动加载Patch可以取消注释（也作为分析“零下萝莉大佬提供的GOD示例”的笔记）
            /*
            //创建线程在背景初始化【自动加载Patch】
            Thread athread = new Thread(new ThreadStart(HarmonyPatches.Init))
            {
                IsBackground = true
            };
            athread.Start();
            */
            #endregion


            //再次提醒：需要YanCore前置插件作为MOD设置界面的支持，才会在游戏中显示
            //给之前声明的MOD赋值（原本是NULL?）
            //也就是创建MOD设置界面
            //括号内两个参数为（插件GUID，该插件在MOD界面的显示名）
            //（这里的显示名用的是两个字符串常数连接的方式，也可以直接手动输入字符串如"想显示的MOD名"））
            Mod = new ModHelper(GUID, ModDisplayName + Version);
            //设置MOD设置界面UI【自适应垂直UI组：MOD设置UI界面】这是MOD设置UI的总框(BOX)
            Mod.SettingUI = new BoxAutoSizeModelGameObject()
            {
                //该组件内的子组件排布设定
                Group =
                {
                    //子组件垂直排布
                    Direction = UnityUIKit.Core.Direction.Vertical,
                    //子组件排布间隔
                    Spacing = 10,
                },
                //大小自适应设置
                SizeFitter =
                {
                    //垂直高度自适应
                    VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize
                },
                //该组件内的子组件
                Children =
                {
                    //【空白标签：调整排布用】
                    new TaiwuLabel()
                    {
                        //这里用作“在按下MOD开关时，不会被隐藏”的凭据
                        //（一些MOD在按了MOD开关按钮后、开关按钮的位置会上下跳动，大约就是没有把那些排布用的组件排除在“切换显示/隐藏”之外吧？）
                        Name = "调整排布用",
                        //不知道怎么调整第一个子组件的空间布局位置，所以尝试用插入一个空白标签的方式来实现
                        Element = { PreferredSize = { 0, 10 } },
                        //不懂，貌似不是太吾样式大边框的设定项，可能是文字描边？
                        //UseOutline = false,
                    },
                    //【MOD总开关】
                    new TaiwuToggle()
                    {
                        //组件的Name ID（是字符串，但并非该组件在游戏中的显示名称）
                        Name = "MOD总开关",
                        //组件的显示文本（会显示在UI上）
                        Text = "显示丰富资源 启用开关",
                        //开关组件的开关状态
                        isOn = Setting.enabled.Value,
                        //当数值改变时的操作
                        onValueChanged = (bool value, Toggle tg) =>
                        {
                            #region 给新手（我自己）的笔记，非实际代码，可删除
                            //先解释一下 Setting.enabled.Value
                            //enabled 就是 Setting.cs 里设定的 public ConfigEntry<bool> enabled;
                            //但因为不是在这里面创建的所以要加上【Setting.】调用（请参看下方的 /* 更多说明 */）
                            //但依然不能给 Setting.enabled 赋值，因为这个 ConfigEntry<bool> 其实是个类
                            //所以还要在加上【.Value】来方便赋值与取值

                            /*
                            //声明公开字段Setting，引用Settings类
                            //注意这里引用的是（命名空间）namespace ShowRichResources下的（类）class Settings
                            //而不是引用Setting.cs这个文件（所以这个文件怎么改名都没关系）
                            public static Settings Setting;
                            
                            private void Awake()
                            {
                                ...
                                Setting = new Settings();
                                ...
                            }
                            */
                            #endregion

                            //将变动后的数值 赋值给 Setting.enabled.Value
                            //（bool型数值）“value”，是由这个动作 onValueChanged = (bool 「value」, Toggle tg) 传出来的
                            Setting.enabled.Value = value;

                            #region （弃用案，仅留下作为笔记）
                            /* 
                            //重新设定本组件的显示文本（有时开启、关闭的显示不正确，不造咋回事，弃用）
                            //“tg”就代指本组件，是由这个动作 onValueChanged = (bool value, Toggle 「tg」) 传出来的
                            //“.”可以理解为“的”。“tg.Text”连起来就是“本组件 的 Text”
                            //A ? B : C 则相当于 if(A){B} else{C}
                            tg.Text = "显示丰富资源 " + (Setting.enabled.Value ? "开启" : "关闭");
                            */
                            #endregion

                            //开关MOD时直接将会应用/卸载 Patch（该方式需要YanCore作为支持）
                            if(value) HarmonyPatches.PatchHandlers["显示丰富资源"].Patch(HarmonyPatches.harmony);
                            else HarmonyPatches.PatchHandlers["显示丰富资源"].Unpatch(HarmonyPatches.harmony);

                            //刷新地块图标（带是否在游戏中的判定）（注：RefreshResourcesIcon有做修改）
                            //开启MOD时，调用本MOD的方法刷新；
                            //关闭MOD时，调用游戏原本的方法刷新。（因为上面已经Unpatch了，所以可以直接调用原生方式。）
                            RefreshResourcesIcon();

                            //将本组件（tg）的父组件（Parent）中的所有子组件（Children）【即可以理解为同级组件】
                            foreach (UnityUIKit.Core.ManagedGameObject managedGameObject in tg.Parent.Children)
                            {
                                //排除自身（Name == "MOD总开关"）排除调整排布用（Name == "调整排布用"）】
                                bool flag = (managedGameObject.Name == "MOD总开关" || managedGameObject.Name == "调整排布用");
                                if (!flag)
                                {
                                    //切换组件的显示/隐藏“SetActive(value)”这里的value来自【onValueChanged = (bool value, Toggle tg)】中的value参数（这个局部参数的名字可以自己改）
                                    managedGameObject.SetActive(value);
                                }
                            }
                        },
                        //设定该元素的首选宽度、首选高度。
                        //边框留白大约12.5，上下合计 或 左右合计 留白大约25。（游戏中MOD设置UI中）单个文字长宽约25。
                        //例比如1个文字，建议设为{ 50, 50 }；6个文字，建议设为{ 175, 50 } (宽度为左右留白25 + 文字宽度25 x 文字字数6)
                        //如果想要 宽度, 高度 自适应，则将对应项设为0即可。除了TaiwuToggle()类，不建议两项皆设为0（可能会导致外边框不显示？我不确定）
                        Element = { PreferredSize = { 0 , 60 } },
                        //粗体
                        UseBoldFont = true,
                    },
                    //【垂直UI容器：选项设置 分隔栏】
                    new Container()
                    {
                        //不建议用设定 Name 的方式来辅助记忆，如果不需要调用操作的话，那组件就不用设定 Name 属性
                        Group =
                        {
                            //子组件垂直排布
                            Direction = UnityUIKit.Core.Direction.Vertical,
                            //子组件排布间隔
                            Spacing = 10,
                        },
                        Element =
                        {
                            //设定首选高度为60。
                            PreferredSize = { 0 , 60 }
                        },
                        Children =
                        {
                            //【标签：选项设置 分隔用】
                            new TaiwuLabel()
                            {
                                //标签文本，采用<color=#DDCCAA>较亮</color><color=#998877>柔和</color>两种文本颜色。默认的颜色太暗了
                                Text = "<color=#DDCCAA>选项设置</color><color=#998877>（更多说明请参看BepInEx\\config文件夹内的cfg文件）</color>",
                                //设定首选高度为60。
                                Element = { PreferredSize = { 0, 60 } },
                                //粗体
                                UseBoldFont = true,
                                UseOutline = true,
                            },
                        }
                    },
                    //【水平UI组：以地块资源上限显示 开关】
                    new Container()
                    {
                        //不建议用设定 Name 的方式来辅助记忆，如果不需要调用操作的话，那组件就不用设定 Name 属性
                        Group =
                        {
                            //子组件水平排布
                            Direction = UnityUIKit.Core.Direction.Horizontal,
                            Spacing = 10,
                        },
                        Element =
                        {
                            //设定首选高度为50。
                            PreferredSize = { 0, 50 }
                        },
                        Children =
                        {
                            //【开关：以地块资源上限显示】
                            new TaiwuToggle()
                            {
                                Text = "以地块资源上限显示",
                                isOn = Setting.showMax.Value,
                                //当数值改变时（开关按钮）
                                onValueChanged = (bool value, Toggle tg) =>
                                {
                                    Setting.showMax.Value = value;
                                    
                                    RefreshResourcesIcon();
                                },
                                Element = { PreferredSize = { 275 } }
                            },
                            //【标签：显示上限 说明】
                            new TaiwuLabel()
                            {
                                Text = "<color=#998877>开启：以地块的资源上限值显示资源丰富图标。关闭：以当前资源值显示。</color>",
                                Element = { PreferredSize = { 0, 50 } },
                                UseOutline = true,
                            },
                        }
                    },
                    //【水平UI组：门派城镇采集修正 开关】
                    new Container()
                    {
                        //不建议用设定 Name 的方式来辅助记忆，如果不需要调用操作的话，那组件就不用设定 Name 属性
                        Group =
                        {
                            //子组件水平排布
                            Direction = UnityUIKit.Core.Direction.Horizontal,
                            Spacing = 10,
                        },
                        Element =
                        {
                            PreferredSize = { 0, 50 }
                        },
                        Children =
                        {
                            //【开关：门派城镇采集修正】
                            new TaiwuToggle()
                            {
                                Text = "门派城镇采集修正计算",
                                isOn = Setting.cityHalve.Value,
                                onValueChanged = (bool value, Toggle tg) =>
                                {
                                    Setting.cityHalve.Value = value;

                                    RefreshResourcesIcon();
                                },
                                Element = { PreferredSize = { 275 } }
                            },
                            //【标签：采集修正 说明】
                            new TaiwuLabel()
                            {
                                Text = "<color=#998877>显示门派城镇地块资源时是否将数值减半计算（门派城镇地块采集需要2人力）</color>",
                                Element = { PreferredSize = { 0, 50 } },
                                UseOutline = true,
                            },
                        },
                    },
                    //【水平UI组：仅显示最高一项资源 开关】
                    new Container()
                    {
                        Group =
                        {
                            //子组件水平排布
                            Direction = UnityUIKit.Core.Direction.Horizontal,
                            Spacing = 10,
                        },
                        Element =
                        {
                            PreferredSize = { 0, 50 }
                        },
                        Children =
                        {
                            //【开关：仅显示最高一项资源】
                            new TaiwuToggle()
                            {
                                Text = "仅显示最高一项资源",
                                isOn = Setting.showSingleIcon.Value,
                                onValueChanged = (bool value, Toggle tg) =>
                                {
                                    Setting.showSingleIcon.Value = value;

                                    RefreshResourcesIcon();
                                },
                                Element = { PreferredSize = { 275 } }
                            },
                            //【标签：显示最高 说明】
                            new TaiwuLabel()
                            {
                                Text = "<color=#998877>是否仅精准显示满足阈值的最高一项资源图标（除银钱）。关闭后会显示两项、但不精准</color>",
                                Element = { PreferredSize = { 0, 50 } },
                                UseOutline = true,
                            },
                        }
                    },
                    //【水平UI组：不显示被天灾破坏地块 开关】
                    new Container()
                    {
                        Group =
                        {
                            //子组件水平排布
                            Direction = UnityUIKit.Core.Direction.Horizontal,
                            Spacing = 10,
                        },
                        Element =
                        {
                            PreferredSize = { 0, 50 }
                        },
                        Children =
                        {
                            //【开关：不显示天灾破坏地块】
                            new TaiwuToggle()
                            {
                                Text = "不显示被天灾破坏地块",
                                isOn = Setting.banDestroied.Value,
                                onValueChanged = (bool value, Toggle tg) =>
                                {
                                    Setting.banDestroied.Value = value;
                                    
                                    RefreshResourcesIcon();
                                },
                                Element = { PreferredSize = { 275 } }
                            },
                            //【标签：不显示破坏 说明】
                            new TaiwuLabel()
                            {
                                Text = "<color=#998877>是否屏蔽处于被破坏状态地块的资源丰富图标</color>",
                                Element = { PreferredSize = { 0, 50 } },
                                UseOutline = true,
                            },
                        }
                    },
                    //【自适应垂直UI组：阈值 输入设定组】
                    new BoxAutoSizeModelGameObject()
                    {
                        Group =
                        {
                            //子组件垂直排布
                            Direction = UnityUIKit.Core.Direction.Vertical,
                            Spacing = 10,
                        },
                        //垂直大小自动适应
                        SizeFitter = { VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize },
                        Children =
                        {
                            //【标签：阈值设定提示】
                            new TaiwuLabel()
                            {
                                //既可以使用颜色代码，也可以使用转义符，但并不是所有<>代码都支持
                                Text = "<color=#DDCCAA>阈值修改范围（1～999）</color>　<color=#998877>游戏原生阈值全部为：100\n【村民采集建议阈值】物料：150　银钱：70 　门派城镇采集修正：开\n【挖掘素材建议阈值】物料：150　银钱：100　门派城镇采集修正：关\n注意：只是更改了图标的显示阈值，实际地块资源量未变动</color>",
                                Element = { PreferredSize = { 0, 110 } },
                                UseOutline = true,
                            },
                            //【水平UI组：物料阈值 输入设定】
                            new Container()
                            {
                                Group =
                                {
                                    //子组件水平排布
                                    Direction = UnityUIKit.Core.Direction.Horizontal,
                                    Spacing = 25
                                },
                                Element =
                                {
                                    //设定首选宽度为600（防止输入框过宽）。首选高度为60（和输入栏的高度一致）。
                                    PreferredSize = { 1200 , 60 }
                                },
                                Children =
                                {
                                    //【标签：物料阈值】
                                    new TaiwuLabel()
                                    {
                                        Text = "<color=#998877>物料资源显示阈值</color>",
                                        Element = { PreferredSize = { 200, 50 } },

                                        UseOutline = true,
                                    },
                                    //【输入栏：物料阈值】
                                    new TaiwuInputField()
                                    {
                                        Text = Setting.yuZhi_Materials.Value.ToString(),
                                        //输入栏为空时的提示文本
                                        Placeholder = "输入物料显示阈值（1～999）",
                                        //限制输入整数数字，需要调用到UnityEngine.UI中基类的枚举参数
                                        ContentType = (UnityEngine.UI.InputField.ContentType)Enum.Parse(typeof(UnityEngine.UI.InputField.ContentType),"IntegerNumber"),
                                        //限制最多输入三个字符，配合上一行、即达成可输入范围：-99 ～ 999
                                        CharacterLimit = 3,
                                        //结束修改时的操作（输入完按下回车、点击其他地方等 只要焦点不在输入栏就算结束修改）
                                        OnEndEdit = (string thresh1, InputField thresh1Input) =>
                                        {
                                            //试图将输入的字符串 thresh1 转换为整数 result1
                                            int.TryParse(thresh1, out int result1);

                                            //判定 result1 是否在大于 0，若满足则修改物料阈值 yuZhi_Materials 
                                            if (result1 > 0)
                                            {
                                                Setting.yuZhi_Materials.Value = result1;

                                                //刷新地块图标（带判定）
                                                RefreshResourcesIcon();
                                            }
                                            //若不满足，则将输入框的显示文本重设回物料阈值 yuZhi_Materials
                                            else
                                            {   thresh1Input.Text = Setting.yuZhi_Materials.Value.ToString();   }
                                        }
                                    },
                                    //【标签：银钱阈值】
                                    new TaiwuLabel()
                                    {
                                        Text = "<color=#998877>银钱资源显示阈值</color>",
                                        Element = { PreferredSize = { 200, 50 } },
                                        UseOutline = true,
                                    },
                                    //【输入框：银钱阈值】
                                    new TaiwuInputField()
                                    {
                                        Text = Setting.yuZhi_Money.Value.ToString(),
                                        //输入栏为空时的提示文本
                                        Placeholder = "输入银钱显示阈值（1～999）",
                                        //限制输入整数数字，需要调用到UnityEngine.UI中基类的枚举参数
                                        ContentType = (UnityEngine.UI.InputField.ContentType)Enum.Parse(typeof(UnityEngine.UI.InputField.ContentType),"IntegerNumber"),
                                        //限制最多输入三个字符，配合上一行、即达成可输入范围：-99 ～ 999
                                        CharacterLimit = 3,
                                        OnEndEdit = (string thresh2, InputField thresh2Input) =>
                                        {
                                            //试图将输入的字符串 thresh2 转换为整数 result2
                                            int.TryParse(thresh2, out int result2);

                                            //判定 result2 是否大于 0，若满足则修改银钱阈值 yuZhi_Money
                                            if (result2 > 0)
                                            {
                                                Setting.yuZhi_Money.Value = result2;

                                                //刷新地块图标（带判定）
                                                RefreshResourcesIcon();
                                            }
                                            //若不满足，则将输入框的显示文本重设回银钱阈值 yuZhi_Money
                                            else
                                            {   thresh2Input.Text = Setting.yuZhi_Money.Value.ToString();   }
                                        },
                                    },
                                },
                            },
                        },
                    },
                    //【自适应垂直UI组：单项资源显示设置】
                    new BoxAutoSizeModelGameObject()
                    {
                        Group =
                        {
                            //子组件垂直排布
                            Direction = UnityUIKit.Core.Direction.Vertical,
                            Spacing = 10,
                        },
                        //垂直大小自动适应
                        SizeFitter = { VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize },
                        Children =
                        {
                            //【标签：单项资源显示设置的提示】
                            new TaiwuLabel()
                            {
                                Text = "<color=#DDCCAA>单项资源显示开关</color>\n<color=#998877>若地块有多种资源皆满足阈值，则只会显示其中最高的两种（银钱例外，单独显示）\n在这种情况下，活用屏蔽可以方便查看所需资源。　可搭配【仅显示最高一项资源】</color>",
                                Element = { PreferredSize = { 0 , 90 } },
                                UseOutline = true,
                            },
                            //【水平UI组：六项资源的独立显示开关(并非开关组，也不需要设为开关组)】
                            new BoxAutoSizeModelGameObject()
                            {
                                Group =
                                {
                                    Direction = UnityUIKit.Core.Direction.Horizontal,
                                    Spacing = 10
                                },
                                SizeFitter = { VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize },
                                Children =
                                {
                                    //【开关：地块是否显示食材图标】
                                    new TaiwuToggle()
                                    {
                                        Text = "食材",
                                        isOn = Setting.yuZhiToggle.Value[0],
                                        onValueChanged = (bool value, Toggle tg) =>
                                        {
                                            Setting.yuZhiToggle.Value[0] = value;
                                            
						                    //刷新地块图标（带判定）
						                    RefreshResourcesIcon();
                                        }
                                    },
                                    //【开关：地块是否显示木材图标】
                                    new TaiwuToggle()
                                    {
                                        Text = "木材",
                                        isOn = Setting.yuZhiToggle.Value[1],
                                        onValueChanged = (bool value, Toggle tg) =>
                                        {
                                            Setting.yuZhiToggle.Value[1] = value;
                                            
						                    RefreshResourcesIcon();
                                        }
                                    },
                                    //【开关：地块是否显示金石图标】
                                    new TaiwuToggle()
                                    {
                                        Text = "金石",
                                        isOn = Setting.yuZhiToggle.Value[2],
                                        onValueChanged = (bool value, Toggle tg) =>
                                        {
                                            Setting.yuZhiToggle.Value[2] = value;
                                            
						                    RefreshResourcesIcon();
                                        }
                                    },
                                    //【开关：地块是否显示织物图标】
                                    new TaiwuToggle()
                                    {
                                        Text = "织物",
                                        isOn = Setting.yuZhiToggle.Value[3],
                                        onValueChanged = (bool value, Toggle tg) =>
                                        {
                                            Setting.yuZhiToggle.Value[3] = value;
                                            
						                    RefreshResourcesIcon();
                                        }
                                    },
                                    //【开关：地块是否显示药材图标】
                                    new TaiwuToggle()
                                    {
                                        Text = "药材",
                                        isOn = Setting.yuZhiToggle.Value[4],
                                        onValueChanged = (bool value, Toggle tg) =>
                                        {
                                            Setting.yuZhiToggle.Value[4] = value;
                                            
						                    RefreshResourcesIcon();
                                        }
                                    },
                                    //【开关：地块是否显示银钱图标】
                                    new TaiwuToggle()
                                    {
                                        Text = "银钱",
                                        isOn = Setting.yuZhiToggle.Value[5],
                                        onValueChanged = (bool value, Toggle tg) =>
                                        {
                                            Setting.yuZhiToggle.Value[5] = value;
                                            
						                    RefreshResourcesIcon();
                                        }
                                    },
                                }
                            }
                        }
                    }
                }
            };
        }

        /// <summary>判断是否在实际游戏中（已读取存档）</summary>
        /// <returns>是/否</returns>
        public static bool IsInGame()
        {
            #region 关于判断“是否在游戏中”的测试（游戏版本0.2.8.4）

            //Bool_1  tbl == null
            //Bool_2  tbl.MianActorID() == 0
            //Bool_3  Characters.HasChar(tbl.MianActorID())

            //以上三个用来判定的布尔值，在下列四种情况下的输出
            //一、新打开游戏，还未载入存档时：    Bool_1 == F Bool_2 == T Bool_3 ==F
            //二、读取存档，在游戏中（含深谷：    Bool_1 == F Bool_2 == F Bool_3 ==T
            //三、读取游戏后，点击返回主菜单：    Bool_1 == F Bool_2 == F Bool_3 ==F
            //四、返回主菜单后，新建人物界面：    Bool_1 == F Bool_2 == T Bool_3 ==F

            //可以可以得出以下结论
            //【Bool_1】完全没有参考价值
            //【Bool_2】在情况【一、二、四】中能作为判定，但在情况【二】下失常，无法做为判定依据
            //【Bool_3】在四种情况下皆能作为判定依据，非常可靠

            //if (tbl == null)//为啥子呀？为啥子呀？为啥子呀？到底是哪里错了？
            //{   Logger.LogDebug($"数据实例不存在");    }
            //if (tbl.MianActorID() == 0)//为啥子呀？为啥子呀？为啥子呀？到底是哪里错了？
            //{   Logger.LogDebug($"太吾的人物ID为0");  }
            //if (Characters.HasChar(tbl.MianActorID()))//为啥子呀？为啥子呀？为啥子呀？到底是哪里错了？
            //{   Logger.LogDebug($"找到了太吾人物ID");  }
            #endregion

            DateFile tbl = DateFile.instance;

            //如果找太吾的人物ID，则返回是；没找到则返回否
            if (Characters.HasChar(tbl.MianActorID()))
            {   return true;   }
            else return false;
        }


        /// <summary>刷新所在地图的资源图标（已带判定）</summary>
        public static void RefreshResourcesIcon()
        {

            //【MOD开启、且在游戏中】，则以本MOD的方法刷新所在地图图标
            if (Setting.enabled.Value && IsInGame())
            {
                //边长
                int bianchang_On = 0;

                //尝试
                try
                {
                    //获取当前地区的地图边长
                    bianchang_On = Int32.Parse(DateFile.instance.partWorldMapDate[DateFile.instance.mianPartId][98]);
                }
                //如果捕捉到异常则提供错误信息
                catch (Exception ex)
                {
                    Logger.LogError($"刷新资源图标失败\n在试图获取当前地区的边长时发生异常\n大概率由IsInGame（判定是否在实际游戏中）失效引起\n请联系作者修复");
                    Logger.LogError(ex);
                }

                //获取到非零边长时（负数也不管了，万一有那种用负数边长表示特殊含义的MOD呢、反正平方后是正的）
                if (bianchang_On != 0)
                {
                    //用边长得出最大地块ID
                    int placeNum_On = bianchang_On * bianchang_On;
                    //手动为当前地图每块地块，…
                    for (int j = 0; j < placeNum_On; j++)
                    {
                        //以本MOD的方法设置图标
                        SetResourcesIcon(j);
                    }
                }

            }
            //【MOD关闭、且在游戏中】，则调用游戏原生方法刷新图标（关闭MOD时若仅Unpatch，地图上的图标还要等过月才刷新）
            else if (!Setting.enabled.Value && IsInGame())
            {
                int bianchang_Off = 0;

                try
                {
                    bianchang_Off = Int32.Parse(DateFile.instance.partWorldMapDate[DateFile.instance.mianPartId][98]);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"刷新资源图标失败\n在试图获取当前地区的边长时发生异常\n大概率由IsInGame（判定是否在实际游戏中）失效引起\n请联系作者修复");
                    Logger.LogError(ex);
                }
                
                if (bianchang_Off != 0)
                {
                    int placeNum_Off = bianchang_Off * bianchang_Off;
                    
                    for (int j = 0; j < placeNum_Off; j++)
                    {
                        //以游戏原生的方法设置图标
                        WorldMapPlace worldMapPlace_Ori = WorldMapSystem.instance.worldMapPlaces[j];
                        worldMapPlace_Ori.UpdatePaceResource();
                    }
                }

            }
            //不在游戏中则不刷新、并输出log（就不在游戏内的MOD设置界面显示了，全部自动处理好）
            else
            { Logger.LogInfo($"游戏存档未载入"); }

        }

        //（完全看不懂…但既然能用俺就先用着就好…用于HarmonyPatch）
        /// <summary>反射获取WorldMapPlace下私有变量</summary>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindingFlags);
            return field.GetValue(instance);
        }

        //耶，学会了自己新建数据类型
        /// <summary>自建数据类型：记录满足显示条件的资源项的数值、与该项资源在compare_Arr数组里的序号</summary>
        class AllowedDisplayResources
        {
            private readonly int _indexValue; //数组中对应项的数值
            private readonly int _indexNum; //数组中对应项的序号

            //创建AllowedDisplayResources对象
            public AllowedDisplayResources(int ADR_Value, int ADR_IndexNum)
            {
                this._indexValue = ADR_Value;
                this._indexNum = ADR_IndexNum;
            }
            //资源值
            public int ADR_Value
            {
                get { return _indexValue; }
            }
            //资源序号
            public int ADR_IndexNum
            {
                get { return _indexNum; }
            }
        }

        /// <summary>设置地块的资源丰富图标</summary>
        /// <param name="placeId">传入的地块ID</param>
        public static void SetResourcesIcon(int placeId)
        {
            //字段缩写
            WorldMapPlace worldMapPlace_Mod = WorldMapSystem.instance.worldMapPlaces[placeId];
            //排除未探索的地块【减少计算量】（话说其实也可以把这个设为可更改选项，让玩家自己改。但还是算了吧，个人感觉那样可能会破坏平衡。虽然别的MOD连地图全开啥的都有了）
            if (DateFile.instance.HaveShow(DateFile.instance.mianPartId, placeId) > 0)
            {
                //排除荒野、暗渊【减少计算量】（注意这里的数字序号对应“PalceWorld_Date.txt”里面的数据列ID，“89 列”为“地块大类”，该列的“值6”代表“荒野/暗渊”）
                if (int.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 89)) != 6)
                {
                    //若 屏蔽天灾破坏地块 开启、且 地块 处于被破坏状态，则排除【减少计算量】
                    //（本来想着配合【显示资源上限】，毕竟找能放村民的高上限地块、找到一个被破坏的可不好，结果发现被破坏的地块会连上限一起降低，顿时这功能就变得鸡肋了，但毕竟写都写了，测试似乎也没有BUG，就不想删…）
                    if (Setting.banDestroied.Value && DateFile.instance.PlaceIsBad(DateFile.instance.mianPartId, placeId))
                    {
                        //资源图标槽不启用
                        worldMapPlace_Mod.resourceIconHolder.gameObject.SetActive(false);
                    }
                    //【开始具体设定资源丰富图标】
                    else
                    {
                        //地块的资源图标槽设为启用
                        worldMapPlace_Mod.resourceIconHolder.gameObject.SetActive(true);


                        //获取地块当前资源数值（注意这里的数字序号是读取到的地块资源里的数组序号，“序号0～5”为“食材～银钱”）
                        //（注意：改为了以单项填入初始值的方式创建，防止浅拷贝造成数组数据联动。万一需要操作…）
                        int[] placeResource_Arr = new int[] 
                        {
                                DateFile.instance.GetPlaceResource(DateFile.instance.mianPartId, placeId)[0],
                                DateFile.instance.GetPlaceResource(DateFile.instance.mianPartId, placeId)[1],
                                DateFile.instance.GetPlaceResource(DateFile.instance.mianPartId, placeId)[2],
                                DateFile.instance.GetPlaceResource(DateFile.instance.mianPartId, placeId)[3],
                                DateFile.instance.GetPlaceResource(DateFile.instance.mianPartId, placeId)[4],
                                DateFile.instance.GetPlaceResource(DateFile.instance.mianPartId, placeId)[5],
                        };
                        //获取地块资源上限数值（注意这里的数字序号对应“PalceWorld_Date.txt”里面的数据列ID，“1～6 列”为“食材～银钱”）
                        int[] placeResourceMax_Arr = new int[]
                        {
                                Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 1)),
                                Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 2)),
                                Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 3)),
                                Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 4)),
                                Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 5)),
                                Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 6)),
                        };
                        /// <summary>记载用于判定的地格资源数组</summary>
                        int[] compare_Arr;
                        //以地格上限值显示、还是以地格当前值显示，并以此赋值数组compare_Arr
                        if (Setting.showMax.Value)
                        {
                            compare_Arr = placeResourceMax_Arr;
                        }
                        else
                        {
                            compare_Arr = placeResource_Arr;                       
                        }
                        //门派城镇计算修正参数
                        int halveNum;
                        //修正开启、且地块为门派城镇（聚落），则参数减半（注意这里的数字序号对应“PalceWorld_Date.txt”里面的数据列ID，“93 列”为“是否聚落”，该列的“值1”代表 “是聚落”）
                        if (Setting.cityHalve.Value && int.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 93)) == 1)
                        { halveNum = 1; }
                        else
                        { halveNum = 2; }
                        #region 门派城镇修正计算 补充说明
                        //关于为何加入该功能？因为俺想要以此来查找地块、以便分配村民采集
                        //而聚落地块采集需要耗费人力2，普通地块采集则只耗费人力1
                        //因此聚落地块想要计算“实际每人力的资源获取量”时，需要除以2
                        //而除法计算可能会产生小数，所以用乘法替代（只需用来判定，不需要实际用到判断符两侧的计算结果）
                        //最终是否满足阈值的判定为：
                        //普通地块资源值*2 >= 阈值*2
                        //聚落地块资源值 >= 阈值*2

                        #endregion
                        //列表：记录满足显示要求的物料资源（资源数值，资源序号）
                        List< AllowedDisplayResources > ADR_List = new List<AllowedDisplayResources>();


                        //【为地块中的各项资源进行if循环历遍】
                        for (int i = 0; i < 6; i++)
                        {
                            //首先重置资源丰富图标为不显示
                            worldMapPlace_Mod.resourceIcon[i].SetActive(false);

                            #region 备份原有物料资源图标设置代码（可用，但若设定多项资源丰富图标。游戏内可能会出现的乱序显示问题）
                            /*
                            //物资图标的设置(因调用阈值参数不同，使用 Setting.yuZhi_Materials.Value)
                            if (i < 5)
                            {
                                worldMapPlace_Mod.resourceIcon[i].SetActive(false);

                                //加入门派城镇地块的资源计算修正
                                if (compare_Arr[i] * halveNum >= Setting.yuZhi_Materials.Value * 2)
                                {
                                    //启用物资丰富图标（i == 0～4对应 食材、木材、金石、织物、药材）
                                    worldMapPlace_Mod.resourceIcon[i].SetActive(true);
                                }
                                else
                                {
                                    worldMapPlace_Mod.resourceIcon[i].SetActive(false);
                                }
                            }
                            */
                            #endregion

                            #region 关于！多项资源启用“资源丰富”图标后，出现乱序显示BUG！的说明
                            //当满足多项时，游戏一般会显示其中两项（具体是怎么决定是哪两项这点我没去研究）
                            //但有时游戏会出现乱序显示BUG
                            //BUG1：地块有多项资源满足阈值，却只显示一项图标(还不是其中最高的那项)
                            //BUG1：地块有多项资源满足阈值，显示两项图标、但显示不是其中最高的两项资源

                            //而在用本MOD把阈值得较低后，这问题就特别明显
                            //猜测原本的单项资源显示开关就是为解决这问题设置的
                            //但还是很麻烦，所以把设置图标的方式改了一下
                            //即【物料资源图标设置代码（新 第一部分）】
                            //和【物料资源图标设置代码（新 第二部分）】
                            #endregion

                            #region 【物料资源图标设置代码（新 第一部分）】
                            //【物料类 仅作判定 不设置】(使用物料阈值)
                            if (i < 5)
                            {
                                //先统一重置/取消资源丰富图标的显示
                                worldMapPlace_Mod.resourceIcon[i].SetActive(false);

                                //【该资源显示开关：开启、且该资源：满足阈值判定（含聚落计算修正）】，则…
                                //俺刚刚学到了 && 和 & 不同， “&&” 会在前方的判断条件不满足的时候、自动略过后方的判断
                                //所以调换了一下判断的顺序，应该能减少计算量。
                                if (Setting.yuZhiToggle.Value[i] && (compare_Arr[i] * halveNum >= Setting.yuZhi_Materials.Value * 2))
                                {
                                    //向列表内记录满足条件的项目（两个参数：满足条件资源项的数值，满足条件资源项的序号）
                                    ADR_List.Add(new AllowedDisplayResources(compare_Arr[i], i));
                                }
                            }
                            #endregion
                            //【银钱类 单独判定 并设置】(使用银钱阈值) 因为银钱数值普遍较低，单设一项阈值较好
                            else if (i == 5)
                            {
                                //【银钱显示开关：开启、且 银钱：满足阈值判定（含聚落计算修正）】，则…
                                if (Setting.yuZhiToggle.Value[i] && (compare_Arr[i] * halveNum >= Setting.yuZhi_Money.Value * 2))
                                {
                                    //启用银钱丰富图标（银钱图标在地块上的显示是独立的，和物料的资源丰富图标分离）
                                    worldMapPlace_Mod.resourceIcon[i].SetActive(true);
                                }
                            }
                        }

                        #region 【物料资源图标设置代码（新 第二部分）】
                        //【列表内 物料资源 若仅单项满足，则…】
                        if (ADR_List.Count == 1)
                        {
                            int r1 = ADR_List[0].ADR_IndexNum;
                            worldMapPlace_Mod.resourceIcon[r1].SetActive(true);

                            #region 『调试用』Debug信息
                            ////『调试用』<-调试完，记得搜索该字段、然后把对应的代码都注释掉，不然要被信息淹没了
                            //string[] resouceName_Arr = { "食材", "木材", "金石", "织物", "药材" };
                            ////不建议偷懒设为Debug，就算不输出文本也多跑好多
                            //Logger.LogInfo("placeId：" + placeId + " 地块资源：" + string.Join(",", compare_Arr) + " 设置第一高项：" + resouceName_Arr[r1] + " " + compare_Arr[r1]);

                            #endregion
                        }
                        //【列表内 物料资源 若有多项满足，则…】
                        else if (ADR_List.Count > 1)
                        {
                            //对列表以资源值大小重新排序（从大到小）（数值相等，会默认按资源序号从小到大排序）
                            ADR_List.Sort((left, right) =>
                            {
                                //其中是 ADR_Value 以此进行排序的属性、由大到小排列
                                //若想小到大排列，此处改为  left.ADR_Value > right.ADR_Value
                                if (left.ADR_Value < right.ADR_Value)
                                    return 1;
                                else if (left.ADR_Value == right.ADR_Value)
                                    return 0;
                                else
                                    return -1;
                            });

                            //取得排序后的列表中的第一个条目（等效第一高资源）的资源序号
                            int r1 = ADR_List[0].ADR_IndexNum;
                            //设置第一高资源图标
                            worldMapPlace_Mod.resourceIcon[r1].SetActive(true);

                            #region 『调试用』Debug信息
                            ////『调试用』<-调试完，记得搜索该字段、然后把对应的代码都注释掉，不然要被信息淹没了
                            //string[] resouceName_Arr = { "食材", "木材", "金石", "织物", "药材" };
                            //string listTextDebug = "记录列表：";
                            //foreach (AllowedDisplayResources res in ADR_List)
                            //{
                            //    listTextDebug = listTextDebug + resouceName_Arr[res.ADR_IndexNum] + " " + res.ADR_Value + " | ";
                            //}
                            ////不建议偷懒设为Debug完事就不注释掉，就算不输出文本也多跑好多
                            //Logger.LogInfo("地块资源：" + string.Join(",", compare_Arr) + " | placeId：" + placeId);
                            //Logger.LogInfo(listTextDebug + "设置第一高项：" + resouceName_Arr[r1] + " " + compare_Arr[r1]);

                            #endregion

                            //【若 仅显示单项图标：未开启，则…】
                            if (!Setting.showSingleIcon.Value)
                            {
                                //取得排序后的列表中的第二个条目（等效第二高资源）的资源序号
                                int r2 = ADR_List[1].ADR_IndexNum;
                                //设置第二高资源图标
                                worldMapPlace_Mod.resourceIcon[r2].SetActive(true);

                                #region 『调试用』Debug信息
                                ////不建议偷懒设为Debug完事就不注释掉，就算不输出文本也多跑好多
                                //Logger.LogInfo("设置第二高项：" + resouceName_Arr[r2] + " " + compare_Arr[r2]);

                                #endregion
                            }

                        }
                        #endregion

                    }
                }
                else
                {
                    //荒野、暗渊地块的资源图标槽不启用
                    worldMapPlace_Mod.resourceIconHolder.gameObject.SetActive(false);
                }
            }
            else
            {
                //未探索地块的资源图标槽不启用
                worldMapPlace_Mod.resourceIconHolder.gameObject.SetActive(false);
            }
        }
    }
}
