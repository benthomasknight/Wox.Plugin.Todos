using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Wox.Infrastructure.Storage;

namespace Wox.Plugin.Macros
{
    public class Main : IPlugin, ISettingProvider, ISavable
    {
        private static Macros _macros;

        private readonly PluginJsonStorage<Settings> _storage;
        private readonly Settings _setting;
        private PluginInitContext _context;

        public Main()
        {
            // Setup the local storage for macros
            _storage = new PluginJsonStorage<Settings>();
            _setting = _storage.Load();
        }

        #region Query
        public List<Result> Query(Query query)
        {
            _macros.ActionKeyword = query.ActionKeyword;
            var help = new Help(_macros.Context, query);

            if (query.FirstSearch.Equals("-"))
            {
                return help.Show;
            }

            if (!query.FirstSearch.StartsWith("-"))
            {
                return Search(query.Search);
            }

            // Find the actual operator used
            MacroCommand op;
            if (!Enum.TryParse(query.FirstSearch.TrimStart('-'), true, out op))
            {
                return Search(query.Search);
            }

            switch (op)
            {
                case MacroCommand.H:
                    return help.Show;
                case MacroCommand.R:
                    if (query.SecondSearch.Equals("--all", StringComparison.OrdinalIgnoreCase))
                    {
                        return new List<Result> {
                            new Result {
                                Title = "Remove all macros?",
                                SubTitle = "click to remove all macros",
                                IcoPath = _macros.GetFilePath(),
                                Action = c => {
                                    _macros.RemoveAll();
                                    return true;
                                }
                            }
                        };
                    }
                    // Setup the removal on click of option. Searched both the key and content
                    var results = _macros.Find(
                        t => t.Key.IndexOf(query.SecondToEndSearch, StringComparison.OrdinalIgnoreCase) >= 0 || t.Content.IndexOf(query.SecondToEndSearch, StringComparison.OrdinalIgnoreCase) >= 0,
                        t2 => "click to remove macro",
                        (c, t3) =>
                        {
                            _macros.Remove(t3);
                            return true;
                        });
                    return results;
                case MacroCommand.A:
                    return new List<Result> {
                        // Adds a query to memory. the full command to get here is <command> <operation> <key> <macro>
                        AddResult(query.SecondSearch, string.Join(" ", query.Search.Split(' ').Where((a,b) => b >= 2)))
                    };
                case MacroCommand.L:
                    return Search(query.SecondToEndSearch);
                case MacroCommand.Rl:
                    return new List<Result> {
                        new Result {
                            Title = "Reload macros from data file?",
                            SubTitle = "click to reload",
                            IcoPath = _macros.GetFilePath(),
                            Action = c => {
                                _macros.Reload();
                                _macros.Context.API.ChangeQuery($"{query.ActionKeyword} ", true);
                                return false;
                            }
                        }
                    };
                default:
                    return Search(query.Search);
            }
        }

        #endregion

        public void Init(PluginInitContext context)
        {
            _macros = new Macros(context, _setting);
            _context = context;
        }

        public Control CreateSettingPanel()
        {
            return new FilePathSetting(_setting);
        }

        #region Utils

        private List<Result> Search(string search, Func<Macro, bool> conditions = null)
        {
            var s = search;
            var results = _macros.Find(
                t => t.Key.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 || t.Content.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0
                && (conditions?.Invoke(t) ?? true),
                null,
                (c, macro) =>
                {
                    // Actually changes the query when this macro is clicked
                    _context.API.ChangeQuery(macro.Content, true);
                    return false;
                });
            if (!string.IsNullOrEmpty(s) && !results.Any())
            {
                results.Insert(0, AddResult(s, null));
            }
            return results;
        }

        private static Result AddResult(string key, string content)
        {
            return new Result
            {
                Title = $"add new item \"{key}\"",
                SubTitle = $"{content}",
                IcoPath = _macros.GetFilePath(),
                Action = c =>
                {
                    _macros.Add(new Macro
                    {
                        Key = key,
                        Content = content,
                        CreatedTime = DateTime.Now
                    });
                    return false;
                }
            };
        }

        public void Save()
        {
            _storage.Save();
        }

        #endregion
    }
}
