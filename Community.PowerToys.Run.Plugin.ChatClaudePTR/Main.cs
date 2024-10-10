using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using ManagedCommon;
using Wox.Infrastructure;
using Wox.Plugin;
using Wox.Plugin.Logger;
using BrowserInfo = Wox.Plugin.Common.DefaultBrowserInfo;

namespace Community.PowerToys.Run.Plugin.ChatClaudePTR;
public class Main : IPlugin, IPluginI18n, IContextMenu, IReloadable, IDisposable
    {
        // Should only be set in Init()
        private Action onPluginError;

        private PluginInitContext _context;

        private string _iconPath;

        private bool _disposed;

        public static string PluginID => "2FA48E560F1D45C09FB969D6C403AA13";

        public string Name => "ChatClaudePTR";

        public string Description => "Chat with Claude AI";

        private static readonly CompositeFormat InBrowserName = System.Text.CompositeFormat.Parse("Chat with Claude");
        private static readonly CompositeFormat Open = System.Text.CompositeFormat.Parse("Open in {0}");
        private static readonly CompositeFormat SearchFailed = System.Text.CompositeFormat.Parse("Could not open search in {0}");

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            return new List<ContextMenuResult>(0);
        }

        public List<Result> Query(Query query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var results = new List<Result>();

            var searchTerm = query.Search;
            if (string.IsNullOrEmpty(query.Search))
            {
                const string arguments = "https://claude.ai/new";
                results.Add(new Result
                {
                    Title = "Chat with Claude AI",
                    SubTitle = string.Format(CultureInfo.CurrentCulture, InBrowserName, BrowserInfo.Name ?? BrowserInfo.MSEdgeName),
                    QueryTextDisplay = string.Empty,
                    IcoPath = _iconPath,
                    ProgramArguments = arguments,
                    Action = action =>
                    {
                        if (Helper.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, arguments))
                            return true;
                        onPluginError();
                        return false;

                    },
                });
                return results;
            }
            else
            {
                var result = new Result
                {
                    Title = searchTerm,
                    SubTitle = string.Format(CultureInfo.CurrentCulture, Open, BrowserInfo.Name ?? BrowserInfo.MSEdgeName),
                    QueryTextDisplay = searchTerm,
                    IcoPath = _iconPath,
                };

                var arguments = $"https://claude.ai/new?q={HttpUtility.UrlEncode(searchTerm)}";

                result.ProgramArguments = arguments;
                result.Action = action =>
                {
                    if (Helper.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, arguments))
                        return true;
                    onPluginError();
                    return false;

                };

                results.Add(result);
            }

            return results;
        }

        public void Init(PluginInitContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(_context.API.GetCurrentTheme());
            BrowserInfo.UpdateIfTimePassed();

            onPluginError = () =>
            {
                var errorMsgString = string.Format(CultureInfo.CurrentCulture, SearchFailed, BrowserInfo.Name ?? BrowserInfo.MSEdgeName);

                Log.Error(errorMsgString, this.GetType());
                _context.API.ShowMsg(
                    "Plugin: ChatClaudePTR",
                    errorMsgString);
            };
        }

        public string GetTranslatedPluginTitle()
        {
            return "ChatClaudePTR";
        }

        public string GetTranslatedPluginDescription()
        {
            return "Chat with Claude AI";
        }

        private void OnThemeChanged(Theme oldTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        private void UpdateIconPath(Theme theme)
        {
            _iconPath = "Images/claude-ai-square.png";
        }

        public void ReloadData()
        {
            if (_context is null)
            {
                return;
            }

            UpdateIconPath(_context.API.GetCurrentTheme());
            BrowserInfo.UpdateIfTimePassed();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed || !disposing) return;
            if (_context is { API: not null })
            {
                _context.API.ThemeChanged -= OnThemeChanged;
            }

            _disposed = true;
        }
    }