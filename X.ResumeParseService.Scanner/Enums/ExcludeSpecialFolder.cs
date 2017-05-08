namespace X.ResumeParseService.Scanner.Enums
{
    /// <summary>
    /// 系统级目录
    /// </summary>
    public enum ExcludeSpecialFolder
    {
        // 摘要:
        //     逻辑桌面，而不是物理文件系统位置。
        Desktop = 0,

        //
        // 摘要:
        //     包含用户程序组的目录。
        Programs = 2,

        //
        // 摘要:
        //     用作文档的公共储存库的目录。
        Personal = 5,

        //
        // 摘要:
        //     “我的文档”文件夹。
        MyDocuments = 5,

        //
        // 摘要:
        //     用作用户收藏夹项的公共储存库的目录。
        Favorites = 6,

        //
        // 摘要:
        //     对应于用户的“启动”程序组的目录。
        Startup = 7,

        //
        // 摘要:
        //     包含用户最近使用过的文档的目录。
        Recent = 8,

        //
        // 摘要:
        //     包含“发送”菜单项的目录。
        SendTo = 9,

        //
        // 摘要:
        //     包含“开始”菜单项的目录。
        StartMenu = 11,

        //
        // 摘要:
        //     “我的音乐”文件夹。
        MyMusic = 13,

        //
        // 摘要:
        //     文件系统目录，充当属于某个用户的视频的存储库。
        MyVideos = 14,

        //
        // 摘要:
        //     用于物理上存储桌面上的文件对象的目录。
        DesktopDirectory = 16,

        //
        // 摘要:
        //     “我的电脑”文件夹。
        MyComputer = 17,

        //
        // 摘要:
        //     文件系统目录，包含“网上邻居”虚拟文件夹中可能存在的链接对象。
        NetworkShortcuts = 19,

        //
        // 摘要:
        //     包含字体的虚拟文件夹。
        Fonts = 20,

        //
        // 摘要:
        //     用作文档模板的公共储存库的目录。
        Templates = 21,

        //
        // 摘要:
        //     文件系统目录，包含在所有用户的“开始”菜单上都出现的程序和文件夹。此特殊文件夹仅对 Windows NT 系统有效。
        CommonStartMenu = 22,

        //
        // 摘要:
        //     存放多个应用程序共享的组件的文件夹。此特殊文件夹仅对 Windows NT、Windows 2000 和 Windows XP 系统有效。
        CommonPrograms = 23,

        //
        // 摘要:
        //     文件系统目录，包含在所有用户的“启动”文件夹中都出现的程序。此特殊文件夹仅对 Windows NT 系统有效。
        CommonStartup = 24,

        //
        // 摘要:
        //     文件系统目录，包含在所有用户桌面上出现的文件和文件夹。此特殊文件夹仅对 Windows NT 系统有效。
        CommonDesktopDirectory = 25,

        //
        // 摘要:
        //     目录，它用作当前漫游用户的应用程序特定数据的公共储存库。
        ApplicationData = 26,

        //
        // 摘要:
        //     文件系统目录，包含“打印机”虚拟文件夹中可能存在的链接对象。
        PrinterShortcuts = 27,

        //
        // 摘要:
        //     目录，它用作当前非漫游用户使用的应用程序特定数据的公共储存库。
        LocalApplicationData = 28,

        //
        // 摘要:
        //     用作 Internet 临时文件的公共储存库的目录。
        InternetCache = 32,

        //
        // 摘要:
        //     用作 Internet Cookie 的公共储存库的目录。
        Cookies = 33,

        //
        // 摘要:
        //     用作 Internet 历史记录项的公共储存库的目录。
        History = 34,

        //
        // 摘要:
        //     目录，它用作所有用户使用的应用程序特定数据的公共储存库。
        CommonApplicationData = 35,

        //
        // 摘要:
        //     Windows 目录或 SYSROOT。它与 %windir% 或 %SYSTEMROOT% 环境变量相对应。
        Windows = 36,

        //
        // 摘要:
        //     “System”目录。
        System = 37,

        //
        // 摘要:
        //     “Program files”目录。
        ProgramFiles = 38,

        //
        // 摘要:
        //     “我的图片”文件夹。
        MyPictures = 39,

        //
        // 摘要:
        //     用户的配置文件文件夹。应用程序不应在此级别上创建文件或文件夹；它们应将其数据放在 System.Environment.SpecialFolder.ApplicationData
        //     所引用的位置之下。
        UserProfile = 40,

        //
        // 摘要:
        //     Windows“System”文件夹。
        SystemX86 = 41,

        //
        // 摘要:
        //     “Program Files”文件夹。
        ProgramFilesX86 = 42,

        //
        // 摘要:
        //     用于应用程序间共享的组件的目录。
        CommonProgramFiles = 43,

        //
        // 摘要:
        //     “Program Files”文件夹。
        CommonProgramFilesX86 = 44,

        //
        // 摘要:
        //     文件系统目录，包含所有用户都可以使用的模板。此特殊文件夹仅对 Windows NT 系统有效。
        CommonTemplates = 45,

        //
        // 摘要:
        //     文件系统目录，包含所有用户共有的文档。此特殊文件夹仅对装有 Shfolder.dll 的 Windows NT 系统、Windows 95 和 Windows
        //     98 系统有效。
        CommonDocuments = 46,

        //
        // 摘要:
        //     文件系统目录，包含计算机所有用户的管理工具。
        CommonAdminTools = 47,

        //
        // 摘要:
        //     文件系统目录，用于存储各个用户的管理工具。Microsoft Management Console (MMC) 会将自定义的控制台保存在此目录中，并且此目录将随用户一起漫游。
        AdminTools = 48,

        //
        // 摘要:
        //     文件系统目录，充当所有用户共有的音乐文件的存储库。
        CommonMusic = 53,

        //
        // 摘要:
        //     文件系统目录，充当所有用户共有的图像文件的存储库。
        CommonPictures = 54,

        //
        // 摘要:
        //     文件系统目录，充当所有用户共有的视频文件的存储库。
        CommonVideos = 55,

        //
        // 摘要:
        //     文件系统目录，包含资源数据。
        Resources = 56,

        //
        // 摘要:
        //     文件系统目录，包含本地化资源数据。
        LocalizedResources = 57,

        //
        // 摘要:
        //     为了实现向后兼容，Windows Vista 中可以识别此值，但该特殊文件夹本身已不再使用。
        CommonOemLinks = 58,

        //
        // 摘要:
        //     文件系统目录，充当等待写入 CD 的文件的临时区域。
        CDBurning = 59,
    }
}