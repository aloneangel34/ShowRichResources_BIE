//引入命名空间（本.cs文件需要用到的、不同于“本文件命名空间”的命名空间，要加在这里）
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//本.cs文件的命名空间
namespace ShowRichResources
{
    /// <summary>
    /// MOD设置选项
    /// </summary>
    public class Settings
    {
        //新的选项参数接口需要先声明，注意它只是接口，不是直接用于设置的实际参数。想要实际读取或写入时，需要加上“.value”调用接口的value属性
        //ConfigEntry<Type>括号内的数值类型（Type）自己填，可以填系统已有的、也可以填写自设数据类型
        //（可以参看 炽翼幻灵 大佬的 梦中情人MOD [BIE]DreamLover v1.1 的代码）
        //当时真的被惊到了，真就啥都往里面硬塞啊，好涩哦！

        //多利用文档注释，这样就算代码再长、也不怕记不住了，鼠标移上去就能看到文档注释的文字。

        /// <summary>总开关：MOD启用与否</summary>
        public ConfigEntry<bool> enabled;
        /// <summary>信息：MOD声明信息</summary>
        public ConfigEntry<String> modClaimInfo;
        /// <summary>开关：不显示天灾破坏地格</summary>
        public ConfigEntry<bool> cityHalveCalc;
        /// <summary>开关：门派城镇采集修正</summary>
        public ConfigEntry<bool> banDestroied;
        /// <summary>开关：以地块资源上限显示</summary>
        public ConfigEntry<bool> showMax;
        /// <summary>开关：仅显示最高一项资源</summary>
        public ConfigEntry<bool> showSingleIcon;
        /// <summary>阈值：物料显示阈值</summary>
        public ConfigEntry<int> yuZhi_Materials;
        /// <summary>阈值：银钱显示阈值</summary>
        public ConfigEntry<int> yuZhi_Money;
        /// <summary>开关[]：六项资源的单独显示开关</summary>
        public ConfigEntry<bool[]> yuZhiToggle;


        //Config配置文件 公开可读、私有可写属性 
        public ConfigFile Config { get; private set; }

