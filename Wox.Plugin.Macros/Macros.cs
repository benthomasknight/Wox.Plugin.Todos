using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Newtonsoft.Json;

namespace Wox.Plugin.Macros
{
    public class Macro
    {
        public string Key { get; set; }
        public string Content { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class Macros
    {
        private const string DataFileName = @"macros.data.json";

        private string _dataFolderPath;

        private List<Macro> _macroList;
        public PluginInitContext Context { get; }

        public string ActionKeyword { get; set; }

        public Macros(PluginInitContext context, Settings setting)
        {
            Context = context;

            if (context.CurrentPluginMetadata.ActionKeywords != null
                && context.CurrentPluginMetadata.ActionKeywords.Any())
            {
                ActionKeyword = context.CurrentPluginMetadata.ActionKeywords[0];
            }

            _dataFolderPath = setting.FolderPath;
            Load();
        }

        public List<Result> Results => ToResults(_macroList);

        public void Reload()
        {
            Load();
        }

        public List<Result> Find(
            Func<Macro, bool> func,
            Func<Macro, string> subTitleFormatter = null,
            Func<ActionContext, Macro, bool> itemAction = null)
        {
            return ToResults(_macroList.Where(func), subTitleFormatter, itemAction);
        }

        public Macros Add(Macro macro, Action callback = null)
        {
            if (string.IsNullOrEmpty(macro.Content))
            {
                return this;
            }
            
            _macroList.Add(macro);
            Save();
            if (callback == null)
            {
                Context.API.ChangeQuery($"{ActionKeyword} ");
            }
            else
            {
                callback();
            }
            return this;
        }

        public Macros Remove(Macro macro, Action callback = null)
        {
            var item = _macroList.FirstOrDefault(t => t.Key == macro.Key);
            if (item != null)
            {
                _macroList.Remove(item);
            }
            Save();
            if (callback == null)
            {
                Context.API.ChangeQuery($"{ActionKeyword} ");
                Alert("Success", "macro removed!");
            }
            else
            {
                callback();
            }
            return this;
        }

        public Macros RemoveAll(Action callback = null)
        {
            _macroList.RemoveAll(t => true);
            Save();
            if (callback == null)
            {
                Context.API.ChangeQuery($"{ActionKeyword} ");
                Alert("Success", "all macros removed!");
            }
            else
            {
                callback();
            }
            return this;
        }

        public void Alert(string title, string content)
        {
            Context.API.ShowMsg(title, content, GetFilePath());
        }

        public string GetFilePath(string icon = "")
        {
            return Path.Combine(Context.CurrentPluginMetadata.PluginDirectory,
                string.IsNullOrEmpty(icon) ? @"ico\app.png" : icon);
        }

        private void Load()
        {
            if (!Directory.Exists(_dataFolderPath))
            {
                _dataFolderPath = Context.CurrentPluginMetadata.PluginDirectory;
            }
            try
            {
                var text = File.ReadAllText(Path.Combine(_dataFolderPath, DataFileName));
                _macroList = JsonConvert.DeserializeObject<List<Macro>>(text);
            }
            catch (FileNotFoundException)
            {                
                Save();
            }
            catch (Exception e)
            {
                throw new Exception($"can't read data file: {e.Message}!");
            }
        }

        private void Save()
        {
            try
            {
                if (_macroList is null)
                {
                    _macroList = new List<Macro>();
                }
                var json = JsonConvert.SerializeObject(_macroList);
                File.WriteAllText(Path.Combine(_dataFolderPath, DataFileName), json);
            }
            catch (Exception e)
            {
                throw new Exception($"write data failed: {e.Message}!");
            }
        }

        private List<Result> ToResults(
            IEnumerable<Macro> macros,
            Func<Macro, string> subTitleFormatter = null,
            Func<ActionContext, Macro, bool> itemAction = null)
        {
            var results = macros.OrderByDescending(t => t.CreatedTime)
                .Select(t => new Result
                {
                    Title = $"{t.Content}",
                    SubTitle = subTitleFormatter == null
                        ? $"{ToRelativeTime(t.CreatedTime)}"
                        : subTitleFormatter(t),
                    IcoPath = GetFilePath(@"ico\macro.png"),
                    Action = c =>
                    {
                        if (itemAction != null)
                        {
                            return itemAction(c, t);
                        }
                        try
                        {
                            Clipboard.SetText(t.Content);
                        }
                        catch (ExternalException)
                        {
                            Alert("Failed", "Copy failed, please try again later");
                        }
                        return true;
                    }
                }).ToList();

            if (!results.Any())
            {
                results.Add(new Result
                {
                    Title = "No results",
                    SubTitle = "click to view help",
                    IcoPath = GetFilePath(),
                    Action = c =>
                    {
                        Context.API.ChangeQuery($"{ActionKeyword} -h");
                        return false;
                    }
                });
            }
            return results;
        }

        private static string ToRelativeTime(DateTime value)
        {
            const int second = 1;
            const int minute = 60 * second;
            const int hour = 60 * minute;
            const int day = 24 * hour;
            const int month = 30 * day;

            var ts = DateTime.Now.Subtract(value);
            var seconds = ts.TotalSeconds;

            if (seconds < 1 * minute)
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

            if (seconds < 60 * minute)
                return ts.Minutes + " minutes ago";

            if (seconds < 120 * minute)
                return "an hour ago";

            if (seconds < 24 * hour)
                return ts.Hours + " hours ago";

            if (seconds < 48 * hour)
                return "yesterday";

            if (seconds < 30 * day)
                return ts.Days + " days ago";

            if (seconds < 12 * month)
            {
                var months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }

            var years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }

        public string getDir()
        {
            return Path.Combine(_dataFolderPath, DataFileName);
        }
    }
}
