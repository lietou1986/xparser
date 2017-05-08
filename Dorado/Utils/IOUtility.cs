using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Dorado.Utils
{
    /// <summary>
    /// 关于路径的工具
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static class IOUtility
    {
        /// <summary>
        /// 修正路径，将其中的"."及".."合并
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string Revise(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (path.IndexOf('.') < 0)
                return path;

            string[] parts = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            string[] newParts = new string[parts.Length];
            bool isPathRoot = Path.IsPathRooted(parts[0]);

            int newIndex = 0;
            for (int index = 0; index < parts.Length; index++)
            {
                string part = parts[index];
                if (part == ".")
                    continue;

                if (part == "..")
                {
                    if (newIndex == 0 || newIndex == 1 && isPathRoot)
                        throw new FormatException("路径格式不正确");

                    newIndex--;
                }
                else
                {
                    newParts[newIndex++] = part;
                }
            }

            return string.Join("\\", newParts, 0, newIndex);
        }

        /// <summary>
        /// 遍历目录
        /// </summary>
        /// <param name="rootDirectory"></param>
        /// <param name = "fileHandler"></param>
        public static void Traversing(IList<string> rootDirectory, Action<FileInfo> fileHandler, Func<FileInfo, bool> fileFilter = null, Func<DirectoryInfo, bool> directoryFilter = null, Action<Exception> errorHandler = null)
        {
            if (fileFilter == null)
                fileFilter = x => { return true; };
            if (directoryFilter == null)
                directoryFilter = x => { return true; };
            if (errorHandler == null)
                errorHandler = (n) => { };

            Queue<string> pathQueue = new Queue<string>();

            foreach (string directory in rootDirectory)
            {
                DirectoryInfo dir = new DirectoryInfo(directory);
                if (directoryFilter(dir))
                    pathQueue.Enqueue(directory);
            }

            while (pathQueue.Count > 0)
            {
                try
                {
                    DirectoryInfo diParent = new DirectoryInfo(pathQueue.Dequeue());
                    foreach (DirectoryInfo diChild in diParent.GetDirectories())
                    {
                        if (directoryFilter(diChild))
                            pathQueue.Enqueue(diChild.FullName);
                    }

                    foreach (FileInfo fi in diParent.GetFiles())
                        if (fileFilter(fi))
                            fileHandler?.Invoke(fi);
                }
                catch (UnauthorizedAccessException ex) { errorHandler(ex); }
                catch (PathTooLongException ex) { errorHandler(ex); }
                catch (Exception ex) { errorHandler(ex); }
            }
        }

        public static void Traversing(string rootDirectory, Action<FileInfo> fileHandler, Func<FileInfo, bool> fileFilter = null, Func<DirectoryInfo, bool> directoryFilter = null, Action<Exception> errorHandler = null)
        {
            Traversing(new List<string>() { rootDirectory }, fileHandler, fileFilter, directoryFilter, errorHandler);
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        public static void DeleteDirectory(bool isReCreate, params string[] paths)
        {
            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                if (isReCreate)
                    Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        public static void DeleteDirectory(params string[] paths)
        {
            DeleteDirectory(false, paths);
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        public static void CreateDirectory(params string[] paths)
        {
            foreach (string path in paths)
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 通过MD5CryptoServiceProvider类中的ComputeHash方法直接传入一个FileStream类实现计算MD5
        /// 操作简单，代码少，调用即可
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <returns>MD5Hash</returns>
        public static string GetFileMD5(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException(string.Format("<{0}>, 不存在", path));
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            byte[] buffer = md5Provider.ComputeHash(fs);
            string result = BitConverter.ToString(buffer);
            result = result.Replace("-", "");
            md5Provider.Clear();
            fs.Close();
            return result;
        }

        /// <summary>
        /// 通过HashAlgorithm的TransformBlock方法对流进行叠加运算获得MD5
        /// 实现稍微复杂，但可使用与传输文件或接收文件时同步计算MD5值
        /// 可自定义缓冲区大小，计算速度较快
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <returns>MD5Hash</returns>
        public static string GetMD5ByHashAlgorithm(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException(string.Format("<{0}>, 不存在", path));
            int bufferSize = 1024 * 16;//自定义缓冲区大小16K
            byte[] buffer = new byte[bufferSize];
            Stream inputStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            HashAlgorithm hashAlgorithm = new MD5CryptoServiceProvider();
            int readLength = 0;//每次读取长度
            var output = new byte[bufferSize];
            while ((readLength = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                //计算MD5
                hashAlgorithm.TransformBlock(buffer, 0, readLength, output, 0);
            }
            //完成最后计算，必须调用(由于上一部循环已经完成所有运算，所以调用此方法时后面的两个参数都为0)
            hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
            string md5 = BitConverter.ToString(hashAlgorithm.Hash);
            hashAlgorithm.Clear();
            inputStream.Close();
            md5 = md5.Replace("-", "");
            return md5;
        }
    }
}