        //默认会先读取 BepInEx\config\ 文件夹下名为 GUID + ".cfg" 的配置文件（例如本MOD则为：TaiwuMOD.ShowRichResources.cfg），然后初始化配置文件
        //（似乎有方法可以更改读取路径、以及可读取多个配置文件。但这块俺不懂）
        /// <summary>设置选项初始化</summary>
        public void Init(ConfigFile Config)
        {
            //将选项参数接口与配置文件绑定，并填入缺省值
            enabled = Config.Bind(Main.ModDisplayName, "enable", true, "【MOD开启】");

            //将选项参数接口与配置文件绑定，并填入缺省值
            //因为BepInEx的Mod没有Info.json，所以在这里记录（如果因MOD更新而改动了“描述/第四项参数”的话，“描述”的文字段会自动更新。包括“描述”被用户手动改掉、初始化后也会按这里的来更新。）
            modClaimInfo = Config.Bind(Main.ModDisplayName, "许可声明请看上方", "", "ShowRichResources/显示丰富资源\n\n原作者：xiaoye1024\n原许可协议声明：MIT许可\n\nBIE版迁移作者：aloneangel34\nBIE版许可协议声明：MIT许可");

            //将选项参数接口与配置文件绑定，并填入缺省值（如果因MOD更新而改动了相应的描述的话，描述类的文字段这些会自动更新）
            //（小括号内的四个参数分别是【选项参数在配置文件里所属标签分类】，【选项参数在配置文件里的Key名称】，【选项参数的缺省值】，【选项参数的描述（如下所示，可以用\n等转义符）】）
            cityHalveCalc = Config.Bind("开关设置项", "cityHalve", false, "【门派城镇采集修正】\n在判定门派城镇（聚落）地格显示资源丰富图标时、是否将其资源数值减半后再计算是否符合阈值\n主要用于查找村民采集地点的时候：\n聚落地块派遣村民采集需要消耗2人力，计算聚落地块平均每人力的采集量时需要除以2");
            showMax = Config.Bind("开关设置项", "showMax", false, "【以地块资源上限显示】\ntrue：按照地块的资源上限数值显示资源丰富图标\nfalse：按照地块的当前资源数值显示");
            showSingleIcon = Config.Bind("开关设置项", "showSingleIcon", true, "【仅显示最高一项资源】建议保持开启\ntrue：地块仅精准显示满足阈值的最高一项资源图标（除银钱）\nfalse：会显示满足阈值的最高两项资源图标（理论上是这样，但似乎游戏本身会打乱显示）\n例如这个例子：同样是“食材60、木材50、其他30”的地格，显示阈值设为40\n“只显示食材”，“只显示木材”，“食材木材都显示、食材图标位于上方”，“食材木材都显示、木材图标位于上方”四种情况都可能出现\n游戏原生阈值较高：100，仅有少数门派地格才会有双超100的资源，所以才没凸现出问题来。");
            yuZhiToggle = Config.Bind("开关设置项", "yuZhiToggle", new bool[] { true, true, true, true, true, true }, "【六项资源的单独显示开关】\n若一个地块有多种资源皆满足阈值，则游戏最多只能显示其中两种（银钱例外，单独显示）\n在这种情况下，活用屏蔽可以方便查看所需资源\n可搭配【仅显示最高一项资源】，两者不会冲突");
            yuZhi_Materials = Config.Bind("阈值设置项", "yuZhi_Materials", 150, "【物料资源显示阈值】\n食材、木材、金石、织物、药草这五项物料达到多少时会在地格中显示对应的资源丰富图标\n阈值修改范围（1～999）　游戏原生阈值为：100\n【用于查看派遣村民采集地块的建议阈值】物料：150　门派城镇采集修正：开\n【用于查看适合挖掘素材地块的建议阈值】物料：150　门派城镇采集修正：关\n不太推荐将物料资源的阈值设为100以下的数字，若确实需要可以搭配【仅显示最高项资源】\n注意：只是更改了图标的显示阈值，地块实际资源量未变动");
            yuZhi_Money = Config.Bind("阈值设置项", "yuZhi_Money", 70, "【银钱资源显示阈值】银钱达到多少时会在地格中显示银钱丰富图标\n阈值修改范围（1～999）　游戏原生阈值为：100\n【用于查看派遣村民采集地块的建议阈值】银钱：70　门派城镇采集修正：开\n【用于查看适合挖掘素材地块的建议阈值】银钱：100　门派城镇采集修正：关\n注意：只是更改了图标的显示阈值，地块实际资源量未变动");

            //读取完后再做个判断，防止在配置文件里被手动乱设数字（至于读取到不匹配的数据类型这种不用管。那时BepInEx会自动忽视、重设回缺省值）
            if (yuZhi_Materials.Value < 1 || yuZhi_Materials.Value > 999) yuZhi_Materials.Value = 150;
            if (yuZhi_Money.Value < 1 || yuZhi_Money.Value > 999) yuZhi_Money.Value = 70;



            //【放到最后，等配置文件全加载了再进行】若读取到配置文件的enable为true时，手动加载Patch（该方式需要YanCore作为支持）
            if (enabled.Value)
                HarmonyPatches.PatchHandlers["显示丰富资源"].Patch(HarmonyPatches.harmony);

            #region 关于Patch的额外说明
            //每个手动Patch都要在这里加上，只在UI界面里的“变动时执行”代码中加的话，进游戏不会自动patch的
            //如果想要自动Patch可以在HarmonyPatches.cs里查看【自动加载Patch相关…】折叠部分的内容
            //如果只是做迁移，不是太懂代码的话（虽然俺也不是太懂代码就是了），
            //就用原UMM那种 harmony.PatchAll();
            //配合 [HarmonyPatch(typeof(WorldMapPlace), "UpdatePaceResource")] 的方式就行
            #endregion


        }
    }
}
