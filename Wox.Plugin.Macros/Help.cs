using System.Collections.Generic;
using System.IO;

namespace Wox.Plugin.Macros
{
    public class Help
    {
        private readonly PluginInitContext _context;
        private readonly Query _query;
        private readonly string _iconPath;

        public Help(PluginInitContext context, Query query)
        {
            _context = context;
            _query = query;
            _iconPath = Path.Combine(_context.CurrentPluginMetadata.PluginDirectory, @"ico\app.png");
        }


        public List<Result> Show
        {
            get
            {
                return new List<Result> {
                    new Result {
                        Title = $"{_query.ActionKeyword} -a [text]",
                        SubTitle = "add macros",
                        IcoPath = _iconPath,
                        Action = c => {
                            _context.API.ChangeQuery($"{_query.ActionKeyword} -a ");
                            return false;
                        }
                    },
                    new Result {
                        Title = $"{_query.ActionKeyword} -rl",
                        SubTitle = "reload macros from data file",
                        IcoPath = _iconPath,
                        Action = c => {
                            _context.API.ChangeQuery($"{_query.ActionKeyword} ");
                            return false;
                        }
                    },
                    new Result {
                        Title = $"{_query.ActionKeyword} [keyword]",
                        SubTitle = "list macros",
                        IcoPath = _iconPath,
                        Action = c => {
                            _context.API.ChangeQuery($"{_query.ActionKeyword} -l ");
                            return false;
                        }
                    },
                    new Result {
                        Title = $"{_query.ActionKeyword} -l [keyword]",
                        SubTitle = "list all macros",
                        IcoPath = _iconPath,
                        Action = c => {
                            _context.API.ChangeQuery($"{_query.ActionKeyword} -l ");
                            return false;
                        }
                    },
                    new Result {
                        Title = $"{_query.ActionKeyword} -r [keyword]",
                        SubTitle = "remove macros",
                        IcoPath = _iconPath,
                        Action = c => {
                            _context.API.ChangeQuery($"{_query.ActionKeyword} -r ");
                            return false;
                        }
                    },
                    new Result {
                        Title = $"{_query.ActionKeyword} -r --all",
                        SubTitle = "remove all macros",
                        IcoPath = _iconPath,
                        Action = c => {
                            _context.API.ChangeQuery($"{_query.ActionKeyword} -r --all");
                            return false;
                        }
                    },
                };
            }
        }
    }
}
