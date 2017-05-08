namespace X.ResumeParseService.Scanner.Enums
{
    /// <summary>
    /// 系统级目录
    /// </summary>
    public enum IncludeSpecialFolder
    {
        // 摘要:
        //     逻辑桌面，而不是物理文件系统位置。
        Desktop = 0,

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
        //     用于物理上存储桌面上的文件对象的目录。
        DesktopDirectory = 16,

        //
        // 摘要:
        //     文件系统目录，包含在所有用户桌面上出现的文件和文件夹。此特殊文件夹仅对 Windows NT 系统有效。
        CommonDesktopDirectory = 25,

        //
        // 摘要:
        //     用户的配置文件文件夹。应用程序不应在此级别上创建文件或文件夹；它们应将其数据放在 System.Environment.SpecialFolder.ApplicationData
        //     所引用的位置之下。
        UserProfile = 40,

        //
        // 摘要:
        //     文件系统目录，包含所有用户共有的文档。此特殊文件夹仅对装有 Shfolder.dll 的 Windows NT 系统、Windows 95 和 Windows
        //     98 系统有效。
        CommonDocuments = 46,
    }
}