//引入命名空间（本.cs文件需要用到的、不同于“本文件命名空间”的命名空间，要加在这里）
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using YanLib;

//本.cs文件的命名空间
namespace ShowRichResources
{
    /// <summary>
    /// Harmony补丁库（需要YanLib作支持）
    /// </summary>
    public class HarmonyPatches
    {

        /// <summary>
        /// 本MOD的Harmony实例
        /// </summary>
        /// 声明公开Harmony类的静态只读字段harmony，初始化为以Main.GUID作为创建的Harmony的实例
        public static readonly Harmony harmony = new Harmony(Main.GUID);
        /// <summary>
        /// 补丁类型
        /// </summary>
        /// 声明公开Type类的静态只读字段PatchesType，初始化为本HarmonyPatches类的类型
        public static readonly Type PatchesType = typeof(HarmonyPatches);

        #region 自动加载Patch相关（需要YanCore前置插件），本MOD未用到的部分，如需自动加载Patch可以取消注释（也作为分析“零下萝莉大佬提供的GOD示例”的笔记）
        /* 
        //声明 AutoPatchHandlers 字段（把所有要在游戏启动时加载 PatchHandler 的全部放进去）
        //注意区分 AutoPatchHandlers 和 PatchHandlers，以及代码顺序最好别弄错，毕竟从上往下执行的
        /// <summary>AutoPatchHandlers自动补丁仓库</summary>
        public static readonly Dictionary<string, PatchHandler> AutoPatchHandlers = new Dictionary<string, PatchHandler>
        {
            //在这里加入，如下示例
            //（？注意最好不要在 AutoPatchHandlers仓库 和 后续的 PatchHandlers仓库 中重复加入同一个PatchHandler？）这点不确定，俺还没研究透、俺太彩了

            ////对应【string, PatchHandler】
            //{ "显示丰富资源", new PatchHandler()
            //    {
            //        TargetType = typeof(WorldMapPlace),
            //        TargetMethonName = "UpdatePaceResource",
            //        Postfix = AccessTools.Method(PatchesType,"UpdatePaceResource_Postfix")
            //    }
            //},

            //新的PatchHandler可以继续往下加
            //GOD代码示例里部分是new PatchHandler，部分是new PatchHandler()。后者是允许传参的？俺新手不是很懂代码

        };

        /// <summary>
        /// 初始化多线程加载补丁
        /// </summary>
        public static void Init()
        {
            Main.Logger.LogInfo($"多线程 Patch 开始");
            //遍历 AutoPatchHandlers 自动补丁仓库内的所有条目
            foreach (var patch in AutoPatchHandlers)
            {
                //尝试
                try
                {
                    Main.Logger.LogInfo($"Patch { patch.Key }");
                    //调用“条目的值”的Patch方法（以harmony为阐述）
                    patch.Value.Patch(harmony);
                }
                //异常捕获（当上方执行出错时，输出错误原因，而不是MOD直接崩溃）
                catch (Exception ex)
                {
                    Main.Logger.LogError($"{ patch.Key } Patch Failed");
                    Main.Logger.LogError(ex);
                }
            }

            //无参PatchAll方法会自动搜索本程序集内的所有HarmonyPatch补丁（当然要按照它所提供的指定格式声明才行）
            harmony.PatchAll();
            Main.Logger.LogInfo($"Patch 完成");
        }
        */
        #endregion

        #region 采用 YanLib 中 PatchHandler 的新式补丁（Patch）方式，支持Unpatch（需要YanCore前置插件）

        //接收游戏本身在UpdatePaceResource时使用的的placeId，并传导调用本MOD的资源图标刷新方式去再刷新一次
        //以及这个“Pace”是游戏本身的茄子代码、就这样写错的，所以这边也只能跟着用
        //这其实是一个对游戏中WorldMapPlace类下的UpdatePaceResource
        /// <summary></summary>
        /// <param name="__instance"></param>
        public static void UpdatePaceResource_Postfix(WorldMapPlace __instance)
        {
            int placeId = Int32.Parse(Main.GetInstanceField(typeof(WorldMapPlace), __instance, "placeId").ToString());
            Main.SetResourcesIcon(placeId);
        }

        //注意区分 AutoPatchHandlers 和 PatchHandlers
        //（GOD示例代码里，这里的字段名是【PatchHandler】，我这里多加了【s】，以便和YanLib.PatchHandler的类名区分开）

        /// <summary>
        /// PatchHandlers手动补丁仓库
        /// </summary>
        /// （这是一个字典：“以string字符串作为键，以PatchHandler类作为值”的泛型键值对集合）
        public static readonly Dictionary<string, PatchHandler> PatchHandlers = new Dictionary<string, PatchHandler>
        {
            //这里面是字典的条目
            //"显示丰富资源"是键；后面创建的PatchHandler对象是该键所对应的值
            //就好像凭钥匙去开保管柜的物品那样，键值对就是凭“键”来获取对应的“值”
            { "显示丰富资源", new PatchHandler()
                {
                    TargetType = typeof(WorldMapPlace),
                    TargetMethonName = "UpdatePaceResource",
                    Postfix = AccessTools.Method(PatchesType,"UpdatePaceResource_Postfix")
                }
            },

            //这样就添加了一个PatchHandler条目来处理上方的 UpdatePaceResource_Postfix 方法（作用：每当游戏刷新地格的资源丰富图标时，以本MOD设定刷新显示）
            
            //可在在特定bool值变化时手动在 Awake 或者 Setting 里设定要 Patch（应用） 还是 Unpatch（还原）
            //HarmonyPatches.PatchHandler["UpdatePaceResource_Postfix"].Patch(HarmonyPatches.harmony)
            //HarmonyPatches.PatchHandler["UpdatePaceResource_Postfix"].Unpatch(HarmonyPatches.harmony)

            //新的PatchHandler可以继续往下加
        };
        #endregion 
    }

    #region 原生 HarmonyPatch 方式（作为对照与备份的原代码）（采用原生方式记得在MOD开头补上 harmony.PatchAll(); 语句）
    /*
    /// <summary>
    /// 每当游戏刷新地块的资源图标时，在其后调用本MOD的SetResourcesIcon方法来重新刷新
    /// </summary>
    //中括号里声明这是一个Harmony补丁，补丁的对象为WorldMapPlace类下的名为"UpdatePaceResource"的方法
    [HarmonyPatch(typeof(WorldMapPlace), "UpdatePaceResource")]
    public static class WorldMapPlace_UpdatePaceResource_Patch
    {
        //方法名为Postfix，即Harmony自动判定这是一个后置补丁。
        //不过方法名也可以自定义（特别比如有必须有多个后置补丁，不改名会冲突的情况），此时用[HarmonyPostfix]来告知这是后置补丁
        public static void Postfix(WorldMapPlace __instance)
        {
            int placeId = Int32.Parse(GetInstanceField(typeof(WorldMapPlace), __instance, "placeId").ToString());
            SetResourcesIcon(placeId);
        }
    }
    */
    #endregion

}
