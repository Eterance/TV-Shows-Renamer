using System.Text.RegularExpressions;

namespace 番剧集数重命名
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Write("请输入文件夹路径: ");
            string path = Console.ReadLine();
            Console.Write("请输入默认季（如S01）。如果路径中的文件夹名没有包含季，将以这个作为默认。回车默认S01 ");
            string season_default = Console.ReadLine();
            if (season_default == null || season_default == "")
            {
                season_default = "S01";
            }
            Console.WriteLine("更改结果预览：");
            List<string> result = SearchFiles(path, new string[] { ".avi", ".mp4", ".mkv", ".srt", ".ass" });
            List<Tuple<string, string>> newNames = new List<Tuple<string, string>>();
            foreach (string fullPath in result)
            {
                var newName = ChangeName(fullPath, season_default);
                if (newName != null)
                {
                    newNames.Add(Tuple.Create(fullPath, newName));
                    Console.WriteLine($"旧：{fullPath}");
                    Console.WriteLine($"新：{newName}");
                    Console.WriteLine("");
                }
            }
            if (newNames.Count == 0)
            {
                Console.Write("没有可替换的。");
            }
            else 
            {
                Console.Write($"共计 {newNames.Count} 个。输入任意字符取消替换，回车开始替换：");
                string command = Console.ReadLine();
                if (command == "")
                {
                    foreach (Tuple<string, string> a in newNames)
                    {
                        try
                        {
                            File.Move(a.Item1, a.Item2);
                        }
                        catch (Exception ex)
                        {
                            Console.Write($"替换因错误被中止。{ex}");
                        }
                    }
                    Console.Write($"替换完成。");
                }
                else 
                {
                    Console.Write("替换被取消。");
                }
            }
            Console.Write("按任意键退出……");
            Console.ReadKey();
        }

        static string ChangeName(string fullPath, string season_default="S01")
        {
            // 匹配季
            string season_pattern = @"\\S\d+\\";
            Match matchSeason = Regex.Match(fullPath, season_pattern);
            if (matchSeason.Success)
            {
                season_default = matchSeason.Value.Replace("\\", "").ToUpper();
            }

            string episode_pattern1 = @"\[\d+\]";
            string episode_pattern2 = @" \d+ ";
            Match match1 = Regex.Match(fullPath, episode_pattern1);
            Match match2 = Regex.Match(fullPath, episode_pattern2);
            string left = "";
            string right = "";
            string ep = "";
            string old_ep = "";
            if (match1.Success)
            {
                left = "["; right = "]";
                old_ep = match1.Value;
            }
            else if (match2.Success)
            {
                left = " "; right = " ";
                old_ep = match2.Value;
            }
            else 
            {
                return null;
            }
            ep = old_ep.Replace(left, "").Replace(right, "");
            string newFullPath = fullPath.Replace(old_ep, $"{left}{season_default}E{ep}{right}");
            return newFullPath;
        }

        static List<string> SearchFiles(string dirPath, string[] extensions)
        {
            List<string> result = new List<string>();
            try
            {
                // 遍历当前目录下的所有指定后缀名的文件
                foreach (var file in Directory.GetFiles(dirPath)
                                            .Where(f => extensions.Contains(Path.GetExtension(f))))
                {
                    result.Add(Path.GetFullPath(file));
                }

                // 遍历当前目录下的所有子目录
                foreach (var subDir in Directory.GetDirectories(dirPath))
                {
                    result.AddRange(SearchFiles(subDir, extensions));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误：{ex.Message}");
            }
            return result;
        }

    }
}